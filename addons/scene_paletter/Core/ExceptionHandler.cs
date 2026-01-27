using Godot;

namespace Addons.ScenePaletter.Core;

public partial class ExceptionHandler
{
    public static void ThrowMissingPluginException(string text = "")
    {
        GD.PrintErr("Plugin is missing! " + text);
    }

    public static void ThrowMissingConfigException(string text = "")
    {
        GD.PrintErr("Config is Missing! " + text);
    }

    public static void ThrowMissingSceneLoaderException(string text = "")
    {
        GD.PrintErr("Scene Loader is Missing! " + text);
    }

    public static void ThrowMissingDockManagerException(string text = "")
    {
        GD.PrintErr("Dock Manager is Missing! " + text);
    }

    public static void ThrowMissingPageException(string page, string text = "")
    {
        GD.PrintErr("Page " + page + " is Missing! " + text);
    }

        public static void ThrowNotAPageException(string page, string text = "")
    {
        GD.PrintErr("Page " + page + " has no script inheriting Page attached! " + text);
    }

    public static void ThrowMissingWidgetException(string widget, string text = "")
    {
        GD.PrintErr("Widget " + widget + " is Missing! " + text);
    }

    public static void ThrowFileNotFoundErrorException(string file, string text = "")
    {
        GD.PrintErr("File " + file + " not found! " + text);
    }

    public static void ThrowFolderNotFoundException(string folder, string text = "")
    {
        GD.PrintErr("Folder " + folder + " not found! " + text);
    }

    // *******************************************************************
    // *                 exceptions for only this plugin                 *
    // *******************************************************************


    public static void ThrowPaletteNotFoundException(string id, string text = "")
    {
        GD.PrintErr("Palette " + id + " not found! " + text);
    }

    public static void ThrowMissingPaletteException(string id, string text = "")
    {
        GD.PrintErr("Palette  " + id + " is Missing! " + text);
    }
}