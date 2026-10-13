namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Dimension of the node a palette entry creates, which decides whether and how it gets a pose.
/// </summary>
public enum SpatialKind
{
    /// <summary>Not a <c>Node2D</c>/<c>Node3D</c> or not resolvable: placed without a pose, nothing to preview.</summary>
    None,

    /// <summary>A <c>Node2D</c> or derived type.</summary>
    Node2D,

    /// <summary>A <c>Node3D</c> or derived type.</summary>
    Node3D,
}
