using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Pure pose math for the next <c>Node2D</c> placement. The 3D twin is <see cref="Extrapolator3D"/>.
/// </summary>
public static class Extrapolator2D
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
    /// Selects the nodes <see cref="Pose"/> extrapolates from: the newest valid, in-tree <c>Node2D</c> entries of
    /// <paramref name="placed"/>, at most 2 for Linear and 3 for the other methods.
    /// </summary>
    /// <param name="method">Extrapolation method that decides how many nodes are needed</param>
    /// <param name="placed">Earlier placements, oldest first (not modified)</param>
    /// <returns>The selected nodes, oldest first; shorter than needed when fewer survive</returns>
    public static Node2D[] Chain(ExtrapolationMethod method, List<Node> placed)
    {
        int need = NeededCount(method);
        List<Node2D> newestFirst = new(need);
        for (int i = placed.Count - 1; i >= 0 && newestFirst.Count < need; i--)
        {
            if (GodotObject.IsInstanceValid(placed[i]) && placed[i].IsInsideTree() && placed[i] is Node2D node)
                newestFirst.Add(node);
        }

        newestFirst.Reverse();
        return newestFirst.ToArray();
    }

    private static int NeededCount(ExtrapolationMethod method) => method == ExtrapolationMethod.Linear ? 2 : 3;

    /// <summary>
    /// Computes the pose of the next placement from the surviving 2D entries of <paramref name="placed"/>.
    /// </summary>
    /// <param name="method">Extrapolation method; needs 2 (Linear) or 3 (others) nodes, fewer degrade as described on <see cref="ExtrapolationMethod"/></param>
    /// <param name="parent">Parent of the new node; a non-<c>Node2D</c> parent contributes a zero pose</param>
    /// <param name="placed">Earlier placements, oldest first (not modified); freed, undone or foreign-tree entries are skipped</param>
    /// <returns>
    /// Global position and rotation, and the local scale to apply. The scale is <c>null</c> when there is no earlier
    /// placement, so the placed scene keeps its authored scale. A result with a non-finite component is replaced by the Linear one.
    /// </returns>
    public static (Vector2 position, float rotation, Vector2? scale) Pose(ExtrapolationMethod method, Node parent, List<Node> placed)
    {
        int need = NeededCount(method);
        Node2D[] recent = Chain(method, placed);

        switch (recent.Length)
        {
            case 0:
                return parent is Node2D p ? (p.GlobalPosition, p.GlobalRotation, null) : (Vector2.Zero, 0f, null);
            case 1:
                return (recent[0].GlobalPosition, recent[0].GlobalRotation, recent[0].Scale);
        }

        Node2D last = recent[^1];
        Node2D previous = recent[^2];
        Vector2 linearScale = 2f * last.Scale - previous.Scale;
        (Vector2 position, float rotation, Vector2? scale) linear = (
            2f * last.GlobalPosition - previous.GlobalPosition,
            2f * last.GlobalRotation - previous.GlobalRotation,
            linearScale);

        if (recent.Length < need)
            return linear;

        Node2D oldest = recent[0];
        Vector2 position = linear.position;
        float rotation = linear.rotation;
        Vector2 scale = linearScale;
        switch (method)
        {
            case ExtrapolationMethod.Quadratic:
                position = 3f * last.GlobalPosition - 3f * previous.GlobalPosition + oldest.GlobalPosition;
                rotation = 3f * last.GlobalRotation - 3f * previous.GlobalRotation + oldest.GlobalRotation;
                scale = 3f * last.Scale - 3f * previous.Scale + oldest.Scale;
                break;
            case ExtrapolationMethod.Exponential:
                position = ExponentialPosition(oldest.GlobalPosition, previous.GlobalPosition, last.GlobalPosition) ?? position;
                scale = new Vector2(
                    ExponentialComponent(previous.Scale.X, last.Scale.X, linearScale.X),
                    ExponentialComponent(previous.Scale.Y, last.Scale.Y, linearScale.Y));
                break;
            case ExtrapolationMethod.Circular:
                position = CircularPosition(oldest.GlobalPosition, previous.GlobalPosition, last.GlobalPosition) ?? position;
                break;
        }

        return IsFinite(position) && float.IsFinite(rotation) && IsFinite(scale) ? (position, rotation, scale) : linear;
    }

    /// <summary>
    /// Continues the position series with the last step multiplied by the capped ratio of the last two steps.
    /// </summary>
    /// <returns>The next position, or null when the first step is too small for a ratio</returns>
    private static Vector2? ExponentialPosition(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 first = p1 - p0;
        Vector2 second = p2 - p1;
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
    /// Continues the position on the circle through three points by the signed angle of the last step.
    /// </summary>
    /// <returns>The next position, or null when the points are (nearly) collinear or coincident</returns>
    private static Vector2? CircularPosition(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 a = p0 - p2;
        Vector2 b = p1 - p2;
        float cross = a.X * b.Y - a.Y * b.X;
        if (cross * cross <= CollinearEpsilon * a.LengthSquared() * b.LengthSquared())
            return null;

        Vector2 w = a.LengthSquared() * b - b.LengthSquared() * a;
        Vector2 center = p2 + new Vector2(w.Y, -w.X) / (2f * cross);
        Vector2 u = p1 - center;
        Vector2 v = p2 - center;
        return center + v.Rotated(u.AngleTo(v));
    }

    private static bool IsFinite(Vector2 v) => float.IsFinite(v.X) && float.IsFinite(v.Y);
}
