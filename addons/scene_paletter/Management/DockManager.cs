using System;
using System.Collections.Generic;
using Godot;
using static Godot.EditorPlugin;

namespace Addons.ScenePaletter.Management;

public class Dockmanager : IDisposable
{
    public Dictionary<UIPosition, PageDock> docks;
    private Plugin plugin;
    private PopupPanel dialogWindow;

    public Dockmanager(Plugin plugin)
    {
        this.plugin = plugin;
    }


    public void InitDocks()
    {
        docks = new Dictionary<UIPosition, PageDock>();

        foreach (UIPosition pos in Enum.GetValues<UIPosition>())
        {
            docks[pos] = null;
        }

        foreach (var item in plugin.config.InitialDocks)
        {
            if (Enum.TryParse<UIPosition>(item.Key.ToString(), out var pos))
            {
                StartDock(pos, item.Value);
            }
        }
    }


    public void ChangeDockPosition(UIPosition from, UIPosition to)
    {
        if (docks[from] == null || !GodotObject.IsInstanceValid(docks[from]))
            return;

        // If destination has a dock, close it
        if (docks[to] != null && GodotObject.IsInstanceValid(docks[to]))
        {
            CloseDock(to);
        }

        PageDock dock = docks[from];
        RemoveDockFromPosition(dock, from);
        docks[from] = null;

        docks[to] = dock;
        SetDockToPosition(dock, to);
    }

    public void ReloadDock(UIPosition uiPosition, object data)
    {
        if (docks[uiPosition] == null || !GodotObject.IsInstanceValid(docks[uiPosition])) return;
        docks[uiPosition].Reload(data);
    }

    public void StartDock(UIPosition uiPosition, string page, object data = null)
    {
        if (docks[uiPosition] != null && GodotObject.IsInstanceValid(docks[uiPosition])) return;
        PageDock dock = new PageDock(plugin);
        dock.Name = "Name";
        docks[uiPosition] = dock;
        SetDockToPosition(dock, uiPosition);
        dock.SwitchPage(page, data);
    }

    public void CloseDock(UIPosition uiPosition)
    {
        if (docks[uiPosition] == null || !GodotObject.IsInstanceValid(docks[uiPosition])) return;
        RemoveDockFromPosition(docks[uiPosition], uiPosition);
        docks[uiPosition].QueueFree();
        docks[uiPosition] = null;
    }

    public void SetDialogSize(Vector2I size)
    {
        if (docks[UIPosition.Dialog] == null || !GodotObject.IsInstanceValid(docks[UIPosition.Dialog]))
            return;

        if (docks[UIPosition.Dialog].GetParent() is PopupPanel popup)
        {
            popup.Size = size;
        }
    }


    private void SetDockToPosition(Control dock, UIPosition pos)
    {
        if (pos == UIPosition.Dialog)
        {
            SetupDialog(dock);
            return;
        }

        if (pos == UIPosition.BottomPanel)
        {
            plugin.AddControlToBottomPanel(dock, dock.Name);
            return;
        }

        if (pos >= UIPosition.LeftDockTopLeft && pos <= UIPosition.RightDockBottomRight)
        {
            plugin.AddControlToDock(GetDockSlot(pos), dock);
            return;
        }

        plugin.AddControlToContainer(GetContainer(pos), dock);
    }

    private void RemoveDockFromPosition(Control dock, UIPosition pos)
    {
        if (pos == UIPosition.Dialog)
        {
            RemoveDialog();
            return;
        }

        if (pos == UIPosition.BottomPanel)
        {
            plugin.RemoveControlFromBottomPanel(dock);
            return;
        }

        if (pos >= UIPosition.LeftDockTopLeft && pos <= UIPosition.RightDockBottomRight)
        {
            plugin.RemoveControlFromDocks(dock);
            return;
        }

        plugin.RemoveControlFromContainer(GetContainer(pos), dock);
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
        foreach (var item in docks)
        {
            if (item.Value != null && GodotObject.IsInstanceValid(item.Value))
            {
                CloseDock(item.Key);
            }
        }
        docks = new Dictionary<UIPosition, PageDock>();
    }
}