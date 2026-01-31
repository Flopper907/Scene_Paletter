using System;
using Godot;

namespace Addons.ScenePaletter.Widgets;

/// <summary>
/// Widget displaying a scene item in the placing view with selection controls.
/// </summary>
[Tool]
public partial class PlacingListItem : PanelContainer
{
    [Export] public TextureRect textureRect;
    [Export] public Button selectButton;
    [Export] public Label nameLabel;
    [Export] public Panel selectionPanel;

    /// <summary>
    /// Configures the widget with scene data and connects the selection callback.
    /// </summary>
    /// <param name="name">Display name of the scene</param>
    /// <param name="selected">Whether the item is currently selected</param>
    /// <param name="selection">Callback invoked when the select button is pressed</param>
    public void SetData(string name, bool selected, Action selection)
    {
        selectButton.Pressed += selection;
        nameLabel.Text = name;
        selectionPanel.Visible = selected;
    }

    /// <summary>
    /// Updates the preview texture for the scene.
    /// </summary>
    /// <param name="texture">Preview texture to display</param>
    public void SetTexture(Texture2D texture)
    {
        textureRect.Texture = texture;
    }
}