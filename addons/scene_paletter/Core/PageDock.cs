using System;
using Godot;

namespace Addons.ScenePaletter.Core;

/// <summary>
/// Manages the lifecycle and displays a single <c>Page</c> within <c>PageDock</c>.
/// Handles page loading, switching, reloading, and automatic title updates.
/// </summary>
/// 
/// <remarks>
/// <para>
/// <c>PageDock</c> holds one <c>Page</c> at a time.
/// It is created and managed by <c>DockManager</c> and should not be instantiated directly.
/// </para>
/// 
/// <para>
/// <c>PageDock</c> works with <c>SceneLoader</c> to provide scenes for UI elements.
/// It relies on the loaded <c>PackedScenes</c> loaded by <c>SceneLoader</c> (specifically the
/// <c>Pages</c> dictionary) to determine which <c>PackedScene</c> should be instantiated.
/// </para>
/// 
/// <para>
/// The dock automatically updates its display name based on the current page's <c>Title</c> property.
/// When switching pages, the previous page is properly disposed before loading the new one.
/// </para>
/// 
/// <para>
/// Typical workflow:
/// </para>
/// <list type="number">
/// <item><c>DockManager</c> creates a <c>PageDock</c> at a specific <c>UIPosition</c></item>
/// <item><c>PageDock</c> loads the initial page via <c>SwitchPage</c></item>
/// <item>User interactions trigger page switches or reloads</item>
/// </list>
/// </remarks>
public partial class PageDock : VBoxContainer
{
    private Control node;
    public Plugin plugin { get; private set; }

    public string page { get; private set; }
    public object data { get; private set; }

    public PageDock(Plugin plugin)
    {
        this.plugin = plugin;
    }

    /// <summary> 
    /// Clears the currently instanced Page.
    /// </summary>
    /// <remarks>
    /// No error is logged if no page is instanced.
    /// </remarks>
    private void Clear()
    {
        if (IsInstanceValid(node))
        {
            RemoveChild(node);
            node.QueueFree();
        }
    }

    /// <summary> 
    /// Switches to a different page, replacing the current page with the new one.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    /// The previous page is disposed before loading the new one.
    /// The dock's title is automatically updated after the page loads.
    /// </para>
    /// 
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPluginException</c> if <c>plugin</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingSceneLoaderException</c> if <c>SceneLoader</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPageException</c> if <c>page</c> is not found.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowResourceLoadException</c> if <c>PackedScene</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowSceneInstantiationException</c> if node could not be spawned.</para>
    /// </remarks>
    public void SwitchPage(string page, object pageData)
    {
        if (plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException($"Dock: {GetType().Name}");
            return;
        }
        if (plugin.sceneLoader == null)
        {
            ExceptionHandler.ThrowMissingSceneLoaderException($"Dock: {GetType().Name}");
            return;
        }
        if (!plugin.sceneLoader.HasPage(page))
        {
            ExceptionHandler.ThrowMissingPageException(page);
            return;
        }

        try
        {
            Clear();
            this.page = page;
            data = pageData;

            PackedScene scene = plugin.sceneLoader.GetPage(page);
            if (scene == null)
            {
                ExceptionHandler.ThrowResourceLoadException(page, nameof(SwitchPage));
                return;
            }

            node = scene.Instantiate() as Control;
            if (node == null)
            {
                ExceptionHandler.ThrowSceneInstantiationException(page, nameof(SwitchPage));
                return;
            }

            AddChild(node);
            CallDeferred(MethodName.UpdateName);
        }
        catch (Exception ex) when (!(ex is NullReferenceException)) // Ignore expected nulls
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{nameof(SwitchPage)} - {page}");
            if (node != null && IsInstanceValid(node))
            {
                node.QueueFree();
                node = null;
            }
        }
    }

    /// <summary> 
    /// Reloads the current page with new <c>pageData</c>, preserving the page type.
    /// </summary>
    /// <remarks>
    /// <para>Internally calls <c>SwitchPage</c> with the current page name.</para>
    /// <para>Useful for refreshing a page's display without changing the page type.</para>
    /// </remarks>
    public void ReloadPage(object pageData)
    {
        SwitchPage(page, pageData);
    }

    /// <summary> 
    /// Updates the display name of the <c>PageDock</c> to match the current page's title.
    /// </summary>
    /// <remarks>
    /// <para>Called automatically after a page loads (via <c>CallDeferred</c>).</para>
    /// <para>Attempts to read the page's <c>Title</c> property and uses it as the dock name.</para>
    /// <para>Falls back to the page name or "PageDock" if no title is available.</para>
    /// <para>Logs <c>ExceptionHandler.ThrowMissingNodeException</c> if no <c>page</c> is instanced</para>
    /// </remarks>
    private void UpdateName()
    {
        if (node == null || !IsInstanceValid(node))
        {
            ExceptionHandler.ThrowMissingNodeException(GetPath(), nameof(UpdateName));
            return;
        }

        string title = "";
        try { title = node.Get("Title").AsString(); } catch { }

        Name = !string.IsNullOrEmpty(title) ? title : (page ?? "PageDock");
    }
}