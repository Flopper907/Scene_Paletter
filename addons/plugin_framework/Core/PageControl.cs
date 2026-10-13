#if TOOLS
using System;
using Godot;

namespace Addons.PluginFramework.Core;

/// <summary>
/// Non-generic base of every <c>Page&lt;TData, TPlugin&gt;</c>. Lets the framework read a page's
/// <c>Title</c> without reflection and hosts the file-dialog helper shared by all pages.
/// </summary>
/// <remarks>
/// Derive from <c>Page&lt;TData, TPlugin&gt;</c> instead of this class directly.
/// </remarks>
[Tool]
public abstract partial class PageControl : Control
{
    private const string FileDialogName = "PaletteFileDialog";

    // Shared by all pages of all plugins; non-generic so it is not duplicated per Page<,> instantiation.
    private static string fileDialogDir;

    /// <summary>
    /// Display title of the page. Set it in <c>Initialize</c>; the owning <c>PageDock</c> uses it as the
    /// dock name and the tab title of the editor dock.
    /// </summary>
    public string Title { get; protected set; }

    /// <summary>
    /// Sets up a file dialog for selecting scenes or other resources.
    /// </summary>
    /// <param name="title">The title of the dialog window.</param>
    /// <param name="filter">File extension filter (e.g., "*.tscn").</param>
    /// <param name="description">Human-readable description of the filter.</param>
    /// <param name="OnSceneFilesSelected">Callback for when files are selected.</param>
    /// <remarks>
    /// <para>
    /// Creates and configures an <c>EditorFileDialog</c> as a sibling of the page and remembers the last used
    /// directory across dialogs.
    /// </para>
    /// <para>
    /// Logs via <c>ExceptionHandler.ThrowNullReferenceException</c> if <c>OnSceneFilesSelected</c> is <c>null</c>.
    /// Logs via <c>ExceptionHandler.ThrowMissingNodeException</c> if the page has no valid parent.
    /// Any exceptions during dialog creation are caught and logged via
    /// <c>ExceptionHandler.ThrowUnexpectedException</c>.
    /// </para>
    /// </remarks>
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
            parent.GetNodeOrNull<EditorFileDialog>(FileDialogName)?.Free();

            EditorFileDialog fileDialog = new EditorFileDialog();
            fileDialog.Name = FileDialogName;
            fileDialog.FileMode = EditorFileDialog.FileModeEnum.OpenFiles;
            fileDialog.Access = EditorFileDialog.AccessEnum.Resources;
            fileDialog.Title = title;

            if (string.IsNullOrEmpty(fileDialogDir) || !DirAccess.DirExistsAbsolute(fileDialogDir))
            {
                fileDialogDir = "res://";
            }
            fileDialog.CurrentDir = fileDialogDir;

            fileDialog.AddFilter(filter, description);

            fileDialog.FilesSelected += RememberDir;
            fileDialog.FilesSelected += OnSceneFilesSelected;
            fileDialog.Canceled += FreeFileDialog;

            parent.AddChild(fileDialog);
            fileDialog.PopupCentered(new Vector2I(800, 600));
        }
        catch (Exception ex)
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{GetType().Name} {nameof(SetupFileDialog)}");
        }
    }

    private void RememberDir(string[] paths)
    {
        if (paths != null && paths.Length > 0)
            fileDialogDir = paths[0].GetBaseDir();
        GetParent()?.GetNodeOrNull<EditorFileDialog>(FileDialogName)?.QueueFree();
    }

    private void FreeFileDialog()
    {
        GetParent()?.GetNodeOrNull<EditorFileDialog>(FileDialogName)?.QueueFree();
    }
}
#endif
