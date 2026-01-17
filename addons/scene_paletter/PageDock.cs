using System;
using System.Collections.Generic;
using System.Formats.Tar;
using Godot;

namespace Addons.ScenePaletter;

public partial class PageDock : VBoxContainer
{
    private Control page;
    public Plugin plugin { get; private set; }

    public PageDock(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void SwitchToState(PackedScene scene)
    {
        if (IsInstanceValid(page))
        {
            RemoveChild(page);
            page.QueueFree();
        }
        page = scene.Instantiate() as Control;

        AddChild(page);
    }
}