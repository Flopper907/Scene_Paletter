using System.Collections.Generic;
using System.Linq;
using Addons.ScenePaletter.Tools;
using Godot;

namespace Addons.ScenePaletter;

public partial class WindowState<T> where T : WindowStateData
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

    public void SavePalette(Palette palette)
    {
        SaveLoad.Save(palette, plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }

    public void DeletePalette(Palette palette)
    {
        SaveLoad.Delete(plugin.config.PalettePath + palette.UID + plugin.config.FileExtension);
    }
}