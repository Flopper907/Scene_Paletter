using System;
using Addons.ScenePaletter.Tools;
using Addons.ScenePaletter.Widgets;
using Godot;
using Godot.Collections;

namespace Addons.ScenePaletter.Pages;

[Tool]
public partial class PlacingPage : Page<PlacingPageData>
{
    [Export] public GridContainer sceneListView;
    [Export] public Label titleLabel;
    [Export] public ScrollContainer scrollContainer;

    public override void Initialize()
    {
        if (data.palette == null) plugin.SwitchState("PalettePage", null);

        Title = "Scene Paletter";

        titleLabel.Text = data.palette.Name;
        sceneListView.Columns = plugin.config.Columns;

        PackedScene packedScene = GD.Load<PackedScene>(plugin.config.WidgetPath + "PlacingListItem.tscn");
        for (int i = 0; i < data.palette.Paths.Count; i++)
        {
            PlacingListItem item = packedScene.Instantiate() as PlacingListItem;
            sceneListView.AddChild(item);

            int index = i;
            string uid = data.palette.Paths[index];

            PackedScene scene = GD.Load<PackedScene>(uid);
            Node node = scene.Instantiate();

            item.SetData(node.Name, index == data.currentElement, () => Select(index));
            ScenePreviewGenerator.GeneratePreview(
                scene,
                plugin.config.PreviewResolution,
                plugin.config.PreviewMargin,
                node is Node2D ? plugin.config.PreviewTransparent2D : plugin.config.PreviewTransparent3D,
                item.SetTexture
            );

            node.Free();
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

    private void ReloadWithoutScrollSave()
    {
        data.savedScrollPosition = 0;
        plugin.ReloadState(data);
    }

    public void Select(int index)
    {
        data.previousElement = data.currentElement;
        data.currentElement = index;
        ReloadWithScrollSave();
    }

    public void Edit()
    {
        plugin.SwitchState("EditingPage", new EditingPageData(data.palette));
    }

    public void Back()
    {
        plugin.SwitchState("PalettePage", null);
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

    public void Place()
    {
        // Check valid selection
        if (data.currentElement < 0 || data.currentElement >= data.palette.Paths.Count)
            return;

        PackedScene packedScene = GD.Load<PackedScene>(data.palette.Paths[data.currentElement]);
        if (packedScene == null)
            return;

        Node parent = GetParentNodeFromEditor();
        Node instance = packedScene.Instantiate();

        bool lastValid = data.lastSpawned != null && IsInstanceValid(data.lastSpawned) && data.lastSpawned.IsInsideTree();
        if (!lastValid) data.lastSpawned = null;
        bool prevValid = data.previousSpawned != null && IsInstanceValid(data.previousSpawned) && data.previousSpawned.IsInsideTree();
        if (!prevValid) data.previousSpawned = null;

        // --- NODE2D BRANCH ---
        if (parent is Node2D parent2D && instance is Node2D instance2D)
        {
            // Handle positioning
            Vector2 spawnPos;
            if (data.lastSpawned == null)
            {
                spawnPos = parent2D.GlobalPosition; // First spawn or reset
            }
            else if (data.previousSpawned == null)
            {
                spawnPos = ((Node2D)data.lastSpawned).GlobalPosition; // Only last exists
            }
            else
            {
                spawnPos = 2f * ((Node2D)data.lastSpawned).GlobalPosition - ((Node2D)data.previousSpawned).GlobalPosition;
            }

            // Add to tree before positioning
            parent2D.AddChild(instance);
            instance.Owner = parent.GetTree().EditedSceneRoot;

            // Apply calculated position
            instance2D.GlobalPosition = spawnPos;

            // Update spawn tracking
            data.previousSpawned = data.lastSpawned;
            data.lastSpawned = instance;
        }
        // --- NODE3D BRANCH ---
        else if (parent is Node3D parent3D && instance is Node3D instance3D)
        {
            // Handle positioning
            Vector3 spawnPos;
            if (data.lastSpawned == null)
            {
                spawnPos = parent3D.GlobalPosition; // First spawn or reset
            }
            else if (data.previousSpawned == null)
            {
                spawnPos = ((Node3D)data.lastSpawned).GlobalPosition; // Only last exists
            }
            else
            {
                spawnPos = 2f * ((Node3D)data.lastSpawned).GlobalPosition - ((Node3D)data.previousSpawned).GlobalPosition;
            }

            // Add to tree before positioning
            parent3D.AddChild(instance);
            instance.Owner = parent.GetTree().EditedSceneRoot;

            // Apply calculated position
            instance3D.GlobalPosition = spawnPos;

            // Update spawn tracking
            data.previousSpawned = data.lastSpawned;
            data.lastSpawned = instance;
        }
        // --- INVALID PARENT / INSTANCE TYPE ---
        else
        {
            GD.PrintErr("Parent and instance type mismatch or unsupported node type");
            instance.Free();
            return;
        }

        // Mark scene as unsaved in the editor
        EditorInterface.Singleton.MarkSceneAsUnsaved();
    }

    private Node GetParentNodeFromEditor()
    {
        EditorInterface editorInterface = EditorInterface.Singleton;

        EditorSelection selection = editorInterface.GetSelection();
        Array<Node> selectedNodes = selection.GetSelectedNodes();

        Node editedSceneRoot = editorInterface.GetEditedSceneRoot();
        Node parentNode = editedSceneRoot;
        if (selectedNodes.Count > 0)
        {
            parentNode = selectedNodes[0]; // Use the first selected node as parent
        }
        return parentNode;
    }
}

public struct PlacingPageData
{

    public PlacingPageData()
    {
        currentElement = 0;
        previousElement = -1;
        lastSpawned = null;
        previousSpawned = null;
        savedScrollPosition = 0;
    }

    public PlacingPageData(Palette palette)
    {
        this.palette = palette;
        currentElement = 0;
        previousElement = -1;
        lastSpawned = null;
        previousSpawned = null;
        savedScrollPosition = 0;
    }

    public Palette palette;
    public int currentElement = 0;
    public int previousElement = -1;
    public Node lastSpawned;
    public Node previousSpawned;
    public int savedScrollPosition;
}