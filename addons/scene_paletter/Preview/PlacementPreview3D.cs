using System.Linq;
using Addons.ScenePaletter.Tools;
using Godot;

namespace Addons.ScenePaletter.Preview;

/// <summary>
/// 3D placement preview: projects the chain and the predicted position through the editor camera of the viewport being drawn.
/// </summary>
public class PlacementPreview3D : PlacementPreview
{
    private Vector3[] chain = [];
    private Vector3? next;

    /// <summary>
    /// Creates an unbound 3D preview.
    /// </summary>
    /// <param name="settings">Settings whose <c>Method</c> is read on every refresh</param>
    public PlacementPreview3D(PaletterSettings settings) : base(settings) { }

    /// <inheritdoc/>
    /// <remarks>
    /// Uses the same parent resolution and <c>Extrapolator3D.Pose</c> as the placer; caches nothing but world points.
    /// Hides (empty cache) when there is no edited scene or the placer would reject a 3D node under the parent.
    /// </remarks>
    public override bool Refresh()
    {
        Vector3[] newChain = [];
        Vector3? newNext = null;

        Node root = EditorInterface.Singleton?.GetEditedSceneRoot();
        if (placed != null && settings != null && root != null)
        {
            Node parent = PlacementTarget.Parent(root, PlacementTarget.Selected());
            if (PlacementTarget.Accepts(SpatialKind.Node3D, parent))
            {
                newChain = Extrapolator3D.Chain(settings.Method, placed).Select(n => n.GlobalPosition).ToArray();
                newNext = Extrapolator3D.Pose(settings.Method, parent, placed).position;
            }
        }

        if (newNext == next && newChain.SequenceEqual(chain))
            return false;

        chain = newChain;
        next = newNext;
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Segments and points behind the camera are skipped. Nothing is drawn if no editor camera matches the overlay.
    /// </remarks>
    public override void Draw(Control overlay)
    {
        if (next == null)
            return;

        Camera3D camera = CameraFor(overlay);
        if (camera == null)
            return;

        DrawScreen(overlay, chain.Select(p => Project(camera, p)).ToArray(), Project(camera, next.Value));
    }

    /// <inheritdoc/>
    protected override void Clear()
    {
        chain = [];
        next = null;
    }

    private static Vector2? Project(Camera3D camera, Vector3 world) =>
        camera.IsPositionBehind(world) ? null : camera.UnprojectPosition(world);

    /// <summary>
    /// Finds the editor camera whose 3D viewport the overlay belongs to: the viewport sharing the deepest ancestor with the overlay.
    /// </summary>
    /// <returns>The camera, or null if none of the four editor viewports is related to <paramref name="overlay"/></returns>
    private static Camera3D CameraFor(Control overlay)
    {
        Camera3D best = null;
        int bestDepth = -1;
        for (int i = 0; i < 4; i++)
        {
            SubViewport viewport = EditorInterface.Singleton.GetEditorViewport3D(i);
            if (viewport == null)
                continue;

            Node shared = viewport.GetParent();
            while (shared != null && !shared.IsAncestorOf(overlay))
                shared = shared.GetParent();
            if (shared == null)
                continue;

            int depth = 0;
            for (Node n = shared; n != null; n = n.GetParent())
                depth++;

            if (depth > bestDepth)
            {
                bestDepth = depth;
                best = viewport.GetCamera3D();
            }
        }

        return best;
    }
}
