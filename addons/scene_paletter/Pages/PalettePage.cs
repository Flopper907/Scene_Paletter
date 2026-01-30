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

        if (data.palettes == null)
        {
            ExceptionHandler.ThrowNullReferenceException("data.palettes", $"{GetType().Name} {nameof(Initialize)}");
            data.palettes = new List<Palette>();
            return;
        }

        PackedScene packedScene = plugin.sceneLoader?.GetWidget("PaletteListItem");
        if (packedScene == null)
        {
            ExceptionHandler.ThrowMissingWidgetException("PaletteListItem", $"{GetType().Name} {nameof(Initialize)}");
            return;
        }

        for (int i = 0; i < data.palettes.Count; i++)
        {
            try
            {
                Palette palette = data.palettes[i];
                if (palette == null)
                {
                    ExceptionHandler.ThrowNullReferenceException($"palette at index {i}", $"{GetType().Name} {nameof(Initialize)}");
                    continue;
                }

                PaletteListItem item = packedScene.Instantiate() as PaletteListItem;
                if (item == null)
                {
                    ExceptionHandler.ThrowSceneInstantiationException("PaletteListItem", $"{GetType().Name} {nameof(Initialize)} - Index: {i}");
                    continue;
                }

                paletteListView.AddChild(item);

                int position = i;
                item.SetData(palette.Name, palette.UID, () => SelectPalette(position), () => DeletePalette(position));
            }
            catch (Exception ex)
            {
                ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(Initialize)} - Processing palette at index {i}");
                continue; // Skip this item and continue with the next
            }
        }
    }

    public void SelectPalette(int index)
    {
        if (index < 0 || index >= data.palettes.Count)
        {
            ExceptionHandler.ThrowInvalidPalettePositionException(index, $"{GetType().Name} {nameof(SelectPalette)}");
            return;
        }

        Palette palette = data.palettes[index];
        if (palette == null)
        {
            ExceptionHandler.ThrowMissingPaletteException($"index {index}", $"{GetType().Name} {nameof(SelectPalette)}");
            return;
        }

        dock.SwitchPage("PlacingPage", new PlacingPageData(palette));
    }

    public void CreatePalette()
    {
        try
        {
            int nextPosition = data.palettes.Count > 0 ? data.palettes[data.palettes.Count - 1].Position + 1 : 0;
            Palette palette = Palette.CreateEmptyPalette(plugin, nextPosition);

            if (palette == null)
            {
                ExceptionHandler.ThrowNullReferenceException("created palette", $"{GetType().Name} {nameof(CreatePalette)}");
                return;
            }

            Palette.SavePalette(plugin, palette);
            dock.ReloadPage(null);
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(CreatePalette)}");
        }
    }

    public void DeletePalette(int index)
    {
        if (index < 0 || index >= data.palettes.Count)
        {
            ExceptionHandler.ThrowInvalidPalettePositionException(index, $"{GetType().Name} {nameof(DeletePalette)}");
            return;
        }

        try
        {
            Palette palette = data.palettes[index];
            if (palette == null)
            {
                ExceptionHandler.ThrowMissingPaletteException($"index {index}", $"{GetType().Name} {nameof(DeletePalette)}");
                return;
            }

            Palette.DeletePalette(plugin, palette);
            dock.ReloadPage(null);
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(DeletePalette)} - Index: {index}");
        }
    }
}