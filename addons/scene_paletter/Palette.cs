using Godot;
using System;
using System.Collections.Generic;

namespace Addons.ScenePaletter;

[Serializable]
public class Palette
{
    public List<string> paths { get; set; }
    public string name { get; set; } = "Untitled";
}