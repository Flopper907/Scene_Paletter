using Godot;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        GD.Print("EmptyEditorPlugin enabled");
    }

    public override void _ExitTree()
    {
        GD.Print("EmptyEditorPlugin disabled");
    }
}