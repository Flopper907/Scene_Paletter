using System.Collections.Generic;
using Addons.ScenePaletter.Tools;

namespace Addons.ScenePaletter;

public partial class WindowState
{
    public void CreatePalette()
    {
        Palette palette = new Palette();
        palette.Name = "Untitled";
        palette.UID = IDGenerator.GenerateID(plugin.config.IdStart, plugin.config.IdEnd).ToString();
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