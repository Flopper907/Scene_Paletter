// using System;
// using Godot;

// namespace Addons.ScenePaletter.Widgets;

// public partial class SceneEditListItem : PanelContainer
// {
//     private static string SelectionPath = "Panel";
//     private static string DeleteButtonPath = "PanelContainer2/Button";
//     private static string SelectButtonPath = "PanelContainer/Button";
//     private static string UpButtonPath = "MarginContainer2/GridContainer/Up";
//     private static string DownButtonPath = "MarginContainer2/GridContainer/Down";
//     private static string LeftButtonPath = "MarginContainer2/GridContainer/Left";
//     private static string RightButtonPath = "MarginContainer2/GridContainer/Right";

//     public static void SetData(Node root, bool selected, Action selection, Action deletion, Action up, Action down, Action left, Action right)
//     {
//         root.GetNode<Panel>(SelectionPath).Visible = selected;
//         root.GetNode<Button>(SelectButtonPath).Pressed += selection;
//         root.GetNode<Button>(DeleteButtonPath).Pressed += deletion;

//         // root.GetNode<Button>(UpButtonPath).Pressed += up;
//         // root.GetNode<Button>(UpButtonPath).Modulate = selected ? new Color(0f, 0f, 0f, 0f) : Colors.White;
//         // root.GetNode<Button>(DownButtonPath).Pressed += down;
//         // root.GetNode<Button>(DownButtonPath).Modulate = selected ? new Color(0f, 0f, 0f, 0f) : Colors.White;
//         // root.GetNode<Button>(LeftButtonPath).Pressed += left;
//         // root.GetNode<Button>(LeftButtonPath).Modulate = selected ? new Color(0f, 0f, 0f, 0f) : Colors.White;
//         // root.GetNode<Button>(RightButtonPath).Pressed += right;
//         // root.GetNode<Button>(RightButtonPath).Modulate = selected ? new Color(0f, 0f, 0f, 0f) : Colors.White;
//     }
// }