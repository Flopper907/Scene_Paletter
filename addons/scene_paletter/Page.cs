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
}