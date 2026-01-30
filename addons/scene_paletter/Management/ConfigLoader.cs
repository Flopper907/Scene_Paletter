using System;
using Addons.ScenePaletter.Core;
using Godot;
using Godot.Collections;

namespace Addons.ScenePaletter.Management;

/// <summary>
/// Manages config file loading from Godot's <c>ConfigFile</c> format.
/// Provides strongly-typed access to plugin settings across all sections.
/// </summary>
/// <remarks>
/// <para>To add a new section:</para>
/// <list type="number">
/// <item>Add public properties for your config values</item>
/// <item>Create a private <c>LoadYourSection()</c> method using the Get helpers</item>
/// <item>Call it from <c>Init</c> inside the SafeExecute block</item>
/// </list>
/// </remarks>
public class ConfigLoader : IDisposable
{
    private ConfigFile configFile;

    // page section
    public Dictionary<string, string> ScenePaths { get; private set; }
    public Dictionary<string, string> WidgetPaths { get; private set; }
    public Dictionary<string, string> InitialDocks { get; private set; }

    // file section
    public string PalettePath { get; private set; }
    public string FileExtension { get; private set; }
    public int IdStart { get; private set; }
    public int IdEnd { get; private set; }

    // ui section
    public int MinColumns { get; private set; }
    public int MaxColumns { get; private set; }
    public int Columns { get; private set; }
    public Vector2I PreviewResolution { get; private set; }
    public Vector2 PreviewMargin { get; private set; }
    public bool PreviewTransparent2D { get; private set; }
    public bool PreviewTransparent3D { get; private set; }

    /// <summary> 
    /// Loads a config file with <c>path</c>
    /// </summary>
    /// <remarks>
    /// <para>Uses the <c>ConfigFile</c> class to load the file. This defines how values are written/read.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowConfigLoadException</c> if the config file is not found.</para>
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
                return;
            }
        }, "ConfigFile.Load", $"Path: {path}");

        if (configFile == null)
        {
            ExceptionHandler.ThrowConfigLoadException(path, "ConfigFile is null after load");
            return;
        }

        ExceptionHandler.SafeExecute(() =>
        {
            // TODO: Add additional section loaders here (e.g., `LoadNetworkSection()`).
            //       Call them from here
            LoadPageSection();
            LoadFileSection();
            LoadUISection();
        }, "ConfigLoader.Init", $"Path: {path}");
    }

    /// <summary> 
    /// Loads all config variables under section <c>file</c>
    /// </summary>
    /// <remarks>
    /// <para>It only uses the <c>GetInt/GetFloat/...</c> methods</para>
    /// </remarks>
    private void LoadFileSection()
    {
        PalettePath = GetString("file", "palette_path");
        FileExtension = GetString("file", "file_extension");
        IdStart = GetInt("file", "id_start");
        IdEnd = GetInt("file", "id_end");
    }

    /// <summary> 
    /// Loads all config variables under section <c>page</c>
    /// </summary>
    /// <remarks>
    /// <para>It only uses the <c>GetInt/GetFloat/...</c> methods</para>
    /// </remarks>
    private void LoadPageSection()
    {
        ScenePaths = GetDictionary("page", "pages", new Dictionary<string, string>());
        WidgetPaths = GetDictionary("page", "widgets", new Dictionary<string, string>());
        InitialDocks = GetDictionary("page", "initial_docks", new Dictionary<string, string>());
    }

    /// <summary> 
    /// Loads all config variables under section <c>ui</c>
    /// </summary>
    /// <remarks>
    /// <para>It only uses the <c>GetBool/GetInt/GetFloat/...</c> methods</para>
    /// </remarks>
    private void LoadUISection()
    {
        MaxColumns = GetInt("ui", "max_columns", 6);
        MinColumns = GetInt("ui", "min_columns", 1);
        Columns = GetInt("ui", "columns", 2);
        PreviewResolution = GetVector2I("ui", "preview_resolution_x", "preview_resolution_y", new Vector2I(256, 256));
        PreviewMargin = GetVector2("ui", "preview_margin_x", "preview_margin_y", new Vector2(10f, 10f));
        PreviewTransparent2D = GetBool("ui", "preview_2d_transparent");
        PreviewTransparent3D = GetBool("ui", "preview_3d_transparent");
    }

    /// <summary> 
    /// Increment <c>Columns</c> until it reaches <c>MaxColumns</c>
    /// </summary>
    /// <remarks>
    /// <para>No error is logged if <c>Columns</c> is already at <c>MaxColumns</c> when calling <c>AddColumn</c>.</para>
    /// </remarks>
    public void AddColumn()
    {
        Columns = Math.Min(MaxColumns, Columns + 1);
    }

    /// <summary> 
    /// Decrement <c>Columns</c> until it reaches <c>MinColumns</c>
    /// </summary>
    /// <remarks>
    /// <para>No error is logged if <c>Columns</c> is already at <c>MinColumns</c> when calling <c>RemoveColumn</c>.</para>
    /// </remarks>
    public void RemoveColumn()
    {
        Columns = Math.Max(MinColumns, Columns - 1);
    }

    /// <summary>
    /// Loads a <c>string</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowConfigLoadException</c> if key is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowInvalidResourceTypeException</c> if type is wrong.</para>
    /// </remarks>
    private string GetString(string section, string key, string defaultValue = "")
    {
        if (!configFile.HasSectionKey(section, key))
        {
            ExceptionHandler.ThrowConfigLoadException($"Tried loading {section}/{key}", "Returning default value");
            return defaultValue;
        }

        Variant v = configFile.GetValue(section, key);
        if (v.VariantType != Variant.Type.String)
        {
            ExceptionHandler.ThrowInvalidResourceTypeException($"Config {section}/{key}", Variant.Type.String.ToString(), v.VariantType.ToString());
            return defaultValue;
        }
        return (string)v;
    }

    /// <summary>
    /// Loads a <c>int</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowConfigLoadException</c> if key is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowInvalidResourceTypeException</c> if type is wrong.</para>
    /// </remarks>
    private int GetInt(string section, string key, int defaultValue = 0)
    {
        if (!configFile.HasSectionKey(section, key))
        {
            ExceptionHandler.ThrowConfigLoadException($"Tried loading {section}/{key}", "Returning default value");
            return defaultValue;
        }

        Variant v = configFile.GetValue(section, key);
        if (v.VariantType != Variant.Type.Int)
        {
            ExceptionHandler.ThrowInvalidResourceTypeException($"Config {section}/{key}", Variant.Type.Int.ToString(), v.VariantType.ToString());
            return defaultValue;
        }
        return (int)v;
    }

    /// <summary>
    /// Loads a <c>float</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowConfigLoadException</c> if key is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowInvalidResourceTypeException</c> if type is wrong.</para>
    /// </remarks>
    private float GetFloat(string section, string key, float defaultValue = 0f)
    {
        if (!configFile.HasSectionKey(section, key))
        {
            ExceptionHandler.ThrowConfigLoadException($"Tried loading {section}/{key}", "Returning default value");
            return defaultValue;
        }

        Variant v = configFile.GetValue(section, key);
        if (v.VariantType != Variant.Type.Float)
        {
            ExceptionHandler.ThrowInvalidResourceTypeException($"Config {section}/{key}", Variant.Type.Float.ToString(), v.VariantType.ToString());
            return defaultValue;
        }
        return (float)v;
    }

    /// <summary>
    /// Loads a <c>bool</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowConfigLoadException</c> if key is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowInvalidResourceTypeException</c> if type is wrong.</para>
    /// </remarks>
    private bool GetBool(string section, string key, bool defaultValue = false)
    {
        if (!configFile.HasSectionKey(section, key))
        {
            ExceptionHandler.ThrowConfigLoadException($"Tried loading {section}/{key}", "Returning default value");
            return defaultValue;
        }

        Variant v = configFile.GetValue(section, key);
        if (v.VariantType != Variant.Type.Bool)
        {
            ExceptionHandler.ThrowInvalidResourceTypeException($"Config {section}/{key}", Variant.Type.Bool.ToString(), v.VariantType.ToString());
            return defaultValue;
        }
        return (bool)v;
    }

    /// <summary>
    /// Loads a <c>Vector2</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Loads each value independently using <c>GetFloat</c>.</para>
    /// </remarks>
    private Vector2 GetVector2(string section, string keyX, string keyY, Vector2 defaultValue)
    {
        float x = GetFloat(section, keyX, defaultValue.X);
        float y = GetFloat(section, keyY, defaultValue.Y);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Loads a <c>Vector2I</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Loads each value independently using <c>GetInt</c>.</para>
    /// </remarks>
    private Vector2I GetVector2I(string section, string keyX, string keyY, Vector2I defaultValue)
    {
        int x = GetInt(section, keyX, defaultValue.X);
        int y = GetInt(section, keyY, defaultValue.Y);
        return new Vector2I(x, y);
    }

    /// <summary>
    /// Loads a <c>Vector3</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Loads each value independently using <c>GetFloat</c>.</para>
    /// </remarks>
    private Vector3 GetVector3(string section, string keyX, string keyY, string keyZ, Vector3 defaultValue)
    {
        float x = GetFloat(section, keyX, defaultValue.X);
        float y = GetFloat(section, keyY, defaultValue.Y);
        float z = GetFloat(section, keyZ, defaultValue.Z);
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Loads a <c>Vector3I</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Loads each value independently using <c>GetInt</c>.</para>
    /// </remarks>
    private Vector3I GetVector3I(string section, string keyX, string keyY, string keyZ, Vector3I defaultValue)
    {
        int x = GetInt(section, keyX, defaultValue.X);
        int y = GetInt(section, keyY, defaultValue.Y);
        int z = GetInt(section, keyZ, defaultValue.Z);
        return new Vector3I(x, y, z);
    }

    /// <summary>
    /// Loads a <c>Dictionary(string, string)</c> from <c>section</c>|<c>key</c> in <c>configFile</c>.
    /// </summary>
    /// <returns>Value from <c>configFile</c> at <c>section</c>|<c>key</c>, when not possible, returns <c>defaultValue</c></returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowConfigLoadException</c> if key is <c>null</c>.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowInvalidResourceTypeException</c> if type is wrong.</para>
    /// </remarks>
    private Dictionary<string, string> GetDictionary(string section, string key, Dictionary<string, string> defaultValue)
    {
        if (!configFile.HasSectionKey(section, key))
        {
            ExceptionHandler.ThrowConfigLoadException($"Tried loading {section}/{key}", "Returning default value");
            return defaultValue;
        }

        Variant v = configFile.GetValue(section, key);
        if (v.VariantType != Variant.Type.Dictionary)
        {
            ExceptionHandler.ThrowInvalidResourceTypeException($"Config {section}/{key}", Variant.Type.Dictionary.ToString(), v.VariantType.ToString());
            return defaultValue;
        }
        return (Dictionary<string, string>)v;
    }

    /// <summary>
    /// Disposes the underlying <c>ConfigFile</c> resource.
    /// </summary>
    public void Dispose()
    {
        configFile?.Dispose();
        configFile = null;
    }
}