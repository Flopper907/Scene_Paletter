using System.Collections.Generic;
using Addons.ScenePaletter;

public partial class EditingData : WindowStateData
{
    public float scroll = 0;
    public Palette palette;
    public Palette old;
    public Dictionary<string, int> selectedElements = new Dictionary<string, int>();
    public bool changed = false;
}