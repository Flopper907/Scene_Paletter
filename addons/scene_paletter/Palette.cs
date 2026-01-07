using Godot;
using System;
using System.Collections.Generic;

namespace Addons.ScenePaletter;

[Serializable]
public class Palette
{
    public List<string> Paths { get; set; } = new List<string>();
    public string Name { get; set; } = "Untitled";
}