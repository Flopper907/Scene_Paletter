using System;
using System.Collections.Generic;
using Addons.ScenePaletter.Tools;
using Addons.ScenePaletter.Widgets;
using Godot;

namespace Addons.ScenePaletter.Pages;

public struct PalettePageData
{
    public PalettePageData()
    {
        palettes = new List<Palette>();
    }
    public List<Palette> palettes;
}

[Tool]
public partial class PalettePage : Page<PalettePageData>
{
    [Export] public VBoxContainer paletteListView;

    public override void Initialize()
    {
        Title = "Scene Paletter";
        data = new PalettePageData();
        data.palettes = LoadPalettes();

        PackedScene packedScene = GD.Load<PackedScene>(plugin.config.WidgetPath + "PaletteListItem.tscn");
        for (int i = 0; i < data.palettes.Count; i++)
        {
            Palette palette = data.palettes[i];
            PaletteListItem item = packedScene.Instantiate() as PaletteListItem;
            paletteListView.AddChild(item);
            int position = i;
            item.SetData(palette.Name, palette.UID, () => SelectPalette(position), () => DeletePalette(position));
        }
    }

    public void SelectPalette(int index)
    {
        plugin.SwitchState("PlacingPage", new PlacingPageData(data.palettes[index]));
    }

    public void CreatePalette()
    {
        Palette palette = CreateEmptyPalette();
        data.palettes.Add(palette);
        SavePalette(palette);
        plugin.ReloadState(data);
    }

    public void DeletePalette(int index)
    {
        DeletePalette(data.palettes[index]);
        plugin.ReloadState(data);
    }

    private List<Palette> LoadPalettes()
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

    private void SavePalette(Palette palette)
    {
        SaveLoad.Save(palette, plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }

    private void DeletePalette(Palette palette)
    {
        SaveLoad.Delete(plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }

    private Palette CreateEmptyPalette()
    {
        Palette palette = new Palette();
        palette.Name = "Untitled";
        palette.UID = IDGenerator.GenerateID(plugin.config.IdStart, plugin.config.IdEnd).ToString();
        palette.Position = data.palettes.Count > 0 ? data.palettes[data.palettes.Count - 1].Position + 1 : 0;
        return palette;
    }
}