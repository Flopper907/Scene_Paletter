using System.Collections.Generic;
using Addons.PluginFramework;
using Addons.PluginFramework.Core;
using Addons.ScenePaletter.Preview;
using Addons.ScenePaletter.Tools;
using Godot;

namespace Addons.ScenePaletter;

/// <summary>
/// Main plugin class for Scene Paletter, a palette-based workflow for quickly placing frequently-used
/// scenes and global C# classes into your levels (2D and 3D, with automatic position extrapolation).
/// </summary>
/// <remarks>
/// <para>
/// Config loading, scene loading and dock creation are done by <c>FrameworkPlugin</c>; this class adds
/// the Scene Paletter specific settings, makes sure the palette directory exists and owns the placement preview:
/// it enables the force-draw overlay hooks, polls the preview while one is shown and forwards the draw calls.
/// </para>
/// </remarks>
[Tool]
public partial class Plugin : FrameworkPlugin
{
    /// <summary>Seconds a shown preview may wait for its first overlay draw call before a diagnostic warning is logged.</summary>
    private const double HookTimeoutSeconds = 3.0;

    /// <summary>Scene Paletter specific settings from <c>plugin.cfg</c>; <c>null</c> while the plugin is disabled.</summary>
    public PaletterSettings settings;

    private PlacementPreview3D preview3D;
    private PlacementPreview2D preview2D;
    private PlacementPreview activePreview;
    private bool drawReceived;
    private bool hookWarned;
    private double secondsWithoutDraw;

    /// <summary>
    /// Whether <c>SetForceDrawOverForwardingEnabled</c> was already called on this native plugin object.
    /// </summary>
    /// <remarks>
    /// A Variant-compatible field, so it survives a C# assembly reload (same native object, registration still active);
    /// disabling the plugin destroys the object, so a re-enable starts with <c>false</c>. Never reset in <c>OnDisable</c>.
    /// </remarks>
    private bool forceDrawOverEnabled;

    /// <summary>
    /// Reads the settings, clears cached previews, creates the palette directory and prepares the placement preview.
    /// </summary>
    protected override void OnEnable()
    {
        settings = new PaletterSettings(config);
        ScenePreviewGenerator.ClearCache();
        SaveLoad.EnsureDirectoryExists(settings.PalettePath);

        preview3D = new PlacementPreview3D(settings);
        preview2D = new PlacementPreview2D(settings);
        // Godot has no removal API and errors on a second call for the same object (e.g. after a C# reload).
        if (!forceDrawOverEnabled)
        {
            SetForceDrawOverForwardingEnabled();
            forceDrawOverEnabled = true;
        }
        SetProcess(false);
    }

    /// <summary>
    /// Hides the placement preview and releases the settings.
    /// </summary>
    protected override void OnDisable()
    {
        HidePreview();
        preview3D = null;
        preview2D = null;
        settings = null;
    }

    /// <summary>
    /// Shows the placement preview for the given entry dimension, or hides it if the preview is switched off in the settings
    /// or <paramref name="kind"/> is <see cref="SpatialKind.None"/>.
    /// </summary>
    /// <param name="kind">Dimension of the selected palette entry</param>
    /// <param name="placed">The placing page's list of placed nodes, read live</param>
    public void ShowPreview(SpatialKind kind, List<Node> placed)
    {
        activePreview?.Unbind();
        activePreview = settings is { ShowPreview: true } ? kind switch
        {
            SpatialKind.Node3D => preview3D,
            SpatialKind.Node2D => preview2D,
            _ => null,
        } : null;
        activePreview?.Bind(placed);

        drawReceived = false;
        secondsWithoutDraw = 0;
        SetProcess(activePreview != null);
        RequestRedraw();
    }

    /// <summary>
    /// Hides the placement preview. Safe to call after the plugin was disabled.
    /// </summary>
    public void HidePreview()
    {
        activePreview?.Unbind();
        activePreview = null;
        SetProcess(false);
        RequestRedraw();
    }

    /// <summary>
    /// Polls the shown preview and repaints the overlays only when its points changed.
    /// </summary>
    /// <param name="delta">Seconds since the last frame</param>
    /// <remarks>
    /// Only runs while a preview is shown. Also logs once if no overlay draw call arrived for <see cref="HookTimeoutSeconds"/>,
    /// which means the force-draw hooks do not fire for the current editor state.
    /// </remarks>
    public override void _Process(double delta)
    {
        if (activePreview == null)
            return;

        try
        {
            if (activePreview.Refresh())
                UpdateOverlays();
        }
        catch (System.Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(_Process)}");
        }

        if (drawReceived || hookWarned)
            return;

        secondsWithoutDraw += delta;
        if (secondsWithoutDraw >= HookTimeoutSeconds)
        {
            hookWarned = true;
            ExceptionHandler.LogWarning(
                $"Placement preview is on but no overlay draw call arrived within {HookTimeoutSeconds:0} s. " +
                "Either no matching 2D/3D viewport is visible, or the force-draw overlay hooks do not fire for the current selection.",
                $"{GetType().Name} {nameof(_Process)}");
        }
    }

    /// <summary>
    /// Draws the 3D placement preview into an editor 3D viewport overlay.
    /// </summary>
    /// <param name="viewportControl">Overlay control of one 3D viewport</param>
    public override void _Forward3DForceDrawOverViewport(Control viewportControl)
    {
        if (activePreview is not PlacementPreview3D preview)
            return;

        drawReceived = true;
        ExceptionHandler.SafeExecute(() => preview.Draw(viewportControl), nameof(PlacementPreview3D.Draw), nameof(_Forward3DForceDrawOverViewport));
    }

    /// <summary>
    /// Draws the 2D placement preview into the editor 2D viewport overlay.
    /// </summary>
    /// <param name="viewportControl">Overlay control of the 2D viewport</param>
    public override void _ForwardCanvasForceDrawOverViewport(Control viewportControl)
    {
        if (activePreview is not PlacementPreview2D preview)
            return;

        drawReceived = true;
        ExceptionHandler.SafeExecute(() => preview.Draw(viewportControl), nameof(PlacementPreview2D.Draw), nameof(_ForwardCanvasForceDrawOverViewport));
    }

    private void RequestRedraw()
    {
        if (IsInsideTree())
            UpdateOverlays();
    }
}
