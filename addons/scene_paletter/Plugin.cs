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

    public override void _Ready()
    {
        config = new ConfigLoader();
        config.Init("res://addons/scene_paletter/plugin.cfg");

        sceneLoader = new SceneLoader();
        sceneLoader.Init(this);
        
        dockManager = new Dockmanager(this);
        dockManager.Init();
    }

    public override void _ExitTree()
    {
        config.Dispose();
        dockManager.Dispose();
        sceneLoader.Dispose();
    }
}