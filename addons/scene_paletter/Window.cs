using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter;

public partial class Window : VBoxContainer
{
    public void Clear()
    {
        foreach (Node node in GetChildren())
            node.QueueFree();
    }

    public void SpawnNodes(List<Control> controls)
    {
        foreach (Control control in controls)
            AddChild(control);
    }
    public void SwitchToState(WindowState state)
    {
        Name = state.title;
        Clear();
        state.Initialize();
        state.Generate();
        SpawnNodes(state.controls);
    }
}