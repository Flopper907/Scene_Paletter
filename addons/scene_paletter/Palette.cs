using Addons.ScenePaletter.Tools;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Addons.ScenePaletter;

/// <summary>
/// Represents a collection of scene paths that can be saved, loaded, and managed.
/// Palettes are serialized to JSON and identified by unique IDs.
/// </summary>
[Serializable]
public class Palette
{
    /// <summary>List of scene resource UIDs included in this palette</summary>
    public List<string> Paths { get; set; } = new List<string>();

    /// <summary>Display name of the palette</summary>
    public string Name { get; set; } = "Untitled";

    /// <summary>Sort position for palette ordering</summary>
    public int Position { get; set; }

    /// <summary>Unique identifier for this palette (not serialized)</summary>
    [JsonIgnore]
    public string UID { get; set; }

    /// <summary>
    /// Returns a string representation of the palette for debugging.
    /// </summary>
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

    /// <summary>
    /// Creates a new empty palette with a generated unique ID.
    /// </summary>
    /// <param name="plugin">Plugin instance for accessing configuration</param>
    /// <param name="position">Sort position for the new palette</param>
    /// <returns>New palette instance with generated UID</returns>
    public static Palette CreateEmptyPalette(Plugin plugin, int position)
    {
        Palette palette = new Palette();
        palette.Name = "Untitled";
        palette.UID = IDGenerator.GenerateID(plugin.config.IdStart, plugin.config.IdEnd).ToString();
        palette.Position = position;
        return palette;
    }

    /// <summary>
    /// Loads all palettes from the configured palette directory.
    /// </summary>
    /// <param name="plugin">Plugin instance for accessing configuration</param>
    /// <returns>List of palettes sorted by position</returns>
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

    /// <summary>
    /// Saves the palette to disk as JSON.
    /// </summary>
    /// <param name="plugin">Plugin instance for accessing configuration</param>
    /// <param name="palette">Palette to save</param>
    public static void SavePalette(Plugin plugin, Palette palette)
    {
        SaveLoad.Save(palette, plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }

    /// <summary>
    /// Deletes the palette file from disk.
    /// </summary>
    /// <param name="plugin">Plugin instance for accessing configuration</param>
    /// <param name="palette">Palette to delete</param>
    public static void DeletePalette(Plugin plugin, Palette palette)
    {
        SaveLoad.Delete(plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }

    /// <summary>
    /// Creates a deep copy of this palette.
    /// </summary>
    /// <returns>New palette instance with copied data</returns>
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

    /// <summary>
    /// Compares palettes by UID only.
    /// </summary>
    /// <param name="other">Palette to compare with</param>
    /// <returns>True if UIDs match, false otherwise</returns>
    public bool EqualsID(Palette other)
    {
        // Compare UID if both have it set
        if (!string.IsNullOrEmpty(UID) && !string.IsNullOrEmpty(other.UID))
        {
            return UID == other.UID;
        }

        return false;
    }

    /// <summary>
    /// Compares palettes by all properties (name, position, paths).
    /// </summary>
    /// <param name="obj">Object to compare with</param>
    /// <returns>True if all properties match, false otherwise</returns>
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

    /// <summary>
    /// Generates a hash code based on UID or all properties if UID is not set.
    /// </summary>
    /// <returns>Hash code for this palette</returns>
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