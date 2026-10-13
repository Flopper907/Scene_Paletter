#if TOOLS
namespace Addons.PluginFramework.Core;

/// <summary>
/// Editor locations a <c>PageDock</c> can be placed at. Used as keys of the <c>initial_docks</c>
/// dictionary in <c>plugin.cfg</c> (by enum name, e.g. <c>"RightDockTopLeft"</c>) and by <c>DockManager.StartDock</c>.
/// </summary>
public enum UIPosition
{
    // Special

    /// <summary>A popup window centered in the editor.</summary>
    Dialog,

    /// <summary>The bottom panel (next to Output, Debugger, ...).</summary>
    BottomPanel,

    // 3D Viewport

    /// <summary>The 3D viewport toolbar.</summary>
    Editor3DToolBar,
    /// <summary>The left side of the 3D viewport.</summary>
    Editor3DLeft,
    /// <summary>The right side of the 3D viewport.</summary>
    Editor3DRight,
    /// <summary>The bottom of the 3D viewport.</summary>
    Editor3DBottom,

    // 2D Viewport

    /// <summary>The 2D viewport toolbar.</summary>
    Editor2DToolBar,
    /// <summary>The left side of the 2D viewport.</summary>
    Editor2DLeft,
    /// <summary>The right side of the 2D viewport.</summary>
    Editor2DRight,
    /// <summary>The bottom of the 2D viewport.</summary>
    Editor2DBottom,

    // Inspector

    /// <summary>The bottom of the inspector.</summary>
    InspectorBottom,

    // Project Settings

    /// <summary>The left side of the Project Settings tab bar.</summary>
    ProjectSettingLeft,
    /// <summary>The right side of the Project Settings tab bar.</summary>
    ProjectSettingRight,

    // Dock (uses EditorDock + DockSlot)

    /// <summary>Left dock area, upper-left slot.</summary>
    LeftDockTopLeft,
    /// <summary>Left dock area, upper-right slot.</summary>
    LeftDockTopRight,
    /// <summary>Left dock area, bottom-left slot.</summary>
    LeftDockBottomLeft,
    /// <summary>Left dock area, bottom-right slot.</summary>
    LeftDockBottomRight,
    /// <summary>Right dock area, upper-left slot.</summary>
    RightDockTopLeft,
    /// <summary>Right dock area, upper-right slot.</summary>
    RightDockTopRight,
    /// <summary>Right dock area, bottom-left slot.</summary>
    RightDockBottomLeft,
    /// <summary>Right dock area, bottom-right slot.</summary>
    RightDockBottomRight
}
#endif
