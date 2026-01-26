using Godot;
using System;
using System.Collections.Generic;

namespace Addons.ScenePaletter.Management;

public class SceneLoader : IDisposable
{
    public Dictionary<string, PackedScene> Pages {get; private set;}
    public Dictionary<string, PackedScene> Widgets {get; private set;}
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

    public void Dispose()
    {
        Pages = new Dictionary<string, PackedScene>();
        Widgets = new Dictionary<string, PackedScene>();
    }
}