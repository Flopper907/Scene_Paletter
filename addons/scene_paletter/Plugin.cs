using System;
using System.Collections.Generic;
using Addons.ScenePaletter.States;
using Godot;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public ConfigFile configFile;
    public Config config;
    private Dictionary<string, WindowState> states;
    public Palette palette;
    public string state;
    private Window panel;

    public override void _EnterTree()
    {
        try
        {
            InitConfig();
            InitWindow();
            InitStates();
        }
        catch(Exception e)
        {
            GD.PrintErr(e);
        }
    }

    public override void _ExitTree()
    {
        if (panel != null)
        {
            RemoveControlFromDocks(panel);
            panel.QueueFree();
        }

        GD.Print("Plugin disabled");
    }

    /* ============== Structs ============== */

    public struct Config
    {
        public string PalettePath;
        public string FileExtension;
    }

    /* ============== Management ============== */

    private void InitConfig()
    {
        configFile = new ConfigFile();
        configFile.Load("res://addons/scene_paletter/plugin.cfg");
        config = new Config();
        config.PalettePath = (string)configFile.GetValue("file_management","palette_path");
        config.FileExtension = (string)configFile.GetValue("file_management","file_extension");
    }
    private void InitStates()
    {
        states = new Dictionary<string, WindowState>
        {
            {"Init",new Init(this)},
            {"Loaded",new Loaded(this)},
        };
        state = "Init";
        SwitchState(state);
    }

    private void InitWindow()
    {
        panel = new Window();
        panel.Name = "Scene Palette";
        AddControlToDock(DockSlot.RightUl, panel);
    }

    /* ============== Helpers ============== */

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
        if (states.ContainsKey(stateName))
        {
            panel.SwitchToState(states[stateName]);
        }
    }
}