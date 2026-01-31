using System;
using Godot;

namespace Addons.ScenePaletter.Widgets;

/// <summary>
/// Widget displaying a palette entry with name, ID, and action buttons.
/// </summary>
[Tool]
public partial class PaletteListItem : PanelContainer
{
    [Export] public Label nameLabel;
    [Export] public Label idLabel;
    [Export] public Button selectButton;
    [Export] public Button deleteButton;

    /// <summary>
    /// Configures the widget with palette data and connects button callbacks.
    /// </summary>
    /// <param name="name">Display name of the palette</param>
    /// <param name="id">Unique identifier of the palette</param>
    /// <param name="selection">Callback invoked when the select button is pressed</param>
    /// <param name="deletion">Callback invoked when the delete button is pressed</param>
    public void SetData(string name, string id, Action selection, Action deletion)
    {
        nameLabel.Text = name;
        idLabel.Text = id;
        selectButton.Pressed += selection;
        deleteButton.Pressed += deletion;
    }
}