using System;
using System.Collections.Generic;
using System.Linq;
using Addons.ScenePaletter.Widgets;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class Editing : WindowState<EditingData>
{
    public Editing(Plugin plugin) : base(plugin)
    {
        Title = "Scene Palette";
    }

    public override void Initialize(EditingData data)
    {
        base.Initialize(data);
        if (data == null || data.palette == null)
        {
            plugin.SwitchState("PaletteList", new PaletteListData());
        }
        if (data.old == null)
        {
            data.old = data.palette.Copy();
        }
        Title = "Scene Palette" + (data.old.Equals(data.palette) ? "" : "*");
    }

    public override void Generate()
    {
        controls = new List<Control>();

        GenerateHeaderBar();
        controls.Add(headerBar);
        LineEdit paletteName = new LineEdit();
        paletteName.Text = data.palette.Name;
        paletteName.Set("theme_override_font_sizes/font_size", 25);
        paletteName.TextSubmitted += (string text) =>
        {
            data.palette.Name = text;
            plugin.ReloadState(data);
        };
        headerBar.AddChild(paletteName);

        GenerateContentArea();
        controls.Add(contentArea);
        HBoxContainer managementButtonsContainer = new HBoxContainer();
        managementButtonsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentArea.AddChild(managementButtonsContainer);

        Button backButton = new Button();
        backButton.Text = "Discard";
        backButton.Pressed += () =>
        {
            plugin.SwitchState("Placing", new PlacingData()
            {
                palette = data.old
            });
        };
        backButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        backButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        managementButtonsContainer.AddChild(backButton);

        Button editButton = new Button();
        editButton.Text = "Save";
        editButton.Pressed += () =>
        {
            SavePalette(data.palette);
            plugin.SwitchState("Placing", new PlacingData()
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
        paletteScrollContent.Columns = 2;
        paletteScrollContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        paletteScrollContent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        paletteScrollBar.AddChild(paletteScrollContent);

        PackedScene ps = GD.Load<PackedScene>(plugin.config.WidgetPath + "SceneEditListItem.tscn");
        for (int i = 0; i < data.palette.Paths.Count; i++)
        {
            PanelContainer item = ps.Instantiate() as PanelContainer;

            int currentIndex = i;
            string uid = data.palette.Paths[currentIndex];

            SceneEditListItem.SetData(item, data.selectedElements.ContainsKey(uid),
                selection: () =>
                {
                    ToggleSelection(uid, currentIndex);
                    plugin.ReloadState(data);
                },
                deletion: () =>
                {
                    AddToSelection(uid, currentIndex);

                    var uidsToRemove = data.selectedElements.Keys.ToList();

                    foreach (var selectedUid in uidsToRemove)
                    {
                        data.palette.Paths.Remove(selectedUid);
                    }

                    data.selectedElements.Clear();

                    plugin.ReloadState(data);
                },
                up: () => { GD.Print("Up"); },
                down: () => { GD.Print("Down"); },
                left: () => { GD.Print("Left"); },
                right: () => { GD.Print("Right"); }
            );
            paletteScrollContent.AddChild(item);
        }

        PackedScene addButton = GD.Load<PackedScene>(plugin.config.WidgetPath + "SceneAddListItem.tscn");
        Control addButtonInstance = addButton.Instantiate() as Control;
        SceneAddListItem.SetData(addButtonInstance, () =>
        {
            SetupFileDialog();
        });

        paletteScrollContent.AddChild(addButtonInstance);
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


        Control spacer = new Control();
        spacer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        footerContent.AddChild(spacer);
        spacer.SizeFlagsStretchRatio = 8f;
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
        contentArea.AddChild(fileDialog);

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
        plugin.ReloadState(data);
    }

    private void ToggleSelection(string s, int i)
    {
        if (data.selectedElements.ContainsKey(s))
        {
            data.selectedElements.Remove(s);
        }
        else
        {
            data.selectedElements.Add(s, i);
        }
    }

    private void AddToSelection(string s, int i)
    {
        if (!data.selectedElements.ContainsKey(s))
        {
            data.selectedElements.Add(s, i);
        }
    }

    private void RemoveFromSelection(string s, int i)
    {
        if (data.selectedElements.ContainsKey(s))
        {
            data.selectedElements.Remove(s);
        }
    }
}