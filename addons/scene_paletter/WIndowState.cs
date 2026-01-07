using System.Collections.Generic;
using Godot;

namespace Addons.ScenePaletter;

public abstract partial class WindowState
{
    public string title { get; protected set; } = "Undefined";
    protected Plugin plugin;
    public List<Control> controls { get; protected set; }
    public WindowState(Plugin plugin)
    {
        this.plugin = plugin;
    }
    public virtual void Initialize() { }
    public abstract void Generate();
}