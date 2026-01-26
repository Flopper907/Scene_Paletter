using System;
using System.Collections.Generic;
using Addons.ScenePaletter.Core;
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
        data.palettes = Palette.LoadPalettes(plugin);

        PackedScene packedScene = plugin.sceneLoader.GetWidget("PaletteListItem");
        for (int i = 0; i < data.palettes.Count; i++)
        {
            Palette palette = data.palettes[i];
            PaletteListItem item = packedScene.Instantiate() as PaletteListItem;
            paletteListView.AddChild(item);
            int position = i;
            item.SetData(palette.Name, palette.UID, () => SelectPalette(position), () => DeletePalette(position));
        }

        //plugin.OpenDialog("DeleteDialog", new DeleteDialogData(() => { }, () => { }));
    }

    public void SelectPalette(int index)
    {
        dock.SwitchPage("PlacingPage", new PlacingPageData(data.palettes[index]));
    }

    public void CreatePalette()
    {
        Palette palette = Palette.CreateEmptyPalette(plugin, data.palettes.Count > 0 ? data.palettes[data.palettes.Count - 1].Position + 1 : 0);
        Palette.SavePalette(plugin, palette);
        dock.Reload(null);
    }

    public void DeletePalette(int index)
    {
        Palette.DeletePalette(plugin, data.palettes[index]);
        dock.Reload(null);
    }
}