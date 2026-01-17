// using System;
// using Godot;

// namespace Addons.ScenePaletter.Widgets;

// public partial class SceneListItem : PanelContainer
// {
//     private static string ImagePath = "MarginContainer/VBoxContainer/TextureRect";
//     private static string SelectButtonPath = "PanelContainer/Button";
//     private static string NameLabelPath = "MarginContainer/VBoxContainer/Label";
//     private static string SelectionPath = "Panel";

//     public static void SetData(Node root, string name, bool selected, Action selection)
//     {
//         root.GetNode<Label>(NameLabelPath).Text = name;
//         //root.GetNode<TextureRect>(ImagePath).Texture = image;
//         root.GetNode<Button>(SelectButtonPath).Pressed += selection;
//         root.GetNode<Panel>(SelectionPath).Visible = selected;
//     }
// }