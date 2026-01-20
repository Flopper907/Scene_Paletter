using Godot;
using System;

namespace Addons.ScenePaletter;

public abstract partial class Page<T> : Control
{
    protected T data;
    protected Plugin plugin;

    public string Title { get; protected set; }


    public abstract void Initialize();

    public override void _Ready()
    {
        // Only run in editor context
        if (!Engine.IsEditorHint())
            return;

        // Only initialize if we're actually part of the plugin dock
        var parent = GetParent();
        if (parent is not PageDock dock)
        {
            return;
        }

        plugin = dock.plugin;

        // Safety check for plugin
        if (plugin == null)
        {
            return;
        }

        // Handle data
        if (plugin.data != null && plugin.data is T typedData)
        {
            data = typedData;
        }
        else
        {
            data = default(T);
        }

        Initialize();
    }


    protected void SetupFileDialog(string title, string filter, string description, EditorFileDialog.FilesSelectedEventHandler OnSceneFilesSelected)
    {
        EditorFileDialog fileDialog = new EditorFileDialog();
        fileDialog.FileMode = EditorFileDialog.FileModeEnum.OpenFiles; // Allow multiple file selection
        fileDialog.Access = EditorFileDialog.AccessEnum.Resources;
        fileDialog.Title = "Select Scene Files";

        // Filter for .tscn files (Godot scene files)
        fileDialog.AddFilter("*.tscn", "Godot Scene Files");

        // Connect the file(s) selected signal
        fileDialog.FilesSelected += OnSceneFilesSelected;

        // Add the dialog to the scene tree
        GetParent().AddChild(fileDialog);

        fileDialog.PopupCentered(new Vector2I(800, 600));
    }
}