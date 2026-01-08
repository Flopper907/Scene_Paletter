using System.Collections.Generic;
using Addons.ScenePaletter.States;
using Addons.ScenePaletter.Tools;
using Godot;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public ConfigFile configFile;
    public Config config;
    public Palette palette;
    public string state;

    private Dictionary<string, WindowState> states;
    private Window panel;
    private Button toolbarButton;

    public override void _EnterTree()
    {
        InitToolbarButton();
    }

    public override void _ExitTree()
    {
        DisposeToolbarButton();
        CloseWindow();
    }

    /* ============== Structs ============== */

    public struct Config
    {
        public string PalettePath;
        public string FileExtension;
        public string StartState;
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
        config.FileExtension = (string)configFile.GetValue("file", "file_extension");
        config.IdStart = (int)configFile.GetValue("file", "id_start");
        config.IdEnd = (int)configFile.GetValue("file", "id_end");
        config.StartState = (string)configFile.GetValue("state", "start_state");
    }

    private void DisposeConfig()
    {
        configFile = null;
        config = new Config();
        config.PalettePath = "";
        config.FileExtension = "";
    }

    private void InitStates()
    {
        states = new Dictionary<string, WindowState>
        {
            {"PaletteList", new PaletteList(this)},
            {"Editing", new Editing(this)},
            {"Placing", new Placing(this)},
        };
        state = config.StartState;
        SwitchState(state);
    }

    private void DisposeStates()
    {
        states = new Dictionary<string, WindowState>();
        state = "";
    }

    private void InitWindow()
    {
        panel = new Window();
        panel.Name = "Scene Palette";
        AddControlToDock(DockSlot.RightUl, panel);
    }

    private void DisposeWindow()
    {
        if (!IsInstanceValid(panel)) return;
        RemoveControlFromDocks(panel);
        panel.QueueFree();
    }

    private void InitToolbarButton()
    {
        toolbarButton = new Button();
        toolbarButton.Text = "Scene Palette";
        toolbarButton.Pressed += () =>
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
        toolbarButton.Icon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon("Node", "EditorIcons");
        AddControlToContainer(CustomControlContainer.CanvasEditorMenu, toolbarButton);
    }

    private void DisposeToolbarButton()
    {
        RemoveControlFromContainer(CustomControlContainer.CanvasEditorMenu, toolbarButton);
    }

    /* ============== Window Helpers ============== */

    private void StartWindow()
    {
        if (IsInstanceValid(panel)) return;
        InitConfig();
        InitWindow();
        InitStates();
    }

    private void CloseWindow()
    {
        if (!IsInstanceValid(panel)) return;
        DisposeConfig();
        DisposeWindow();
        DisposeStates();
    }

    /* ============== Management ============== */

    public void LoadPalette(string name)
    {
        Palette palette = SaveLoad.Load<Palette>(config.PalettePath + name + config.FileExtension);
        palette.Name = name;
        this.palette = palette;
    }

    public void SavePalette(Palette palette)
    {
        SaveLoad.Save(palette, config.PalettePath + palette.Name + config.FileExtension);
    }

    public void SwitchState(string stateName)
    {
        if (panel == null) return;
        if (states.ContainsKey(stateName))
        {
            panel.SwitchToState(states[stateName]);
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