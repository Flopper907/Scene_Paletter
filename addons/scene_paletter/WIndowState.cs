using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter;

// public abstract partial class WindowState<T> where T : WindowStateData
// {
//     public string Title { get; protected set; } = "Undefined";
//     protected Plugin plugin;
//     protected T data;
//     protected PanelContainer headerBar;
//     protected VBoxContainer contentArea;
//     protected PanelContainer footerBar;

//     public List<Control> controls { get; protected set; }

//     public WindowState(Plugin plugin)
//     {
//         this.plugin = plugin;
//     }

//     public virtual void Initialize(T data)
//     {
//         this.data = data;
//     }
//     public abstract void Generate();
// }

// Add this new base class
public abstract partial class WindowStateBase
{
    public string Title { get; protected set; } = "Undefined";
    protected Plugin plugin;

    public List<Control> controls { get; protected set; }

    protected WindowStateBase(Plugin plugin)
    {
        this.plugin = plugin;
    }

    public abstract void InitializeBase(WindowStateData data);
    public abstract void Generate();
}

// Then modify your generic class to inherit from it
public abstract partial class WindowState<T> : WindowStateBase where T : WindowStateData
{
    protected T data;
    protected PanelContainer headerBar;
    protected VBoxContainer contentArea;
    protected PanelContainer footerBar;

    public WindowState(Plugin plugin) : base(plugin)
    {
    }

    public virtual void Initialize(T data)
    {
        this.data = data;
    }

    public override void InitializeBase(WindowStateData data)
    {
        if (data is T typedData)
        {
            Initialize(typedData);
        }
    }
}