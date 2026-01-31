using System;
using Godot;

namespace Addons.ScenePaletter.Widgets;

/// <summary>
/// Widget displaying a scene item in the editing view with selection and deletion controls.
/// </summary>
[Tool]
public partial class EditingListItem : PanelContainer
{
    [Export] public TextureRect textureRect;
    [Export] public Button selectButton;
    [Export] public Button deleteButton;
    [Export] public Label nameLabel;
    [Export] public Panel selectionPanel;

    /// <summary>
    /// Configures the widget with scene data and connects button callbacks.
    /// </summary>
    /// <param name="name">Display name of the scene</param>
    /// <param name="selected">Whether the item is currently selected</param>
    /// <param name="selection">Callback invoked when the select button is pressed</param>
    /// <param name="deletion">Callback invoked when the delete button is pressed</param>
    public void SetData(string name, bool selected, Action selection, Action deletion)
    {
        selectButton.Pressed += selection;
        deleteButton.Pressed += deletion;
        nameLabel.Text = name;
        selectionPanel.Visible = selected;
    }

    // <summary>
    /// Updates the preview texture for the scene.
    /// </summary>
    /// <param name="texture">Preview texture to display</param>
    public void SetTexture(Texture2D texture)
    {
        textureRect.Texture = texture;
    }
}