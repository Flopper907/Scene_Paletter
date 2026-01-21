using System;
using Addons.ScenePaletter.Tools;
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
    }

    public override void _ExitTree()
    {
        DisposeToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        DisposeToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);
        CloseWindow();
    }

    /* ============== Structs ============== */

    public struct Config
    {
        public string WidgetPath;
        public string PalettePath;
        public string FileExtension;
        public int IdStart;
        public int IdEnd;
        public string StartState;
        public int MinColums;
        public int MaxColums;
        public int Columns;
        public Vector2I PreviewResolution;
        public Vector2 PreviewMargin;
        public bool PreviewTransparent;

        public void AddColumn()
        {
            Columns = Math.Min(MaxColums, Columns + 1);
        }

        public void RemoveColumn()
        {
            Columns = Math.Max(MinColums, Columns - 1);
        }
    }

    /* ============== Init/Dispose ============== */

    private void InitConfig()
    {
        configFile = new ConfigFile();
        configFile.Load("res://addons/scene_paletter/plugin.cfg");

        config = new Config();
        config.WidgetPath = (string)configFile.GetValue("file", "widget_path");
        config.PalettePath = (string)configFile.GetValue("file", "palette_path");
        config.FileExtension = (string)configFile.GetValue("file", "file_extension");
        config.IdStart = (int)configFile.GetValue("file", "id_start");
        config.IdEnd = (int)configFile.GetValue("file", "id_end");

        config.StartState = (string)configFile.GetValue("state", "start_state");
        statePaths = (Dictionary<string, string>)configFile.GetValue("state", "states");

        config.MaxColums = (int)configFile.GetValue("ui", "max_columns");
        config.MinColums = (int)configFile.GetValue("ui", "min_columns");
        config.Columns = Math.Clamp(2, config.MinColums, config.MaxColums);
        config.PreviewResolution = new Vector2I(
            (int)configFile.GetValue("ui", "preview_resolution_x"),
            (int)configFile.GetValue("ui", "preview_resolution_y")
        );
        config.PreviewMargin = new Vector2(
            (float)configFile.GetValue("ui", "preview_margin_x"),
            (float)configFile.GetValue("ui", "preview_margin_x")
        );
        config.PreviewTransparent = (bool)configFile.GetValue("ui", "preview_transparent");
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
        InitConfig();
        InitStates();
        InitWindow();
        SwitchState(config.StartState, null);
    }

    private void CloseWindow()
    {
        if (!IsInstanceValid(panel)) return;
        DisposeStates();
        DisposeWindow();
        DisposeConfig();
        ScenePreviewGenerator.ClearCache();
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