using Godot;
using Addons.ScenePaletter.Management;
using Addons.ScenePaletter.Core;
using Addons.ScenePaletter.Tools;

namespace Addons.ScenePaletter;

/// <summary>
/// Main plugin class for Scene Paletter.
/// Manages the plugin lifecycle, initializes core systems, and provides centralized
/// access to configuration, scene loading, and dock management.
/// </summary>
/// <remarks>
/// <para>
/// Scene Paletter is a Godot editor plugin that provides a palette-based workflow
/// for quickly placing frequently-used scenes into your levels. It supports both
/// 2D and 3D scenes with automatic position extrapolation for rapid placement.
/// </para>
/// 
/// <para>
/// Initialization order:
/// <list type="number">
///   <item><c>ConfigLoader</c> - Loads plugin configuration from plugin.cfg</item>
///   <item><c>SceneLoader</c> - Loads all page and widget scenes defined in config</item>
///   <item><c>DockManager</c> - Creates and positions initial docks in the editor UI</item>
/// </list>
/// </para>
/// 
/// <para>
/// All initialization operations are wrapped in <c>SafeExecute</c> to prevent
/// plugin crashes from halting the editor. Errors are logged to the console
/// via <c>ExceptionHandler</c>.
/// </para>
/// </remarks>
[Tool]
public partial class Plugin : EditorPlugin
{
    /// <summary>Configuration loader providing access to plugin settings</summary>
    public ConfigLoader config;

    /// <summary>Dock manager responsible for creating and positioning UI docks</summary>
    public DockManager dockManager;

    /// <summary>Scene loader managing all page and widget packed scenes</summary>
    public SceneLoader sceneLoader;

    /// <summary>
    /// Initializes the plugin by loading configuration and setting up core systems.
    /// Creates the initial dock layout as defined in the plugin configuration.
    /// </summary>
    public override void _Ready()
    {
        ScenePreviewGenerator.ClearCache();
        config = new ConfigLoader();
        ExceptionHandler.SafeExecute(() => config.Init("res://addons/scene_paletter/plugin.cfg"), nameof(config.Init), nameof(_Ready));

        SaveLoad.EnsureDirectoryExists(config.PalettePath);

        sceneLoader = new SceneLoader();
        ExceptionHandler.SafeExecute(() => sceneLoader.Init(this), nameof(sceneLoader.Init), nameof(_Ready));

        dockManager = new DockManager(this);
        ExceptionHandler.SafeExecute(() => dockManager.Init(), nameof(dockManager.Init), nameof(_Ready));
    }

    /// <summary>
    /// Cleans up plugin resources when the plugin is disabled or the editor closes.
    /// Disposes all docks, clears loaded scenes, and releases configuration resources.
    /// </summary>
    public override void _ExitTree()
    {
        config.Dispose();
        dockManager.Dispose();
        sceneLoader.Dispose();
    }
}