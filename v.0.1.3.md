# ScenePaletter v0.1.3 – Godot 4.7, Native Drag & Drop & Improved Stability

**Place scenes faster with a more robust and responsive workflow**

---

### What's New in v0.1.3?

This update focuses on modernizing the interaction model and fixing core stability issues:

**Native Drag & Drop Placement**:

* Completely rewritten drag-and-drop logic. Scenes now use Godot’s native file-drag format, allowing you to drag them directly into your 2D/3D viewports with perfect accuracy.

**Refined Interaction Model**:

* Removed "invisible button" workarounds on both Placing and Editing pages.
* Switched to native `_GuiInput` handling for selections, resulting in a snappier, conflict-free, and more reliable input response.

**Improved Stability**:

* Fixed issues regarding JSON cache clearing to ensure your palette data remains consistent and reliable during session changes.

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