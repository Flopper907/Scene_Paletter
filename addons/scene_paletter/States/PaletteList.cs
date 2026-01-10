using System.Collections.Generic;
using Addons.ScenePaletter.Tools;
using Addons.ScenePaletter.Widgets;
using Godot;

namespace Addons.ScenePaletter.States;

public partial class PaletteList : WindowState<PaletteListData>
{
    public PaletteList(Plugin plugin) : base(plugin)
    {
        Title = "Scene Palette";
    }

    public override void Initialize(PaletteListData data)
    {
        base.Initialize(data);
        LoadPalettes(data);
    }

    public override void Generate()
    {
        controls = new List<Control>();


        GenerateHeaderBar();
        controls.Add(headerBar);
        Label text = new Label();
        text.Text = "Palettes";
        LabelSettings labelSettings = new LabelSettings();
        labelSettings.FontSize = 25; // Set your desired size
        text.LabelSettings = labelSettings;
        headerBar.AddChild(text);

        GenerateContentArea();
        controls.Add(contentArea);
        ScrollContainer paletteScrollBar = new ScrollContainer();
        paletteScrollBar.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        VBoxContainer paletteScrollContent = new VBoxContainer();
        paletteScrollContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        paletteScrollContent.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        paletteScrollBar.AddChild(paletteScrollContent);
        foreach (Palette palette in data.palettes)
        {
            PackedScene ps = GD.Load<PackedScene>(plugin.config.WidgetPath + "PaletteListItem.tscn");
            PanelContainer item = ps.Instantiate() as PanelContainer;
            paletteScrollContent.AddChild(item);
            PaletteListItem.SetData(
                item,
                palette.Name,
                palette.UID,
                () =>
                {
                    plugin.SwitchState("Placing", new PlacingData() { palette = palette });
                },
                () =>
                {
                    DeletePalette(palette);
                    plugin.ReloadState(new PaletteListData());
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
            plugin.ReloadState(new PaletteListData());
        };
        footerBar.AddChild(createButton);
    }


    public void LoadPalettes(PaletteListData data)
    {
        List<Palette> palettes = new List<Palette>();
        var paletteData = SaveLoad.LoadAllWithFile<Palette>("res://addons/scene_paletter/Palettes/", ".json");
        foreach (var p in paletteData)
        {
            p.data.UID = p.filename.Replace(plugin.config.FileExtension, "");
            palettes.Add(p.data);
        }

        palettes.Sort((a, b) => a.Position.CompareTo(b.Position));

        data.palettes = palettes;
    }


    public void CreatePalette()
    {
        Palette palette = new Palette();
        palette.Name = "Untitled";
        palette.UID = IDGenerator.GenerateID(plugin.config.IdStart, plugin.config.IdEnd).ToString();
        palette.Position = data.palettes.Count > 0 ? data.palettes[data.palettes.Count - 1].Position + 1 : 0;
        data.palettes.Add(palette);
        SavePalette(palette);
    }
}