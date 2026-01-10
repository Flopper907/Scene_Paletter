using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class Editing : WindowState<EditingData>
{
    public Editing(Plugin plugin) : base(plugin)
    {
        Title = "Scene Palette";
    }

    public override void Initialize(EditingData data)
    {
        base.Initialize(data);
    }

    public override void Generate()
    {
        controls = new List<Control>();
    }
}