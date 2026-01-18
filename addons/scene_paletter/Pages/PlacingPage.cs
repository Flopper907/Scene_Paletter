using System;
using Addons.ScenePaletter.Widgets;
using Godot;
using Godot.Collections;

namespace Addons.ScenePaletter.Pages;

[Tool]
public partial class PlacingPage : Page<PlacingPageData>
{
    [Export] public GridContainer paletteListView;
    [Export] public Label titleLabel;

    public override void Initialize()
    {
        if (data.palette == null) plugin.SwitchState("PalettePage", null);

        titleLabel.Text = data.palette.Name;
        paletteListView.Columns = plugin.config.Columns;

        PackedScene packedScene = GD.Load<PackedScene>(plugin.config.WidgetPath + "PlacingListItem.tscn");
        for (int i = 0; i < data.palette.Paths.Count; i++)
        {
            PlacingListItem item = packedScene.Instantiate() as PlacingListItem;
            paletteListView.AddChild(item);

            int index = i;
            item.SetData(data.palette.Paths[i], i == data.currentElement, () => Select(index));
        }
    }

    public void Select(int index)
    {
        data.previousElement = data.currentElement;
        data.currentElement = index;
        plugin.ReloadState(data);
    }

    public void Edit()
    {
        plugin.SwitchState("EditPage", null);
    }

    public void Back()
    {
        plugin.SwitchState("PalettePage", null);
    }

    public void AddColumn()
    {
        plugin.config.Columns = Math.Min(plugin.config.MaxColums, plugin.config.Columns + 1);
        plugin.ReloadState(data);
    }

    public void RemoveColumn()
    {
        plugin.config.Columns = Math.Max(plugin.config.MinColums, plugin.config.Columns - 1);
        plugin.ReloadState(data);
    }

    public void Place()
    {
        PackedScene packedScene = GD.Load<PackedScene>(data.palette.Paths[data.currentElement]);
        if (packedScene == null) return;
        Node parent = GetParentNodeFromEditor();
        Node instance = packedScene.Instantiate();

        if (parent is Node2D parent2D && instance is Node2D instance2D)
        {
            // Handle positioning
            if (data.lastSpawned is Node2D last2D)
            {
                if (parent2D == last2D)
                {
                    parent2D = last2D.GetParent() as Node2D;
                }
                if (data.previousSpawned is Node2D previous2D)
                {
                    Vector2 nextPos = CalculateNextPosition(previous2D, last2D);
                    instance2D.GlobalPosition = nextPos;
                }
                else
                {
                    // Only one previous spawn, place at same position
                    instance2D.GlobalPosition = last2D.GlobalPosition;
                }
            }
            // else: first spawn, use default position (0,0 or wherever parent is)

            parent2D.AddChild(instance);
            instance.Owner = parent.GetTree().EditedSceneRoot;

            // Update spawn tracking
            data.previousSpawned = data.lastSpawned;
            data.lastSpawned = instance;
        }
        else if (parent is Node3D parent3D && instance is Node3D instance3D)
        {
            // Handle positioning
            if (data.lastSpawned is Node3D last3D)
            {
                if (parent3D == last3D)
                {
                    parent3D = last3D.GetParent() as Node3D;
                }
                if (data.previousSpawned is Node3D previous3D)
                {
                    Vector3 nextPos = CalculateNextPosition(previous3D, last3D);
                    instance3D.GlobalPosition = nextPos;
                }
                else
                {
                    // Only one previous spawn, place at same position
                    instance3D.GlobalPosition = last3D.GlobalPosition;
                }
            }
            // else: first spawn, use default position (0,0,0 or wherever parent is)

            parent3D.AddChild(instance);
            instance.Owner = parent.GetTree().EditedSceneRoot;

            // Update spawn tracking
            data.previousSpawned = data.lastSpawned;
            data.lastSpawned = instance;
        }
        else
        {
            GD.PrintErr("Parent and instance type mismatch or unsupported node type");
            instance.Free();
        }

        // Mark scene as unsaved
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

    private Vector2 CalculateNextPosition(Node2D previous, Node2D last)
    {
        Vector2 spawnPosition = Vector2.Zero;

        if (GodotObject.IsInstanceValid(last))
        {
            if (GodotObject.IsInstanceValid(previous))
            {
                spawnPosition = 2f * last.GlobalPosition - previous.GlobalPosition;
            }
            else
            {
                spawnPosition = last.GlobalPosition;
            }
        }

        return spawnPosition;
    }

    private Vector3 CalculateNextPosition(Node3D previous, Node3D last)
    {
        Vector3 spawnPosition = Vector3.Zero;

        if (GodotObject.IsInstanceValid(last))
        {
            if (GodotObject.IsInstanceValid(previous))
            {
                spawnPosition = 2f * last.GlobalPosition - previous.GlobalPosition;
            }
            else
            {
                spawnPosition = last.GlobalPosition;
            }
        }

        return spawnPosition;
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
    }

    public PlacingPageData(Palette palette)
    {
        this.palette = palette;
        currentElement = 0;
        previousElement = -1;
        lastSpawned = null;
        previousSpawned = null;
    }

    public Palette palette;
    public int currentElement = 0;
    public int previousElement = -1;
    public Node lastSpawned;
    public Node previousSpawned;
}