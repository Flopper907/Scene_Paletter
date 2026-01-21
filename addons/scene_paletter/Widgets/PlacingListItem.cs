using System;
using Godot;

namespace Addons.ScenePaletter.Widgets;

[Tool]
public partial class PlacingListItem : PanelContainer
{
    [Export] public TextureRect textureRect;
    [Export] public Button selectButton;
    [Export] public Label nameLabel;
    [Export] public Panel selectionPanel;

    public void SetData(string name, bool selected, Action selection)
    {
        selectButton.Pressed += selection;
        nameLabel.Text = name;
        selectionPanel.Visible = selected;
    }

    public void SetTexture(Texture2D texture)
    {
        textureRect.Texture = texture;
    }
}