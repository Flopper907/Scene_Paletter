using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter;

public partial class Window : VBoxContainer
{
    public void SwitchToState(string state)
    {
        Clear();
        List<Control> controls;
        switch (state)
        {
            default:
                controls = new List<Control>();
                break;
        }

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