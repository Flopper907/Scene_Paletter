using System.Collections.Generic;
using System.Linq;
using Addons.ScenePaletter.Tools;
using Godot;

namespace Addons.ScenePaletter;

public partial class WindowState
{
    protected void GenerateHeaderBar()
    {
        headerBar = new PanelContainer();
        headerBar.CustomMinimumSize = new Vector2(0, 40);
    }

    protected void GenerateContentArea()
    {
        contentArea = new VBoxContainer();
        contentArea.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        contentArea.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
    }


    protected void GenerateFooterBar()
    {
        footerBar = new PanelContainer();
        footerBar.CustomMinimumSize = new Vector2(0, 32);
    }

    public void CreatePalette()
    {
        Palette palette = new Palette();
        palette.Name = "Untitled";
        palette.UID = IDGenerator.GenerateID(plugin.config.IdStart, plugin.config.IdEnd).ToString();
        palette.Position = plugin.palettes.Count;
        plugin.palettes.Add(palette);
        SavePalette(palette);
    }

    public void LoadPalettes()
    {
        List<Palette> palettes = new List<Palette>();
        var paletteData = SaveLoad.LoadAllWithFile<Palette>("res://addons/scene_paletter/Palettes/", ".json");
        foreach (var p in paletteData)
        {
            p.data.UID = p.filename.Replace(plugin.config.FileExtension, "");
            palettes.Add(p.data);
        }

        palettes.Sort((a, b) => a.Position.CompareTo(b.Position));

        plugin.palettes = palettes;
    }

    public void SavePalette(Palette palette)
    {
        SaveLoad.Save(palette, plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }


    public void DeletePalette(Palette palette)
    {
        SaveLoad.Delete(plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }
}