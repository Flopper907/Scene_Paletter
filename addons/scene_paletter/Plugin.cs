using System;
using Godot;
using System.Collections.Generic;

namespace Addons.ScenePaletter;

[Tool]
public partial class Plugin : EditorPlugin
{
    public Dictionary<UIPosition, PageDock> docks;
    public ConfigFile configFile;
    public Config config;

    private Godot.Collections.Dictionary<string, string> scenePaths;
    public Dictionary<string, PackedScene> Scenes;

    private Godot.Collections.Dictionary<string, string> startingDockPages;
    public Dictionary<UIPosition, string> StartingDocks;

    private PopupPanel dialogWindow;
    private Button toolbarButton2D;
    private Button toolbarButton3D;
    private Action toolbarButtonAction;

    public override void _EnterTree()
    {
        InitToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        InitToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);
        InitDocks();
        InitConfig();
        InitScenes();
        InitStartingDocks();
    }

    public override void _Ready()
    {
        foreach (var item in StartingDocks)
        {
            StartDock(item.Key, item.Value);
        }
    }

    public override void _ExitTree()
    {
        DisposeToolbarButton(ref toolbarButton2D, CustomControlContainer.CanvasEditorMenu);
        DisposeToolbarButton(ref toolbarButton3D, CustomControlContainer.SpatialEditorMenu);
        DisposeStartingDocks();
        DisposeDocks();
        DisposeConfig();
        DisposeScenes();
    }

    /* ============== Structs ============== */

    public struct Config
    {
        public string WidgetPath;
        public string PalettePath;
        public string FileExtension;
        public int IdStart;
        public int IdEnd;
        public int MinColums;
        public int MaxColums;
        public int Columns;
        public Vector2I PreviewResolution;
        public Vector2 PreviewMargin;
        public bool PreviewTransparent2D;
        public bool PreviewTransparent3D;

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

        scenePaths = (Godot.Collections.Dictionary<string, string>)configFile.GetValue("page", "pages");
        startingDockPages = (Godot.Collections.Dictionary<string, string>)configFile.GetValue("page", "start_docks");

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
        config.PreviewTransparent2D = (bool)configFile.GetValue("ui", "preview_2d_transparent");
        config.PreviewTransparent3D = (bool)configFile.GetValue("ui", "preview_3d_transparent");
    }

    private void DisposeConfig()
    {
        if (configFile == null) return;
        configFile.Dispose();
        configFile = null;
    }

    private void InitScenes()
    {
        if (configFile != null)
        {
            Scenes = new Dictionary<string, PackedScene>();
            foreach (var item in scenePaths)
            {
                Scenes[item.Key] = GD.Load<PackedScene>(item.Value);
            }
        }
    }

    private void DisposeScenes()
    {
        Scenes = new Dictionary<string, PackedScene>();
    }

    private void InitStartingDocks()
    {
        var startStates = (Godot.Collections.Dictionary)configFile.GetValue("page", "start_docks");
        StartingDocks = new Dictionary<UIPosition, string>();
        foreach (var item in startStates)
        {
            if (Enum.TryParse<UIPosition>(item.Key.ToString(), out var pos))
            {
                StartingDocks[pos] = (string)item.Value;
            }
        }
    }

    private void DisposeStartingDocks()
    {
        StartingDocks = new Dictionary<UIPosition, string>();
    }

    private void InitDocks()
    {
        docks = new Dictionary<UIPosition, PageDock>();

        foreach (UIPosition pos in Enum.GetValues<UIPosition>())
        {
            docks[pos] = null;
        }
    }

    private void DisposeDocks()
    {
        foreach (var item in docks)
        {
            if (item.Value != null && IsInstanceValid(item.Value))
            {
                CloseDock(item.Key);
            }
        }
        docks = new Dictionary<UIPosition, PageDock>();
    }

    private void InitToolbarButton(ref Button button, CustomControlContainer container)
    {
        if (IsInstanceValid(button)) return;

        toolbarButtonAction = () =>
        {
            if (docks[UIPosition.RightDockTopLeft] == null || !IsInstanceValid(docks[UIPosition.RightDockTopLeft]))
            {
                StartDock(UIPosition.RightDockTopLeft, "PalettePage");
            }
            else
            {
                ChangeDockPosition(UIPosition.RightDockTopLeft, UIPosition.Dialog);
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

    /* ============== Dock Helpers ============== */
    public void ChangeDockPosition(UIPosition from, UIPosition to)
    {
        if (docks[from] == null || !IsInstanceValid(docks[from]))
            return;

        // If destination has a dock, close it
        if (docks[to] != null && IsInstanceValid(docks[to]))
        {
            CloseDock(to);
        }

        PageDock dock = docks[from];
        RemoveDockFromPosition(dock, from);
        docks[from] = null;

        docks[to] = dock;
        SetDockToPosition(dock, to);
    }

    public void ReloadDock(UIPosition uiPosition, object data)
    {
        if (docks[uiPosition] == null || !IsInstanceValid(docks[uiPosition])) return;
        docks[uiPosition].Reload(data);
    }

    public void StartDock(UIPosition uiPosition, string page, object data)
    {
        if (docks[uiPosition] != null && IsInstanceValid(docks[uiPosition])) return;
        PageDock dock = new PageDock(this);
        dock.Name = "Name";
        docks[uiPosition] = dock;
        SetDockToPosition(dock, uiPosition);
        dock.SwitchPage(page, data);
    }

    public void StartDock(UIPosition uiPosition, string page)
    {
        StartDock(uiPosition, page, null);
    }

    public void CloseDock(UIPosition uiPosition)
    {
        if (docks[uiPosition] == null || !IsInstanceValid(docks[uiPosition])) return;
        RemoveDockFromPosition(docks[uiPosition], uiPosition);
        docks[uiPosition].QueueFree();
        docks[uiPosition] = null;
    }

    public void SetDialogSize(Vector2I size)
    {
        if (docks[UIPosition.Dialog] == null || !IsInstanceValid(docks[UIPosition.Dialog]))
            return;

        if (docks[UIPosition.Dialog].GetParent() is PopupPanel popup)
        {
            popup.Size = size;
        }
    }

    private void SetDockToPosition(Control dock, UIPosition uiPosition)
    {
        switch (uiPosition)
        {
            case UIPosition.Dialog:
                Control dialogContent = dock;

                PopupPanel window = new PopupPanel();
                window.Size = new Vector2I(400, 300);
                window.Borderless = false;
                window.Unresizable = false;

                window.AddChild(dialogContent);

                dialogContent.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
                dialogContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);

                window.PopupHide += () =>
                {
                    CloseDock(UIPosition.Dialog);
                };

                EditorInterface.Singleton.GetBaseControl().AddChild(window);
                window.PopupCentered();

                dialogWindow = window;
                break;
            case UIPosition.BottomPanel:
                AddControlToBottomPanel(dock, dock.Name);
                break;
            case UIPosition.Editor2DToolBar:
                AddControlToContainer(CustomControlContainer.CanvasEditorMenu, dock);
                break;
            case UIPosition.Editor2DLeft:
                AddControlToContainer(CustomControlContainer.CanvasEditorSideLeft, dock);
                break;
            case UIPosition.Editor2DRight:
                AddControlToContainer(CustomControlContainer.CanvasEditorSideRight, dock);
                break;
            case UIPosition.Editor2DBottom:
                AddControlToContainer(CustomControlContainer.CanvasEditorBottom, dock);
                break;
            case UIPosition.Editor3DToolBar:
                AddControlToContainer(CustomControlContainer.SpatialEditorMenu, dock);
                break;
            case UIPosition.Editor3DLeft:
                AddControlToContainer(CustomControlContainer.SpatialEditorSideLeft, dock);
                break;
            case UIPosition.Editor3DRight:
                AddControlToContainer(CustomControlContainer.SpatialEditorSideRight, dock);
                break;
            case UIPosition.Editor3DBottom:
                AddControlToContainer(CustomControlContainer.SpatialEditorBottom, dock);
                break;
            case UIPosition.InspectorBottom:
                AddControlToContainer(CustomControlContainer.InspectorBottom, dock);
                break;
            case UIPosition.ProjectSettingLeft:
                AddControlToContainer(CustomControlContainer.ProjectSettingTabLeft, dock);
                break;
            case UIPosition.ProjectSettingRight:
                AddControlToContainer(CustomControlContainer.ProjectSettingTabRight, dock);
                break;
            case UIPosition.LeftDockTopLeft:
                AddControlToDock(DockSlot.LeftUl, dock);
                break;
            case UIPosition.LeftDockTopRight:
                AddControlToDock(DockSlot.LeftUr, dock);
                break;
            case UIPosition.LeftDockBottomLeft:
                AddControlToDock(DockSlot.LeftBl, dock);
                break;
            case UIPosition.LeftDockBottomRight:
                AddControlToDock(DockSlot.LeftBr, dock);
                break;
            case UIPosition.RightDockTopLeft:
                AddControlToDock(DockSlot.RightUl, dock);
                break;
            case UIPosition.RightDockTopRight:
                AddControlToDock(DockSlot.RightUr, dock);
                break;
            case UIPosition.RightDockBottomLeft:
                AddControlToDock(DockSlot.RightBl, dock);
                break;
            case UIPosition.RightDockBottomRight:
                AddControlToDock(DockSlot.RightBr, dock);
                break;
        }
    }

    private void RemoveDockFromPosition(Control dock, UIPosition uiPosition)
    {
        switch (uiPosition)
        {
            case UIPosition.Dialog:
                if (IsInstanceValid(dialogWindow))
                {
                    dialogWindow.QueueFree();
                    dialogWindow = null;
                }
                break;

            case UIPosition.BottomPanel:
                RemoveControlFromBottomPanel(dock);
                break;

            case UIPosition.Editor2DToolBar:
                RemoveControlFromContainer(CustomControlContainer.CanvasEditorMenu, dock);
                break;

            case UIPosition.Editor2DLeft:
                RemoveControlFromContainer(CustomControlContainer.CanvasEditorSideLeft, dock);
                break;

            case UIPosition.Editor2DRight:
                RemoveControlFromContainer(CustomControlContainer.CanvasEditorSideRight, dock);
                break;

            case UIPosition.Editor2DBottom:
                RemoveControlFromContainer(CustomControlContainer.CanvasEditorBottom, dock);
                break;

            case UIPosition.Editor3DToolBar:
                RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, dock);
                break;

            case UIPosition.Editor3DLeft:
                RemoveControlFromContainer(CustomControlContainer.SpatialEditorSideLeft, dock);
                break;

            case UIPosition.Editor3DRight:
                RemoveControlFromContainer(CustomControlContainer.SpatialEditorSideRight, dock);
                break;

            case UIPosition.Editor3DBottom:
                RemoveControlFromContainer(CustomControlContainer.SpatialEditorBottom, dock);
                break;

            case UIPosition.InspectorBottom:
                RemoveControlFromContainer(CustomControlContainer.InspectorBottom, dock);
                break;

            case UIPosition.ProjectSettingLeft:
                RemoveControlFromContainer(CustomControlContainer.ProjectSettingTabLeft, dock);
                break;

            case UIPosition.ProjectSettingRight:
                RemoveControlFromContainer(CustomControlContainer.ProjectSettingTabRight, dock);
                break;
            case UIPosition.LeftDockTopLeft:
            case UIPosition.LeftDockTopRight:
            case UIPosition.LeftDockBottomLeft:
            case UIPosition.LeftDockBottomRight:
            case UIPosition.RightDockTopLeft:
            case UIPosition.RightDockTopRight:
            case UIPosition.RightDockBottomLeft:
            case UIPosition.RightDockBottomRight:
                RemoveControlFromDocks(dock);
                break;
        }
    }
}

public enum UIPosition
{
    // Special
    Dialog,
    BottomPanel,

    // 3D Viewport
    Editor3DToolBar,
    Editor3DLeft,
    Editor3DRight,
    Editor3DBottom,

    // 2D Viewport
    Editor2DToolBar,
    Editor2DLeft,
    Editor2DRight,
    Editor2DBottom,

    // Inspector
    InspectorBottom,

    // Project Settings
    ProjectSettingLeft,
    ProjectSettingRight,

    // Dock
    LeftDockTopLeft,
    LeftDockTopRight,
    LeftDockBottomLeft,
    LeftDockBottomRight,
    RightDockTopLeft,
    RightDockTopRight,
    RightDockBottomLeft,
    RightDockBottomRight
}