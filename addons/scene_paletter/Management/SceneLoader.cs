using Addons.ScenePaletter.Core;
using Godot;
using System;
using System.Collections.Generic;

namespace Addons.ScenePaletter.Management;

/// <summary>
/// Loads and manages <c>PackedScene</c> assets for <c>Pages</c> and <c>Widgets</c>
/// used in the Godot Editor UI. Acts as a central registry for all UI scenes
/// defined in the plugin's configuration.
/// </summary>
/// <remarks>
/// <para>
/// <c>SceneLoader</c> works with <c>PageDock</c> to provide scenes for UI elements.
/// It relies on configuration values loaded by <c>ConfigLoader</c> (specifically the
/// <c>ScenePaths</c> and <c>WidgetPaths</c> dictionaries) to determine which
/// <c>PackedScene</c> resources should be loaded at startup.
/// </para>
/// 
/// <para>
/// To automatically load a <c>Page</c> and <c>Widget</c> when the plugin starts:
/// </para>
/// <list type="number">
/// <item>Add a <c>[page]</c> section to the config file</item>
/// <item>Define a <c>pages</c> and a <c>widgets</c> dictionary</item>
/// <item>Map <c>Name</c> as <c>string</c> to <c>Path</c> as <c>string</c></item>
/// </list>
/// <example>
/// Define your scenes and widgets in the plugin config file:
/// <code>
/// [page]
/// ; Map page names to their PackedScene resource UIDs
/// pages={
///     "InitPage": "uid://abcdefghijklm"
/// }
/// ; Map widget names to their PackedScene resource UIDs
/// widgets={
///     "TextListItem": "uid://mlkjihgfedcba"
/// }
/// </code>
/// </example>
/// </remarks>
public class SceneLoader : IDisposable
{
    private Dictionary<string, PackedScene> Pages;
    private Dictionary<string, PackedScene> Widgets;

    /// <summary> 
    /// Loads all <c>PackedScenes</c> for <c>Pages</c> and <c>Widgets</c> from the <c>config</c>.
    /// </summary>
    /// <remarks>
    /// Requires <c>ConfigLoader</c> to be initialized before.
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPluginException</c> if <c>plugin</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingConfigException</c> if <c>config</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowNullReferenceException</c> if <c>config.ScenePaths</c> is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowResourceLoadException</c> if <c>PackedScenes</c> could not be loaded.</para>
    /// </remarks>
    public void Init(Plugin plugin)
    {
        if (plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException(nameof(Init));
            return;
        }

        if (plugin.config == null)
        {
            ExceptionHandler.ThrowMissingConfigException(nameof(Init));
            return;
        }

        if (plugin.config.ScenePaths == null)
        {
            ExceptionHandler.ThrowNullReferenceException(nameof(plugin.config.ScenePaths), nameof(Init));
            return;
        }

        Pages = new Dictionary<string, PackedScene>();
        Widgets = new Dictionary<string, PackedScene>();

        // Load Pages
        foreach (var (key, path) in plugin.config.ScenePaths)
        {
            var scene = GD.Load<PackedScene>(path);
            if (scene == null)
            {
                ExceptionHandler.ThrowResourceLoadException(path, nameof(Init));
                continue;
            }

            Pages[key] = scene;
        }

        // Load Widgets
        if (plugin.config.WidgetPaths == null)
            return; // widgets are optional

        foreach (var (key, path) in plugin.config.WidgetPaths)
        {
            var scene = GD.Load<PackedScene>(path);
            if (scene == null)
            {
                ExceptionHandler.ThrowResourceLoadException(path, nameof(Init));
                continue;
            }

            Widgets[key] = scene;
        }
    }

    /// <summary>
    /// Ensures the <c>SceneLoader</c> has been initialized.
    /// </summary>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingSceneLoaderException</c> if <c>Pages</c> or <c>Widgets</c> is <c>null</c>.</para>
    /// </remarks>
    private void EnsureInitialized(string caller)
    {
        if (Pages == null || Widgets == null)
            ExceptionHandler.ThrowMissingSceneLoaderException(caller);
    }

    /// <summary> 
    /// Returns if <c>Pages</c> contains <c>page</c>. 
    /// </summary>
    /// <remarks>
    /// <para>No error is logged if <c>Pages</c> is <c>null</c> or <c>Pages</c> does not contains <c>page</c>.</para>
    /// </remarks>
    public bool HasPage(string page)
    {
        return Pages != null && Pages.ContainsKey(page);
    }

    /// <summary> 
    /// Returns the <c>PackedScene</c> of <c>page</c>. 
    /// </summary>
    /// <returns>The <c>PackedScene</c> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPageException</c> if <c>page</c> is not found.</para>
    /// </remarks>
    public PackedScene GetPage(string page)
    {
        EnsureInitialized(nameof(GetPage));

        if (!HasPage(page))
        {
            ExceptionHandler.ThrowMissingPageException(page, nameof(GetPage));
            return null;
        }

        return Pages[page];
    }

    /// <summary>
    /// Returns the <c>PackedScene</c> for <c>page</c> or throws an exception if it does not exist.
    /// </summary>
    /// <returns>The <c>PackedScene</c> associated with <c>page</c>.</returns>
    /// <param name="context">Additional context for error logging (e.g., method name or caller info).</param>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingPageException</c> if <c>page</c> is not found.</para>
    /// </remarks>
    public PackedScene GetPageOrThrow(string page, string context = "")
    {
        EnsureInitialized(nameof(GetPageOrThrow));

        if (!HasPage(page))
        {
            ExceptionHandler.ThrowMissingPageException(page, context);
        }

        return Pages[page];
    }

    /// <summary> 
    /// Returns if <c>Widgets</c> contains <c>widget</c>. 
    /// </summary>
    /// <remarks>
    /// <para>No error is logged if <c>Widgets</c> is <c>null</c> or <c>Widgets</c> does not contains <c>widget</c>.</para>
    /// </remarks>
    public bool HasWidget(string widget)
    {
        return Widgets != null && Widgets.ContainsKey(widget);
    }

    /// <summary> 
    /// Returns the <c>PackedScene</c> of <c>widget</c>. 
    /// </summary>
    /// <returns>The <c>PackedScene</c> if found; otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingWidgetException</c> if <c>widget</c> is not found.</para>
    /// </remarks>
    public PackedScene GetWidget(string widget)
    {
        EnsureInitialized(nameof(GetWidget));

        if (!HasWidget(widget))
        {
            ExceptionHandler.ThrowMissingWidgetException(widget, nameof(GetWidget));
            return null;
        }

        return Widgets[widget];
    }

    /// <summary>
    /// Returns the <c>PackedScene</c> for <c>widget</c> or throws an exception if it does not exist.
    /// </summary>
    /// <returns>The <c>PackedScene</c> associated with <c>widget</c>.</returns>
    /// <param name="context">Additional context for error logging (e.g., method name or caller info).</param>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowMissingWidgetException</c> if <c>widget</c> is not found.</para>
    /// </remarks>
    public PackedScene GetWidgetOrThrow(string widget, string context = "")
    {
        EnsureInitialized(nameof(GetWidgetOrThrow));

        if (!HasWidget(widget))
            ExceptionHandler.ThrowMissingWidgetException(widget, context);

        return Widgets[widget];
    }

    /// <summary>
    /// Clears <c>Pages</c> and <c>Widgets</c>.
    /// </summary>
    /// <remarks>
    /// No error is logged when <c>Pages</c> or <c>Widgets</c> is <c>null</c>.
    /// </remarks>
    public void Dispose()
    {
        Pages?.Clear();
        Widgets?.Clear();

        Pages = null;
        Widgets = null;
    }
}