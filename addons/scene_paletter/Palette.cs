using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Addons.ScenePaletter;

[Serializable]
public class Palette
{
    public List<string> Paths { get; set; } = new List<string>();
    public string Name { get; set; } = "Untitled";

    [JsonIgnore]
    public string UID { get; set; }

    public override string ToString()
    {
        string s = "Paths{";
        foreach (string path in Paths)
        {
            s += path + ",";
        }
        s += "}, Name: " + Name;
        s += ",: UID: " + UID;
        return s;
    }

}