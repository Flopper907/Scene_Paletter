using System.Collections.Generic;
using Addons.ScenePaletter.Widgets;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class PaletteList : WindowState
{
    public PaletteList(Plugin plugin) : base(plugin)
    {
        title = "Scene Palette";
    }

    public override void Initialize()
    {
        LoadPalettes();
        base.Initialize();
    }

    public override void Generate()
    {
        controls = new List<Control>();

        GenerateHeaderBar();
        controls.Add(headerBar);
        LineEdit searchbar = new LineEdit();
        searchbar.PlaceholderText = "Search";
        searchbar.TextChanged += (string text) => { GD.Print(text); };
        headerBar.AddChild(searchbar);

        GenerateContentArea();
        controls.Add(contentArea);
        ScrollContainer paletteScrollBar = new ScrollContainer();
        paletteScrollBar.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        VBoxContainer paletteScrollContent = new VBoxContainer();
        paletteScrollContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        paletteScrollContent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        paletteScrollBar.AddChild(paletteScrollContent);
        foreach (Palette palette in plugin.palettes)
        {
            PackedScene ps = GD.Load<PackedScene>("res://addons/scene_paletter/Widgets/PaletteListItem.tscn");
            PanelContainer item = ps.Instantiate() as PanelContainer;
            paletteScrollContent.AddChild(item);
            PaletteListItem.SetData(
                item,
                palette.Name,
                palette.UID,
                () =>
                {
                    plugin.currentPalette = palette;
                    plugin.SwitchState("Placing");
                },
                () =>
                {
                    DeletePalette(palette);
                    plugin.SwitchState("PaletteList");
                }
            );
        }
        contentArea.AddChild(paletteScrollBar);



        GenerateFooterBar();
        controls.Add(footerBar);
        Button createButton = new Button();
        createButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        createButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        createButton.Text = "Create New";
        createButton.Pressed += () =>
        {
            CreatePalette();
            plugin.SwitchState("PaletteList");
        };
        footerBar.AddChild(createButton);
    }
}