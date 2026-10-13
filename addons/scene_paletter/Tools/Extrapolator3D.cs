using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Pure pose math for the next <c>Node3D</c> placement. The 2D twin is <see cref="Extrapolator2D"/>.
/// </summary>
public static class Extrapolator3D
{
    /// <summary>Upper bound of the step growth factor of <see cref="ExtrapolationMethod.Exponential"/>.</summary>
    private const float MaxRatio = 4f;

    /// <summary>Squared step length at or below which the Exponential ratio is undefined and Linear is used.</summary>
    private const float MinStepSquared = 1e-8f;

    /// <summary>Scale values at or below this are not valid for a ratio.</summary>
    private const float MinScale = 1e-6f;

    /// <summary>Relative squared cross product at or below which three points count as collinear.</summary>
    private const float CollinearEpsilon = 1e-6f;

    /// <summary>
    /// Selects the nodes <see cref="Pose"/> extrapolates from: the newest valid, in-tree <c>Node3D</c> entries of
    /// <paramref name="placed"/>, at most 2 for Linear and 3 for the other methods.
    /// </summary>
    /// <param name="method">Extrapolation method that decides how many nodes are needed</param>
    /// <param name="placed">Earlier placements, oldest first (not modified)</param>
    /// <returns>The selected nodes, oldest first; shorter than needed when fewer survive</returns>
    public static Node3D[] Chain(ExtrapolationMethod method, List<Node> placed)
    {
        int need = NeededCount(method);
        List<Node3D> newestFirst = new(need);
        for (int i = placed.Count - 1; i >= 0 && newestFirst.Count < need; i--)
        {
            if (GodotObject.IsInstanceValid(placed[i]) && placed[i].IsInsideTree() && placed[i] is Node3D node)
                newestFirst.Add(node);
        }

        newestFirst.Reverse();
        return newestFirst.ToArray();
    }

    private static int NeededCount(ExtrapolationMethod method) => method == ExtrapolationMethod.Linear ? 2 : 3;

    /// <summary>
    /// Computes the pose of the next placement from the surviving 3D entries of <paramref name="placed"/>.
    /// </summary>
    /// <param name="method">Extrapolation method; needs 2 (Linear) or 3 (others) nodes, fewer degrade as described on <see cref="ExtrapolationMethod"/></param>
    /// <param name="parent">Parent of the new node; a non-<c>Node3D</c> parent contributes a zero pose</param>
    /// <param name="placed">Earlier placements, oldest first (not modified); freed, undone or foreign-tree entries are skipped</param>
    /// <returns>
    /// Global position and rotation (Euler), and the local scale to apply. The scale is <c>null</c> when there is no earlier
    /// placement, so the placed scene keeps its authored scale. A result with a non-finite component is replaced by the Linear one.
    /// </returns>
    public static (Vector3 position, Vector3 rotation, Vector3? scale) Pose(ExtrapolationMethod method, Node parent, List<Node> placed)
    {
        int need = NeededCount(method);
        Node3D[] recent = Chain(method, placed);

        switch (recent.Length)
        {
            case 0:
                return parent is Node3D p ? (p.GlobalPosition, p.GlobalRotation, null) : (Vector3.Zero, Vector3.Zero, null);
            case 1:
                return (recent[0].GlobalPosition, recent[0].GlobalRotation, recent[0].Scale);
        }

        Node3D last = recent[^1];
        Node3D previous = recent[^2];
        Vector3 linearScale = 2f * last.Scale - previous.Scale;
        (Vector3 position, Vector3 rotation, Vector3? scale) linear = (
            2f * last.GlobalPosition - previous.GlobalPosition,
            2f * last.GlobalRotation - previous.GlobalRotation,
            linearScale);

        if (recent.Length < need)
            return linear;

        Node3D oldest = recent[0];
        Vector3 position = linear.position;
        Vector3 rotation = linear.rotation;
        Vector3 scale = linearScale;
        switch (method)
        {
            case ExtrapolationMethod.Quadratic:
                position = 3f * last.GlobalPosition - 3f * previous.GlobalPosition + oldest.GlobalPosition;
                rotation = 3f * last.GlobalRotation - 3f * previous.GlobalRotation + oldest.GlobalRotation;
                scale = 3f * last.Scale - 3f * previous.Scale + oldest.Scale;
                break;
            case ExtrapolationMethod.Exponential:
                position = ExponentialPosition(oldest.GlobalPosition, previous.GlobalPosition, last.GlobalPosition) ?? position;
                scale = new Vector3(
                    ExponentialComponent(previous.Scale.X, last.Scale.X, linearScale.X),
                    ExponentialComponent(previous.Scale.Y, last.Scale.Y, linearScale.Y),
                    ExponentialComponent(previous.Scale.Z, last.Scale.Z, linearScale.Z));
                break;
            case ExtrapolationMethod.Circular:
                position = CircularPosition(oldest.GlobalPosition, previous.GlobalPosition, last.GlobalPosition) ?? position;
                break;
        }

        return IsFinite(position) && IsFinite(rotation) && IsFinite(scale) ? (position, rotation, scale) : linear;
    }

    /// <summary>
    /// Continues the position series with the last step multiplied by the capped ratio of the last two steps.
    /// </summary>
    /// <returns>The next position, or null when the first step is too small for a ratio</returns>
    private static Vector3? ExponentialPosition(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 first = p1 - p0;
        Vector3 second = p2 - p1;
        if (first.LengthSquared() <= MinStepSquared)
            return null;

        float ratio = Mathf.Min(second.Length() / first.Length(), MaxRatio);
        return p2 + second * ratio;
    }

    /// <summary>
    /// Continues one scale component as <c>last * (last / previous)</c>; non-positive values use <paramref name="linear"/>.
    /// </summary>
    private static float ExponentialComponent(float previous, float last, float linear) =>
        previous <= MinScale || last <= MinScale ? linear : last * (last / previous);

    /// <summary>
    /// Continues the position on the circle through three points by the angle of the last step.
    /// </summary>
    /// <returns>The next position, or null when the points are (nearly) collinear or coincident</returns>
    private static Vector3? CircularPosition(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector3 a = p0 - p2;
        Vector3 b = p1 - p2;
        Vector3 c = a.Cross(b);
        float cSquared = c.LengthSquared();
        if (cSquared <= CollinearEpsilon * a.LengthSquared() * b.LengthSquared())
            return null;

        Vector3 center = p2 + (a.LengthSquared() * b - b.LengthSquared() * a).Cross(c) / (2f * cSquared);
        Vector3 u = p1 - center;
        Vector3 v = p2 - center;
        Vector3 normal = u.Cross(v);
        if (normal.LengthSquared() <= 0f)
            return null;

        float angle = Mathf.Atan2(normal.Length(), u.Dot(v));
        return center + v.Rotated(normal.Normalized(), angle);
    }

    private static bool IsFinite(Vector3 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && float.IsFinite(v.Z);
}
