using Godot;

namespace Addons.ScenePaletter.Tools;

public record PaletteEntryInfo(string Name, Resource Resource, string NativeBase, string Error);

public static class PaletteEntry
{
    public static bool TryValidateForAdd(string path, out string uid)
    {
        uid = null;
        long id = ResourceLoader.GetResourceUid(path);
        if (id == ResourceUid.InvalidId)
        {
            PaletterErrors.ThrowInvalidScenePathException(path, "", "file has no UID");
            return false;
        }

        uid = ResourceUid.IdToText(id);
        if (path.EndsWith(".cs"))
        {
            string error = Resolve(uid).Error;
            if (error != null)
            {
                PaletterErrors.ThrowDataValidationException("PaletteEntry", error, path);
                return false;
            }
        }
        return true;
    }

    public static PaletteEntryInfo Resolve(string uid)
    {
        if (!ResourceUid.HasId(ResourceUid.TextToId(uid)))
            return new PaletteEntryInfo(null, null, null, "uid not found");

        Resource resource = ResourceLoader.Load(uid);
        if (resource is PackedScene)
            return new PaletteEntryInfo(null, resource, null, null);

        if (resource is not Script script)
            return new PaletteEntryInfo(null, null, null, "unsupported resource type");

        string nativeBase = script.GetInstanceBaseType();
        if (string.IsNullOrEmpty(nativeBase))
            return new PaletteEntryInfo(null, null, null, "not a built Godot class (build the project first, or the file has no Node class)");

        string globalName = script.GetGlobalName();
        if (string.IsNullOrEmpty(globalName))
            return new PaletteEntryInfo(null, null, null, "not a [GlobalClass]");
        if (script.IsAbstract())
            return new PaletteEntryInfo(null, null, null, "abstract class");
        if (!ClassDB.IsParentClass(nativeBase, "Node"))
            return new PaletteEntryInfo(null, null, null, "not a Node type");

        return new PaletteEntryInfo(globalName, script, nativeBase, null);
    }

    public static Texture2D GetIcon(PaletteEntryInfo info)
    {
        return EditorInterface.Singleton.GetEditorTheme().GetIcon(info.NativeBase, "EditorIcons");
    }
}
