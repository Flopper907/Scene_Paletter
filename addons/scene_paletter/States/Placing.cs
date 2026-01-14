using System;
using System.Collections.Generic;
using Addons.ScenePaletter.Widgets;
using Godot;
using Godot.Collections;

namespace Addons.ScenePaletter.States;

public partial class Placing : WindowState<PlacingData>
{
    public Placing(Plugin plugin) : base(plugin)
    {
        Title = "Scene Palette";
    }

    public override void Initialize(PlacingData data)
    {
        base.Initialize(data);
        if (data == null || data.palette == null)
        {
            plugin.SwitchState("PaletteList", new PaletteListData());
        }
        if (data.palette.Paths.Count > 0 && data.currentElement == "")
        {
            data.currentElement = data.palette.Paths[0];
            data.previousElement = data.currentElement;
        }
    }

    public override void Generate()
    {
        controls = new List<Control>();


        GenerateHeaderBar();
        controls.Add(headerBar);
        Label paletteName = new Label();
        paletteName.Text = data.palette.Name;
        LabelSettings paletteNameSettings = new LabelSettings();
        paletteNameSettings.FontSize = 25; // Set your desired size
        paletteName.LabelSettings = paletteNameSettings;
        headerBar.AddChild(paletteName);



        GenerateContentArea();
        controls.Add(contentArea);
        HBoxContainer managementButtonsContainer = new HBoxContainer();
        managementButtonsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentArea.AddChild(managementButtonsContainer);

        Button backButton = new Button();
        backButton.Text = "Back";
        backButton.Pressed += () =>
        {
            plugin.SwitchState("PaletteList", new PaletteListData());
        };
        backButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        backButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        managementButtonsContainer.AddChild(backButton);

        Button editButton = new Button();
        editButton.Text = "Edit";
        editButton.Pressed += () =>
        {
            plugin.SwitchState("Editing", new EditingData()
            {
                palette = data.palette
            });
        };
        editButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        editButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        managementButtonsContainer.AddChild(editButton);



        HSeparator hSeparator = new HSeparator();
        hSeparator.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentArea.AddChild(hSeparator);


        ScrollContainer paletteScrollBar = new ScrollContainer();
        paletteScrollBar.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        paletteScrollBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        paletteScrollBar.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;

        GridContainer paletteScrollContent = new GridContainer();
        paletteScrollContent.Columns = plugin.config.Columns;
        paletteScrollContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        paletteScrollContent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        paletteScrollBar.AddChild(paletteScrollContent);

        PackedScene ps = GD.Load<PackedScene>(plugin.config.WidgetPath + "SceneListItem.tscn");
        foreach (string uid in data.palette.Paths)
        {
            PanelContainer item = ps.Instantiate() as PanelContainer;
            SceneListItem.SetData(item, uid, data.currentElement == uid, () =>
            {
                data.previousElement = data.currentElement;
                data.currentElement = uid;

                if (data.currentElement != data.previousElement)
                {
                    data.previousSpawned = null;
                    data.lastSpawned = null;
                }

                plugin.ReloadState(data);
            });
            paletteScrollContent.AddChild(item);
        }

        contentArea.AddChild(paletteScrollBar);





        GenerateFooterBar();
        controls.Add(footerBar);

        HBoxContainer footerContent = new HBoxContainer();
        footerContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        footerContent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        footerBar.AddChild(footerContent);

        Button columnRemoveButton = new Button();
        columnRemoveButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        columnRemoveButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        columnRemoveButton.SizeFlagsStretchRatio = 1f;
        footerContent.AddChild(columnRemoveButton);

        columnRemoveButton.Text = "-";
        columnRemoveButton.Pressed += () =>
        {
            plugin.config.Columns = Math.Max(plugin.config.MinColums, plugin.config.Columns - 1);
            plugin.ReloadState(data);
        };


        Button columnAddButton = new Button();
        columnAddButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        columnAddButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        columnAddButton.SizeFlagsStretchRatio = 1f;
        footerContent.AddChild(columnAddButton);

        columnAddButton.Text = "+";
        columnAddButton.Pressed += () =>
        {
            plugin.config.Columns = Math.Min(plugin.config.MaxColums, plugin.config.Columns + 1);
            plugin.ReloadState(data);
        };


        Button placeButton = new Button();
        placeButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        placeButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        footerContent.AddChild(placeButton);
        placeButton.SizeFlagsStretchRatio = 8f;
        placeButton.Text = "Place";
        placeButton.Pressed += () =>
        {
            PackedScene packedScene = GD.Load<PackedScene>(data.currentElement);
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
        };
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