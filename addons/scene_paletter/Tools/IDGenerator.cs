using System.Security.Cryptography;

namespace Addons.ScenePaletter.Tools;

public class IDGenerator
{
    public static int GenerateID(int from, int to)
    {
        return RandomNumberGenerator.GetInt32(from, to);
    }
}