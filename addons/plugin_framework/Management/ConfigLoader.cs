#if TOOLS
using System;
using Addons.PluginFramework.Core;
using Godot;
using Godot.Collections;

namespace Addons.PluginFramework.Management;

/// <summary>
/// Loads a plugin's <c>plugin.cfg</c> (Godot <c>ConfigFile</c> format) and provides typed access to its values.
/// </summary>
/// <remarks>
/// <para>
/// The loader itself only knows the <c>[page]</c> section used by the framework (<c>pages</c>,
/// <c>widgets</c>, <c>initial_docks</c>). Any other section is plugin specific: read it with the public
/// <c>GetString/GetInt/GetFloat/GetBool/GetColor/GetVector*/GetDictionary</c> helpers, typically from a small
/// settings class created in <c>FrameworkPlugin.OnEnable</c>.
/// </para>
/// <para>
/// A key that is missing but has a default value silently returns the default. A key with the wrong
/// type is logged as an error and returns the default.
/// </para>
/// </remarks>
public class ConfigLoader : IDisposable
{
    private ConfigFile configFile;

    /// <summary>Page name to scene path (or uid) map from <c>[page] pages</c>.</summary>
    public Dictionary<string, string> ScenePaths { get; private set; }

    /// <summary>Widget name to scene path (or uid) map from <c>[page] widgets</c>.</summary>
    public Dictionary<string, string> WidgetPaths { get; private set; }

    /// <summary><c>UIPosition</c> name to page name map from <c>[page] initial_docks</c>.</summary>
    public Dictionary<string, string> InitialDocks { get; private set; }

    /// <summary>
    /// Loads the config file at <paramref name="path"/> and reads the <c>[page]</c> section.
    /// </summary>
    /// <param name="path">Path of the <c>plugin.cfg</c> file.</param>
    /// <remarks>
    /// Logs via <c>ExceptionHandler.ThrowConfigLoadException</c> if the file cannot be loaded.
    /// </remarks>
    public void Init(string path)
    {
        ExceptionHandler.SafeExecute(() =>
        {
            configFile = new ConfigFile();
            var error = configFile.Load(path);

            if (error != Error.Ok)
            {
                ExceptionHandler.ThrowConfigLoadException(path, $"Error code: {error}");
                configFile.Dispose();
                configFile = null;
            }
        }, "ConfigFile.Load", $"Path: {path}");

        if (configFile == null)
        {
            ExceptionHandler.ThrowConfigLoadException(path, "ConfigFile is null after load");
            return;
        }

        ExceptionHandler.SafeExecute(LoadPageSection, "ConfigLoader.Init", $"Path: {path}");
    }

    private void LoadPageSection()
    {
        ScenePaths = GetDictionary("page", "pages", new Dictionary<string, string>());
        WidgetPaths = GetDictionary("page", "widgets", new Dictionary<string, string>());
        InitialDocks = GetDictionary("page", "initial_docks", new Dictionary<string, string>());
    }

    /// <summary>
    /// Reads a typed value, or returns <paramref name="defaultValue"/> when the file is not loaded,
    /// the key is missing (silently) or the stored type differs (logged).
    /// </summary>
    private bool TryGet(string section, string key, Variant.Type expected, out Variant value)
    {
        value = default;
        if (configFile == null || !configFile.HasSectionKey(section, key))
            return false;

        value = configFile.GetValue(section, key);
        if (value.VariantType != expected)
        {
            ExceptionHandler.ThrowInvalidResourceTypeException($"Config {section}/{key}", expected.ToString(), value.VariantType.ToString());
            return false;
        }

        return true;
    }

    /// <summary>Reads a <c>string</c> from <paramref name="section"/>/<paramref name="key"/>.</summary>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if missing or of the wrong type.</returns>
    public string GetString(string section, string key, string defaultValue = "")
        => TryGet(section, key, Variant.Type.String, out var v) ? (string)v : defaultValue;

    /// <summary>Reads an <c>int</c> from <paramref name="section"/>/<paramref name="key"/>.</summary>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if missing or of the wrong type.</returns>
    public int GetInt(string section, string key, int defaultValue = 0)
        => TryGet(section, key, Variant.Type.Int, out var v) ? (int)v : defaultValue;

    /// <summary>Reads a <c>float</c> from <paramref name="section"/>/<paramref name="key"/>.</summary>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if missing or of the wrong type.</returns>
    public float GetFloat(string section, string key, float defaultValue = 0f)
        => TryGet(section, key, Variant.Type.Float, out var v) ? (float)v : defaultValue;

    /// <summary>Reads a <c>bool</c> from <paramref name="section"/>/<paramref name="key"/>.</summary>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if missing or of the wrong type.</returns>
    public bool GetBool(string section, string key, bool defaultValue = false)
        => TryGet(section, key, Variant.Type.Bool, out var v) ? (bool)v : defaultValue;

    /// <summary>Reads a <c>Color</c> from <paramref name="section"/>/<paramref name="key"/>.</summary>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if missing or of the wrong type.</returns>
    /// <remarks>
    /// Accepts a <c>Color</c> variant, or a <c>string</c> in any format <c>Color.FromString</c> understands (e.g. <c>"#00ccff38"</c>).
    /// </remarks>
    public Color GetColor(string section, string key, Color defaultValue)
    {
        if (configFile == null || !configFile.HasSectionKey(section, key))
            return defaultValue;

        Variant v = configFile.GetValue(section, key);
        switch (v.VariantType)
        {
            case Variant.Type.Color:
                return (Color)v;
            case Variant.Type.String:
                return Color.FromString((string)v, defaultValue);
            default:
                ExceptionHandler.ThrowInvalidResourceTypeException($"Config {section}/{key}", Variant.Type.Color.ToString(), v.VariantType.ToString());
                return defaultValue;
        }
    }

    /// <summary>Reads a <c>Vector2</c> stored as two float keys.</summary>
    /// <remarks>Each component is loaded independently using <c>GetFloat</c>.</remarks>
    public Vector2 GetVector2(string section, string keyX, string keyY, Vector2 defaultValue)
        => new(GetFloat(section, keyX, defaultValue.X), GetFloat(section, keyY, defaultValue.Y));

    /// <summary>Reads a <c>Vector2I</c> stored as two int keys.</summary>
    /// <remarks>Each component is loaded independently using <c>GetInt</c>.</remarks>
    public Vector2I GetVector2I(string section, string keyX, string keyY, Vector2I defaultValue)
        => new(GetInt(section, keyX, defaultValue.X), GetInt(section, keyY, defaultValue.Y));

    /// <summary>Reads a <c>Vector3</c> stored as three float keys.</summary>
    /// <remarks>Each component is loaded independently using <c>GetFloat</c>.</remarks>
    public Vector3 GetVector3(string section, string keyX, string keyY, string keyZ, Vector3 defaultValue)
        => new(GetFloat(section, keyX, defaultValue.X), GetFloat(section, keyY, defaultValue.Y), GetFloat(section, keyZ, defaultValue.Z));

    /// <summary>Reads a <c>Vector3I</c> stored as three int keys.</summary>
    /// <remarks>Each component is loaded independently using <c>GetInt</c>.</remarks>
    public Vector3I GetVector3I(string section, string keyX, string keyY, string keyZ, Vector3I defaultValue)
        => new(GetInt(section, keyX, defaultValue.X), GetInt(section, keyY, defaultValue.Y), GetInt(section, keyZ, defaultValue.Z));

    /// <summary>Reads a <c>Dictionary&lt;string, string&gt;</c> from <paramref name="section"/>/<paramref name="key"/>.</summary>
    /// <returns>The stored value, or <paramref name="defaultValue"/> if missing or of the wrong type.</returns>
    public Dictionary<string, string> GetDictionary(string section, string key, Dictionary<string, string> defaultValue)
        => TryGet(section, key, Variant.Type.Dictionary, out var v) ? (Dictionary<string, string>)v : defaultValue;

    /// <summary>
    /// Disposes the underlying <c>ConfigFile</c> resource.
    /// </summary>
    public void Dispose()
    {
        configFile?.Dispose();
        configFile = null;
    }
}
#endif
