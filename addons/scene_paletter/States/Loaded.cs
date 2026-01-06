using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class Loaded : WindowState
{
    public Loaded(Plugin plugin) : base(plugin)
    {
        title = "Scene Palette*";
    }

    public override void Generate()
    {
        controls = new List<Control>();

        Label title = new Label();
        title.Text = "Loaded";
        controls.Add(title);

        Button button = new Button();
        button.Text = "Change State";
        button.Pressed += () => plugin.SwitchState("Init");
        controls.Add(button);
    }
}