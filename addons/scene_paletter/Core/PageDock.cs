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
        if (plugin == null || !plugin.sceneLoader.HasPage(page)) return;
        Clear();
        this.page = page;
        data = pageData;
        node = plugin.sceneLoader.GetPage(page).Instantiate() as Control;

        AddChild(node);
        CallDeferred(MethodName.UpdateName);
    }

    public void Reload(object pageData)
    {
        SwitchPage(page, pageData);
    }

    private void UpdateName()
    {
        if (node == null || !IsInstanceValid(node)) return;

        string title = "";
        try { title = node.Get("Title").AsString(); } catch { }

        Name = !string.IsNullOrEmpty(title) ? title : (page ?? "PageDock");
    }
}