using Godot;
using System;
using System.Collections.Generic;

namespace Addons.ScenePaletter.Management;

public class SceneLoader : IDisposable
{
    private Dictionary<string, PackedScene> Pages;
    private Dictionary<string, PackedScene> Widgets;
    private Plugin plugin;

    public void Init(Plugin plugin)
    {
        this.plugin = plugin;
        if (plugin.config != null)
        {
            Pages = new Dictionary<string, PackedScene>();
            foreach (var item in plugin.config.ScenePaths)
            {
                Pages[item.Key] = GD.Load<PackedScene>(item.Value);
            }

            Widgets = new Dictionary<string, PackedScene>();
            foreach (var item in plugin.config.WidgetPaths)
            {
                Widgets[item.Key] = GD.Load<PackedScene>(item.Value);
            }
        }
    }

    public PackedScene GetPage(string page)
    {
        if (HasPage(page))
        {
            return Pages[page];
        }
        return null;
    }

    public bool HasPage(string page)
    {
        return Pages.ContainsKey(page);
    }

    public PackedScene GetWidget(string widget)
    {
        if (HasWidget(widget))
        {
            return Widgets[widget];
        }
        return null;
    }

    public bool HasWidget(string widget)
    {
        return Widgets.ContainsKey(widget);
    }

    public void Dispose()
    {
        Pages = new Dictionary<string, PackedScene>();
        Widgets = new Dictionary<string, PackedScene>();
    }
}