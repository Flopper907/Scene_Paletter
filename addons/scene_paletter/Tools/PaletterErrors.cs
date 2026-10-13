using Godot;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Error logging that only makes sense for Scene Paletter (palettes, previews, scene paths).
/// Generic errors live in <c>Addons.PluginFramework.Core.ExceptionHandler</c>.
/// </summary>
/// <remarks>
/// Like <c>ExceptionHandler</c> these methods do not throw; they log via <c>GD.PushError</c>.
/// </remarks>
public static class PaletterErrors
{
    /// <summary>Logs an error when a palette is missing or null.</summary>
    /// <param name="paletteId">ID of the missing palette</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowMissingPaletteException(string paletteId, string context = "")
    {
        LogError($"Palette is missing or null: '{paletteId}'", context);
    }

    /// <summary>Logs an error when an invalid palette position is specified.</summary>
    /// <param name="position">Invalid position value</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidPalettePositionException(int position, string context = "")
    {
        LogError($"Invalid palette position: {position}", context);
    }

    /// <summary>Logs an error when a scene path is invalid.</summary>
    /// <param name="scenePath">Invalid scene path</param>
    /// <param name="paletteId">ID of the palette containing the invalid path (optional)</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidScenePathException(string scenePath, string paletteId = "", string context = "")
    {
        string paletteInfo = !string.IsNullOrEmpty(paletteId) ? $" in palette '{paletteId}'" : "";
        LogError($"Invalid scene path: '{scenePath}'{paletteInfo}", context);
    }

    /// <summary>Logs an error when data validation fails.</summary>
    /// <param name="dataType">Type of data being validated</param>
    /// <param name="validationError">Description of the validation error</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowDataValidationException(string dataType, string validationError, string context = "")
    {
        LogError($"Data validation failed for {dataType}: {validationError}", context);
    }

    /// <summary>Logs an error when a node has an incorrect type.</summary>
    /// <param name="nodePath">Path to the node</param>
    /// <param name="expectedType">Expected node type</param>
    /// <param name="actualType">Actual node type</param>
    public static void ThrowInvalidNodeTypeException(string nodePath, string expectedType, string actualType = "")
    {
        LogError($"Node '{nodePath}' has wrong type. Expected: {expectedType}, Got: {actualType}");
    }

    /// <summary>Logs an error when preview generation fails for a scene.</summary>
    /// <param name="scenePath">Path to the scene for which preview generation failed</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowPreviewGenerationException(string scenePath, string context = "")
    {
        LogError($"Failed to generate preview for scene: '{scenePath}'", context);
    }

    /// <summary>Logs an error when a preview setting has an invalid value.</summary>
    /// <param name="setting">Name of the invalid setting</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidPreviewSettingsException(string setting, string context = "")
    {
        LogError($"Invalid preview setting: {setting}", context);
    }

    private static void LogError(string message, string context = "")
    {
        GD.PushError(string.IsNullOrEmpty(context) ? message : $"{message} (Context: {context})");
    }
}
