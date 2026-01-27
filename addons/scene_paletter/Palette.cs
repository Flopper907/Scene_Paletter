using Addons.ScenePaletter.Tools;
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
    public int Position { get; set; }
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
        s += "}, Position: " + Position;
        s += ",: UID: " + UID;
        return s;
    }

    public static Palette CreateEmptyPalette(Plugin plugin, int position)
    {
        Palette palette = new Palette();
        palette.Name = "Untitled";
        palette.UID = IDGenerator.GenerateID(plugin.config.IdStart, plugin.config.IdEnd).ToString();
        palette.Position = position;
        return palette;
    }

    public static List<Palette> LoadPalettes(Plugin plugin)
    {
        List<Palette> palettes = new List<Palette>();
        var paletteData = SaveLoad.LoadAllWithFile<Palette>(plugin.config.PalettePath, ".json");
        foreach (var p in paletteData)
        {
            p.data.UID = p.filename.Replace(plugin.config.FileExtension, "");
            palettes.Add(p.data);
        }

        palettes.Sort((a, b) => a.Position.CompareTo(b.Position));

        return palettes;
    }

    public static void SavePalette(Plugin plugin, Palette palette)
    {
        SaveLoad.Save(palette, plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }

    public static void DeletePalette(Plugin plugin, Palette palette)
    {
        SaveLoad.Delete(plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }

    public Palette Copy()
    {
        return new Palette
        {
            Paths = new List<string>(this.Paths),
            Name = this.Name,
            Position = this.Position,
            UID = this.UID
        };
    }

    public bool EqualsID(Palette other)
    {
        // Compare UID if both have it set
        if (!string.IsNullOrEmpty(UID) && !string.IsNullOrEmpty(other.UID))
        {
            return UID == other.UID;
        }

        return false;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Palette other = (Palette)obj;

        // Otherwise compare all properties
        if (this.Name != other.Name || this.Position != other.Position)
        {
            return false;
        }

        // Compare Paths lists
        if (this.Paths.Count != other.Paths.Count)
        {
            return false;
        }

        for (int i = 0; i < this.Paths.Count; i++)
        {
            if (this.Paths[i] != other.Paths[i])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        // If UID is set, use it for hash code
        if (!string.IsNullOrEmpty(UID))
        {
            return UID.GetHashCode();
        }

        // Otherwise combine hash codes of all properties
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (Name?.GetHashCode() ?? 0);
            hash = hash * 23 + Position.GetHashCode();

            foreach (string path in Paths)
            {
                hash = hash * 23 + (path?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}