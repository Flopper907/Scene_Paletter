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
        ExceptionHandler.SafeExecute(() => config.Init("res://addons/scene_paletter/plugin.cfg"), nameof(config.Init), nameof(_Ready));

        sceneLoader = new SceneLoader();
        ExceptionHandler.SafeExecute(() => sceneLoader.Init(this), nameof(sceneLoader.Init), nameof(_Ready));

        dockManager = new Dockmanager(this);
        ExceptionHandler.SafeExecute(() => dockManager.Init(), nameof(dockManager.Init), nameof(_Ready));
    }

    public override void _ExitTree()
    {
        config.Dispose();
        dockManager.Dispose();
        sceneLoader.Dispose();
    }
}