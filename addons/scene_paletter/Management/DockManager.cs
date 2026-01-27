using Godot;

using System;
using System.Collections.Generic;

using Addons.ScenePaletter.Core;
using static Godot.EditorPlugin;

namespace Addons.ScenePaletter.Management;

public class Dockmanager : IDisposable
{
    private Dictionary<UIPosition, PageDock> docks;
    private Plugin plugin;
    private PopupPanel dialogWindow;

    public Dockmanager(Plugin plugin)
    {
        this.plugin = plugin;
    }


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

    public bool IsDockInstanced(UIPosition position)
    {
        return docks != null && docks.TryGetValue(position, out var d) && GodotObject.IsInstanceValid(d);
    }

    private void EnsureInitialized(string caller)
    {
        if (docks == null)
            ExceptionHandler.ThrowNullReferenceException("DockManager.docks", caller);
    }

    private PageDock GetDockOrThrow(UIPosition position, string caller)
    {
        if (!IsDockInstanced(position))
            ExceptionHandler.ThrowDockNotFoundException(position, caller);

        return docks[position];
    }


    public void ChangeDockPosition(UIPosition from, UIPosition to)
    {
        EnsureInitialized(nameof(ChangeDockPosition));

        var dock = GetDockOrThrow(from, nameof(ChangeDockPosition));

        if (IsDockInstanced(to))
            CloseDock(to);

        RemoveDockFromPosition(dock, from);
        docks[from] = null;

        docks[to] = dock;
        SetDockToPosition(dock, to);
    }

    public void ReloadDock(UIPosition position, object data)
    {
        EnsureInitialized(nameof(ReloadDock));

        if (!IsDockInstanced(position))
            return;

        docks[position].Reload(data);
    }

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

    public void CloseDock(UIPosition position)
    {
        EnsureInitialized(nameof(CloseDock));

        var dock = GetDockOrThrow(position, nameof(CloseDock));

        RemoveDockFromPosition(dock, position);
        dock.QueueFree();
        docks[position] = null;
    }

    public void SetDialogSize(Vector2I size)
    {
        EnsureInitialized(nameof(SetDialogSize));

        var dock = GetDockOrThrow(UIPosition.Dialog, nameof(SetDialogSize));

        var parent = dock.GetParent();
        if (!GodotObject.IsInstanceValid(parent))
        {
            ExceptionHandler.ThrowMissingNodeException(dock.GetPath(), nameof(SetDialogSize));
            return;
        }

        if (parent is not PopupPanel panel)
        {
            ExceptionHandler.ThrowInvalidNodeTypeException(
                parent.GetPath(),
                typeof(PopupPanel).ToString(),
                parent.GetType().ToString()
            );
            return;
        }

        panel.Size = size;
    }

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

    private void RemoveDialog()
    {
        if (GodotObject.IsInstanceValid(dialogWindow))
        {
            dialogWindow.QueueFree();
            dialogWindow = null;
        }
    }

    private CustomControlContainer GetContainer(UIPosition pos) => pos switch
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
        _ => throw new ArgumentException($"Not a container position: {pos}")
    };

    private DockSlot GetDockSlot(UIPosition pos) => pos switch
    {
        UIPosition.LeftDockTopLeft => DockSlot.LeftUl,
        UIPosition.LeftDockTopRight => DockSlot.LeftUr,
        UIPosition.LeftDockBottomLeft => DockSlot.LeftBl,
        UIPosition.LeftDockBottomRight => DockSlot.LeftBr,
        UIPosition.RightDockTopLeft => DockSlot.RightUl,
        UIPosition.RightDockTopRight => DockSlot.RightUr,
        UIPosition.RightDockBottomLeft => DockSlot.RightBl,
        UIPosition.RightDockBottomRight => DockSlot.RightBr,
        _ => throw new ArgumentException($"Not a dock position: {pos}")
    };


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
    }
}