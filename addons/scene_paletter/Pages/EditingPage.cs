using Godot;
using System.Collections.Generic;
using Addons.ScenePaletter.Tools;
using Addons.ScenePaletter.Widgets;
using Addons.ScenePaletter.Core;
using Addons.ScenePaletter.Dialogs;

namespace Addons.ScenePaletter.Pages;

/// <summary>
/// Data passed to the <c>EditingPage</c> for palette editing.
/// </summary>
public struct EditingPageData
{
    /// <summary>
    /// Initializes editing page data with a palette to edit.
    /// </summary>
    /// <param name="palette">Palette to edit</param>
    public EditingPageData(Palette palette)
    {
        this.palette = palette;
        old = palette.Copy();
        selectedElements = new List<int>();
        savedScrollPosition = 0;
    }
    /// <summary>Current palette being edited</summary>
    public Palette palette;

    /// <summary>Original palette state for change detection</summary>
    public Palette old;

    /// <summary>List of selected scene indices</summary>
    public List<int> selectedElements;

    /// <summary>Whether changes have been made</summary>
    public bool changed;

    /// <summary>Saved scroll position for view restoration</summary>
    public int savedScrollPosition;
}


/// <summary>
/// Page for editing palette contents - adding, removing, and selecting scenes.
/// Displays all scenes in the palette with preview images and provides controls
/// for managing scene entries and palette metadata.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item>Multi-selection of scenes for batch deletion</item>
///   <item>Scene preview generation with configurable resolution</item>
///   <item>Scroll position preservation across reloads</item>
///   <item>Dynamic column adjustment for grid layout</item>
///   <item>Change tracking against original palette state</item>
/// </list>
/// </para>
/// </remarks>
[Tool]
public partial class EditingPage : Page<EditingPageData>
{
    [Export] public GridContainer sceneListView;
    [Export] public LineEdit titleLineEdit;
    [Export] public ScrollContainer scrollContainer;

    /// <summary>
    /// Initializes the editing view with the current palette's scenes.
    /// Generates preview images for each scene and sets up interaction callbacks.
    /// </summary>
    public override void Initialize()
    {
        if (data.palette == null)
        {
            ExceptionHandler.ThrowMissingPaletteException("null", nameof(Initialize));
            dock.SwitchPage("PalettePage", null);
            return;
        }

        Title = "Scene Paletter" + (data.old.Equals(data.palette) ? "" : "*");

        titleLineEdit.Text = data.palette.Name;
        sceneListView.Columns = plugin.config.Columns;

        PackedScene packedScene = plugin.sceneLoader?.GetWidget("EditingListItem");
        if (packedScene == null)
        {
            ExceptionHandler.ThrowMissingWidgetException("EditingListItem", $"{this.GetType()} {nameof(Initialize)}");
            return;
        }

        for (int i = 0; i < data.palette.Paths.Count; i++)
        {
            try
            {
                int index = i;
                string uid = data.palette.Paths[index];

                PackedScene scene = GD.Load<PackedScene>(uid);
                if (scene == null)
                {
                    ExceptionHandler.ThrowResourceLoadException(uid, $"{GetType().Name} {nameof(Initialize)} - Index: {index}");
                    continue;
                }

                Node node = scene.Instantiate();
                if (node == null)
                {
                    ExceptionHandler.ThrowSceneInstantiationException(uid, $"{GetType().Name} {nameof(Initialize)} - Index: {index}");
                    continue;
                }

                string name = node.Name;
                node.Free();

                EditingListItem item = packedScene.Instantiate() as EditingListItem;
                if (item == null)
                {
                    ExceptionHandler.ThrowSceneInstantiationException("EditingListItem", $"{GetType().Name} {nameof(Initialize)} - Index: {index}");
                    continue;
                }

                sceneListView.AddChild(item);

                item.SetData(name, data.selectedElements.Contains(index), () => ToggleSelect(index),
                    () => ShowDeleteDialog(index)
                );
                ScenePreviewGenerator.GeneratePreview(
                    scene,
                    plugin.config.PreviewResolution,
                    plugin.config.PreviewMargin,
                    node is Node2D ? plugin.config.PreviewTransparent2D : plugin.config.PreviewTransparent3D,
                    item.SetTexture
                );
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(Initialize)} - Processing palette item at index {i}");
                continue;
            }
        }

        CallDeferred(MethodName.ApplyScrollPosition);
    }

    /// <summary>
    /// Shows a confirmation dialog before deleting a scene from the palette.
    /// </summary>
    /// <param name="index">Index of the scene to delete</param>
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
                Delete(index);
            }
        );

        plugin.dockManager.StartDock(UIPosition.Dialog, "DeleteDialog", dialogData);
        plugin.dockManager.SetDialogSize(new Vector2I(400, 200));
    }

    /// <summary>
    /// Restores the saved scroll position after the view is loaded.
    /// </summary>
    private async void ApplyScrollPosition()
    {
        if (scrollContainer != null && data.savedScrollPosition >= 0)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (IsInstanceValid(scrollContainer))
            {
                scrollContainer.ScrollVertical = data.savedScrollPosition;
            }
        }
    }

    /// <summary>
    /// Reloads the page while preserving the current scroll position.
    /// </summary>
    private void ReloadWithScrollSave()
    {
        data.savedScrollPosition = scrollContainer.ScrollVertical;
        dock.ReloadPage(data);
    }

    /// <summary>
    /// Reloads the page and resets scroll to the top.
    /// </summary>
    private void ReloadWithoutScrollSave()
    {
        data.savedScrollPosition = 0;
        dock.ReloadPage(data);
    }

    /// <summary>
    /// Updates the palette name and refreshes the view.
    /// </summary>
    /// <param name="text">New palette name</param>
    public void SetTitle(string text)
    {
        data.palette.Name = text;
        ReloadWithScrollSave();
    }

    /// <summary>
    /// Toggles selection state for a scene item.
    /// </summary>
    /// <param name="index">Index of the scene to toggle</param>
    public void ToggleSelect(int index)
    {
        if (data.selectedElements.Contains(index))
        {
            data.selectedElements.Remove(index);
        }
        else
        {
            data.selectedElements.Add(index);
        }
        ReloadWithScrollSave();
    }

    /// <summary>
    /// Deletes the specified scene and all selected scenes from the palette.
    /// </summary>
    /// <param name="index">Index of the primary scene to delete</param>
    public void Delete(int index)
    {
        List<string> newPaths = [.. data.palette.Paths];

        if (index < 0 || index > newPaths.Count) return;

        newPaths.Remove(data.palette.Paths[index]);
        for (int i = 0; i < data.selectedElements.Count; i++)
        {
            newPaths.Remove(data.palette.Paths[data.selectedElements[i]]);
        }
        data.palette.Paths = newPaths;
        data.selectedElements.Clear();
        ReloadWithoutScrollSave();
    }

    /// <summary>
    /// Discards changes and returns to placing mode with the original palette state.
    /// </summary>
    public void Discard()
    {
        dock.SwitchPage("PlacingPage", new PlacingPageData(data.old));
    }

    /// <summary>
    /// Saves the palette to disk and switches to placing mode.
    /// </summary>
    public void Save()
    {
        Palette.SavePalette(plugin, data.palette);
        dock.SwitchPage("PlacingPage", new PlacingPageData(data.palette));
    }

    /// <summary>
    /// Opens a file dialog to add new scenes to the palette.
    /// </summary>
    public void Add()
    {
        SetupFileDialog("Select Scene Files", "*.tscn", "Godot Scene Files", OnSceneFilesSelected);
    }

    /// <summary>
    /// Increases the number of columns in the grid layout.
    /// </summary>
    public void AddColumn()
    {
        plugin.config.AddColumn();
        ReloadWithoutScrollSave();
    }

    /// <summary>
    /// Decreases the number of columns in the grid layout.
    /// </summary>
    public void RemoveColumn()
    {
        plugin.config.RemoveColumn();
        ReloadWithoutScrollSave();
    }

    /// <summary>
    /// Callback invoked when scene files are selected in the file dialog.
    /// Converts file paths to resource UIDs and adds them to the palette.
    /// </summary>
    /// <param name="paths">Array of selected scene file paths</param>
    protected void OnSceneFilesSelected(string[] paths)
    {
        if (paths == null) return;
        foreach (string path in paths)
        {
            long uid = ResourceLoader.GetResourceUid(path);
            string uidString = ResourceUid.IdToText(uid);
            if (!data.palette.Paths.Contains(uidString))
            {
                data.palette.Paths.Add(uidString);
            }
        }

        ReloadWithoutScrollSave();
    }
}