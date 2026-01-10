using System;
using Godot;

namespace Addons.ScenePaletter.Widgets;

public partial class PaletteListItem : PanelContainer
{
    private static string DeleteButtonPath = "HBoxContainer/TextureButton";
    private static string SelectButtonPath = "HBoxContainer/Button";
    private static string NameLabelPath = "HBoxContainer/Button/Label";
    private static string IDLabelPath = "HBoxContainer/Button/Label2";

    public static void SetData(Node root, string name, string id, Action selection, Action deletion)
    {
        root.GetNode<Label>(NameLabelPath).Text = name;
        root.GetNode<Label>(IDLabelPath).Text = id;
        root.GetNode<Button>(SelectButtonPath).Pressed += selection;
        root.GetNode<Button>(DeleteButtonPath).Pressed += deletion;
    }
}