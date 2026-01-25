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
        if (data.palette == null) dock.SwitchPage("PalettePage", null);

        Title = "Scene Paletter" + (data.old.Equals(data.palette) ? "" : "*");

        titleLineEdit.Text = data.palette.Name;
        sceneListView.Columns = plugin.config.Columns;

        PackedScene packedScene = GD.Load<PackedScene>(plugin.config.WidgetPath + "EditingListItem.tscn");
        for (int i = 0; i < data.palette.Paths.Count; i++)
        {
            EditingListItem item = packedScene.Instantiate() as EditingListItem;
            sceneListView.AddChild(item);

            int index = i;
            string uid = data.palette.Paths[index];

            PackedScene scene = GD.Load<PackedScene>(uid);
            Node node = scene.Instantiate();
            string name = node.Name;
            node.Free();

            item.SetData(name, data.selectedElements.Contains(index), () => ToggleSelect(index), () => Delete(index));
            ScenePreviewGenerator.GeneratePreview(
                scene,
                plugin.config.PreviewResolution,
                plugin.config.PreviewMargin,
                node is Node2D ? plugin.config.PreviewTransparent2D : plugin.config.PreviewTransparent3D,
                item.SetTexture
            );
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
        dock.Reload(data);
    }

    private void ReloadWithoutScrollSave()
    {
        data.savedScrollPosition = 0;
        dock.Reload(data);
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
        ReloadWithoutScrollSave();
    }

    public void Discard()
    {
        dock.SwitchPage("PlacingPage", new PlacingPageData(data.old));
    }

    public void Save()
    {
        Palette.SavePalette(plugin, data.palette);
        dock.SwitchPage("PlacingPage", new PlacingPageData(data.palette));
    }

    public void Add()
    {
        SetupFileDialog("Select Scene Files", "*.tscn", "Godot Scene Files", OnSceneFilesSelected);
    }

    public void AddColumn()
    {
        plugin.config.AddColumn();
        ReloadWithoutScrollSave();
    }

    public void RemoveColumn()
    {
        plugin.config.RemoveColumn();
        ReloadWithoutScrollSave();
    }

    protected void OnSceneFilesSelected(string[] paths)
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

        ReloadWithoutScrollSave();
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