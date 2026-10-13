namespace Addons.ScenePaletter.Tools;

/// <summary>
/// How <c>PlacingPage</c> continues the series of already placed nodes when it computes the pose of the next one.
/// </summary>
/// <remarks>
/// The numeric values equal the item order of the Method <c>OptionButton</c> in <c>PlacingPage.tscn</c>.
/// <c>Linear</c> needs the last two placed nodes, every other method the last three; with fewer nodes the placer
/// degrades to Linear, then to a copy of the single node, then to the parent's pose.
/// </remarks>
public enum ExtrapolationMethod
{
    /// <summary>Constant step: <c>2 * last - previous</c> for position, rotation and scale.</summary>
    Linear = 0,

    /// <summary>Constant second difference: <c>3 * last - 3 * previous + oldest</c> for position, rotation and scale.</summary>
    Quadratic = 1,

    /// <summary>The position step is multiplied by the (capped) ratio of the last two steps; scale grows by the ratio of the last two values; rotation is linear.</summary>
    Exponential = 2,

    /// <summary>The position continues on the circle through the last three points by the last step angle; rotation and scale are linear.</summary>
    Circular = 3,
}
