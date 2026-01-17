using System;
using System.ComponentModel;
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

    internal void SetData(string name, string uID, object v1, object v2)
    {
        throw new NotImplementedException();
    }

}