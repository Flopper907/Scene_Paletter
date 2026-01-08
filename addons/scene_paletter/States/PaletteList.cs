using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class PaletteList : WindowState
{
    public PaletteList(Plugin plugin) : base(plugin)
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