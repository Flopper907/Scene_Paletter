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
/// <para>1. Add a <c>[page]</c> section to the config file</para>
/// <para>2. Define a <c>pages</c> and a <c>widgets</c> dictionary</para>
/// <para>3. Map <c>Name</c> as <c>string</c> to <c>Path</c> as <c>string</c></para>
/// 
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

    private void EnsureInitialized(string caller)
    {
        if (Pages == null || Widgets == null)
            ExceptionHandler.ThrowMissingSceneLoaderException(caller);
    }

    public bool HasPage(string page)
    {
        return Pages != null && Pages.ContainsKey(page);
    }

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

    public PackedScene GetPageOrThrow(string page, string context = "")
    {
        EnsureInitialized(nameof(GetPageOrThrow));

        if (!HasPage(page))
            ExceptionHandler.ThrowMissingPageException(page, context);

        return Pages[page];
    }


    public bool HasWidget(string widget)
    {
        return Widgets != null && Widgets.ContainsKey(widget);
    }

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

    public PackedScene GetWidgetOrThrow(string widget, string context = "")
    {
        EnsureInitialized(nameof(GetWidgetOrThrow));

        if (!HasWidget(widget))
            ExceptionHandler.ThrowMissingWidgetException(widget, context);

        return Widgets[widget];
    }

    public void Dispose()
    {
        Pages?.Clear();
        Widgets?.Clear();

        Pages = null;
        Widgets = null;
    }
}