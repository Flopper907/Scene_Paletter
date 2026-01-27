using System;
using Godot;

namespace Addons.ScenePaletter.Core;

public static partial class ExceptionHandler
{
    // ===================================================================
    // CORE FRAMEWORK EXCEPTIONS
    // ===================================================================

    public static void ThrowMissingPluginException(string context = "")
    {
        LogError("Plugin is missing!", context);
    }

    public static void ThrowMissingConfigException(string context = "")
    {
        LogError("Config is missing!", context);
    }

    public static void ThrowMissingSceneLoaderException(string context = "")
    {
        LogError("SceneLoader is missing!", context);
    }

    public static void ThrowMissingDockManagerException(string context = "")
    {
        LogError("DockManager is missing!", context);
    }

    public static void ThrowMissingPageException(string pageName, string context = "")
    {
        LogError($"Page '{pageName}' is missing!", context);
    }

    public static void ThrowNotAPageException(string pageName, string context = "")
    {
        LogError($"Scene '{pageName}' has no script inheriting Page<T> attached!", context);
    }

    public static void ThrowMissingWidgetException(string widgetName, string context = "")
    {
        LogError($"Widget '{widgetName}' is missing!", context);
    }

    public static void ThrowInvalidPageDataException(string pageName, string expectedType, string actualType = "null")
    {
        LogError($"Page '{pageName}' received invalid data. Expected: {expectedType}, Got: {actualType}");
    }

    // ===================================================================
    // FILE/RESOURCE EXCEPTIONS
    // ===================================================================

    public static void ThrowFileNotFoundException(string filePath, string context = "")
    {
        LogError($"File not found: '{filePath}'", context);
    }

    public static void ThrowFolderNotFoundException(string folderPath, string context = "")
    {
        LogError($"Folder not found: '{folderPath}'", context);
    }

    public static void ThrowResourceLoadException(string resourcePath, string context = "")
    {
        LogError($"Failed to load resource: '{resourcePath}'", context);
    }

    public static void ThrowInvalidResourceTypeException(string resourcePath, string expectedType, string actualType = "")
    {
        LogError($"Resource '{resourcePath}' has wrong type. Expected: {expectedType}, Got: {actualType}");
    }

    public static void ThrowSceneInstantiationException(string scenePath, string context = "")
    {
        LogError($"Failed to instantiate scene: '{scenePath}'", context);
    }

    // ===================================================================
    // CONFIG EXCEPTIONS
    // ===================================================================

    public static void ThrowConfigLoadException(string configPath, string context = "")
    {
        LogError($"Failed to load config file: '{configPath}'", context);
    }

    public static void ThrowMissingConfigKeyException(string section, string key, string context = "")
    {
        LogError($"Config missing required key: [{section}]/{key}", context);
    }

    public static void ThrowInvalidConfigValueException(string section, string key, string expectedType, string context = "")
    {
        LogError($"Invalid config value for [{section}]/{key}. Expected type: {expectedType}", context);
    }

    public static void ThrowConfigParseException(string configPath, string parseError, string context = "")
    {
        LogError($"Failed to parse config '{configPath}': {parseError}", context);
    }

    // ===================================================================
    // DOCK/UI EXCEPTIONS
    // ===================================================================

    public static void ThrowDockAlreadyExistsException(UIPosition position, string context = "")
    {
        LogError($"Dock already exists at position: {position}", context);
    }

    public static void ThrowDockNotFoundException(UIPosition position, string context = "")
    {
        LogError($"No dock found at position: {position}", context);
    }

    public static void ThrowInvalidUIPositionException(string positionName, string context = "")
    {
        LogError($"Invalid UIPosition: '{positionName}'", context);
    }

    public static void ThrowDockOperationException(UIPosition position, string operation, string context = "")
    {
        LogError($"Failed to {operation} dock at position: {position}", context);
    }

    public static void ThrowMissingNodeException(string nodePath, string parentContext = "")
    {
        LogError($"Node not found at path: '{nodePath}'", parentContext);
    }

    public static void ThrowMissingDockParentException(string nodePath, string parentContext = "")
    {
        LogError($"Dock Parent not found at path: '{nodePath}'", parentContext);
    }

    public static void ThrowInvalidNodeTypeException(string nodePath, string expectedType, string actualType = "")
    {
        LogError($"Node '{nodePath}' has wrong type. Expected: {expectedType}, Got: {actualType}");
    }

    // ===================================================================
    // DATA/SERIALIZATION EXCEPTIONS
    // ===================================================================

    public static void ThrowSerializationException(string dataType, string context = "")
    {
        LogError($"Failed to serialize data of type: {dataType}", context);
    }

    public static void ThrowDeserializationException(string dataType, string filePath, string context = "")
    {
        LogError($"Failed to deserialize {dataType} from: '{filePath}'", context);
    }

    public static void ThrowInvalidDataFormatException(string filePath, string expectedFormat, string context = "")
    {
        LogError($"Invalid data format in '{filePath}'. Expected: {expectedFormat}", context);
    }

    public static void ThrowDataValidationException(string dataType, string validationError, string context = "")
    {
        LogError($"Data validation failed for {dataType}: {validationError}", context);
    }

    // ===================================================================
    // PALETTE-SPECIFIC EXCEPTIONS (Your Plugin)
    // ===================================================================

    public static void ThrowPaletteNotFoundException(string paletteId, string context = "")
    {
        LogError($"Palette not found: '{paletteId}'", context);
    }

    public static void ThrowMissingPaletteException(string paletteId, string context = "")
    {
        LogError($"Palette is missing or null: '{paletteId}'", context);
    }

    public static void ThrowPaletteSaveException(string paletteId, string context = "")
    {
        LogError($"Failed to save palette: '{paletteId}'", context);
    }

    public static void ThrowPaletteLoadException(string paletteId, string context = "")
    {
        LogError($"Failed to load palette: '{paletteId}'", context);
    }

    public static void ThrowPaletteDeleteException(string paletteId, string context = "")
    {
        LogError($"Failed to delete palette: '{paletteId}'", context);
    }

    public static void ThrowInvalidPalettePositionException(int position, string context = "")
    {
        LogError($"Invalid palette position: {position}", context);
    }

    public static void ThrowDuplicatePaletteException(string paletteId, string context = "")
    {
        LogError($"Palette with ID '{paletteId}' already exists", context);
    }

    public static void ThrowInvalidScenePathException(string scenePath, string paletteId = "", string context = "")
    {
        string paletteInfo = !string.IsNullOrEmpty(paletteId) ? $" in palette '{paletteId}'" : "";
        LogError($"Invalid scene path: '{scenePath}'{paletteInfo}", context);
    }

    // ===================================================================
    // PREVIEW GENERATION EXCEPTIONS
    // ===================================================================

    public static void ThrowPreviewGenerationException(string scenePath, string context = "")
    {
        LogError($"Failed to generate preview for scene: '{scenePath}'", context);
    }

    public static void ThrowInvalidPreviewSettingsException(string setting, string context = "")
    {
        LogError($"Invalid preview setting: {setting}", context);
    }

    // ===================================================================
    // GENERAL EXCEPTIONS
    // ===================================================================

    public static void ThrowNullReferenceException(string variableName, string context = "")
    {
        LogError($"Null reference: '{variableName}'", context);
    }

    public static void ThrowInvalidOperationException(string operation, string reason, string context = "")
    {
        LogError($"Invalid operation '{operation}': {reason}", context);
    }

    public static void ThrowUnexpectedException(Exception ex, string context = "")
    {
        LogError($"Unexpected exception: {ex.GetType().Name} - {ex.Message}", context);
        if (!string.IsNullOrEmpty(ex.StackTrace))
        {
            GD.PrintErr($"Stack trace:\n{ex.StackTrace}");
        }
    }

    public static void ThrowNotImplementedException(string feature, string context = "")
    {
        LogError($"Feature not implemented: {feature}", context);
    }

    // ===================================================================
    // WARNINGS (Non-critical issues)
    // ===================================================================

    public static void LogWarning(string message, string context = "")
    {
        string fullMessage = string.IsNullOrEmpty(context)
            ? $"[WARNING] {message}"
            : $"[WARNING] {message} (Context: {context})";
        GD.PushWarning(fullMessage);
    }

    public static void WarnDeprecatedFeature(string feature, string alternative = "")
    {
        string message = $"Feature '{feature}' is deprecated.";
        if (!string.IsNullOrEmpty(alternative))
        {
            message += $" Use '{alternative}' instead.";
        }
        LogWarning(message);
    }

    public static void WarnMissingOptionalConfig(string section, string key)
    {
        LogWarning($"Optional config key missing: [{section}]/{key}. Using default value.");
    }

    // ===================================================================
    // HELPER METHODS
    // ===================================================================

    private static void LogError(string message, string context = "")
    {
        string fullMessage = string.IsNullOrEmpty(context)
            ? $"{message}"
            : $"{message} (Context: {context})";
        GD.PushError(fullMessage);
    }

    public static void SafeExecute(Action action, string operationName, string context = "")
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception ex)
        {
            ThrowUnexpectedException(ex, $"{operationName} - {context}");
        }
    }

    public static T SafeExecute<T>(Func<T> func, T defaultValue, string operationName, string context = "")
    {
        try
        {
            return func != null ? func() : defaultValue;
        }
        catch (Exception ex)
        {
            ThrowUnexpectedException(ex, $"{operationName} - {context}");
            return defaultValue;
        }
    }
}