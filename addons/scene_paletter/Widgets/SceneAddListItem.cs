using System;
using Godot;

public partial class SceneAddListItem : WindowStateData
{
    private static string ButtonPath = "PanelContainer/Button";
    public static void SetData(Node root, Action add)
    {
        root.GetNode<Button>(ButtonPath).Pressed += add;
    }
}