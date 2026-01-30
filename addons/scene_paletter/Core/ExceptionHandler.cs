using System;
using Godot;

namespace Addons.ScenePaletter.Core;

/// <summary>
/// Provides centralized error logging and exception handling for the plugin.
/// Offers a comprehensive set of typed exception methods for consistent error reporting
/// across all plugin components.
/// </summary>
/// <remarks>
/// <para>
/// <c>ExceptionHandler</c> does not throw actual exceptions—instead, it logs errors
/// via Godot's <c>GD.PushError</c> and <c>GD.PushWarning</c> methods. This approach
/// prevents plugin crashes while maintaining detailed error tracking in the editor console.
/// </para>
/// 
/// <para>
/// All exception methods follow a consistent pattern:
/// <list type="bullet">
///   <item>Accept a descriptive error message or identifier</item>
///   <item>Accept an optional <c>context</c> parameter for additional debugging information</item>
///   <item>Log formatted error messages to the Godot console</item>
///   <item>Return control to the caller (non-throwing)</item>
/// </list>
/// </para>
/// 
/// <para>
/// Exception categories:
/// <list type="bullet">
///   <item><strong>Core Framework:</strong> Plugin, config, scene loader, dock manager</item>
///   <item><strong>File/Resource:</strong> File loading, resource types, scene instantiation</item>
///   <item><strong>Config:</strong> Config loading, missing keys, type validation</item>
///   <item><strong>Dock/UI:</strong> Dock operations, UI positions, node references</item>
///   <item><strong>Data/Serialization:</strong> Data validation, format errors</item>
///   <item><strong>Palette-Specific:</strong> Palette operations, scene paths</item>
///   <item><strong>Preview Generation:</strong> Preview creation, settings validation</item>
///   <item><strong>General:</strong> Null references, unexpected exceptions</item>
/// </list>
/// </para>
/// 
/// <para>
/// Helper methods <c>SafeExecute</c> and <c>SafeExecute&lt;T&gt;</c> provide automatic
/// exception handling for operations that might fail, wrapping them in try-catch blocks
/// and logging any exceptions via <c>ThrowUnexpectedException</c>.
/// </para>
/// </remarks>
public static partial class ExceptionHandler
{
    // ===================================================================
    // CORE FRAMEWORK EXCEPTIONS
    // ===================================================================

    /// <summary>
    /// Logs an error when the <c>Plugin</c> instance is missing or null.
    /// </summary>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowMissingPluginException(string context = "")
    {
        LogError("Plugin is missing!", context);
    }

    /// <summary>
    /// Logs an error when the <c>ConfigLoader</c> instance is missing or null.
    /// </summary>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowMissingConfigException(string context = "")
    {
        LogError("Config is missing!", context);
    }

    /// <summary>
    /// Logs an error when the <c>SceneLoader</c> instance is missing or null.
    /// </summary>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowMissingSceneLoaderException(string context = "")
    {
        LogError("SceneLoader is missing!", context);
    }

    /// <summary>
    /// Logs an error when the <c>DockManager</c> instance is missing or null.
    /// </summary>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowMissingDockManagerException(string context = "")
    {
        LogError("DockManager is missing!", context);
    }

    /// <summary>
    /// Logs an error when a requested page is not found in <c>SceneLoader</c>.
    /// </summary>
    /// <param name="pageName">Name of the missing page</param>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowMissingPageException(string pageName, string context = "")
    {
        LogError($"Page '{pageName}' is missing!", context);
    }

    /// <summary>
    /// Logs an error when a scene does not have a script inheriting from <c>Page&lt;T&gt;</c>.
    /// </summary>
    /// <param name="pageName">Name of the scene</param>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowNotAPageException(string pageName, string context = "")
    {
        LogError($"Scene '{pageName}' has no script inheriting Page<T> attached!", context);
    }

    /// <summary>
    /// Logs an error when a requested widget is not found in <c>SceneLoader</c>.
    /// </summary>
    /// <param name="widgetName">Name of the missing widget</param>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowMissingWidgetException(string widgetName, string context = "")
    {
        LogError($"Widget '{widgetName}' is missing!", context);
    }

    /// <summary>
    /// Logs an error when a page receives data of an incorrect type.
    /// </summary>
    /// <param name="pageName">Name of the page</param>
    /// <param name="expectedType">Expected data type</param>
    /// <param name="actualType">Actual data type received (or "null" if no data)</param>
    public static void ThrowInvalidPageDataException(string pageName, string expectedType, string actualType = "null")
    {
        LogError($"Page '{pageName}' received invalid data. Expected: {expectedType}, Got: {actualType}");
    }

    // ===================================================================
    // FILE/RESOURCE EXCEPTIONS
    // ===================================================================

    /// <summary>
    /// Logs an error when a file cannot be found at the specified path.
    /// </summary>
    /// <param name="filePath">Path to the missing file</param>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowFileNotFoundException(string filePath, string context = "")
    {
        LogError($"File not found: '{filePath}'", context);
    }

    /// <summary>
    /// Logs an error when a folder cannot be found at the specified path.
    /// </summary>
    /// <param name="folderPath">Path to the missing folder</param>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowFolderNotFoundException(string folderPath, string context = "")
    {
        LogError($"Folder not found: '{folderPath}'", context);
    }

    /// <summary>
    /// Logs an error when a resource fails to load from the specified path.
    /// </summary>
    /// <param name="resourcePath">Path to the resource that failed to load</param>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowResourceLoadException(string resourcePath, string context = "")
    {
        LogError($"Failed to load resource: '{resourcePath}'", context);
    }

    /// <summary>
    /// Logs an error when a resource has an incorrect type.
    /// </summary>
    /// <param name="resourcePath">Path to the resource</param>
    /// <param name="expectedType">Expected resource type</param>
    /// <param name="actualType">Actual resource type</param>
    public static void ThrowInvalidResourceTypeException(string resourcePath, string expectedType, string actualType = "")
    {
        LogError($"Resource '{resourcePath}' has wrong type. Expected: {expectedType}, Got: {actualType}");
    }

    /// <summary>
    /// Logs an error when a scene fails to instantiate.
    /// </summary>
    /// <param name="scenePath">Path to the scene that failed to instantiate</param>
    /// <param name="context">Additional context for debugging (e.g., method name)</param>
    public static void ThrowSceneInstantiationException(string scenePath, string context = "")
    {
        LogError($"Failed to instantiate scene: '{scenePath}'", context);
    }

    // ===================================================================
    // CONFIG EXCEPTIONS
    // ===================================================================

    /// <summary>
    /// Logs an error when a config file fails to load.
    /// </summary>
    /// <param name="configPath">Path to the config file</param>
    /// <param name="context">Additional context (e.g., error code or reason)</param>
    public static void ThrowConfigLoadException(string configPath, string context = "")
    {
        LogError($"Failed to load config file: '{configPath}'", context);
    }

    /// <summary>
    /// Logs an error when a required config key is missing.
    /// </summary>
    /// <param name="section">Config section name</param>
    /// <param name="key">Missing key name</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowMissingConfigKeyException(string section, string key, string context = "")
    {
        LogError($"Config missing required key: [{section}]/{key}", context);
    }

    /// <summary>
    /// Logs an error when a config value has an invalid type.
    /// </summary>
    /// <param name="section">Config section name</param>
    /// <param name="key">Config key name</param>
    /// <param name="expectedType">Expected value type</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidConfigValueException(string section, string key, string expectedType, string context = "")
    {
        LogError($"Invalid config value for [{section}]/{key}. Expected type: {expectedType}", context);
    }

    /// <summary>
    /// Logs an error when a config file cannot be parsed.
    /// </summary>
    /// <param name="configPath">Path to the config file</param>
    /// <param name="parseError">Description of the parsing error</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowConfigParseException(string configPath, string parseError, string context = "")
    {
        LogError($"Failed to parse config '{configPath}': {parseError}", context);
    }

    // ===================================================================
    // DOCK/UI EXCEPTIONS
    // ===================================================================

    /// <summary>
    /// Logs an error when attempting to create a dock at a position that already has a dock.
    /// </summary>
    /// <param name="position">UI position where a dock already exists</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowDockAlreadyExistsException(UIPosition position, string context = "")
    {
        LogError($"Dock already exists at position: {position}", context);
    }

    /// <summary>
    /// Logs an error when no dock is found at the specified position.
    /// </summary>
    /// <param name="position">UI position where no dock was found</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowDockNotFoundException(UIPosition position, string context = "")
    {
        LogError($"No dock found at position: {position}", context);
    }

    /// <summary>
    /// Logs an error when a UI position name cannot be parsed.
    /// </summary>
    /// <param name="positionName">Invalid position name</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidUIPositionException(string positionName, string context = "")
    {
        LogError($"Invalid UIPosition: '{positionName}'", context);
    }

    /// <summary>
    /// Logs an error when a dock operation fails.
    /// </summary>
    /// <param name="position">UI position where the operation failed</param>
    /// <param name="operation">Name of the failed operation (e.g., "create", "move", "close")</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowDockOperationException(UIPosition position, string operation, string context = "")
    {
        LogError($"Failed to {operation} dock at position: {position}", context);
    }

    /// <summary>
    /// Logs an error when a node cannot be found at the specified path.
    /// </summary>
    /// <param name="nodePath">Path to the missing node</param>
    /// <param name="parentContext">Context of the parent operation</param>
    public static void ThrowMissingNodeException(string nodePath, string parentContext = "")
    {
        LogError($"Node not found at path: '{nodePath}'", parentContext);
    }

    /// <summary>
    /// Logs an error when a dock's parent node cannot be found.
    /// </summary>
    /// <param name="nodePath">Path to the missing parent node</param>
    /// <param name="parentContext">Context of the parent operation</param>
    public static void ThrowMissingDockParentException(string nodePath, string parentContext = "")
    {
        LogError($"Dock Parent not found at path: '{nodePath}'", parentContext);
    }

    /// <summary>
    /// Logs an error when a node has an incorrect type.
    /// </summary>
    /// <param name="nodePath">Path to the node</param>
    /// <param name="expectedType">Expected node type</param>
    /// <param name="actualType">Actual node type</param>
    public static void ThrowInvalidNodeTypeException(string nodePath, string expectedType, string actualType = "")
    {
        LogError($"Node '{nodePath}' has wrong type. Expected: {expectedType}, Got: {actualType}");
    }

    // ===================================================================
    // DATA/SERIALIZATION EXCEPTIONS
    // ===================================================================

    /// <summary>
    /// Logs an error when data serialization fails.
    /// </summary>
    /// <param name="dataType">Type of data being serialized</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowSerializationException(string dataType, string context = "")
    {
        LogError($"Failed to serialize data of type: {dataType}", context);
    }

    /// <summary>
    /// Logs an error when data deserialization fails.
    /// </summary>
    /// <param name="dataType">Type of data being deserialized</param>
    /// <param name="filePath">Path to the file being deserialized</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowDeserializationException(string dataType, string filePath, string context = "")
    {
        LogError($"Failed to deserialize {dataType} from: '{filePath}'", context);
    }

    /// <summary>
    /// Logs an error when data has an invalid format.
    /// </summary>
    /// <param name="filePath">Path to the file with invalid format</param>
    /// <param name="expectedFormat">Expected data format</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidDataFormatException(string filePath, string expectedFormat, string context = "")
    {
        LogError($"Invalid data format in '{filePath}'. Expected: {expectedFormat}", context);
    }

    /// <summary>
    /// Logs an error when data validation fails.
    /// </summary>
    /// <param name="dataType">Type of data being validated</param>
    /// <param name="validationError">Description of the validation error</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowDataValidationException(string dataType, string validationError, string context = "")
    {
        LogError($"Data validation failed for {dataType}: {validationError}", context);
    }

    // ===================================================================
    // PALETTE-SPECIFIC EXCEPTIONS (Your Plugin)
    // ===================================================================

    /// <summary>
    /// Logs an error when a palette cannot be found.
    /// </summary>
    /// <param name="paletteId">ID of the missing palette</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowPaletteNotFoundException(string paletteId, string context = "")
    {
        LogError($"Palette not found: '{paletteId}'", context);
    }

    /// <summary>
    /// Logs an error when a palette is missing or null.
    /// </summary>
    /// <param name="paletteId">ID of the missing palette</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowMissingPaletteException(string paletteId, string context = "")
    {
        LogError($"Palette is missing or null: '{paletteId}'", context);
    }

    /// <summary>
    /// Logs an error when a palette fails to save.
    /// </summary>
    /// <param name="paletteId">ID of the palette that failed to save</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowPaletteSaveException(string paletteId, string context = "")
    {
        LogError($"Failed to save palette: '{paletteId}'", context);
    }

    /// <summary>
    /// Logs an error when a palette fails to load.
    /// </summary>
    /// <param name="paletteId">ID of the palette that failed to load</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowPaletteLoadException(string paletteId, string context = "")
    {
        LogError($"Failed to load palette: '{paletteId}'", context);
    }

    /// <summary>
    /// Logs an error when a palette fails to delete.
    /// </summary>
    /// <param name="paletteId">ID of the palette that failed to delete</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowPaletteDeleteException(string paletteId, string context = "")
    {
        LogError($"Failed to delete palette: '{paletteId}'", context);
    }

    /// <summary>
    /// Logs an error when an invalid palette position is specified.
    /// </summary>
    /// <param name="position">Invalid position value</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidPalettePositionException(int position, string context = "")
    {
        LogError($"Invalid palette position: {position}", context);
    }

    /// <summary>
    /// Logs an error when attempting to create a palette with a duplicate ID.
    /// </summary>
    /// <param name="paletteId">Duplicate palette ID</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowDuplicatePaletteException(string paletteId, string context = "")
    {
        LogError($"Palette with ID '{paletteId}' already exists", context);
    }

    /// <summary>
    /// Logs an error when a scene path is invalid.
    /// </summary>
    /// <param name="scenePath">Invalid scene path</param>
    /// <param name="paletteId">ID of the palette containing the invalid path (optional)</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidScenePathException(string scenePath, string paletteId = "", string context = "")
    {
        string paletteInfo = !string.IsNullOrEmpty(paletteId) ? $" in palette '{paletteId}'" : "";
        LogError($"Invalid scene path: '{scenePath}'{paletteInfo}", context);
    }

    // ===================================================================
    // PREVIEW GENERATION EXCEPTIONS
    // ===================================================================

    /// <summary>
    /// Logs an error when preview generation fails for a scene.
    /// </summary>
    /// <param name="scenePath">Path to the scene for which preview generation failed</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowPreviewGenerationException(string scenePath, string context = "")
    {
        LogError($"Failed to generate preview for scene: '{scenePath}'", context);
    }

    /// <summary>
    /// Logs an error when a preview setting has an invalid value.
    /// </summary>
    /// <param name="setting">Name of the invalid setting</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidPreviewSettingsException(string setting, string context = "")
    {
        LogError($"Invalid preview setting: {setting}", context);
    }

    // ===================================================================
    // GENERAL EXCEPTIONS
    // ===================================================================

    /// <summary>
    /// Logs an error when a null reference is encountered.
    /// </summary>
    /// <param name="variableName">Name of the null variable</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowNullReferenceException(string variableName, string context = "")
    {
        LogError($"Null reference: '{variableName}'", context);
    }

    /// <summary>
    /// Logs an error when an invalid operation is attempted.
    /// </summary>
    /// <param name="operation">Name of the invalid operation</param>
    /// <param name="reason">Reason why the operation is invalid</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowInvalidOperationException(string operation, string reason, string context = "")
    {
        LogError($"Invalid operation '{operation}': {reason}", context);
    }

    /// <summary>
    /// Logs an unexpected exception with full details including stack trace.
    /// </summary>
    /// <param name="ex">The exception that was caught</param>
    /// <param name="context">Additional context for debugging</param>
    /// <remarks>
    /// <para>
    /// Outputs both the exception type/message and the full stack trace (if available)
    /// to aid in debugging unexpected errors.
    /// </para>
    /// </remarks>
    public static void ThrowUnexpectedException(Exception ex, string context = "")
    {
        LogError($"Unexpected exception: {ex.GetType().Name} - {ex.Message}", context);
        if (!string.IsNullOrEmpty(ex.StackTrace))
        {
            GD.PrintErr($"Stack trace:\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Logs an error when a feature is not yet implemented.
    /// </summary>
    /// <param name="feature">Name of the unimplemented feature</param>
    /// <param name="context">Additional context for debugging</param>
    public static void ThrowNotImplementedException(string feature, string context = "")
    {
        LogError($"Feature not implemented: {feature}", context);
    }

    // ===================================================================
    // WARNINGS (Non-critical issues)
    // ===================================================================

    /// <summary>
    /// Logs a non-critical warning message.
    /// </summary>
    /// <param name="message">Warning message</param>
    /// <param name="context">Additional context for debugging</param>
    public static void LogWarning(string message, string context = "")
    {
        string fullMessage = string.IsNullOrEmpty(context)
            ? $"[WARNING] {message}"
            : $"[WARNING] {message} (Context: {context})";
        GD.PushWarning(fullMessage);
    }

    /// <summary>
    /// Logs a warning when a deprecated feature is used.
    /// </summary>
    /// <param name="feature">Name of the deprecated feature</param>
    /// <param name="alternative">Recommended alternative (optional)</param>
    public static void WarnDeprecatedFeature(string feature, string alternative = "")
    {
        string message = $"Feature '{feature}' is deprecated.";
        if (!string.IsNullOrEmpty(alternative))
        {
            message += $" Use '{alternative}' instead.";
        }
        LogWarning(message);
    }

    /// <summary>
    /// Logs a warning when an optional config key is missing.
    /// </summary>
    /// <param name="section">Config section name</param>
    /// <param name="key">Missing optional key name</param>
    public static void WarnMissingOptionalConfig(string section, string key)
    {
        LogWarning($"Optional config key missing: [{section}]/{key}. Using default value.");
    }

    // ===================================================================
    // HELPER METHODS
    // ===================================================================

    /// <summary>
    /// Internal helper method that formats and logs error messages.
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="context">Additional context</param>
    private static void LogError(string message, string context = "")
    {
        string fullMessage = string.IsNullOrEmpty(context)
            ? $"{message}"
            : $"{message} (Context: {context})";
        GD.PushError(fullMessage);
    }

    /// <summary>
    /// Executes an action within a try-catch block, logging any exceptions that occur.
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="operationName">Name of the operation (for error logging)</param>
    /// <param name="context">Additional context for debugging</param>
    /// <remarks>
    /// <para>
    /// Useful for wrapping operations that might fail without crashing the entire plugin.
    /// Any exceptions are caught and logged via <c>ThrowUnexpectedException</c>.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Executes a function within a try-catch block, returning a default value if an exception occurs.
    /// </summary>
    /// <typeparam name="T">Return type of the function</typeparam>
    /// <param name="func">Function to execute</param>
    /// <param name="defaultValue">Value to return if the function fails</param>
    /// <param name="operationName">Name of the operation (for error logging)</param>
    /// <param name="context">Additional context for debugging</param>
    /// <returns>The function's return value, or <paramref name="defaultValue"/> if an exception occurs</returns>
    /// <remarks>
    /// <para>
    /// Useful for operations that return a value but might fail. Ensures graceful degradation
    /// by returning a safe default value instead of propagating exceptions.
    /// </para>
    /// </remarks>
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