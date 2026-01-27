using Addons.ScenePaletter.Core;
using Godot;
using System;
using System.Collections.Generic;

namespace Addons.ScenePaletter.Management;

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