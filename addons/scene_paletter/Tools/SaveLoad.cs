using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace Addons.ScenePaletter.Tools;

public class SaveLoad
{
    public static void Save<T>(T data, string path)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase // Optional: to format keys in camelCase
            };
            string jsonData = JsonSerializer.Serialize(data, options);
            File.WriteAllText(ProjectSettings.GlobalizePath(path), jsonData);
            GD.Print("Data saved successfully to: " + ProjectSettings.GlobalizePath(path));
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error saving data: " + ex.Message);
        }
    }

    public static T Load<T>(string path) where T : new() // Ensure T has a parameterless constructor
    {
        try
        {
            if (File.Exists(ProjectSettings.GlobalizePath(path)))
            {
                string jsonData = File.ReadAllText(ProjectSettings.GlobalizePath(path));
                T data = JsonSerializer.Deserialize<T>(jsonData);
                GD.Print("Data loaded successfully from: " + ProjectSettings.GlobalizePath(path));
                return data;
            }
            else
            {
                GD.PrintErr("File new created, because not found: " + path);
                T newData = new T();
                Save(newData, path);
                return newData;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error loading data: " + ex.Message);
        }

        return new T();
    }

    public static T TryLoad<T>(string path) where T : new() // Ensure T has a parameterless constructor
    {
        try
        {
            if (File.Exists(ProjectSettings.GlobalizePath(path)))
            {
                string jsonData = File.ReadAllText(ProjectSettings.GlobalizePath(path));
                T data = JsonSerializer.Deserialize<T>(jsonData);
                GD.Print("Data loaded successfully from: " + ProjectSettings.GlobalizePath(path));
                return data;
            }
            else
            {
                GD.PrintErr("File new created, because not found: " + path);
                return default;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error loading data: " + ex.Message);
        }

        return new T();
    }

    public static List<T> LoadAll<T>(string folder, string endsWith) where T : new()
    {
        List<T> results = new List<T>();

        try
        {
            string globalPath = ProjectSettings.GlobalizePath(folder);

            // Check if directory exists
            if (!Directory.Exists(globalPath))
            {
                GD.PrintErr($"Folder not found: {folder}");
                return results;
            }

            // Get all files in the directory
            string[] files = Directory.GetFiles(globalPath);

            foreach (string file in files)
            {
                // Check if file ends with the specified extension
                if (file.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string jsonData = File.ReadAllText(file);
                        T data = JsonSerializer.Deserialize<T>(jsonData);

                        if (data != null)
                        {
                            results.Add(data);
                            GD.Print($"Loaded: {file}");
                        }
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Error loading file {file}: {ex.Message}");
                        // Continue loading other files even if one fails
                    }
                }
            }

            GD.Print($"Successfully loaded {results.Count} file(s) from: {folder}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error loading files from folder: {ex.Message}");
        }

        return results;
    }

    public static List<(T data, string filename)> LoadAllWithFile<T>(string folder, string endsWith) where T : new()
    {
        List<(T data, string filename)> results = new List<(T data, string filename)>();

        try
        {
            string globalPath = ProjectSettings.GlobalizePath(folder);

            // Check if directory exists
            if (!Directory.Exists(globalPath))
            {
                GD.PrintErr($"Folder not found: {folder}");
                return results;
            }

            // Get all files in the directory
            string[] files = Directory.GetFiles(globalPath);

            foreach (string file in files)
            {
                // Check if file ends with the specified extension
                if (file.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string jsonData = File.ReadAllText(file);
                        T data = JsonSerializer.Deserialize<T>(jsonData);

                        if (data != null)
                        {
                            string filename = Path.GetFileName(file);
                            results.Add((data, filename));
                            GD.Print($"Loaded: {filename}");
                        }
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Error loading file {file}: {ex.Message}");
                    }
                }
            }

            GD.Print($"Successfully loaded {results.Count} file(s) from: {folder}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error loading files from folder: {ex.Message}");
        }

        return results;
    }

    public static bool Delete(string path)
    {
        try
        {
            string globalPath = ProjectSettings.GlobalizePath(path);

            if (File.Exists(globalPath))
            {
                File.Delete(globalPath);
                GD.Print("File deleted successfully: " + globalPath);
                return true;
            }
            else
            {
                GD.PrintErr("File not found: " + path);
                return false;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr("Error deleting file: " + ex.Message);
            return false;
        }
    }
}