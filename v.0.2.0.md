# ScenePaletter v0.2.0 – Extrapolation Methods, Viewport Preview & Global Classes

**Predict, preview and place more than just scenes**

---

### What's New in v0.2.0?

This update adds smarter placement, visual feedback in the viewport and support for C# classes. It also moves the shared plugin code into its own addon.

**More Extrapolation Methods**:

* Pick how the next pose is calculated with the new **Method** dropdown on the placing page:
  * **Linear** – constant step, `2 × last − previous` (the behavior of earlier versions)
  * **Quadratic** – constant second difference, `3 × last − 3 × previous + oldest`
  * **Exponential** – the step grows or shrinks by the ratio of the last two steps (growth is capped)
  * **Circular** – continues on the circle through the last three points
* Extrapolation now covers **position, rotation and scale**, in both 2D and 3D.
* With too few placed nodes the placer falls back gracefully: Linear, then a copy of the single node, then the parent's pose. Freed or undone nodes are skipped.

**Placement Preview**:

* New **Preview** checkbox draws an overlay in the 2D and 3D viewports.
* It shows the chain of nodes the placer extrapolates from and a marker at the predicted next position.

**Global Class Support**:

* Palettes can now hold C# scripts marked `[GlobalClass]` as well as scenes. They are placed as new nodes, with the icon of their base type.
* Invalid entries (abstract classes, non-Node types, project not built) are rejected with a clear message.

**Placement Fixes & Improvements**:

* A selection inside an instanced sub-scene now places under the scene root instead of failing.
* Placing a scene into itself is detected and refused.
* Placement is a single undoable editor action.
* Fixed problems when the plugin is disabled or unloaded (leftover docks, resources and cached data).

**Under the Hood**:

* The reusable page, dock and config code now lives in its own addon, `addons/plugin_framework`.
* Plugin-specific settings are read by `PaletterSettings`.
* New palette ID range: `100000000` – `999999999`. Existing palettes keep their IDs.

---

## Upgrading from v0.1.x

* Copy **both** `addons/scene_paletter` and `addons/plugin_framework` into your project's `addons/` directory.
* Build the project once so the C# code compiles, then re-enable the plugin in Project Settings → Plugins.
* If you edited `plugin.cfg`, merge your changes into the new file. It has the new `widget_path` and `initial_docks` entries.

---

## Installation
### Option 1: Godot Asset Library (Recommended)

- Open the Godot Editor.
- Go to the AssetLib tab.
- Search for "Scene Paletter" and click Download → Install.
- Build the project.
- Enable the plugin in Project Settings → Plugins.


### Option 2: Manual Installation

Download the latest release from [GitHub Releases](https://github.com/Flopper907/Scene_Paletter/releases).
Extract the addons/scene_paletter and addons/plugin_framework folders into your project's addons/ directory.
Build the project.
Enable the plugin in Project Settings → Plugins.

## Demo & Screenshots
[ScenePaletter Demo](https://github.com/Flopper907/Scene_Paletter/releases/screenshots/demo.gif)
Demo uses assets from [Kenney Tiny Battle Pack](https://kenney.nl/assets/tiny-battle-pack) (CC0).

For full details, check the [README](https://github.com/Flopper907/Scene_Paletter#readme).

## License & Credits
ScenePaletter is licensed under the MIT License—free for personal and commercial use.

Special thanks to:

[Kenney](https://kenney.nl/) for the free CC0 assets used in the demo.
The Godot community for inspiration and support.

 Happy scene placing!
— Flopper907
