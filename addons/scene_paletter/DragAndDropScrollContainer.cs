using Godot;

namespace Addons.ScenePaletter;

/// <summary>
/// A custom <see cref="ScrollContainer"/> that handles drag-and-drop operations for Godot scene files (.tscn).
/// Emits a signal when valid scene files are dropped into the container.
/// </summary>
[Tool]
public partial class DragAndDropScrollContainer : ScrollContainer
{
    /// <summary>
    /// Signal emitted when one or more valid Godot scene files (.tscn) are dropped into the container.
    /// Provides an array of file paths as argument.
    /// </summary>
    /// <param name="paths">Array of dropped scene file paths.</param>
    [Signal]
    public delegate void ScenesDroppedEventHandler(string[] paths);

    /// <summary>
    /// Determines whether the dropped data can be accepted by this container.
    /// Only accepts Godot dictionary data containing scene files (.tscn).
    /// </summary>
    /// <param name="atPosition">The position where the data would be dropped (unused in this implementation).</param>
    /// <param name="data">The dragged data to check.</param>
    /// <returns>
    /// <c>true</c> if the data contains one or more .tscn files; otherwise, <c>false</c>.
    /// </returns>
    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (data.VariantType != Variant.Type.Dictionary)
            return false;

        var dict = data.AsGodotDictionary();

        if (!dict.ContainsKey("files"))
            return false;

        var files = dict["files"].AsGodotArray<string>();

        foreach (string file in files)
        {
            if (file.EndsWith(".tscn"))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Handles the drop operation by extracting valid scene file paths and emitting the <see cref="ScenesDropped"/> signal.
    /// </summary>
    /// <param name="atPosition">The position where the data was dropped (unused in this implementation).</param>
    /// <param name="data">The dropped data containing file paths.</param>
    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var dict = data.AsGodotDictionary();
        var files = dict["files"].AsGodotArray<string>();

        var validScenes = new System.Collections.Generic.List<string>();

        foreach (string file in files)
        {
            if (file.EndsWith(".tscn"))
                validScenes.Add(file);
        }

        EmitSignal(SignalName.ScenesDropped, validScenes.ToArray());
    }
}