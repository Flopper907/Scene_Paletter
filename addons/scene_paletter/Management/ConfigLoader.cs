using System;
using Godot;
using Godot.Collections;

namespace Addons.ScenePaletter.Management;

public class ConfigLoader : IDisposable
{
    private ConfigFile configFile;

    // file section
    public string WidgetPath { get; private set; }
    public string PalettePath { get; private set; }
    public string FileExtension { get; private set; }
    public int IdStart { get; private set; }
    public int IdEnd { get; private set; }

    // page section
    public Dictionary<string, string> ScenePaths { get; private set; }
    public Dictionary<string, string> InitialDocks { get; private set; }

    // ui section
    public int MinColums { get; private set; }
    public int MaxColums { get; private set; }
    public int Columns { get; private set; }
    public Vector2I PreviewResolution { get; private set; }
    public Vector2 PreviewMargin { get; private set; }
    public bool PreviewTransparent2D { get; private set; }
    public bool PreviewTransparent3D { get; private set; }

    public void InitConfig(string path)
    {
        configFile = new ConfigFile();
        configFile.Load(path);

        // file section
        WidgetPath = GetString("file", "widget_path");
        PalettePath = GetString("file", "palette_path");
        FileExtension = GetString("file", "file_extension");
        IdStart = GetInt("file", "id_start");
        IdEnd = GetInt("file", "id_end");

        // page section
        ScenePaths = GetDictionary("page", "pages", new Dictionary<string, string>());
        InitialDocks = GetDictionary("page", "initial_docks", new Dictionary<string, string>());

        // ui section
        MaxColums = GetInt("ui", "max_columns");
        MinColums = GetInt("ui", "min_columns");
        Columns = GetInt("ui", "columns");
        PreviewResolution = GetVector2I("ui", "preview_resolution_x", "preview_resolution_y", Vector2I.Zero);
        PreviewMargin = GetVector2("ui", "preview_margin_x", "preview_margin_y", Vector2.Zero);
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
            GD.PushWarning($"Config missing: {section}/{key}, using default: {defaultValue}");
            return defaultValue;
        }
        return (string)configFile.GetValue(section, key);
    }

    private int GetInt(string section, string key, int defaultValue = 0)
    {
        if (!configFile.HasSectionKey(section, key))
        {
            GD.PushWarning($"Config missing: {section}/{key}, using default: {defaultValue}");
            return defaultValue;
        }
        return (int)configFile.GetValue(section, key);
    }

    private float GetFloat(string section, string key, float defaultValue = 0f)
    {
        if (!configFile.HasSectionKey(section, key))
        {
            GD.PushWarning($"Config missing: {section}/{key}, using default: {defaultValue}");
            return defaultValue;
        }
        return (float)configFile.GetValue(section, key);
    }

    private bool GetBool(string section, string key, bool defaultValue = false)
    {
        if (!configFile.HasSectionKey(section, key))
        {
            GD.PushWarning($"Config missing: {section}/{key}, using default: {defaultValue}");
            return defaultValue;
        }
        return (bool)configFile.GetValue(section, key);
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
            GD.PushWarning($"Config missing: {section}/{key}, using default: {defaultValue}");
            return defaultValue;
        }
        return (Dictionary<string, string>)configFile.GetValue(section, key);
    }

    public void Dispose()
    {
        configFile?.Dispose();
        configFile = null;
    }
}