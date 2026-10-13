using Godot;
using Godot.Collections;
using Addons.ScenePaletter.Tools;
using Addons.ScenePaletter.Widgets;
using Addons.PluginFramework.Core;
using System;
using System.Collections.Generic;

namespace Addons.ScenePaletter.Pages;

/// <summary>
/// Data passed to the <c>PlacingPage</c> for scene placement operations.
/// </summary>
public struct PlacingPageData
{
    /// <summary>
    /// Initializes placing page data with default values.
    /// </summary>
    public PlacingPageData()
    {
        currentElement = 0;
        placed = new List<Node>();
    }

    /// <summary>
    /// Initializes placing page data with a specific palette.
    /// </summary>
    /// <param name="palette">Palette to use for placing scenes</param>
    public PlacingPageData(Palette palette)
    {
        this.palette = palette;
        currentElement = 0;
        placed = new List<Node>();
    }

    /// <summary>Active palette for scene placement</summary>
    public Palette palette;

    /// <summary>Index of currently selected scene</summary>
    public int currentElement;

    /// <summary>
    /// Positioned instances placed so far, oldest first, used for pose extrapolation.
    /// Entries may be out of the tree (undone or in another scene) or freed; readers filter them and use only the newest
    /// 2 (Linear) or 3 (other methods) survivors. The list is not capped so that older nodes stay available after an undo.
    /// </summary>
    public List<Node> placed;
}

/// <summary>
/// Page for placing scenes from a palette into the editor.
/// Displays all scenes in the palette with previews and handles scene instantiation
/// with automatic position extrapolation based on previously placed nodes.
/// </summary>
/// <remarks>
/// <para>
/// The placement system tracks the placed nodes to calculate
/// position offsets, allowing for rapid sequential placement with predictable spacing.
/// Supports both 2D and 3D scenes with appropriate position handling for each.
/// Every placement is one entry in the edited scene's undo history.
/// </para>
/// </remarks>
[Tool]
public partial class PlacingPage : Page<PlacingPageData, Plugin>
{
    [Export] public GridContainer sceneListView;
    [Export] public Label titleLabel;
    [Export] public ScrollContainer scrollContainer;
    [Export] public OptionButton methodButton;
    [Export] public CheckButton previewButton;

    private PlacingListItem selectedItem;

    /// <summary>
    /// Initializes the placing view with the current palette's scenes.
    /// Generates preview images and sets up selection callbacks.
    /// Double-clicking a tile opens its scene or script in the editor.
    /// </summary>
    public override void Initialize()
    {
        if (data.palette == null)
        {
            PaletterErrors.ThrowMissingPaletteException("null", $"{GetType().Name} {nameof(Initialize)}");
            dock.SwitchPage("PalettePage", null);
            return;
        }

        Title = "Scene Paletter";

        titleLabel.Text = data.palette.Name;
        sceneListView.Columns = plugin.settings.Columns;
        methodButton.Selected = (int)plugin.settings.Method;
        previewButton.SetPressedNoSignal(plugin.settings.ShowPreview);

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

                PaletteEntryInfo info = PaletteEntry.Resolve(uid);
                if (info.Error != null)
                    ExceptionHandler.ThrowResourceLoadException(uid, $"{GetType().Name} {nameof(Initialize)} - Index: {index} - {info.Error}");

                PlacingListItem item = packedScene.Instantiate() as PlacingListItem;
                if (item == null)
                {
                    ExceptionHandler.ThrowSceneInstantiationException("PlacingListItem", $"{GetType().Name} {nameof(Initialize)} - Index: {index}");
                    continue;
                }

                sceneListView.AddChild(item);
                if (index == data.currentElement) selectedItem = item;

                if (info.Error != null)
                {
                    item.SetData("Missing: " + uid, index == data.currentElement, "", () => Select(index, item),
                        () => ExceptionHandler.LogWarning($"Cannot open {uid}: {info.Error}", $"{GetType().Name} Open"));
                    item.SetTexture(GD.Load<Texture2D>("res://addons/scene_paletter/Icons/Empty.svg"));
                }
                else if (info.Resource is PackedScene scene)
                {
                    Node node = scene.Instantiate();
                    if (node == null)
                    {
                        ExceptionHandler.ThrowSceneInstantiationException(uid, $"{GetType().Name} {nameof(Initialize)} - Index: {index}");
                        item.QueueFree();
                        continue;
                    }

                    string name = node.Name;
                    node.Free();

                    item.SetData(name, index == data.currentElement, uid, () => Select(index, item),
                        () => EditorInterface.Singleton.OpenSceneFromPath(scene.ResourcePath));
                    ScenePreviewGenerator.GeneratePreview(
                        scene,
                        plugin.settings.PreviewResolution,
                        plugin.settings.PreviewMargin,
                        node is Node2D ? plugin.settings.PreviewTransparent2D : plugin.settings.PreviewTransparent3D,
                        item.SetTexture
                    );
                }
                else
                {
                    item.SetData(info.Name, index == data.currentElement, "", () => Select(index, item),
                        () => EditorInterface.Singleton.EditScript((Script)info.Resource));
                    item.SetTexture(PaletteEntry.GetIcon(info));
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(Initialize)} - Processing palette item at index {i}");
                continue; // Skip this item and continue with the next
            }
        }

        UpdatePreview();
    }

    /// <summary>
    /// Hides the viewport placement preview when the page is closed or replaced.
    /// </summary>
    public override void _ExitTree()
    {
        if (IsInstanceValid(plugin))
            plugin.HidePreview();
    }

    /// <summary>
    /// Selects a scene from the palette for placement by moving the highlight; the page is not rebuilt.
    /// </summary>
    /// <param name="index">Index of the scene to select</param>
    /// <param name="item">List item that represents <paramref name="index"/></param>
    public void Select(int index, PlacingListItem item)
    {
        if (index < 0 || index >= data.palette.Paths.Count)
        {
            PaletterErrors.ThrowInvalidPalettePositionException(index, $"{GetType().Name} {nameof(Select)}");
            return;
        }

        if (IsInstanceValid(selectedItem))
            selectedItem.SetSelected(false);

        item.SetSelected(true);
        selectedItem = item;
        data.currentElement = index;
        UpdatePreview();
    }

    /// <summary>
    /// Switches to editing mode for the current palette.
    /// </summary>
    public void Edit()
    {
        if (data.palette == null)
        {
            PaletterErrors.ThrowMissingPaletteException("null", $"{GetType().Name} {nameof(Edit)}");
            return;
        }

        dock.SwitchPage("EditingPage", new EditingPageData(data.palette));
    }

    /// <summary>
    /// Returns to the palette selection page.
    /// </summary>
    public void Back()
    {
        dock.SwitchPage("PalettePage", null);
    }

    /// <summary>
    /// Increases the number of columns in the grid layout.
    /// </summary>
    public void AddColumn()
    {
        plugin.settings.AddColumn();
        dock.ReloadPage(data);
    }

    /// <summary>
    /// Decreases the number of columns in the grid layout.
    /// </summary>
    public void RemoveColumn()
    {
        plugin.settings.RemoveColumn();
        dock.ReloadPage(data);
    }

    /// <summary>
    /// Stores the extrapolation method picked in the Method dropdown; the page is not rebuilt.
    /// </summary>
    /// <param name="index">Selected item index, equal to the <see cref="ExtrapolationMethod"/> value</param>
    public void SelectMethod(long index)
    {
        plugin.settings.Method = (ExtrapolationMethod)index;
    }

    /// <summary>
    /// Switches the viewport placement preview on or off.
    /// </summary>
    /// <param name="on">Whether the preview should be shown</param>
    public void SetPreview(bool on)
    {
        plugin.settings.ShowPreview = on;
        UpdatePreview();
    }

    /// <summary>
    /// Instantiates the selected entry and places it in the edited scene as one undoable action.
    /// All checks run before the scene is touched; the instance is freed if any of them fails.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Position calculation (only when the instance is a <c>Node2D</c>/<c>Node3D</c>; the parent may be a plain node,
    /// in which case the base pose is zero):
    /// <list type="bullet">
    ///   <item>No earlier placement: the parent's global pose</item>
    ///   <item>One earlier placement: that node's global pose</item>
    ///   <item>Two earlier placements, or three or more with Linear: <c>2 * last - previous</c></item>
    ///   <item>Three or more with another method: that method on the last three (see <see cref="ExtrapolationMethod"/>)</item>
    /// </list>
    /// The newest 2 (Linear) or 3 (others) surviving placements are used, so after an undo the chain continues from the older ones.
    /// Earlier placements that were undone, freed, or belong to another scene are skipped.
    /// </para>
    /// <para>
    /// A selected node inside an instanced sub-scene is replaced by the edited scene root as parent.
    /// A spatial parent of the other dimension than the instance is rejected.
    /// Instances that are neither <c>Node2D</c> nor <c>Node3D</c> are added without a pose and are not recorded.
    /// </para>
    /// </remarks>
    public void Place()
    {
        Node instance = null;
        bool committed = false;
        try
        {
            if (data.currentElement < 0 || data.currentElement >= data.palette.Paths.Count)
            {
                PaletterErrors.ThrowInvalidPalettePositionException(data.currentElement, $"{GetType().Name} {nameof(Place)}");
                return;
            }

            string uid = data.palette.Paths[data.currentElement];
            PaletteEntryInfo info = PaletteEntry.Resolve(uid);
            if (info.Error != null)
            {
                ExceptionHandler.LogWarning($"Cannot place {uid}: {info.Error}", $"{GetType().Name} {nameof(Place)}");
                return;
            }

            Node root = EditorInterface.Singleton?.GetEditedSceneRoot();
            if (root == null)
            {
                ExceptionHandler.ThrowMissingNodeException("EditedSceneRoot", $"{GetType().Name} {nameof(Place)}");
                return;
            }

            Node selected = GetSelectedNode();
            Node parent = PlacementTarget.Parent(root, selected);
            if (selected != null && parent != selected)
                ExceptionHandler.LogWarning($"{selected.Name} belongs to an instanced sub-scene, placing under the scene root instead", $"{GetType().Name} {nameof(Place)}");

            if (info.Resource.ResourcePath == root.SceneFilePath)
            {
                ExceptionHandler.LogWarning($"Cannot place {uid} into itself", $"{GetType().Name} {nameof(Place)}");
                return;
            }

            instance = Create(info, uid);
            if (instance == null)
            {
                ExceptionHandler.ThrowSceneInstantiationException(uid, $"{GetType().Name} {nameof(Place)}");
                return;
            }

            if (!PlacementTarget.Accepts(KindOf(instance), parent))
            {
                PaletterErrors.ThrowInvalidNodeTypeException(
                    instance.Name,
                    $"{parent.GetType().Name} (2D/3D)",
                    $"Parent: {parent.GetType().Name}, Instance: {instance.GetType().Name}"
                );
                return;
            }

            EditorUndoRedoManager undo = plugin.GetUndoRedo();
            undo.CreateAction($"Place {instance.Name}", UndoRedo.MergeMode.Disable, root);
            undo.AddDoMethod(parent, Node.MethodName.AddChild, instance, true);
            undo.AddDoProperty(instance, Node.PropertyName.Owner, root);

            bool positioned = true;
            if (instance is Node3D)
            {
                (Vector3 position, Vector3 rotation, Vector3? scale) = Extrapolator3D.Pose(plugin.settings.Method, parent, data.placed);
                undo.AddDoProperty(instance, Node3D.PropertyName.GlobalPosition, position);
                undo.AddDoProperty(instance, Node3D.PropertyName.GlobalRotation, rotation);
                if (scale.HasValue)
                    undo.AddDoProperty(instance, Node3D.PropertyName.Scale, scale.Value);
            }
            else if (instance is Node2D)
            {
                (Vector2 position, float rotation, Vector2? scale) = Extrapolator2D.Pose(plugin.settings.Method, parent, data.placed);
                undo.AddDoProperty(instance, Node2D.PropertyName.GlobalPosition, position);
                undo.AddDoProperty(instance, Node2D.PropertyName.GlobalRotation, rotation);
                if (scale.HasValue)
                    undo.AddDoProperty(instance, Node2D.PropertyName.Scale, scale.Value);
            }
            else
            {
                positioned = false;
            }

            undo.AddDoReference(instance);
            undo.AddUndoMethod(parent, Node.MethodName.RemoveChild, instance);
            undo.CommitAction();
            committed = true;

            data.placed.RemoveAll(n => !IsInstanceValid(n));
            if (positioned)
                data.placed.Add(instance);
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(Place)}");
        }
        finally
        {
            if (!committed && instance != null && IsInstanceValid(instance) && instance.GetParent() == null)
                instance.Free();
        }
    }

    /// <summary>
    /// Creates the node for a palette entry without adding it to any tree.
    /// </summary>
    /// <param name="info">Resolved palette entry</param>
    /// <param name="uid">Entry uid, stored as the custom-type marker for script entries</param>
    /// <returns>The new node, or null if it could not be created</returns>
    private static Node Create(PaletteEntryInfo info, string uid)
    {
        if (info.Resource is PackedScene packedScene)
            return packedScene.Instantiate(PackedScene.GenEditState.Instance);

        Node node = ((Script)info.Resource).Call("new").As<Node>();
        if (node != null)
        {
            node.Name = info.Name;
            // Same marker the editor's Add Node dialog sets for global classes
            node.SetMeta("_custom_type_script", uid);
        }
        return node;
    }

    /// <summary>
    /// Returns the first node selected in the editor, logging if the selection service is missing.
    /// </summary>
    /// <returns>The selected node, or null if nothing is selected or the selection is unavailable</returns>
    private Node GetSelectedNode()
    {
        if (EditorInterface.Singleton?.GetSelection() == null)
        {
            ExceptionHandler.ThrowNullReferenceException("EditorSelection", $"{GetType().Name} {nameof(GetSelectedNode)}");
            return null;
        }

        return PlacementTarget.Selected();
    }

    /// <summary>
    /// Publishes the dimension of the selected entry to the plugin's viewport preview (or hides it).
    /// </summary>
    private void UpdatePreview()
    {
        plugin.ShowPreview(KindOf(data.currentElement), data.placed);
    }

    /// <summary>
    /// Returns the dimension of the node a palette entry would create.
    /// </summary>
    /// <param name="index">Palette index</param>
    /// <returns><see cref="SpatialKind.None"/> if the index is invalid, the entry is unresolved or not a 2D/3D node</returns>
    private SpatialKind KindOf(int index)
    {
        if (data.palette == null || index < 0 || index >= data.palette.Paths.Count)
            return SpatialKind.None;

        PaletteEntryInfo info = PaletteEntry.Resolve(data.palette.Paths[index]);
        if (info.Error != null)
            return SpatialKind.None;

        if (info.Resource is not PackedScene scene)
            return ClassDB.IsParentClass(info.NativeBase, "Node3D") ? SpatialKind.Node3D
                : ClassDB.IsParentClass(info.NativeBase, "Node2D") ? SpatialKind.Node2D
                : SpatialKind.None;

        Node node = scene.Instantiate();
        SpatialKind kind = KindOf(node);
        node?.Free();
        return kind;
    }

    private static SpatialKind KindOf(Node node) => node switch
    {
        Node3D => SpatialKind.Node3D,
        Node2D => SpatialKind.Node2D,
        _ => SpatialKind.None,
    };
}
