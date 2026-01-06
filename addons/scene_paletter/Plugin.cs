using Godot;
using System;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{

    public static Palette loadedPalette;
    public static string state;
    private Window panel;

    public override void _EnterTree()
    {
        panel = new Window();
        // Add the panel as a dock in the editor
        AddControlToDock(DockSlot.RightUl, panel);

        GD.Print("Plugin enabled");
        GD.Print($"DockPanel added: {panel.GetParent() != null}");
    }

    public override void _ExitTree()
    {
        if (panel != null)
        {
            RemoveControlFromDocks(panel);
            panel.QueueFree();
        }

        GD.Print("Plugin disabled");
    }
}