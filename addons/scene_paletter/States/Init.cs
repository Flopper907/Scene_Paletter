using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class Init : WindowState
{
    public Init(Plugin plugin) : base(plugin)
    {
        title = "Scene Palette";
    }

    public override void Generate()
    {
        controls = new List<Control>();

        Label title = new Label();
        title.Text = "Init";
        controls.Add(title);

        Button button = new Button();
        button.Text = "Change State";
        button.Pressed += () => plugin.SwitchState("Loaded");
        controls.Add(button);

    }
}