# Scene Paletter

A Godot 4.7 editor plugin for rapid scene placement using customizable palettes. Perfect for level design, prototyping, and any workflow requiring frequent scene instantiation.

**Note:** Demo uses assets from [Kenney Tiny Battle Pack](https://kenney.nl/assets/tiny-battle) (Creative Commons CC0).

---

## Features

- **Multiple Palettes** - Organize scenes into collections for different workflows
- **Scenes & Global Classes** - Palettes hold `.tscn` scenes as well as C# `[GlobalClass]` Node scripts
- **2D & 3D Support** - Seamlessly handles both Node2D and Node3D scenes
- **Smart Placement** - Position, rotation and scale extrapolation (Linear, Quadratic, Exponential, Circular)
- **Viewport Preview** - Overlay showing the chain of previous placements and the predicted next position
- **Drag & Drop** - Add scenes to a palette by dropping them in, or drag palette entries into the scene
- **Live Previews** - Generated thumbnails for all palette entries
- **Easy Management** - Add, remove, and organize entries with intuitive controls
- **Fully Documented** - Complete XML documentation for all classes and methods

---

## Installation

### From Godot Asset Library (Recommended)
1. Open Godot Editor
2. Go to **AssetLib** tab
3. Search for "Scene Paletter"
4. Click **Download** → **Install**
5. Build the project
   - Required on first installation to compile C# code
6. Enable the plugin in **Project Settings → Plugins**

### Manual Installation
1. Download the latest release from [Releases](https://github.com/Flopper907/Scene_Paletter/releases)
2. Extract the `addons/scene_paletter` **and** `addons/plugin_framework` folders to your project's `addons/` directory
3. Build the project
   - Required on first installation to compile C# code
4. Enable the plugin in **Project Settings → Plugins**

---

## Quick Start

### Creating Your First Palette

1. **Open the Plugin**
   - After enabling, Scene Paletter appears as a dock in the editor
   - Default location: Bottom panel or right dock area

2. **Create a Palette**
   - Click the **"Create New"** button
   - Give it a descriptive name (e.g., "Level Objects", "Enemies", "Props")

3. **Add Scenes**
   - Click **"Edit"**, then add `.tscn` files (or `[GlobalClass]` C# Node scripts) with the add button, or drag them from the FileSystem dock into the list
   - Preview images generate automatically

4. **Start Placing**
   - Select an entry from your palette
   - Click **"Place"**, or drag the entry into the scene
   - Each placement calculates the next pose from the previous placements

### Placement Behavior

The next pose (position, rotation and scale) is extrapolated from the nodes you placed earlier. Pick the
method in the **Method** dropdown on the placing page:

| Method | Needs | Behavior |
|--------|-------|----------|
| Linear | 2 nodes | Constant step: `2 × last - previous` |
| Quadratic | 3 nodes | Constant second difference: `3 × last - 3 × previous + oldest` |
| Exponential | 3 nodes | Step is multiplied by the (capped) ratio of the last two steps |
| Circular | 3 nodes | Continues on the circle through the last three points |

With fewer nodes the placer degrades to Linear, then to a copy of the single node, then to the parent's pose.
The first placement keeps the scene's authored scale.

Enable the **Preview** checkbox to see the chain of used nodes and the predicted next position in the viewport.

---

## Usage

### Managing Palettes

**Create Palette**
- Click **"Create New"** on the palette selection page
- Automatically assigns a unique ID

**Edit Palette**
- Select a palette → Click **"Edit"**
- Rename, add, or remove entries
- Multi-select entries for batch deletion
- Drag scene files into the list to add them

**Delete Palette**
- Click the **delete button** next to a palette
- Confirms before deletion

### Placing Scenes

**Basic Placement**
1. Select a palette
2. Click a scene to select it
3. Click **"Place"** (or drag the entry into the scene)
4. The node is instantiated at the calculated pose; placement is undoable

**Advanced Tips**
- Select a parent node before placing to control hierarchy
- Place under 2D/3D scene roots for proper type handling
- Nodes placed under an instanced sub-scene go under the scene root instead
- Placed nodes automatically get proper owner assignment for scene saving

### Customization

**Grid Layout**
- Use **+/-** buttons to adjust column count
- Configured in `plugin.cfg`:
```ini
  [ui]
  min_columns = 1
  max_columns = 4
  columns = 2
```

**Preview Settings**
- Adjust preview resolution and transparency:
```ini
  [ui]
  preview_resolution_x = 256
  preview_resolution_y = 256
  preview_2d_transparent = true
  preview_3d_transparent = false
```

---

## Configuration

Edit `addons/scene_paletter/plugin.cfg` to customize:

**Note:** After changing `plugin.cfg`, you must reload the plugin:

1. Go to **Project Settings → Plugins**
2. Disable Scene Paletter
3. Enable Scene Paletter again
4. Configuration changes will take effect

```ini
[file]
widget_path = "res://addons/scene_paletter/Widgets/"
palette_path = "res://addons/scene_paletter/Palettes/"
file_extension = ".json"
id_start = 100000000
id_end = 999999999

[page]
pages = {...}  # Internal page definitions
widgets = {...}  # Internal widget definitions
initial_docks = {...}  # Which page opens in which dock

[ui]
min_columns = 1
max_columns = 4
columns = 2
preview_resolution_x = 256
preview_resolution_y = 256
preview_margin_x = 1.05
preview_margin_y = 1.05
preview_2d_transparent = true
preview_3d_transparent = false
```

### Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `palette_path` | Directory where palettes are saved | `res://addons/scene_paletter/Palettes/` |
| `file_extension` | Palette file extension | `.json` |
| `id_start` / `id_end` | Range for unique palette ID generation | 100000000 - 999999999 |
| `min_columns` / `max_columns` | Grid column constraints | 1 - 4 |
| `columns` | Default column count | 2 |
| `preview_resolution_x/y` | Preview image size | 256x256 |
| `preview_margin_x/y` | Camera margin multiplier for previews | 1.05 |
| `preview_2d/3d_transparent` | Transparent preview backgrounds | true / false |

---

## Architecture

Built on a small reusable page/dock framework (`addons/plugin_framework`) plus the plugin itself:
```
addons/
├── plugin_framework/       # Reusable framework (separate addon)
│   ├── Core/                   # Page<T, TPlugin>, PageDock, ExceptionHandler, UIPosition
│   └── Management/             # ConfigLoader, DockManager, SceneLoader
│
└── scene_paletter/
    ├── Pages/                  # PalettePage, PlacingPage, EditingPage
    ├── Dialogs/                # DeleteDialog, StartToolDialog
    ├── Widgets/                # List items
    ├── Preview/                # Viewport placement preview (2D / 3D)
    ├── Tools/                  # Extrapolator2D/3D, PaletteEntry, SaveLoad, settings, ...
    └── Plugin.cs               # Main plugin entry point
```

### Key Design Patterns

- **Generic Page System**: Type-safe data passing between pages
- **Centralized Error Handling**: Non-throwing error logging via `ExceptionHandler`
- **Resource Management**: Proper disposal and lifecycle management
- **Preview Caching**: Asynchronous preview generation with caching

---

## Extending the Plugin

### Creating Custom Pages
```csharp
using Addons.PluginFramework.Core;

public struct MyPageData {
    public string someData;
}

[Tool]
public partial class MyPage : Page<MyPageData, Plugin>
{
    public override void Initialize()
    {
        // Access data via this.data
        // Access plugin via this.plugin
        // Access dock via this.dock
        
        Title = "My Custom Page";
        // ... setup your UI
    }
}
```

Add to `plugin.cfg`:
```ini
[page]
pages = {
    "MyPage": "uid://your_scene_uid"
}
```

### Adding Configuration Sections

1. Add properties to `PaletterSettings.cs`
2. Read them from the `ConfigLoader` in its constructor
3. Add the corresponding section to `plugin.cfg`

Plugin-specific keys are read in [PaletterSettings.cs](addons/scene_paletter/Tools/PaletterSettings.cs); see also [ConfigLoader.cs](addons/plugin_framework/Management/ConfigLoader.cs) for examples.

---

## Troubleshooting

### Plugin doesn't appear after enabling
- Restart Godot Editor
- Check console for error messages
- Verify `plugin.cfg` exists and is valid

### Scenes don't place correctly
- Ensure you're placing under a Node2D or Node3D parent
- Check that selected entries match the parent type (2D → 2D, 3D → 3D)
- Global classes must be `[GlobalClass]`, non-abstract Node types, and the project must be built
- Verify the edited scene root is valid

### Previews not generating
- Check console for resource loading errors
- Verify the scenes in the palette still exist (entries are stored as UIDs)
- Ensure preview resolution settings are reasonable (<1024px)

### Palettes not saving
- Check write permissions for `palette_path` directory
- Verify `file_extension` setting in config
- Look for serialization errors in console

---

## Development

Started as a learning project and a template for building plugins as a learning project and for building a template to easily build plugins.

### Tech Stack
- **Language**: C# (Godot 4.7)
- **Architecture**: Modular, documented, extensible
- **Error Handling**: Comprehensive logging without exceptions
- **Preview System**: Async generation with caching

### Building from Source
```bash
git clone https://github.com/Flopper907/Scene_Paletter.git
cd Scene_Paletter
# Copy addons/scene_paletter to your Godot project
```

### Contributing
Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Follow existing code style and documentation patterns
4. Test thoroughly in Godot 4.7
5. Submit a pull request

---

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- Built with [Godot Engine](https://godotengine.org/)
- Demo screenshots use [Kenney Tiny Battle Pack](https://kenney.nl/assets/tiny-battle) (Creative Commons CC0)
- Inspired by palette-based workflows in level editors

---

## Links

- **Asset Library**: [Scene Paletter on Godot Asset Library](https://godotengine.org/asset-library/asset/4730)
- **Issues**: [Report bugs or request features](https://github.com/Flopper907/Scene_Paletter/issues)

---

**Made with <3 by [Flopper907](https://github.com/Flopper907)**

