using System.Collections.Generic;
using Addons.ScenePaletter.Tools;
using Godot;

namespace Addons.ScenePaletter.Preview;

/// <summary>
/// Editor viewport overlay that shows the nodes the placer extrapolates from and the position of the next placement.
/// The 2D/3D subclasses compute world points from <c>Extrapolator*.Chain</c>/<c>Pose</c> and project them;
/// this class holds the bound list, the look and the shared screen-space drawing.
/// </summary>
/// <remarks>
/// Owned by <see cref="Plugin"/>, which polls <see cref="Refresh"/> and calls <see cref="Draw"/> from the editor's
/// force-draw overlay hooks. The preview writes no scene state.
/// </remarks>
public abstract class PlacementPreview
{
    /// <summary>Colour of the chain lines and dots.</summary>
    protected static readonly Color ChainColor = new(1.0f, 0.35f, 0.9f);

    /// <summary>Colour of the prediction ring and its dashed line.</summary>
    protected static readonly Color NextColor = new(0.55f, 1.0f, 0.3f);

    private const float LineWidth = 2f;
    private const float DotRadius = 4f;
    private const float RingRadius = 8f;
    private const float DashLength = 6f;
    private const int RingPoints = 32;

    /// <summary>Settings of the owning plugin; read live for the extrapolation method.</summary>
    protected readonly PaletterSettings settings;

    /// <summary>The placer's <c>PlacingPageData.placed</c> list (shared reference); null while unbound.</summary>
    protected List<Node> placed;

    /// <summary>
    /// Creates an unbound preview.
    /// </summary>
    /// <param name="settings">Settings whose <c>Method</c> is read on every refresh</param>
    protected PlacementPreview(PaletterSettings settings)
    {
        this.settings = settings;
    }

    /// <summary>
    /// Starts previewing the given placement list.
    /// </summary>
    /// <param name="placed">The page's list of placed nodes, not copied</param>
    public void Bind(List<Node> placed)
    {
        this.placed = placed;
        Clear();
    }

    /// <summary>
    /// Stops previewing and forgets all cached points.
    /// </summary>
    public void Unbind()
    {
        placed = null;
        Clear();
    }

    /// <summary>
    /// Recomputes the cached world points from the current scene state.
    /// </summary>
    /// <returns>True if the points changed since the last call, so the overlay must be redrawn</returns>
    public abstract bool Refresh();

    /// <summary>
    /// Draws the cached points into the editor viewport overlay.
    /// </summary>
    /// <param name="overlay">Overlay control passed to the force-draw hook; drawing is only valid during that call</param>
    public abstract void Draw(Control overlay);

    /// <summary>Drops the cached points.</summary>
    protected abstract void Clear();

    /// <summary>
    /// Draws chain lines, chain dots, the dashed line to the prediction and the prediction ring.
    /// </summary>
    /// <param name="overlay">Overlay control to draw on</param>
    /// <param name="chain">Chain points in overlay pixels, oldest first; null entries (behind the camera) are skipped</param>
    /// <param name="next">Predicted point in overlay pixels, or null if not visible</param>
    protected static void DrawScreen(Control overlay, Vector2?[] chain, Vector2? next)
    {
        for (int i = 1; i < chain.Length; i++)
        {
            if (chain[i - 1] is Vector2 from && chain[i] is Vector2 to)
                overlay.DrawLine(from, to, ChainColor, LineWidth);
        }

        foreach (Vector2? point in chain)
        {
            if (point is Vector2 p)
                overlay.DrawCircle(p, DotRadius, ChainColor);
        }

        if (next is not Vector2 target)
            return;

        if (chain.Length > 0 && chain[^1] is Vector2 last)
            overlay.DrawDashedLine(last, target, NextColor, LineWidth, DashLength);
        overlay.DrawArc(target, RingRadius, 0f, Mathf.Tau, RingPoints, NextColor, LineWidth);
    }
}
