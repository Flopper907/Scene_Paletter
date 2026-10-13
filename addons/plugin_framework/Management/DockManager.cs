#if TOOLS
using Godot;

using System;
using System.Collections.Generic;

using Addons.PluginFramework.Core;
using static Godot.EditorPlugin;

namespace Addons.PluginFramework.Management;

/// <summary>
/// Manages all <c>PageDock</c> instances created by a plugin.
/// Responsible for creating, positioning, reloading, and disposing docks
/// within the Godot Editor UI.
/// </summary>
/// <remarks>
/// <para>
/// <c>DockManager</c> is owned by a <c>FrameworkPlugin</c> and relies on the configuration values
/// loaded by <c>ConfigLoader</c> to determine which docks should be created during plugin initialization.
/// </para>
///
/// <para>
/// To automatically initialize a <c>PageDock</c> when the plugin starts:
/// </para>
/// <list type="number">
/// <item>Add a <c>[page]</c> section to the config file</item>
/// <item>Define an <c>initial_docks</c> dictionary</item>
/// <item>Map <c>UIPosition</c> as <c>string</c> to page names</item>
/// </list>
/// <example>
/// <code>
/// [page]
/// initial_docks={
///     "RightDockTopLeft": "InitPage"
/// }
/// </code>
/// </example>
///
/// <para>
/// Each entry will create a <c>PageDock</c> at the given <c>UIPosition</c>
/// and load the specified page on startup.
/// </para>
///
/// <para>
/// The eight dock positions (<c>LeftDock*</c> / <c>RightDock*</c>) and <c>BottomPanel</c> are wrapped in an <c>EditorDock</c>
/// that is registered with <c>EditorPlugin.AddDock</c>; its tab title follows the page's <c>Title</c>.
/// </para>
/// </remarks>
public class DockManager : IDisposable
{
    private Dictionary<UIPosition, PageDock> docks;
    private readonly Dictionary<UIPosition, EditorDock> editorDocks = new();
    private readonly FrameworkPlugin plugin;
    private PopupPanel dialogWindow;

    /// <summary>
    /// Creates a manager for <paramref name="plugin"/>. Call <c>Init</c> to start the configured docks.
    /// </summary>
    public DockManager(FrameworkPlugin plugin)
    {
        this.plugin = plugin;
    }

    /// <summary>
    /// Initializes internal state and starts all <c>initial_docks</c> defined in the <c>config</c>.
    /// </summary>
    /// <remarks>
    /// Requires <c>ConfigLoader</c> to be initialized before.
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPluginException</c> if <c>plugin</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingConfigException</c> if <c>config</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowNullReferenceException</c> if <c>config.InitialDocks</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowInvalidUIPositionException</c> if a key is not a <c>UIPosition</c>.</para>
    /// </remarks>
    public void Init()
    {
        docks = new Dictionary<UIPosition, PageDock>();
        foreach (UIPosition pos in Enum.GetValues<UIPosition>())
        {
            docks[pos] = null;
        }

        if (plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException("DockManager.Init()");
            return;
        }
        if (plugin.config == null)
        {
            ExceptionHandler.ThrowMissingConfigException("DockManager.Init()");
            return;
        }
        if (plugin.config.InitialDocks == null)
        {
            ExceptionHandler.ThrowNullReferenceException("config.InitialDocks", "DockManager.Init()");
            return;
        }

        foreach (var item in plugin.config.InitialDocks)
        {
            if (Enum.TryParse<UIPosition>(item.Key, out var pos))
                StartDock(pos, item.Value);
            else
                ExceptionHandler.ThrowInvalidUIPositionException(item.Key, "DockManager.Init()");
        }
    }

    /// <summary>
    /// Returns whether a valid <c>PageDock</c> exists at the given position.
    /// </summary>
    public bool IsDockInstanced(UIPosition position)
    {
        return docks != null && docks.TryGetValue(position, out var d) && GodotObject.IsInstanceValid(d);
    }

    /// <summary>
    /// Returns the dock positions (<c>LeftDock*</c> / <c>RightDock*</c> / <c>BottomPanel</c>) whose <c>EditorDock</c> is
    /// currently visible, i.e. its tab is the selected one and its slot (or the bottom panel) is open.
    /// </summary>
    /// <remarks>
    /// Container positions and the dialog are never returned. Returns an empty list when no dock exists.
    /// </remarks>
    public List<UIPosition> GetVisibleDockPositions()
    {
        var visible = new List<UIPosition>();
        foreach (var (pos, editorDock) in editorDocks)
        {
            if (GodotObject.IsInstanceValid(editorDock) && editorDock.IsVisibleInTree())
                visible.Add(pos);
        }
        return visible;
    }

    /// <summary>
    /// Focuses the <c>EditorDock</c> at <paramref name="position"/> via <c>EditorDock.MakeVisible</c>: selects its tab,
    /// opens it if it was closed and, for <c>BottomPanel</c>, shows the bottom panel.
    /// </summary>
    /// <remarks>
    /// Does nothing (no error logged) if there is no <c>EditorDock</c> at <paramref name="position"/>, e.g. a container
    /// position, the dialog, or a dock that failed to start.
    /// </remarks>
    public void FocusDock(UIPosition position)
    {
        if (editorDocks.TryGetValue(position, out var editorDock) && GodotObject.IsInstanceValid(editorDock))
            editorDock.MakeVisible();
    }

    /// <summary>
    /// Ensures the internal dock registry has been initialized.
    /// </summary>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowNullReferenceException</c> if <c>docks</c> is <c>null</c>.</para>
    /// </remarks>
    private void EnsureInitialized(string caller)
    {
        if (docks == null)
            ExceptionHandler.ThrowNullReferenceException("DockManager.docks", caller);
    }

    /// <summary>
    /// Returns an existing dock or logs an error if it does not exist.
    /// </summary>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowDockNotFoundException</c> if dock at <c>position</c> is <c>null</c>.</para>
    /// </remarks>
    private PageDock GetDockOrThrow(UIPosition position, string caller)
    {
        if (!IsDockInstanced(position))
        {
            ExceptionHandler.ThrowDockNotFoundException(position, caller);
            return null;
        }

        return docks[position];
    }

    /// <summary>
    /// Initializes a dock with <c>page</c> at <c>position</c> with <c>data</c>.
    /// </summary>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPluginException</c> if <c>plugin</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowDockAlreadyExistsException</c> if <c>position</c> is occupied.</para>
    /// <para>
    /// If the page cannot be loaded, the dock is discarded and nothing is added to the editor, so no empty
    /// dock stays behind.
    /// </para>
    /// </remarks>
    public void StartDock(UIPosition position, string page, object data = null)
    {
        if (plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException(nameof(StartDock));
            return;
        }

        EnsureInitialized(nameof(StartDock));
        if (docks == null)
            return;

        if (IsDockInstanced(position))
        {
            ExceptionHandler.ThrowDockAlreadyExistsException(position, nameof(StartDock));
            return;
        }

        var dock = new PageDock
        {
            Name = page,
            plugin = plugin
        };

        if (!dock.SwitchPage(page, data))
        {
            dock.Free();
            return;
        }

        docks[position] = dock;
        try
        {
            SetDockToPosition(dock, position);
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{nameof(StartDock)} - {position}");
            docks[position] = null;
            if (dock.GetParent() == null)
                dock.Free();
            else
                dock.QueueFree();
        }
    }

    /// <summary>
    /// Closes and disposes the dock from <c>position</c>.
    /// </summary>
    /// <remarks>
    /// Uses <c>GetDockOrThrow</c> to get the dock at <c>position</c>.
    /// </remarks>
    public void CloseDock(UIPosition position)
    {
        EnsureInitialized(nameof(CloseDock));

        var dock = GetDockOrThrow(position, nameof(CloseDock));

        if (dock == null) return;

        // Unregister first: removing a dialog can raise PopupHide, which calls CloseDock again.
        docks[position] = null;
        RemoveDockFromPosition(dock, position);
        dock.QueueFree();
    }

    /// <summary>
    /// Sets the window size of the dialog dock.
    /// </summary>
    /// <remarks>
    /// Requires a dock instance at <c>UIPosition.Dialog</c>.
    /// <para>Uses <c>GetDockOrThrow</c> to get the dock.</para>
    /// </remarks>
    public void SetDialogSize(Vector2I size)
    {
        EnsureInitialized(nameof(SetDialogSize));

        GetDockOrThrow(UIPosition.Dialog, nameof(SetDialogSize));

        if (!GodotObject.IsInstanceValid(dialogWindow))
            return;

        dialogWindow.Size = size;
    }

    /// <summary>
    /// Adds the <c>dock</c> into the editor structure of Godot depending on the <c>position</c>.
    /// </summary>
    private void SetDockToPosition(PageDock dock, UIPosition pos)
    {
        switch (pos)
        {
            case UIPosition.Dialog:
                SetupDialog(dock);
                break;

            case UIPosition.BottomPanel:
            case UIPosition.LeftDockTopLeft:
            case UIPosition.LeftDockTopRight:
            case UIPosition.LeftDockBottomLeft:
            case UIPosition.LeftDockBottomRight:
            case UIPosition.RightDockTopLeft:
            case UIPosition.RightDockTopRight:
            case UIPosition.RightDockBottomLeft:
            case UIPosition.RightDockBottomRight:
                AddEditorDock(dock, pos);
                break;

            default:
                plugin.AddControlToContainer(GetContainer(pos), dock);
                break;
        }
    }

    /// <summary>
    /// Removes the <c>dock</c> from the editor structure of Godot depending on the <c>position</c>.
    /// </summary>
    private void RemoveDockFromPosition(PageDock dock, UIPosition pos)
    {
        switch (pos)
        {
            case UIPosition.Dialog:
                RemoveDialog(dock);
                break;

            case UIPosition.BottomPanel:
            case UIPosition.LeftDockTopLeft:
            case UIPosition.LeftDockTopRight:
            case UIPosition.LeftDockBottomLeft:
            case UIPosition.LeftDockBottomRight:
            case UIPosition.RightDockTopLeft:
            case UIPosition.RightDockTopRight:
            case UIPosition.RightDockBottomLeft:
            case UIPosition.RightDockBottomRight:
                RemoveEditorDock(pos);
                break;

            default:
                plugin.RemoveControlFromContainer(GetContainer(pos), dock);
                break;
        }
    }

    /// <summary>
    /// Wraps <c>dock</c> in an <c>EditorDock</c> for the slot of <c>pos</c> and registers it with the editor.
    /// The tab title follows the page's title.
    /// </summary>
    private void AddEditorDock(PageDock dock, UIPosition pos)
    {
        var editorDock = new EditorDock
        {
            Title = dock.Name,
            DefaultSlot = GetDockSlot(pos)
        };
        editorDock.AddChild(dock);
        dock.TitleChanged += () =>
        {
            if (GodotObject.IsInstanceValid(editorDock) && GodotObject.IsInstanceValid(dock))
                editorDock.Title = dock.Name;
        };

        editorDocks[pos] = editorDock;
        plugin.AddDock(editorDock);
    }

    /// <summary>
    /// Unregisters and frees the <c>EditorDock</c> of <c>pos</c> (including the <c>PageDock</c> inside it).
    /// </summary>
    private void RemoveEditorDock(UIPosition pos)
    {
        if (!editorDocks.Remove(pos, out var editorDock))
            return;

        if (!GodotObject.IsInstanceValid(editorDock))
            return;

        plugin.RemoveDock(editorDock);
        editorDock.QueueFree();
    }

    /// <summary>
    /// Sets up a new Dialog with <c>dock</c> as the content.
    /// Stores the created window internally for later resizing and disposal.
    /// </summary>
    private void SetupDialog(Control dock)
    {
        PopupPanel window = new PopupPanel();
        window.Size = new Vector2I(400, 300);
        window.Borderless = false;
        window.Unresizable = false;

        window.AddChild(dock);

        dock.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        window.PopupHide += () =>
        {
            if (IsDockInstanced(UIPosition.Dialog))
                CloseDock(UIPosition.Dialog);
        };

        EditorInterface.Singleton.GetBaseControl().AddChild(window);
        window.PopupCentered();
        dialogWindow = window;
    }

    /// <summary>
    /// Removes the active Dialog window.
    /// </summary>
    /// <remarks>
    /// No error is logged when <c>dialogWindow</c> is <c>null</c>.
    /// </remarks>
    private void RemoveDialog(Control dock)
    {
        if (GodotObject.IsInstanceValid(dialogWindow))
        {
            if (dock.GetParent() == dialogWindow)
                dialogWindow.RemoveChild(dock);
            dialogWindow.QueueFree();
        }
        dialogWindow = null;
    }

    /// <summary>
    /// Maps the <c>UIPosition position</c> onto Godot's <c>CustomControlContainer</c>.
    /// </summary>
    private CustomControlContainer GetContainer(UIPosition position) => position switch
    {
        UIPosition.Editor3DToolBar => CustomControlContainer.SpatialEditorMenu,
        UIPosition.Editor3DLeft => CustomControlContainer.SpatialEditorSideLeft,
        UIPosition.Editor3DRight => CustomControlContainer.SpatialEditorSideRight,
        UIPosition.Editor3DBottom => CustomControlContainer.SpatialEditorBottom,
        UIPosition.Editor2DToolBar => CustomControlContainer.CanvasEditorMenu,
        UIPosition.Editor2DLeft => CustomControlContainer.CanvasEditorSideLeft,
        UIPosition.Editor2DRight => CustomControlContainer.CanvasEditorSideRight,
        UIPosition.Editor2DBottom => CustomControlContainer.CanvasEditorBottom,
        UIPosition.InspectorBottom => CustomControlContainer.InspectorBottom,
        UIPosition.ProjectSettingLeft => CustomControlContainer.ProjectSettingTabLeft,
        UIPosition.ProjectSettingRight => CustomControlContainer.ProjectSettingTabRight,
        _ => throw new ArgumentException($"Not a container position: {position}")
    };

    /// <summary>
    /// Maps the <c>UIPosition position</c> onto Godot's <c>EditorDock.DockSlot</c>.
    /// </summary>
    private static EditorDock.DockSlot GetDockSlot(UIPosition position) => position switch
    {
        UIPosition.LeftDockTopLeft => EditorDock.DockSlot.LeftUl,
        UIPosition.LeftDockTopRight => EditorDock.DockSlot.LeftUr,
        UIPosition.LeftDockBottomLeft => EditorDock.DockSlot.LeftBl,
        UIPosition.LeftDockBottomRight => EditorDock.DockSlot.LeftBr,
        UIPosition.RightDockTopLeft => EditorDock.DockSlot.RightUl,
        UIPosition.RightDockTopRight => EditorDock.DockSlot.RightUr,
        UIPosition.RightDockBottomLeft => EditorDock.DockSlot.RightBl,
        UIPosition.RightDockBottomRight => EditorDock.DockSlot.RightBr,
        UIPosition.BottomPanel => EditorDock.DockSlot.Bottom,
        _ => throw new ArgumentException($"Not a dock position: {position}")
    };

    /// <summary>
    /// Closes all of the opened docks and clears <c>docks</c>.
    /// </summary>
    /// <remarks>
    /// No error is logged when <c>docks</c> is <c>null</c>.
    /// </remarks>
    public void Dispose()
    {
        if (docks == null)
            return;

        foreach (var pos in new List<UIPosition>(docks.Keys))
        {
            if (IsDockInstanced(pos))
                CloseDock(pos);
        }

        docks.Clear();
        docks = null;
    }
}
#endif
