using Godot;
using Addons.PluginFramework.Core;
using Addons.ScenePaletter.Tools;

namespace Addons.ScenePaletter.Dialogs;

[Tool]
public partial class StartToolDialog : Page<object, Plugin>
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