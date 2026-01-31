using Godot;

using System;
using System.Collections.Generic;

using Addons.ScenePaletter.Core;
using static Godot.EditorPlugin;

namespace Addons.ScenePaletter.Management;

/// <summary>
/// Manages all <c>PageDock</c> instances created by the plugin.
/// Responsible for creating, positioning, moving, reloading, and disposing docks
/// within the Godot Editor UI.
/// </summary>
/// <remarks>
/// <para>
/// <c>DockManager</c> is tightly coupled to <c>Plugin</c> and relies on
/// configuration values loaded by <c>ConfigLoader</c> to determine which docks
/// should be created during plugin initialization.
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
/// </remarks>
public class DockManager : IDisposable
{
    private Dictionary<UIPosition, PageDock> docks;
    private Plugin plugin;
    private PopupPanel dialogWindow;

    public DockManager(Plugin plugin)
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
            if (Enum.TryParse<UIPosition>(item.Key.ToString(), out var pos))
            {
                StartDock(pos, item.Value);
            }
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
    /// Moves a dock from <c>from</c> to <c>to</c>.
    /// </summary>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowNullReferenceException</c> if manager is not initialized.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowDockNotFoundException</c> if no dock exists at <c>from</c>.</para>
    /// 
    /// <para>
    /// If a dock already exists at <c>to</c>, it is closed without logging.
    /// </para>
    /// </remarks>
    public void ChangeDockPosition(UIPosition from, UIPosition to)
    {
        EnsureInitialized(nameof(ChangeDockPosition));

        var dock = GetDockOrThrow(from, nameof(ChangeDockPosition));

        if (dock == null) return;

        if (IsDockInstanced(to))
            CloseDock(to);

        RemoveDockFromPosition(dock, from);
        docks[from] = null;

        docks[to] = dock;
        SetDockToPosition(dock, to);
    }

    /// <summary>
    /// Reloads the dock at <c>position</c> with <c>data</c>.
    /// </summary>
    /// <remarks>
    /// Uses <c>GetDockOrThrow</c> to get the dock at <c>position</c>.
    /// </remarks>
    public void ReloadDock(UIPosition position, object data)
    {
        EnsureInitialized(nameof(ReloadDock));

        if (!IsDockInstanced(position))
            return;

        var dock = GetDockOrThrow(position, nameof(ReloadDock));
        if (dock == null) return;
        dock.ReloadPage(data);
    }

    /// <summary>
    /// Initializes a dock with <c>page</c> at <c>position</c> with <c>data</c>.
    /// </summary>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPluginException</c> if <c>plugin</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowDockAlreadyExistsException</c> if <c>position</c> is occupied.</para>
    /// </remarks>
    public void StartDock(UIPosition position, string page, object data = null)
    {
        if (plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException(nameof(StartDock));
            return;
        }

        EnsureInitialized(nameof(StartDock));

        if (IsDockInstanced(position))
        {
            ExceptionHandler.ThrowDockAlreadyExistsException(position, nameof(StartDock));
            return;
        }

        var dock = new PageDock(plugin)
        {
            Name = page
        };

        docks[position] = dock;
        SetDockToPosition(dock, position);
        dock.SwitchPage(page, data);
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

        RemoveDockFromPosition(dock, position);
        dock.QueueFree();
        docks[position] = null;
    }

    /// <summary>
    /// Sets the window size of the dialog dock.
    /// </summary>
    /// <remarks>
    /// Requires a dock instance at <c>UIPosition.Dialog</c>.
    /// <para>Uses <c>GetDockOrThrow</c> to get the dock.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingNodeException</c> if <c>dialogWindow</c> is missing or invalid.</para>
    /// </remarks>
    public void SetDialogSize(Vector2I size)
    {
        EnsureInitialized(nameof(SetDialogSize));

        var dock = GetDockOrThrow(UIPosition.Dialog, nameof(SetDialogSize));

        if (!GodotObject.IsInstanceValid(dialogWindow))
        {
            //ExceptionHandler.ThrowMissingNodeException(dock.GetPath(), nameof(SetDialogSize));
            return;
        }

        dialogWindow.Size = size;
    }

    /// <summary>
    /// Adds the <c>dock</c> into the editor structure of Godot depending on the <c>position</c>.
    /// </summary>
    private void SetDockToPosition(Control dock, UIPosition pos)
    {
        switch (pos)
        {
            case UIPosition.Dialog:
                SetupDialog(dock);
                break;

            case UIPosition.BottomPanel:
                plugin.AddControlToBottomPanel(dock, dock.Name);
                break;

            case UIPosition.LeftDockTopLeft:
            case UIPosition.LeftDockTopRight:
            case UIPosition.LeftDockBottomLeft:
            case UIPosition.LeftDockBottomRight:
            case UIPosition.RightDockTopLeft:
            case UIPosition.RightDockTopRight:
            case UIPosition.RightDockBottomLeft:
            case UIPosition.RightDockBottomRight:
                plugin.AddControlToDock(GetDockSlot(pos), dock);
                break;

            default:
                plugin.AddControlToContainer(GetContainer(pos), dock);
                break;
        }
    }

    /// <summary>
    /// Removes the <c>dock</c> from the editor structure of Godot depending on the <c>position</c>.
    /// </summary>
    private void RemoveDockFromPosition(Control dock, UIPosition pos)
    {
        switch (pos)
        {
            case UIPosition.Dialog:
                RemoveDialog();
                break;

            case UIPosition.BottomPanel:
                plugin.RemoveControlFromBottomPanel(dock);
                break;

            case UIPosition.LeftDockTopLeft:
            case UIPosition.LeftDockTopRight:
            case UIPosition.LeftDockBottomLeft:
            case UIPosition.LeftDockBottomRight:
            case UIPosition.RightDockTopLeft:
            case UIPosition.RightDockTopRight:
            case UIPosition.RightDockBottomLeft:
            case UIPosition.RightDockBottomRight:
                plugin.RemoveControlFromDocks(dock);
                break;

            default:
                plugin.RemoveControlFromContainer(GetContainer(pos), dock);
                break;
        }
    }

    /// <summary>
    /// Sets up a new Dialog with <c>dock</c> as the content.
    /// Stores the created window internally for later resizing and disposal.
    /// </summary>
    private void SetupDialog(Control dock)
    {
        Control dialogContent = dock;

        PopupPanel window = new PopupPanel();
        window.Size = new Vector2I(400, 300);
        window.Borderless = false;
        window.Unresizable = false;

        window.AddChild(dialogContent);

        dialogContent.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        dialogContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        window.PopupHide += () =>
        {
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
    private void RemoveDialog()
    {
        if (GodotObject.IsInstanceValid(dialogWindow))
        {
            dialogWindow.RemoveChild(docks[UIPosition.Dialog]);
            dialogWindow.QueueFree();
        }
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
    /// Maps the <c>UIPosition position</c> onto Godot's <c>DockSlot</c>.
    /// </summary>
    private DockSlot GetDockSlot(UIPosition position) => position switch
    {
        UIPosition.LeftDockTopLeft => DockSlot.LeftUl,
        UIPosition.LeftDockTopRight => DockSlot.LeftUr,
        UIPosition.LeftDockBottomLeft => DockSlot.LeftBl,
        UIPosition.LeftDockBottomRight => DockSlot.LeftBr,
        UIPosition.RightDockTopLeft => DockSlot.RightUl,
        UIPosition.RightDockTopRight => DockSlot.RightUr,
        UIPosition.RightDockBottomLeft => DockSlot.RightBl,
        UIPosition.RightDockBottomRight => DockSlot.RightBr,
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

        foreach (var (pos, dock) in docks)
        {
            if (GodotObject.IsInstanceValid(dock))
                CloseDock(pos);
        }

        docks.Clear();
        docks = null;
    }
}