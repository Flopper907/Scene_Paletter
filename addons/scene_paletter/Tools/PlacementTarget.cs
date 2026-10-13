using Godot;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Parent resolution shared by the placer and the placement preview, so both agree on where a node would land.
/// </summary>
public static class PlacementTarget
{
    /// <summary>
    /// Returns the first node selected in the editor, without logging.
    /// </summary>
    /// <returns>The selected node, or null if nothing is selected or the selection is unavailable</returns>
    public static Node Selected()
    {
        Godot.Collections.Array<Node> selected = EditorInterface.Singleton?.GetSelection()?.GetSelectedNodes();
        return selected != null && selected.Count > 0 ? selected[0] : null;
    }

    /// <summary>
    /// Resolves the parent of a new placement.
    /// </summary>
    /// <param name="root">Edited scene root</param>
    /// <param name="selected">First selected node, may be null</param>
    /// <returns><paramref name="selected"/>, or <paramref name="root"/> if nothing is selected or the selection belongs to an instanced sub-scene</returns>
    public static Node Parent(Node root, Node selected)
    {
        if (selected == null || (selected != root && selected.Owner != root))
            return root;

        return selected;
    }

    /// <summary>
    /// Whether the placer accepts an entry of <paramref name="kind"/> under <paramref name="parent"/>.
    /// </summary>
    /// <returns>False only when <paramref name="parent"/> is spatial of the other dimension than <paramref name="kind"/></returns>
    public static bool Accepts(SpatialKind kind, Node parent) => kind switch
    {
        SpatialKind.Node3D => parent is not Node2D,
        SpatialKind.Node2D => parent is not Node3D,
        _ => true,
    };
}
