using System;
using Godot;

namespace Addons.ScenePaletter.Widgets;

[Tool]
public partial class EditingListItem : PanelContainer
{
    [Export] public TextureRect textureRect;
    [Export] public Button selectButton;
    [Export] public Button deleteButton;
    [Export] public Label nameLabel;
    [Export] public Panel selectionPanel;

    public void SetData(string name, bool selected, Action selection, Action deletion)
    {
        selectButton.Pressed += selection;
        deleteButton.Pressed += deletion;
        nameLabel.Text = name;
        selectionPanel.Visible = selected;
    }

    public void SetTexture(Texture2D texture)
    {
        textureRect.Texture = texture;
    }
}