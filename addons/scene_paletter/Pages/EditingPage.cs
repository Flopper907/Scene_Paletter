using System;
using System.Collections.Generic;
using Addons.ScenePaletter.Tools;
using Addons.ScenePaletter.Widgets;
using Godot;

namespace Addons.ScenePaletter.Pages;

[Tool]
public partial class EditingPage : Page<EditingPageData>
{
    [Export] public GridContainer sceneListView;
    [Export] public LineEdit titleLineEdit;
    [Export] public ScrollContainer scrollContainer;

    public override void Initialize()
    {
        if (data.palette == null) plugin.SwitchState("PalettePage", null);

        Title = "Scene Paletter" + (data.old.Equals(data.palette) ? "" : "*");

        titleLineEdit.Text = data.palette.Name;
        sceneListView.Columns = plugin.config.Columns;

        PackedScene packedScene = GD.Load<PackedScene>(plugin.config.WidgetPath + "EditingListItem.tscn");
        for (int i = 0; i < data.palette.Paths.Count; i++)
        {
            EditingListItem item = packedScene.Instantiate() as EditingListItem;
            sceneListView.AddChild(item);

            int index = i;
            item.SetData(data.palette.Paths[i], data.selectedElements.Contains(index), () => ToggleSelect(index), () => Delete(index));
        }

        CallDeferred(MethodName.ApplyScrollPosition);
    }

    private async void ApplyScrollPosition()
    {
        if (scrollContainer != null && data.savedScrollPosition >= 0)
        {
            // Wait for the next frame to ensure layout is complete
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (IsInstanceValid(scrollContainer))
            {
                scrollContainer.ScrollVertical = data.savedScrollPosition;
            }
        }
    }

    private void ReloadWithScrollSave()
    {
        data.savedScrollPosition = scrollContainer.ScrollVertical;
        plugin.ReloadState(data);
    }

    public void SetTitle(string text)
    {
        data.palette.Name = text;
        ReloadWithScrollSave();
    }

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

    public void Delete(int index)
    {
        List<string> newPaths = [.. data.palette.Paths];

        newPaths.Remove(data.palette.Paths[index]);
        for (int i = 0; i < data.selectedElements.Count; i++)
        {
            newPaths.Remove(data.palette.Paths[data.selectedElements[i]]);
        }
        data.palette.Paths = newPaths;
        data.selectedElements.Clear();
        data.savedScrollPosition = 0;
        plugin.ReloadState(data);
    }

    public void Discard()
    {
        SaveLoad.Save(data.old, plugin.config.PalettePath + data.old.UID + plugin.config.FileExtension);
        plugin.SwitchState("PlacingPage", new PlacingPageData(data.old));
    }

    public void Save()
    {
        SaveLoad.Save(data.palette, plugin.config.PalettePath + data.palette.UID + plugin.config.FileExtension);
        plugin.SwitchState("PlacingPage", new PlacingPageData(data.palette));
    }

    public void Add()
    {
        SetupFileDialog();
    }

    public void AddColumn()
    {
        plugin.config.Columns = Math.Min(plugin.config.MaxColums, plugin.config.Columns + 1);
        data.savedScrollPosition = 0;
        plugin.ReloadState(data);
    }

    public void RemoveColumn()
    {
        plugin.config.Columns = Math.Max(plugin.config.MinColums, plugin.config.Columns - 1);
        data.savedScrollPosition = 0;
        plugin.ReloadState(data);
    }

    private void SetupFileDialog()
    {
        EditorFileDialog fileDialog = new EditorFileDialog();
        fileDialog.FileMode = EditorFileDialog.FileModeEnum.OpenFiles; // Allow multiple file selection
        fileDialog.Access = EditorFileDialog.AccessEnum.Resources;
        fileDialog.Title = "Select Scene Files";

        // Filter for .tscn files (Godot scene files)
        fileDialog.AddFilter("*.tscn", "Godot Scene Files");

        // Connect the file(s) selected signal
        fileDialog.FilesSelected += OnSceneFilesSelected;

        // Add the dialog to the scene tree
        GetParent().AddChild(fileDialog);

        fileDialog.PopupCentered(new Vector2I(800, 600));
    }

    private void OnSceneFilesSelected(string[] paths)
    {
        foreach (string path in paths)
        {
            // Get the UID for the scene file
            long uid = ResourceLoader.GetResourceUid(path);
            string uidString = ResourceUid.IdToText(uid);
            if (!data.palette.Paths.Contains(uidString))
            {
                data.palette.Paths.Add(uidString);
            }
        }

        data.savedScrollPosition = 0;
        plugin.ReloadState(data);
    }
}


public struct EditingPageData
{
    public EditingPageData(Palette palette)
    {
        this.palette = palette;
        old = palette.Copy();
        selectedElements = new List<int>();
        savedScrollPosition = 0;
    }
    public Palette palette;
    public Palette old;
    public List<int> selectedElements;
    public bool changed = false;
    public int savedScrollPosition;
}