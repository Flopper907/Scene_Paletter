# ScenePaletter v0.1.2 – Smarter Placement & Faster Workflow
**Place scenes faster with drag & drop and rotation extrapolation**

---

### What's New in v0.1.2?

This update focuses on speeding up your scene placement workflow with two meaningful improvements:

**Drag & Drop Multi-Scene Import**:
- You can now **drag and drop multiple scenes at once** directly into the palette — no more adding them one by one through the file dialog.

**Rotation Extrapolation**:
- The placement system now **extrapolates rotation** alongside position when placing scenes sequentially.
- Just like position, rotation follows the formula `2 × last − previous`, so each new scene continues the rotational pattern of the previous two — great for placing objects along curves or at consistent angles.
- Works for both **2D** (single radian value) and **3D** (per-axis Euler angles).
---

## Installation
### Option 1: Godot Asset Library (Recommended)

- Open the Godot Editor.
- Go to the AssetLib tab.
- Search for "Scene Paletter" and click Download → Install.
- Enable the plugin in Project Settings → Plugins.


### Option 2: Manual Installation

Download the latest release from [GitHub Releases](https://github.com/Flopper907/Scene_Paletter/releases).
Extract the addons/scene_paletter folder into your project’s addons/ directory.
Enable the plugin in Project Settings → Plugins.

## Demo & Screenshots
[ScenePaletter Demo](https://github.com/Flopper907/Scene_Paletter/releases/screenshots/demo.gif)
Demo uses assets from [Kenney Tiny Battle Pack](https://kenney.nl/assets/tiny-battle-pack) (CC0).

## Key Features & Documentation

Modular Architecture: Built for extensibility.
Fully Documented: XML docs for all classes/methods.
Error-Resistant: Comprehensive validation and logging.
Configurable: Adjust grid layout, preview settings, and more via plugin.cfg.
For full details, check the [README](https://github.com/Flopper907/Scene_Paletter#readme).

## For Developers
ScenePaletter is open for extensions! You can:

Create custom pages (inherit from Page<T>).
Add new config sections.
Adapt the plugin to your specific needs.
Contributions welcome!
Fork the repo, create a branch, and submit a pull request.

## Technical Foundation

ScenePaletter is built on top of [Empty_Plugin](https://github.com/Flopper907/Empty_Plugin), a reusable Godot plugin template I created that provides:
- Core plugin architecture
- Configuration system
- Scene loading infrastructure
- Dock management foundation

Both projects are MIT licensed and available for developers to use as starting points for their own plugins.


## License & Credits
ScenePaletter is licensed under the MIT License—free for personal and commercial use.

Special thanks to:

[Kenney](https://kenney.nl/) for the free CC0 assets used in the demo.
The Godot community for inspiration and support.

## Spread the Word!
Like ScenePaletter? Help others discover it!

## Star the repo on GitHub.
Rate it on the Godot Asset Library.
Share it with your dev friends!


 Happy scene placing!
— Flopper907