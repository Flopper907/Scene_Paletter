using System;
using Godot;
using Godot.Collections;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public object data;
    public ConfigFile configFile;
    public Config config;
    public string state;

    private Dictionary<string, string> statePaths;
    private Dictionary<string, PackedScene> states;
    private PageDock panel;
    private Button toolbarButton2D;
    private Button toolbarButton3D;
    private Action toolbarButtonAction;

    public override void _EnterTree()
    {
        InitToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        InitToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);
        InitConfig();
    }

    public override void _ExitTree()
    {
        DisposeToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        DisposeToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);
        DisposeConfig();
        CloseWindow();
    }

    /* ============== Structs ============== */

    public struct Config
    {
        public string PalettePath;
        public string WidgetPath;
        public string FileExtension;
        public string StartState;
        public int MinColums;
        public int MaxColums;
        public int Columns;
        public int IdStart;
        public int IdEnd;
    }

    /* ============== Init/Dispose ============== */

    private void InitConfig()
    {
        configFile = new ConfigFile();
        configFile.Load("res://addons/scene_paletter/plugin.cfg");

        config = new Config();
        config.PalettePath = (string)configFile.GetValue("file", "palette_path");
        config.WidgetPath = (string)configFile.GetValue("file", "widget_path");
        config.FileExtension = (string)configFile.GetValue("file", "file_extension");
        config.MinColums = (int)configFile.GetValue("state", "min_columns");
        config.MaxColums = (int)configFile.GetValue("state", "max_columns");
        config.Columns = Math.Clamp(2, config.MinColums, config.MaxColums);
        config.IdStart = (int)configFile.GetValue("file", "id_start");
        config.IdEnd = (int)configFile.GetValue("file", "id_end");
        config.StartState = (string)configFile.GetValue("state", "start_state");
        statePaths = (Dictionary<string, string>)configFile.GetValue("state", "states");
    }

    private void DisposeConfig()
    {
        if (configFile == null) return;
        configFile.Dispose();
        configFile = null;
        states = null;
    }

    private void InitStates()
    {
        if (configFile != null)
        {
            states = new Dictionary<string, PackedScene>();
            foreach (var item in statePaths)
            {
                states[item.Key] = GD.Load<PackedScene>(item.Value);
            }
        }
    }

    private void DisposeStates()
    {
        states = new Dictionary<string, PackedScene>();
    }

    private void InitWindow()
    {
        panel = new PageDock(this);
        panel.Name = "Scene Palette";
        AddControlToDock(DockSlot.RightUl, panel);
    }

    private void DisposeWindow()
    {
        if (!IsInstanceValid(panel)) return;
        RemoveControlFromDocks(panel);
        panel.QueueFree();
        panel = null;
    }

    private void InitToolbarButton(ref Button button, CustomControlContainer container)
    {
        if (IsInstanceValid(button)) return;

        toolbarButtonAction = () =>
        {
            if (!IsInstanceValid(panel))
            {
                StartWindow();
            }
            else
            {
                CloseWindow();
            }
        };
        button = new Button();
        button.Text = "Scene Palette";
        button.Pressed += toolbarButtonAction;
        button.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("Node", "EditorIcons");
        AddControlToContainer(container, button);
    }

    private void DisposeToolbarButton(ref Button button, CustomControlContainer container)
    {
        if (IsInstanceValid(button))
        {
            RemoveControlFromContainer(container, button);
            button.QueueFree();
        }
    }

    /* ============== Window Helpers ============== */

    private void StartWindow()
    {
        if (IsInstanceValid(panel)) return;
        InitStates();
        InitWindow();
        SwitchState(config.StartState, null);
    }

    private void CloseWindow()
    {
        if (!IsInstanceValid(panel)) return;
        DisposeStates();
        DisposeWindow();
    }

    /* ============== Management ============== */
    public void SwitchState(string stateName, object data)
    {
        if (panel == null) return;
        if (states.ContainsKey(stateName))
        {
            state = stateName;
            this.data = data;
            panel.SwitchToState(states[stateName]);
        }
    }

    public void ReloadState(object data)
    {
        if (panel == null) return;
        if (states.ContainsKey(state))
        {
            this.data = data;
            panel.SwitchToState(states[state]);
        }
    }
}



// POSITIONS
// ========== MAIN EDITOR ==========
//     AddControlToContainer(CustomControlContainer.Toolbar, toolbarButton);
//     // Main toolbar (top of editor, right side after Scene/Project/Debug/Editor/Help)

//     // ========== TOOL EDITOR ==========
//     AddControlToContainer(CustomControlContainer.SpatialEditorMenu, toolbarButton);
//     // 3D viewport top toolbar (where Select/Move/Rotate/Scale tools are)

//     AddControlToContainer(CustomControlContainer.CanvasEditorMenu, toolbarButton);
//     // 2D viewport top toolbar (where Select/Move/Rotate/Scale tools are)


//     AddControlToContainer(CustomControlContainer.SpatialEditorSideLeft, toolbarButton);
//     // 3D viewport left side panel

//     AddControlToContainer(CustomControlContainer.CanvasEditorSideLeft, toolbarButton);
//     // 2D viewport left side panel


//     AddControlToContainer(CustomControlContainer.SpatialEditorSideRight, toolbarButton);
//     // 3D viewport right side panel

//     AddControlToContainer(CustomControlContainer.CanvasEditorSideRight, toolbarButton);
//     // 2D viewport right side panel


//     AddControlToContainer(CustomControlContainer.SpatialEditorBottom, toolbarButton);
//     // 3D viewport bottom panel

//     AddControlToContainer(CustomControlContainer.CanvasEditorBottom, toolbarButton);
//     // 2D viewport bottom panel


//     // ========== PROPERTY INSPECTOR ==========
//     AddControlToContainer(CustomControlContainer.InspectorBottom, toolbarButton);
//     // Bottom of the Inspector panel

//     AddControlToContainer(CustomControlContainer.PropertyEditorBottom, toolbarButton);
//     // Bottom of the property editor section

//     // ========== PROJECT SETTINGS ==========
//     AddControlToContainer(CustomControlContainer.ProjectSettingTabLeft, toolbarButton);
//     // Left side of project settings tabs

//     AddControlToContainer(CustomControlContainer.ProjectSettingTabRight, toolbarButton);
//     // Right side of project settings tabs