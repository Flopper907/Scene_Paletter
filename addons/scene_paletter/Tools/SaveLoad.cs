using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using Addons.ScenePaletter.Core;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Provides JSON-based serialization utilities for saving and loading data in the Godot editor.
/// Handles file operations, error recovery, and type-safe data management.
/// </summary>
/// <remarks>
/// <para>
/// Features:
/// <list type="bullet">
///   <item>Type-safe JSON serialization/deserialization</item>
///   <item>Automatic file path globalization for cross-platform compatibility</item>
///   <item>Error recovery with fallback values</item>
///   <item>Batch loading of multiple files</item>
///   <item>Optional filename preservation when loading multiple files</item>
/// </list>
/// </para>
/// <para>
/// Uses <c>System.Text.Json</c> with camelCase naming policy and indented formatting.
/// All file paths are processed through <c>ProjectSettings.GlobalizePath</c>.
/// </para>
/// </remarks>
public static class SaveLoad
{
    /// <summary>
    /// JSON serialization options with camelCase naming and indented formatting.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Saves data to a JSON file at the specified path.
    /// </summary>
    /// <typeparam name="T">Type of data to save.</typeparam>
    /// <param name="data">Data object to serialize.</param>
    /// <param name="path">Target file path (will be globalized).</param>
    /// <remarks>
    /// <para>
    /// Serializes the data to JSON and writes it to the specified file.
    /// </para>
    /// 
    /// <para>Logs via <c>ExceptionHandler.ThrowSerializationException</c> if serialization fails.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> for any other errors.</para>
    /// 
    /// </remarks>
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

    /// <summary>
    /// Ensures that a directory exists at the specified path, creating it if necessary.
    /// </summary>
    /// <param name="path">Directory path to ensure exists (will be globalized).</param>
    /// <returns>True if directory exists or was created successfully, false otherwise.</returns>
    /// <remarks>
    /// <para>
    /// Creates the directory and any necessary parent directories if they don't exist.
    /// If the directory already exists, returns true without modification.
    /// </para>
    /// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> if directory creation fails.</para>
    /// </remarks>
    public static bool EnsureDirectoryExists(string path)
    {
        string globalPath = ProjectSettings.GlobalizePath(path);

        try
        {
            if (!Directory.Exists(globalPath))
            {
                Directory.CreateDirectory(globalPath);
            }

            return true;
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, nameof(EnsureDirectoryExists));
            return false;
        }
    }

    /// <summary>
    /// Loads data from a JSON file, creating a new instance if the file doesn't exist.
    /// </summary>
    /// <typeparam name="T">Type of data to load. Must have a parameterless constructor.</typeparam>
    /// <param name="path">Source file path (will be globalized).</param>
    /// <returns>Deserialized data or a new instance if file doesn't exist.</returns>
    /// <remarks>
    /// <para>
    /// If the file doesn't exist, creates a new default instance, saves it, and returns it.
    /// If deserialization fails, returns a new default instance.
    /// </para>
    /// <para>Logs via <c>ExceptionHandler.ThrowFileNotFoundException</c> if file doesn't exist.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowDeserializationException</c> if deserialization fails.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> for any other errors.</para>
    /// </remarks>
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

    /// <summary>
    /// Attempts to load data from a JSON file, returning default if any error occurs.
    /// </summary>
    /// <typeparam name="T">Type of data to load. Must have a parameterless constructor.</typeparam>
    /// <param name="path">Source file path (will be globalized).</param>
    /// <returns>Deserialized data or default(T) if any error occurs.</returns>
    /// <remarks>
    /// <para>
    /// Unlike <c>Load</c>, this method returns default(T) for any error (file not found,
    /// deserialization failure, etc.) without creating a new file.
    /// </para>
    /// <para>Logs via <c>ExceptionHandler.ThrowFileNotFoundException</c> if file doesn't exist.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowDeserializationException</c> if deserialization fails.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> for any other errors.</para>
    /// </remarks>
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

    /// <summary>
    /// Loads all JSON files of type T from a folder that match the specified extension.
    /// </summary>
    /// <typeparam name="T">Type of data to load. Must have a parameterless constructor.</typeparam>
    /// <param name="folder">Folder path to search (will be globalized).</param>
    /// <param name="endsWith">File extension to match (e.g., ".json").</param>
    /// <returns>List of successfully deserialized objects.</returns>
    /// <remarks>
    /// <para>
    /// Searches the specified folder for files ending with <c>endsWith</c>,
    /// attempts to deserialize each as type T, and returns all successful results.
    /// </para>
    /// <para>Logs via <c>ExceptionHandler.ThrowFolderNotFoundException</c> if folder doesn't exist.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowDeserializationException</c> for individual file failures.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> for any other errors.</para>
    /// </remarks>
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

    /// <summary>
    /// Loads all JSON files of type T from a folder that match the specified extension,
    /// including their filenames.
    /// </summary>
    /// <typeparam name="T">Type of data to load. Must have a parameterless constructor.</typeparam>
    /// <param name="folder">Folder path to search (will be globalized).</param>
    /// <param name="endsWith">File extension to match (e.g., ".json").</param>
    /// <returns>List of tuples containing deserialized data and corresponding filenames.</returns>
    /// <remarks>
    /// <para>
    /// Similar to <c>LoadAll</c>, but also includes the filename for each loaded object.
    /// </para>
    /// <para>Logs via <c>ExceptionHandler.ThrowFolderNotFoundException</c> if folder doesn't exist.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowDeserializationException</c> for individual file failures.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> for any other errors.</para>
    /// </remarks>
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

    /// <summary>
    /// Deletes the file at the specified path.
    /// </summary>
    /// <param name="path">File path to delete (will be globalized).</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    /// <remarks>
    /// <para>Logs via <c>ExceptionHandler.ThrowFileNotFoundException</c> if file doesn't exist.</para>
    /// <para>Logs via <c>ExceptionHandler.ThrowUnexpectedException</c> for any other errors.</para>
    /// </remarks>
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