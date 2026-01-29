using System;
using Godot;

namespace Addons.ScenePaletter.Core;

/// <summary>
/// Base class for all plugin pages. Provides lifecycle management, data handling, and docking integration.
/// </summary>
/// <remarks>
/// <para>Pages are automatically initialized when added to a <see cref="PageDock"/>.</para>
/// <para>Override <see cref="Initialize"/> to set up your page's UI and logic.</para>
/// <para>Access the plugin instance via the <see cref="plugin"/> property.</para>
/// </remarks>
/// <example>
/// <code>
/// [Tool]
/// public class MyPage : Page&lt;MyData&gt;
/// {
///     public override void Initialize()
///     {
///         // Setup your page here
///     }
/// }
/// </code>
/// </example>
public abstract partial class Page<T> : Control
{
    protected T data;
    protected PageDock dock;
    protected Plugin plugin;

    public string Title { get; protected set; }


    public abstract void Initialize();

    public override void _Ready()
    {
        // Only run in editor context
        if (!Engine.IsEditorHint())
        {
            return;
        }

        // Only initialize if we're actually part of the plugin dock
        var parent = GetParent();
        if (parent is not PageDock dock)
        {
            // Exception is correct, but throws everytime a Pagebase scene is opened 
            // ExceptionHandler.ThrowMissingDockParentException(GetPath());
            return;
        }

        this.dock = dock;
        plugin = dock.plugin;

        // Safety check for plugin
        if (dock.plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException($"{GetType().Name} {nameof(_Ready)}");
            return;
        }

        // Handle data
        if (dock.data != null && dock.data is T typedData)
        {
            data = typedData;
        }
        else if (dock.data != null) // Data exists but wrong type
        {
            ExceptionHandler.ThrowInvalidPageDataException(
                GetType().Name,
                typeof(T).Name,
                dock.data.GetType().Name
            );
            data = default;
        }
        else
        {
            data = default;
        }

        try
        {
            Initialize();
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"Page.Initialize - {GetType().Name} {nameof(_Ready)}");
        }
    }


    protected void SetupFileDialog(string title, string filter, string description, EditorFileDialog.FilesSelectedEventHandler OnSceneFilesSelected)
    {
        if (OnSceneFilesSelected == null)
        {
            ExceptionHandler.ThrowNullReferenceException(
                nameof(OnSceneFilesSelected),
                $"{GetType().Name} {nameof(SetupFileDialog)}"
            );
            return;
        }

        var parent = GetParent();
        if (parent == null)
        {
            ExceptionHandler.ThrowMissingNodeException(
                "Parent",
                $"{GetType().Name} {nameof(SetupFileDialog)}"
            );
            return;
        }

        try
        {
            EditorFileDialog fileDialog = new EditorFileDialog();
            fileDialog.FileMode = EditorFileDialog.FileModeEnum.OpenFiles;
            fileDialog.Access = EditorFileDialog.AccessEnum.Resources;
            fileDialog.Title = title; // Use the parameter!

            // Use the parameters properly
            fileDialog.AddFilter(filter, description);

            fileDialog.FilesSelected += OnSceneFilesSelected;

            parent.AddChild(fileDialog);
            fileDialog.PopupCentered(new Vector2I(800, 600));
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(SetupFileDialog)}");
        }
    }
}