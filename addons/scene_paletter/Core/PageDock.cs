using System;
using Godot;

namespace Addons.ScenePaletter.Core;

public partial class PageDock : VBoxContainer
{
    private Control node;
    private UIPosition position;
    public Plugin plugin { get; private set; }

    public string page { get; private set; }
    public object data { get; private set; }

    public PageDock(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public void Clear()
    {
        if (IsInstanceValid(node))
        {
            RemoveChild(node);
            node.QueueFree();
        }
    }

    public void SwitchPage(string page, object pageData)
    {
        if (plugin == null)
        {
            ExceptionHandler.ThrowMissingPluginException($"Dock: {GetType().Name}");
            return;
        }
        if (plugin.sceneLoader == null)
        {
            ExceptionHandler.ThrowMissingSceneLoaderException($"Dock: {GetType().Name}");
            return;
        }
        if (!plugin.sceneLoader.HasPage(page))
        {
            ExceptionHandler.ThrowMissingPageException(page);
            return;
        }

        try
        {
            Clear();
            this.page = page;
            data = pageData;

            PackedScene scene = plugin.sceneLoader.GetPage(page);
            if (scene == null)
            {
                ExceptionHandler.ThrowResourceLoadException(page, nameof(SwitchPage));
                return;
            }

            node = scene.Instantiate() as Control;
            if (node == null)
            {
                ExceptionHandler.ThrowSceneInstantiationException(page, nameof(SwitchPage));
                return;
            }

            AddChild(node);
            CallDeferred(MethodName.UpdateName);
        }
        catch (Exception ex) when (!(ex is NullReferenceException)) // Ignore expected nulls
        {
            ExceptionHandler.ThrowUnexpectedException(ex, $"{nameof(SwitchPage)} - {page}");
            if (node != null && IsInstanceValid(node))
            {
                node.QueueFree();
                node = null;
            }
        }
    }

    public void Reload(object pageData)
    {
        SwitchPage(page, pageData);
    }

    private void UpdateName()
    {
        if (node == null || !IsInstanceValid(node))
        {
            ExceptionHandler.ThrowMissingNodeException(GetPath(), nameof(UpdateName));
            return;
        }

        string title = "";
        try { title = node.Get("Title").AsString(); } catch { }

        Name = !string.IsNullOrEmpty(title) ? title : (page ?? "PageDock");
    }
}