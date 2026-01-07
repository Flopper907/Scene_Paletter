using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class Editing : WindowState
{
    public Editing(Plugin plugin) : base(plugin)
    {
        title = "Scene Palette";
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Generate()
    {
        controls = new List<Control>();
    }
}