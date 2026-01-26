using Godot;
using Addons.ScenePaletter.Management;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public ConfigLoader config;
    public Dockmanager dockManager;
    public SceneLoader sceneLoader;

    public override void _EnterTree()
    {
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
        config.Dispose();
        dockManager.Dispose();
        sceneLoader.Dispose();
    }
}