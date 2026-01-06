using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter;

public partial class Window : VBoxContainer
{
    public void SwitchToState(WindowState state)
    {
        Clear();
        List<Control> controls = state.Generate();

        SpawnNodes(controls);
    }

    private void Clear()
    {
        foreach (Node node in GetChildren())
            node.QueueFree();
    }
    private void SpawnNodes(List<Control> controls)
    {
        foreach (Control control in controls)
            AddChild(control);
    }




}