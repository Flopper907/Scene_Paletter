using Godot;
using Godot.Collections;
using Addons.ScenePaletter.Tools;
using Addons.ScenePaletter.Widgets;
using Addons.ScenePaletter.Core;
using System;

namespace Addons.ScenePaletter.Pages;

[Tool]
public partial class PlacingPage : Page<PlacingPageData>
{
    [Export] public GridContainer sceneListView;
    [Export] public Label titleLabel;
    [Export] public ScrollContainer scrollContainer;

    public override void Initialize()
    {
        if (data.palette == null)
        {
            ExceptionHandler.ThrowMissingPaletteException("null", $"{GetType().Name} {nameof(Initialize)}");
            dock.SwitchPage("PalettePage", null);
            return;
        }

        Title = "Scene Paletter";

        titleLabel.Text = data.palette.Name;
        sceneListView.Columns = plugin.config.Columns;

        PackedScene packedScene = plugin.sceneLoader?.GetWidget("PlacingListItem");
        if (packedScene == null)
        {
            ExceptionHandler.ThrowMissingWidgetException("PlacingListItem", $"{GetType().Name} {nameof(Initialize)}");
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

                PlacingListItem item = packedScene.Instantiate() as PlacingListItem;
                if (item == null)
                {
                    ExceptionHandler.ThrowSceneInstantiationException("PlacingListItem", $"{GetType().Name} {nameof(Initialize)} - Index: {index}");
                    continue;
                }

                sceneListView.AddChild(item);

                item.SetData(name, index == data.currentElement, () => Select(index));
                ScenePreviewGenerator.GeneratePreview(
                    scene,
                    plugin.config.PreviewResolution,
                    plugin.config.PreviewMargin,
                    node is Node2D ? plugin.config.PreviewTransparent2D : plugin.config.PreviewTransparent3D,
                    item.SetTexture
                );
            }
            catch (Exception ex)
            {
                ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(Initialize)} - Processing palette item at index {i}");
                continue; // Skip this item and continue with the next
            }
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

    public void Select(int index)
    {
        if (index < 0 || index >= data.palette.Paths.Count)
        {
            ExceptionHandler.ThrowInvalidPalettePositionException(index, $"{GetType().Name} {nameof(Select)}");
            return;
        }

        data.previousElement = data.currentElement;
        data.currentElement = index;
        ReloadWithScrollSave();
    }

    public void Edit()
    {
        if (data.palette == null)
        {
            ExceptionHandler.ThrowMissingPaletteException("null", $"{GetType().Name} {nameof(Edit)}");
            return;
        }

        dock.SwitchPage("EditingPage", new EditingPageData(data.palette));
    }

    public void Back()
    {
        dock.SwitchPage("PalettePage", null);
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
        try
        {
            // Check valid selection
            if (data.currentElement < 0 || data.currentElement >= data.palette.Paths.Count)
            {
                ExceptionHandler.ThrowInvalidPalettePositionException(data.currentElement, $"{GetType().Name} {nameof(Place)}");
                return;
            }

            string scenePath = data.palette.Paths[data.currentElement];
            PackedScene packedScene = GD.Load<PackedScene>(scenePath);
            if (packedScene == null)
            {
                ExceptionHandler.ThrowResourceLoadException(scenePath, $"{GetType().Name} {nameof(Place)}");
                return;
            }

            Node parent = GetParentNodeFromEditor();
            if (parent == null)
            {
                ExceptionHandler.ThrowMissingNodeException("Editor parent node", $"{GetType().Name} {nameof(Place)}");
                return;
            }

            Node instance = packedScene.Instantiate();
            if (instance == null)
            {
                ExceptionHandler.ThrowSceneInstantiationException(scenePath, $"{GetType().Name} {nameof(Place)}");
                return;
            }

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

                var tree = parent.GetTree();
                if (tree == null)
                {
                    ExceptionHandler.ThrowMissingNodeException("SceneTree", $"{GetType().Name} {nameof(Place)}");
                    instance.QueueFree();
                    return;
                }

                var editedSceneRoot = tree.EditedSceneRoot;
                if (editedSceneRoot == null)
                {
                    ExceptionHandler.ThrowMissingNodeException("EditedSceneRoot", $"{GetType().Name} {nameof(Place)}");
                    instance.QueueFree();
                    return;
                }

                instance.Owner = editedSceneRoot;

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

                var tree = parent.GetTree();
                if (tree == null)
                {
                    ExceptionHandler.ThrowMissingNodeException("SceneTree", $"{GetType().Name} {nameof(Place)}");
                    instance.QueueFree();
                    return;
                }

                var editedSceneRoot = tree.EditedSceneRoot;
                if (editedSceneRoot == null)
                {
                    ExceptionHandler.ThrowMissingNodeException("EditedSceneRoot", $"{GetType().Name} {nameof(Place)}");
                    instance.QueueFree();
                    return;
                }

                instance.Owner = editedSceneRoot;

                // Apply calculated position
                instance3D.GlobalPosition = spawnPos;

                // Update spawn tracking
                data.previousSpawned = data.lastSpawned;
                data.lastSpawned = instance;
            }
            // --- INVALID PARENT / INSTANCE TYPE ---
            else
            {
                ExceptionHandler.ThrowInvalidNodeTypeException(
                    instance.GetPath(),
                    $"{parent.GetType().Name} (2D/3D)",
                    $"Parent: {parent.GetType().Name}, Instance: {instance.GetType().Name}"
                );
                instance.Free();
                return;
            }

            // Mark scene as unsaved in the editor
            if (EditorInterface.Singleton != null)
            {
                EditorInterface.Singleton.MarkSceneAsUnsaved();
            }
            else
            {
                ExceptionHandler.LogWarning("EditorInterface.Singleton is null", $"{GetType().Name} {nameof(Place)}");
            }
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(Place)}");
        }
    }

    private Node GetParentNodeFromEditor()
    {
        try
        {
            EditorInterface editorInterface = EditorInterface.Singleton;
            if (editorInterface == null)
            {
                ExceptionHandler.ThrowNullReferenceException("EditorInterface.Singleton", $"{GetType().Name} {nameof(GetParentNodeFromEditor)}");
                return null;
            }

            EditorSelection selection = editorInterface.GetSelection();
            if (selection == null)
            {
                ExceptionHandler.ThrowNullReferenceException("EditorSelection", $"{GetType().Name} {nameof(GetParentNodeFromEditor)}");
                return null;
            }

            Array<Node> selectedNodes = selection.GetSelectedNodes();

            Node editedSceneRoot = editorInterface.GetEditedSceneRoot();
            if (editedSceneRoot == null)
            {
                ExceptionHandler.ThrowMissingNodeException("EditedSceneRoot", $"{GetType().Name} {nameof(GetParentNodeFromEditor)}");
                return null;
            }

            Node parentNode = editedSceneRoot;
            if (selectedNodes != null && selectedNodes.Count > 0)
            {
                parentNode = selectedNodes[0]; // Use the first selected node as parent
            }

            return parentNode;
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(GetParentNodeFromEditor)}");
            return null;
        }
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