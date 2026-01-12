using System;
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
    public void SwitchToState(WindowStateBase state, WindowStateData data)
    {
        Clear();
        state.InitializeBase(data);
        Name = state.Title;
        state.Generate();
        SpawnNodes(state.controls);
    }
}