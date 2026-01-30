using Godot;
using Addons.ScenePaletter.Core;
using Addons.ScenePaletter.Tools;

namespace Addons.ScenePaletter.Dialogs;

[Tool]
public partial class StartToolDialog : Page<object>
{
    public override void Initialize()
    {
    }

    public void ToggleWindow()
    {
        if (!plugin.dockManager.IsDockInstanced(UIPosition.RightDockTopLeft))
        {
            plugin.dockManager.StartDock(UIPosition.RightDockTopLeft, "PalettePage");
            ScenePreviewGenerator.ClearCache();
        }
        else
        {
            plugin.dockManager.CloseDock(UIPosition.RightDockTopLeft);
        }
    }
}