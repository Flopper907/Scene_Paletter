using System;
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
}