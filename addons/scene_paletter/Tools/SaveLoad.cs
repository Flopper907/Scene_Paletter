using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using Addons.ScenePaletter.Core;

namespace Addons.ScenePaletter.Tools;

public static class SaveLoad
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    // --------------------------------------------------
    // Save
    // --------------------------------------------------

    public static void Save<T>(T data, string path)
    {
        try
        {
            string jsonData = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(ProjectSettings.GlobalizePath(path), jsonData);
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowSerializationException(typeof(T).Name, nameof(Save));
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(Save));
        }
    }

    // --------------------------------------------------
    // Load (guaranteed return)
    // --------------------------------------------------

    public static T Load<T>(string path) where T : new()
    {
        string globalPath = ProjectSettings.GlobalizePath(path);

        try
        {
            if (!File.Exists(globalPath))
            {
                ExceptionHandler.ThrowFileNotFoundException(path, nameof(Load));
                T newData = new T();
                Save(newData, path);
                return newData;
            }

            string jsonData = File.ReadAllText(globalPath);
            T data = JsonSerializer.Deserialize<T>(jsonData);

            if (data == null)
            {
                ExceptionHandler.ThrowDeserializationException(typeof(T).Name, path, nameof(Load));
                return new T();
            }

            return data;
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(Load));
            return new T();
        }
    }

    // --------------------------------------------------
    // TryLoad (soft fail)
    // --------------------------------------------------

    public static T TryLoad<T>(string path) where T : new()
    {
        string globalPath = ProjectSettings.GlobalizePath(path);

        try
        {
            if (!File.Exists(globalPath))
            {
                ExceptionHandler.ThrowFileNotFoundException(path, nameof(TryLoad));
                return default;
            }

            string jsonData = File.ReadAllText(globalPath);
            T data = JsonSerializer.Deserialize<T>(jsonData);

            if (data == null)
            {
                ExceptionHandler.ThrowDeserializationException(typeof(T).Name, path, nameof(TryLoad));
                return default;
            }

            return data;
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(TryLoad));
            return default;
        }
    }

    // --------------------------------------------------
    // Load All
    // --------------------------------------------------

    public static List<T> LoadAll<T>(string folder, string endsWith) where T : new()
    {
        var results = new List<T>();
        string globalPath = ProjectSettings.GlobalizePath(folder);

        try
        {
            if (!Directory.Exists(globalPath))
            {
                ExceptionHandler.ThrowFolderNotFoundException(folder, nameof(LoadAll));
                return results;
            }

            foreach (string file in Directory.GetFiles(globalPath))
            {
                if (!file.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    string jsonData = File.ReadAllText(file);
                    T data = JsonSerializer.Deserialize<T>(jsonData);

                    if (data != null)
                        results.Add(data);
                    else
                        ExceptionHandler.ThrowDeserializationException(typeof(T).Name, file, nameof(LoadAll));
                }
                catch (Exception ex)
                {
                    ExceptionHandler.ThrowUnexpectedException(ex, $"{nameof(LoadAll)}:{file}");
                }
            }
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(LoadAll));
        }

        return results;
    }

    // --------------------------------------------------
    // Load All With Filename
    // --------------------------------------------------

    public static List<(T data, string filename)> LoadAllWithFile<T>(string folder, string endsWith) where T : new()
    {
        var results = new List<(T data, string filename)>();
        string globalPath = ProjectSettings.GlobalizePath(folder);

        try
        {
            if (!Directory.Exists(globalPath))
            {
                ExceptionHandler.ThrowFolderNotFoundException(folder, nameof(LoadAllWithFile));
                return results;
            }

            foreach (string file in Directory.GetFiles(globalPath))
            {
                if (!file.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    string jsonData = File.ReadAllText(file);
                    T data = JsonSerializer.Deserialize<T>(jsonData);

                    if (data != null)
                        results.Add((data, Path.GetFileName(file)));
                    else
                        ExceptionHandler.ThrowDeserializationException(typeof(T).Name, file, nameof(LoadAllWithFile));
                }
                catch (Exception ex)
                {
                    ExceptionHandler.ThrowUnexpectedException(ex, $"{nameof(LoadAllWithFile)}:{file}");
                }
            }
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(LoadAllWithFile));
        }

        return results;
    }

    // --------------------------------------------------
    // Delete
    // --------------------------------------------------

    public static bool Delete(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);

        try
        {
            if (!File.Exists(globalPath))
            {
                ExceptionHandler.ThrowFileNotFoundException(path, nameof(Delete));
                return false;
            }

            File.Delete(globalPath);
            return true;
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(Delete));
            return false;
        }
    }
}