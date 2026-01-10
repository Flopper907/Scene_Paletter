using System.Collections.Generic;
using Addons.ScenePaletter.Widgets;
using Godot;

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

        GridContainer paletteScrollContent = new GridContainer();
        paletteScrollContent.Columns = 2;
        paletteScrollContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        paletteScrollContent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        paletteScrollBar.AddChild(paletteScrollContent);

        foreach (string uid in data.palette.Paths)
        {
            PackedScene ps = GD.Load<PackedScene>(plugin.config.WidgetPath + "SceneListItem.tscn");
            PanelContainer item = ps.Instantiate() as PanelContainer;
            SceneListItem.SetData(item, uid, data.currentElement == uid, () =>
            {
                data.currentElement = uid;
                plugin.ReloadState(data);
            });
            paletteScrollContent.AddChild(item);
        }
        contentArea.AddChild(paletteScrollBar);


        GenerateFooterBar();
        controls.Add(footerBar);
        Button placeButton = new Button();
        placeButton.Text = "Place";
        placeButton.Pressed += () =>
        {
            GD.Print("Place");
        };
        placeButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        placeButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        footerBar.AddChild(placeButton);

    }
}