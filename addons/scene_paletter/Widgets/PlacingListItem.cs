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
    [Export] public Label nameLabel;
    [Export] public Panel selectionPanel;

    private string _scenePath;
    private Action _selectionCallback;

    /// <summary>
    /// Configures the widget with scene data and connects the selection callback.
    /// </summary>
    /// <param name="name">Display name of the scene</param>
    /// <param name="selected">Whether the item is currently selected</param>
    /// <param name="scenePath">The full path/UID of the scene file</param>
    /// <param name="selection">Callback invoked when the select button is pressed</param>
    public void SetData(string name, bool selected, string scenePath, Action selection)
    {
        _selectionCallback = selection;

        nameLabel.Text = name;
        selectionPanel.Visible = selected;
        _scenePath = scenePath;

        MouseDefaultCursorShape = CursorShape.PointingHand;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
            if (!mouseEvent.Pressed)
                _selectionCallback?.Invoke();
    }

    /// <summary>
    /// Updates the preview texture for the scene.
    /// </summary>
    /// <param name="texture">Preview texture to display</param>
    public void SetTexture(Texture2D texture)
    {
        textureRect.Texture = texture;
    }

    /// <summary>
    /// Packs the scene path into Godot's native file-drag format, allowing 
    /// the user to drag items directly into 2D/3D viewports without extrapolation.
    /// </summary>
    public override Variant _GetDragData(Vector2 atPosition)
    {
        GD.Print("Drag gestartet! Es funktioniert!");

        if (string.IsNullOrEmpty(_scenePath))
            return default;

        // Erstelle das native Godot-Dictionary
        var dragData = new Godot.Collections.Dictionary();
        dragData["type"] = "files";

        var filesArray = new Godot.Collections.Array<string>();
        filesArray.Add(_scenePath);
        dragData["files"] = filesArray;

        // Vorschau generieren
        if (textureRect.Texture != null)
        {
            var previewCtrl = new TextureRect
            {
                Texture = textureRect.Texture,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                CustomMinimumSize = new Vector2(64, 64),
                Modulate = new Color(1, 1, 1, 0.7f)
            };

            SetDragPreview(previewCtrl);
        }

        return dragData;
    }
}