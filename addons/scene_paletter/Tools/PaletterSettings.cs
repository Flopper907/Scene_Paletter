using System;
using Addons.PluginFramework.Management;
using Godot;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Scene Paletter specific settings read from the <c>[file]</c> and <c>[ui]</c> sections of <c>plugin.cfg</c>,
/// plus the mutable column count of the list views.
/// </summary>
/// <remarks>
/// Created once in <c>Plugin.OnEnable</c>. The framework's <c>ConfigLoader</c> knows nothing about these keys.
/// </remarks>
public class PaletterSettings
{
    /// <summary>Directory the palette files are stored in.</summary>
    public string PalettePath { get; }

    /// <summary>File extension of palette files, including the dot.</summary>
    public string FileExtension { get; }

    /// <summary>Smallest generated palette id (inclusive).</summary>
    public int IdStart { get; }

    /// <summary>Largest generated palette id (inclusive).</summary>
    public int IdEnd { get; }

    /// <summary>Lowest column count of the list views.</summary>
    public int MinColumns { get; }

    /// <summary>Highest column count of the list views.</summary>
    public int MaxColumns { get; }

    /// <summary>Current column count of the list views; changed with <c>AddColumn</c> / <c>RemoveColumn</c>.</summary>
    public int Columns { get; private set; }

    /// <summary>
    /// Method the placer uses to extrapolate the next pose; set by the Method dropdown of <c>PlacingPage</c>.
    /// Kept in memory only (like <c>Columns</c>): it survives page rebuilds but resets to Linear on editor restart.
    /// </summary>
    public ExtrapolationMethod Method { get; set; } = ExtrapolationMethod.Linear;

    /// <summary>
    /// Whether the Placing page draws the placement preview (chain lines and next-position marker) in the viewport;
    /// set by the Preview checkbox. Kept in memory only (like <c>Method</c>): resets to on at editor restart.
    /// </summary>
    public bool ShowPreview { get; set; } = true;

    /// <summary>Pixel size of generated scene previews.</summary>
    public Vector2I PreviewResolution { get; }

    /// <summary>Margin factor around the previewed scene.</summary>
    public Vector2 PreviewMargin { get; }

    /// <summary>Whether 2D previews are rendered with a transparent background.</summary>
    public bool PreviewTransparent2D { get; }

    /// <summary>Whether 3D previews are rendered with a transparent background.</summary>
    public bool PreviewTransparent3D { get; }

    /// <summary>
    /// Reads all Scene Paletter keys from <paramref name="config"/>.
    /// </summary>
    public PaletterSettings(ConfigLoader config)
    {
        PalettePath = config.GetString("file", "palette_path");
        FileExtension = config.GetString("file", "file_extension");
        IdStart = config.GetInt("file", "id_start");
        IdEnd = config.GetInt("file", "id_end");

        MaxColumns = config.GetInt("ui", "max_columns", 6);
        MinColumns = config.GetInt("ui", "min_columns", 1);
        Columns = config.GetInt("ui", "columns", 2);
        PreviewResolution = config.GetVector2I("ui", "preview_resolution_x", "preview_resolution_y", new Vector2I(256, 256));
        PreviewMargin = config.GetVector2("ui", "preview_margin_x", "preview_margin_y", new Vector2(10f, 10f));
        PreviewTransparent2D = config.GetBool("ui", "preview_2d_transparent");
        PreviewTransparent3D = config.GetBool("ui", "preview_3d_transparent");
    }

    /// <summary>Increments <c>Columns</c> up to <c>MaxColumns</c>.</summary>
    public void AddColumn()
    {
        Columns = Math.Min(MaxColumns, Columns + 1);
    }

    /// <summary>Decrements <c>Columns</c> down to <c>MinColumns</c>.</summary>
    public void RemoveColumn()
    {
        Columns = Math.Max(MinColumns, Columns - 1);
    }
}
