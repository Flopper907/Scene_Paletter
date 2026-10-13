#if TOOLS
using Addons.PluginFramework.Core;
using Addons.PluginFramework.Management;
using Godot;

namespace Addons.PluginFramework;

/// <summary>
/// Abstract base class of every editor plugin built on the page/dock framework.
/// Creates and owns the shared services (<c>ConfigLoader</c>, <c>SceneLoader</c>, <c>DockManager</c>)
/// and tears them down again, so a concrete plugin only implements its own behaviour in
/// <c>OnEnable</c> / <c>OnDisable</c>.
/// </summary>
/// <remarks>
/// <para>
/// Lifecycle: <c>_EnterTree</c> loads the <c>plugin.cfg</c> that sits next to the plugin script, loads the
/// scenes listed in its <c>[page]</c> section, calls <c>OnEnable</c> and only then creates the
/// <c>initial_docks</c>, so pages can rely on everything <c>OnEnable</c> set up (settings, services). <c>_ExitTree</c> calls <c>OnDisable</c> first (while the docks still exist) and then
/// disposes the docks, scenes and config.
/// </para>
/// <para>
/// C# assembly reload (editor Build button): Godot re-creates the plugin object, is not expected to call
/// <c>_ExitTree</c> / <c>_EnterTree</c>, and every plain C# field (the services above) is lost. Therefore the same teardown runs in
/// <c>OnBeforeSerialize</c>, while the old assembly and the docks are still valid, and the same setup runs deferred
/// after <c>OnAfterDeserialize</c>. Docks reopen on their <c>initial_docks</c> page (page state is not carried over), and
/// docks that were visible before the reload are focused again. Setup and teardown are idempotent, so an engine that also
/// re-runs <c>_EnterTree</c> still ends with one copy.
/// </para>
/// <para>
/// The reload pass (<c>OnBeforeSerialize</c>, and the <c>OnDisable</c> it runs) must not touch nodes of the edited scene:
/// Godot's thread guard rejects those calls there (<c>The caller thread can't call the function ...</c>). Editor UI
/// (docks, containers) is fine. Use <c>IsReloading</c> to skip scene-node work in <c>OnDisable</c>.
/// </para>
/// <para>
/// Gizmo plugins on reload: <c>OnDisable</c> -> <c>RemoveGizmoPlugin</c> only unregisters the old plugin and keeps it in a
/// Godot-typed field, so it is not destroyed inside the pass (its destructor would call <c>remove_gizmo</c> on scene nodes,
/// which the thread guard rejects, leaving gizmos with a null plugin). The deferred reload setup then runs <c>OnEnable</c>
/// (-> <c>AddGizmoPlugin</c> registers the new plugin and re-creates the edited scene's gizmos on the main thread),
/// refreshes once more, and only then disposes and releases the old plugins, whose gizmos are gone by then (unless the
/// editor still holds one, e.g. for a selected node).
/// </para>
/// <para>
/// Gizmo lifetimes (C# gizmo plugins crash the editor at exit otherwise, engine issue #110790): every gizmo plugin is
/// disposed on the main thread right after its gizmos were dropped (<c>RemoveGizmoPlugin</c>, deferred reload setup), so
/// its native destructor never runs in the shutdown disposal or on the .NET finalizer thread. Every <c>_Redraw</c>
/// override must dispose its <c>EditorNode3DGizmo</c> argument (<c>try</c>/<c>finally</c>); a live managed wrapper keeps
/// the native gizmo alive after its node was freed, and the plugin's destructor would then touch the freed node.
/// </para>
/// <para>
/// Do not override <c>_EnterTree</c> / <c>_ExitTree</c> in a subclass; use <c>OnEnable</c> / <c>OnDisable</c>.
/// </para>
/// <example>
/// <code>
/// [Tool]
/// public partial class Plugin : FrameworkPlugin
/// {
///     protected override void OnEnable() { /* register gizmos, read settings from config, ... */ }
///     protected override void OnDisable() { /* undo what OnEnable did */ }
/// }
/// </code>
/// </example>
/// </remarks>
[Tool]
public abstract partial class FrameworkPlugin : EditorPlugin, ISerializationListener
{
    /// <summary>Config of the plugin's <c>plugin.cfg</c>; <c>null</c> while the plugin is disabled.</summary>
    public ConfigLoader config;

    /// <summary>Registry of the page and widget scenes named in <c>[page]</c>; <c>null</c> while disabled.</summary>
    public SceneLoader sceneLoader;

    /// <summary>Creates and positions the <c>PageDock</c>s in the editor; <c>null</c> while disabled.</summary>
    public DockManager dockManager;

    /// <summary>
    /// <c>UIPosition</c>s (as <c>int</c>) of the docks that were visible right before an assembly reload; empty otherwise.
    /// </summary>
    /// <remarks>
    /// Godot-typed on purpose: Godot saves it after <c>OnBeforeSerialize</c> and restores it before
    /// <c>OnAfterDeserialize</c>, unlike the plain C# services.
    /// </remarks>
    private Godot.Collections.Array<int> reloadVisibleDocks = new();

    /// <summary>
    /// Gizmo plugins removed during the reload pass, kept alive until the deferred reload setup re-created the gizmos.
    /// </summary>
    /// <remarks>
    /// Godot-typed so the references (and with them the native objects) survive the reload; releasing them inside the
    /// pass would run their destructor there. The deferred reload setup disposes each element <b>before</b>
    /// <c>Clear()</c>: the dispose drops only the managed reference, and the native destructor then runs inside
    /// <c>Clear()</c> on the main thread, instead of later on the .NET finalizer thread. Do not swap the order.
    /// </remarks>
    private Godot.Collections.Array<EditorNode3DGizmoPlugin> retiredGizmoPlugins = new();

    private bool inReloadPass;

    /// <summary>
    /// <c>true</c> while the teardown runs inside the assembly reload pass (<c>OnBeforeSerialize</c>).
    /// </summary>
    /// <remarks>
    /// While <c>true</c>, <c>OnDisable</c> must not touch nodes of the edited scene (Godot's thread guard rejects the calls);
    /// skip that work, the deferred reload setup rebuilds the state anyway.
    /// </remarks>
    protected bool IsReloading => inReloadPass;

    /// <summary>
    /// Initializes config and scenes, calls <c>OnEnable</c>, then creates the initial docks.
    /// </summary>
    /// <remarks>
    /// Logs via <c>ExceptionHandler.ThrowMissingPluginException</c> and stops if the plugin script (and therefore
    /// the location of <c>plugin.cfg</c>) cannot be determined.
    /// </remarks>
    public override void _EnterTree() => Setup();

    /// <summary>
    /// Calls <c>OnDisable</c>, then disposes docks, scenes and config.
    /// </summary>
    public override void _ExitTree() => Teardown();

    /// <summary>
    /// Called by Godot right before a C# assembly reload, while this instance and the old assembly are still valid.
    /// Remembers which docks are visible, then tears the plugin down exactly like <c>_ExitTree</c>, so no dock or
    /// toolbar control is left behind disconnected from the reloaded plugin.
    /// </summary>
    /// <remarks>
    /// Runs inside Godot's serialization pass over all script instances: nothing called from here may
    /// <c>Free()</c> a scripted object synchronously or touch nodes of the edited scene (see <c>OnDisable</c>).
    /// <c>IsReloading</c> is <c>true</c> for the duration of the teardown.
    /// </remarks>
    public void OnBeforeSerialize()
    {
        reloadVisibleDocks.Clear();
        if (dockManager != null)
        {
            foreach (var pos in dockManager.GetVisibleDockPositions())
                reloadVisibleDocks.Add((int)pos);
        }

        inReloadPass = true;
        try
        {
            Teardown();
        }
        finally
        {
            inReloadPass = false;
        }
    }

    /// <summary>
    /// Called by Godot after the assembly reload re-created this instance. Schedules the setup deferred, so the
    /// UI is rebuilt only after the reload has completely finished.
    /// </summary>
    public void OnAfterDeserialize() => CallDeferred(MethodName.SetupAfterReload);

    /// <summary>
    /// Called during setup (<c>_EnterTree</c>, and again after every assembly reload) after config and scenes are
    /// loaded but <b>before</b> the initial docks are created, so pages already see everything set up here. Override
    /// to register gizmos, read plugin specific settings from <c>config</c>, ... Do not use <c>dockManager</c> docks
    /// here; they do not exist yet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Plain C# fields are <c>null</c> after an assembly reload (only Godot-typed fields survive), so this must
    /// rebuild everything the plugin needs instead of assuming earlier state.
    /// </para>
    /// <para>
    /// The native plugin object survives a reload, so an engine registration that has no matching removal API (e.g.
    /// <c>SetForceDrawOverForwardingEnabled</c>, <c>SetInputEventForwardingAlwaysEnabled</c>) must be made only once per
    /// object: guard it with a Variant-compatible <c>bool</c> field that is not reset in <c>OnDisable</c>.
    /// </para>
    /// <para>
    /// Register 3D gizmo plugins with <c>AddGizmoPlugin</c>, never with <c>AddNode3DGizmoPlugin</c> directly.
    /// </para>
    /// </remarks>
    protected virtual void OnEnable() { }

    /// <summary>
    /// Called at the start of teardown (<c>_ExitTree</c>, and right before every assembly reload), while the docks and
    /// services still exist. Override to undo everything <c>OnEnable</c> registered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Before a reload this runs inside Godot's serialization pass: never <c>Free()</c> a node or object with a C#
    /// script synchronously here (dangling instance, editor crash), use <c>QueueFree</c>, and keep it null-safe. The
    /// same applies to every page/widget <c>_ExitTree</c> and <c>TreeExiting</c> handler, because the teardown removes
    /// the docks synchronously in the same pass.
    /// </para>
    /// <para>
    /// While <c>IsReloading</c> is <c>true</c>, do not touch nodes of the edited scene (e.g. <c>UpdateGizmos</c> on a
    /// selected mesh): Godot's thread guard rejects those calls in the pass. Editor UI controls are fine.
    /// </para>
    /// <para>
    /// Unregister 3D gizmo plugins with <c>RemoveGizmoPlugin</c>, never with <c>RemoveNode3DGizmoPlugin</c> directly:
    /// removing a gizmo plugin does not detach the gizmos it already created on nodes. <c>RemoveGizmoPlugin</c> disposes
    /// the plugin (outside the reload pass); null the field that held it right after the call.
    /// </para>
    /// </remarks>
    protected virtual void OnDisable() { }

    /// <summary>
    /// Registers <paramref name="gizmoPlugin"/> with the 3D editor and re-creates the gizmos of every <c>Node3D</c> in
    /// the edited scene, so nodes that already exist pick up the new plugin.
    /// </summary>
    /// <remarks>
    /// Use this instead of <c>AddNode3DGizmoPlugin</c>, paired with <c>RemoveGizmoPlugin</c>. After an assembly reload
    /// (<c>OnEnable</c> from the deferred reload setup) the refresh also drops the gizmos of the retired old plugin and
    /// attaches the new plugin's ones. Does nothing if <paramref name="gizmoPlugin"/> is <c>null</c> or freed.
    /// </remarks>
    /// <param name="gizmoPlugin">The gizmo plugin to register.</param>
    protected void AddGizmoPlugin(EditorNode3DGizmoPlugin gizmoPlugin)
    {
        if (!IsInstanceValid(gizmoPlugin))
            return;

        AddNode3DGizmoPlugin(gizmoPlugin);
        RefreshSceneGizmos(EditorInterface.Singleton.GetEditedSceneRoot());
    }

    /// <summary>
    /// Unregisters <paramref name="gizmoPlugin"/> from the 3D editor and drops every gizmo on the edited scene's
    /// <c>Node3D</c>s, which the editor then re-creates (deferred) from the plugins that are still registered.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>RemoveNode3DGizmoPlugin</c> alone leaves the gizmos the plugin already created on their nodes; once the plugin
    /// object is destroyed they lose their plugin and every redraw logs <c>Parameter "gizmo_plugin" is null</c>. The
    /// plugin is removed <b>first</b>, then the gizmos are dropped. Nodes outside the edited scene are not touched; they
    /// are out of the tree, and Godot already cleared their gizmos.
    /// </para>
    /// <para>
    /// Afterwards <paramref name="gizmoPlugin"/> is <b>disposed</b>, so its native destructor runs now, on the main thread,
    /// while every node is alive. Do not use the object after this call; null the field that held it.
    /// </para>
    /// <para>
    /// Inside the reload pass (<c>IsReloading</c>) scene nodes must not be touched, so the plugin is only unregistered
    /// and kept alive in <c>retiredGizmoPlugins</c>; the deferred reload setup drops its gizmos and then disposes and
    /// releases it.
    /// </para>
    /// <para>
    /// The dropped gizmos must be native <c>EditorNode3DGizmo</c>s without a C# script (no gizmo plugin may override
    /// <c>_CreateGizmo</c> with a scripted gizmo). Does nothing if <paramref name="gizmoPlugin"/> is <c>null</c> or freed.
    /// </para>
    /// </remarks>
    /// <param name="gizmoPlugin">The gizmo plugin to unregister.</param>
    protected void RemoveGizmoPlugin(EditorNode3DGizmoPlugin gizmoPlugin)
    {
        if (!IsInstanceValid(gizmoPlugin))
            return;

        RemoveNode3DGizmoPlugin(gizmoPlugin);
        if (inReloadPass)
        {
            retiredGizmoPlugins.Add(gizmoPlugin);
            return;
        }
        RefreshSceneGizmos(EditorInterface.Singleton.GetEditedSceneRoot());
        // The native destructor must run now on the main thread while the nodes are alive, not at shutdown or on the finalizer thread.
        gizmoPlugin.Dispose();
    }

    /// <summary>
    /// Clears the gizmos of <paramref name="node"/> and every <c>Node3D</c> below it, then requests new ones from the
    /// currently registered gizmo plugins (<c>UpdateGizmos</c> on a node without gizmos re-requests them deferred).
    /// Main thread only.
    /// </summary>
    private void RefreshSceneGizmos(Node node)
    {
        // Scene nodes fail Godot's thread guard inside the reload pass; the deferred reload setup refreshes instead.
        if (inReloadPass || !IsInstanceValid(node))
            return;

        if (node is Node3D node3D)
        {
            node3D.ClearGizmos();
            node3D.UpdateGizmos();
        }

        foreach (Node child in node.GetChildren())
            RefreshSceneGizmos(child);
    }

    /// <summary>
    /// Deferred reload setup (main thread): rebuilds the plugin, drops the gizmos of the retired gizmo plugins, disposes and
    /// releases them, then focuses the docks that were visible before the reload.
    /// </summary>
    /// <remarks>
    /// The extra refresh covers a plugin whose <c>OnEnable</c> registers no gizmo plugin. The retired plugins are disposed
    /// and released only after it, when none of their gizmos is left on a node.
    /// </remarks>
    private void SetupAfterReload()
    {
        Setup();

        if (retiredGizmoPlugins.Count > 0)
        {
            RefreshSceneGizmos(EditorInterface.Singleton.GetEditedSceneRoot());
            // Dispose before Clear: the Array's ref keeps the native object alive, so its destructor runs in Clear, here on the main thread.
            foreach (var retired in retiredGizmoPlugins)
            {
                if (IsInstanceValid(retired))
                    retired.Dispose();
            }
            retiredGizmoPlugins.Clear();
        }

        if (dockManager != null)
        {
            foreach (int pos in reloadVisibleDocks)
                dockManager.FocusDock((UIPosition)pos);
        }
        reloadVisibleDocks.Clear();
    }

    // Skips when disabled before the deferred call ran, or when the engine already re-ran _EnterTree.
    private void Setup()
    {
        if (!IsInsideTree() || config != null)
            return;

        string configPath = GetConfigPath();
        if (configPath == null)
        {
            ExceptionHandler.ThrowMissingPluginException($"{GetType().Name} {nameof(Setup)}: plugin script not found");
            return;
        }

        config = new ConfigLoader();
        ExceptionHandler.SafeExecute(() => config.Init(configPath), nameof(config.Init), nameof(Setup));

        sceneLoader = new SceneLoader();
        ExceptionHandler.SafeExecute(() => sceneLoader.Init(this), nameof(sceneLoader.Init), nameof(Setup));

        dockManager = new DockManager(this);

        ExceptionHandler.SafeExecute(OnEnable, nameof(OnEnable), nameof(Setup));

        ExceptionHandler.SafeExecute(() => dockManager.Init(), nameof(dockManager.Init), nameof(Setup));
    }

    private void Teardown()
    {
        ExceptionHandler.SafeExecute(OnDisable, nameof(OnDisable), nameof(Teardown));

        dockManager?.Dispose();
        sceneLoader?.Dispose();
        config?.Dispose();

        dockManager = null;
        sceneLoader = null;
        config = null;
    }

    private string GetConfigPath()
    {
        var script = GetScript().As<Script>();
        if (script == null || string.IsNullOrEmpty(script.ResourcePath))
            return null;

        return script.ResourcePath.GetBaseDir().PathJoin("plugin.cfg");
    }
}
#endif
