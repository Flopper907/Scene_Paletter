using System;
using Godot;
using System.Collections.Generic;
using Addons.ScenePaletter.Management;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public ConfigLoader config;
    public Dockmanager dockManager;

    public Dictionary<string, PackedScene> Scenes;

    private Button toolbarButton2D;
    private Button toolbarButton3D;
    private Action toolbarButtonAction;

    public override void _EnterTree()
    {
        InitToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        InitToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);

        config = new ConfigLoader();
        config.InitConfig("res://addons/scene_paletter/plugin.cfg");

        dockManager = new Dockmanager(this);
        InitScenes();
    }

    public override void _Ready()
    {
        dockManager.InitDocks();
    }

    public override void _ExitTree()
    {
        DisposeToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        DisposeToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);
        config.Dispose();
        dockManager.Dispose();
        DisposeScenes();
    }

    /* ============== Init/Dispose ============== */

    private void InitScenes()
    {
        if (config != null)
        {
            Scenes = new Dictionary<string, PackedScene>();
            foreach (var item in config.ScenePaths)
            {
                Scenes[item.Key] = GD.Load<PackedScene>(item.Value);
            }
        }
    }

    private void DisposeScenes()
    {
        Scenes = new Dictionary<string, PackedScene>();
    }

    private void InitToolbarButton(ref Button button, CustomControlContainer container)
    {
        if (IsInstanceValid(button)) return;

        toolbarButtonAction = () =>
        {
            if (dockManager.docks[UIPosition.RightDockTopLeft] == null || !IsInstanceValid(dockManager.docks[UIPosition.RightDockTopLeft]))
            {
                dockManager.StartDock(UIPosition.RightDockTopLeft, "PalettePage");
            }
            else
            {
                dockManager.ChangeDockPosition(UIPosition.RightDockTopLeft, UIPosition.Dialog);
            }
        };
        button = new Button();
        button.Text = "Scene Palette";
        button.Pressed += toolbarButtonAction;
        button.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("Node", "EditorIcons");
        AddControlToContainer(container, button);
    }

    private void DisposeToolbarButton(ref Button button, CustomControlContainer container)
    {
        if (IsInstanceValid(button))
        {
            RemoveControlFromContainer(container, button);
            button.QueueFree();
        }
    }
}

public enum UIPosition
{
    // Special
    Dialog,
    BottomPanel,

    // 3D Viewport
    Editor3DToolBar,
    Editor3DLeft,
    Editor3DRight,
    Editor3DBottom,

    // 2D Viewport
    Editor2DToolBar,
    Editor2DLeft,
    Editor2DRight,
    Editor2DBottom,

    // Inspector
    InspectorBottom,

    // Project Settings
    ProjectSettingLeft,
    ProjectSettingRight,

    // Dock
    LeftDockTopLeft,
    LeftDockTopRight,
    LeftDockBottomLeft,
    LeftDockBottomRight,
    RightDockTopLeft,
    RightDockTopRight,
    RightDockBottomLeft,
    RightDockBottomRight
}