using System;
using Addons.ScenePaletter.Core;
using Godot;
using Godot.Collections;

namespace Addons.ScenePaletter.Management;

public class ConfigLoader : IDisposable
{
    private ConfigFile configFile;

    // file section
    public string PalettePath { get; private set; }
    public string FileExtension { get; private set; }
    public int IdStart { get; private set; }
    public int IdEnd { get; private set; }

    // page section
    public Dictionary<string, string> ScenePaths { get; private set; }
    public Dictionary<string, string> WidgetPaths { get; private set; }
    public Dictionary<string, string> InitialDocks { get; private set; }

    // ui section
    public int MinColums { get; private set; }
    public int MaxColums { get; private set; }
    public int Columns { get; private set; }
    public Vector2I PreviewResolution { get; private set; }
    public Vector2 PreviewMargin { get; private set; }
    public bool PreviewTransparent2D { get; private set; }
    public bool PreviewTransparent3D { get; private set; }

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
            LoadFileSection();
            LoadPageSection();
            LoadUISection();
        }, "ConfigLoader.Init", $"Path: {path}");
    }

    private void LoadFileSection()
    {
        PalettePath = GetString("file", "palette_path");
        FileExtension = GetString("file", "file_extension");
        IdStart = GetInt("file", "id_start");
        IdEnd = GetInt("file", "id_end");
    }

    private void LoadPageSection()
    {
        ScenePaths = GetDictionary("page", "pages", new Dictionary<string, string>());
        WidgetPaths = GetDictionary("page", "widgets", new Dictionary<string, string>());
        InitialDocks = GetDictionary("page", "initial_docks", new Dictionary<string, string>());
    }

    private void LoadUISection()
    {
        MaxColums = GetInt("ui", "max_columns", 6);
        MinColums = GetInt("ui", "min_columns", 1);
        Columns = GetInt("ui", "columns", 2);
        PreviewResolution = GetVector2I("ui", "preview_resolution_x", "preview_resolution_y", new Vector2I(256, 256));
        PreviewMargin = GetVector2("ui", "preview_margin_x", "preview_margin_y", new Vector2(10f, 10f));
        PreviewTransparent2D = GetBool("ui", "preview_2d_transparent");
        PreviewTransparent3D = GetBool("ui", "preview_3d_transparent");
    }

    public void AddColumn()
    {
        Columns = Math.Min(MaxColums, Columns + 1);
    }

    public void RemoveColumn()
    {
        Columns = Math.Max(MinColums, Columns - 1);
    }

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

    private Vector2 GetVector2(string section, string keyX, string keyY, Vector2 defaultValue)
    {
        float x = GetFloat(section, keyX, defaultValue.X);
        float y = GetFloat(section, keyY, defaultValue.Y);
        return new Vector2(x, y);
    }

    private Vector2I GetVector2I(string section, string keyX, string keyY, Vector2I defaultValue)
    {
        int x = GetInt(section, keyX, defaultValue.X);
        int y = GetInt(section, keyY, defaultValue.Y);
        return new Vector2I(x, y);
    }

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

    public void Dispose()
    {
        configFile?.Dispose();
        configFile = null;
    }
}