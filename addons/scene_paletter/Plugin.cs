using System.Collections.Generic;
using Addons.ScenePaletter.States;
using Godot;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    private Dictionary<string, WindowState> states;
    public Palette palette;
    public string state;
    private Window panel;

    public override void _EnterTree()
    {
        InitWindow();
        InitStates();
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

    /* ============== Management ============== */

    private void InitStates()
    {
        states = new Dictionary<string, WindowState>
        {
            {"Init",new Init(this)},
            {"Loaded",new Loaded(this)},
        };
        state = "Init";
        SwitchState(state);
    }

    private void InitWindow()
    {
        panel = new Window();
        panel.Name = "Scene Palette";
        AddControlToDock(DockSlot.RightUl, panel);
    }

    /* ============== Helpers ============== */

    public void SwitchState(string stateName)
    {
        if (states.ContainsKey(stateName))
        {
            panel.SwitchToState(states[stateName]);
        }
    }
}