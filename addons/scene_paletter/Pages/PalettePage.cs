using System;
using System.Collections.Generic;
using Addons.ScenePaletter.Core;
using Addons.ScenePaletter.Dialogs;
using Addons.ScenePaletter.Widgets;
using Godot;

namespace Addons.ScenePaletter.Pages;

/// <summary>
/// Data passed to the <c>PalettePage</c> for palette selection.
/// </summary>
public struct PalettePageData
{
    /// <summary>
    /// Initializes an empty palette page data structure.
    /// </summary>
    public PalettePageData()
    {
        palettes = new List<Palette>();
    }

    /// <summary>List of all available palettes</summary>
    public List<Palette> palettes;
}

/// <summary>
/// Page for selecting and managing palettes.
/// Displays all available palettes and provides controls for creating, selecting, and deleting them.
/// </summary>
[Tool]
public partial class PalettePage : Page<PalettePageData>
{
    [Export] public VBoxContainer paletteListView;

    /// <summary>
    /// Initializes the palette selection view by loading all palettes from disk
    /// and creating list items for each one.
    /// </summary>
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
                item.SetData(palette.Name, palette.UID, () => SelectPalette(position), () => ShowDeleteDialog(position));
            }
            catch (Exception ex)
            {
                ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(Initialize)} - Processing palette at index {i}");
                continue;
            }
        }
    }

    /// <summary>
    /// Shows a confirmation dialog before deleting a palette.
    /// </summary>
    /// <param name="index">Index of the palette to delete</param>
    public void ShowDeleteDialog(int index)
    {
        DeleteDialogData dialogData = new DeleteDialogData(
            cancelAction: () =>
            {
                plugin.dockManager.CloseDock(UIPosition.Dialog);
            },
            deleteAction: () =>
            {
                plugin.dockManager.CloseDock(UIPosition.Dialog);
                DeletePalette(index);
            }
        );

        plugin.dockManager.StartDock(UIPosition.Dialog, "DeleteDialog", dialogData);
        plugin.dockManager.SetDialogSize(new Vector2I(400, 200));
    }

    /// <summary>
    /// Switches to placing mode for the selected palette.
    /// </summary>
    /// <param name="index">Index of the palette to select</param>
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

    /// <summary>
    /// Creates a new empty palette with a generated ID and saves it to disk.
    /// </summary>
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

    /// <summary>
    /// Deletes the specified palette from disk and reloads the view.
    /// </summary>
    /// <param name="index">Index of the palette to delete</param>
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