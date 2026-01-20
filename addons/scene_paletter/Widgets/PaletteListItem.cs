using System;
using Godot;

namespace Addons.ScenePaletter.Widgets;

[Tool]
public partial class PaletteListItem : PanelContainer
{
    [Export] public Label nameLabel;
    [Export] public Label idLabel;
    [Export] public Button selectButton;
    [Export] public Button deleteButton;

    public void SetData(string name, string id, Action selection, Action deletion)
    {
        nameLabel.Text = name;
        idLabel.Text = id;
        selectButton.Pressed += selection;
        deleteButton.Pressed += deletion;
    }
}