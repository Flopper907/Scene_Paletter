using System.Linq;
using Addons.ScenePaletter.Tools;
using Godot;

namespace Addons.ScenePaletter.Preview;

/// <summary>
/// 2D placement preview: maps the chain and the predicted position through the 2D editor's canvas transform (pan and zoom).
/// </summary>
public class PlacementPreview2D : PlacementPreview
{
    private Vector2[] chain = [];
    private Vector2? next;

    /// <summary>
    /// Creates an unbound 2D preview.
    /// </summary>
    /// <param name="settings">Settings whose <c>Method</c> is read on every refresh</param>
    public PlacementPreview2D(PaletterSettings settings) : base(settings) { }

    /// <inheritdoc/>
    /// <remarks>
    /// Uses the same parent resolution and <c>Extrapolator2D.Pose</c> as the placer; caches nothing but world points.
    /// Hides (empty cache) when there is no edited scene or the placer would reject a 2D node under the parent.
    /// </remarks>
    public override bool Refresh()
    {
        Vector2[] newChain = [];
        Vector2? newNext = null;

        Node root = EditorInterface.Singleton?.GetEditedSceneRoot();
        if (placed != null && settings != null && root != null)
        {
            Node parent = PlacementTarget.Parent(root, PlacementTarget.Selected());
            if (PlacementTarget.Accepts(SpatialKind.Node2D, parent))
            {
                newChain = Extrapolator2D.Chain(settings.Method, placed).Select(n => n.GlobalPosition).ToArray();
                newNext = Extrapolator2D.Pose(settings.Method, parent, placed).position;
            }
        }

        if (newNext == next && newChain.SequenceEqual(chain))
            return false;

        chain = newChain;
        next = newNext;
        return true;
    }

    /// <inheritdoc/>
    public override void Draw(Control overlay)
    {
        if (next == null)
            return;

        Transform2D toOverlay = EditorInterface.Singleton.GetEditorViewport2D().GlobalCanvasTransform;
        DrawScreen(overlay, chain.Select(p => (Vector2?)(toOverlay * p)).ToArray(), toOverlay * next.Value);
    }

    /// <inheritdoc/>
    protected override void Clear()
    {
        chain = [];
        next = null;
    }
}
