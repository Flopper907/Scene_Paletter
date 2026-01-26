using System;
using Godot;
using Addons.ScenePaletter.Management;
using Addons.ScenePaletter.Core;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public ConfigLoader config;
    public Dockmanager dockManager;
    public SceneLoader sceneLoader;

    private Button toolbarButton2D;
    private Button toolbarButton3D;
    private Action toolbarButtonAction;

    public override void _EnterTree()
    {
        InitToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        InitToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);

        config = new ConfigLoader();
        config.InitConfig("res://addons/scene_paletter/plugin.cfg");

        sceneLoader = new SceneLoader();
        sceneLoader.Init(this);
    }

    public override void _Ready()
    {
        dockManager = new Dockmanager(this);
        dockManager.InitDocks();
    }

    public override void _ExitTree()
    {
        DisposeToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        DisposeToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);

        config.Dispose();
        dockManager.Dispose();
        sceneLoader.Dispose();
    }

    /* ============== Init/Dispose ============== */

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