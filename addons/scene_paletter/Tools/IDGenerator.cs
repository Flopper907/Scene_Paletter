using System.Security.Cryptography;

namespace Addons.ScenePaletter.Tools;

/// <summary>
/// Provides secure ID generation functionality using cryptographic random number generation.
/// </summary>
/// <remarks>
/// <para>
/// Uses <c>System.Security.Cryptography.RandomNumberGenerator</c> to generate
/// cryptographically strong random numbers, ensuring unique and unpredictable IDs.
/// </para>
/// <para>
/// Suitable for:
/// <list type="bullet">
///   <item>Generating unique identifiers in editor plugins</item>
///   <item>Creating unpredictable IDs for scene elements</item>
///   <item>Any use case requiring secure random number generation</item>
/// </list>
/// </para>
/// </remarks>
public class IDGenerator
{
    /// <summary>
    /// Generates a cryptographically secure random integer within the specified range.
    /// </summary>
    /// <param name="from">Inclusive lower bound of the random number returned.</param>
    /// <param name="to">Exclusive upper bound of the random number returned.</param>
    /// <returns>A random integer greater than or equal to <c>from</c> and less than <c>to</c>.</returns>
    /// <remarks>
    /// <para>
    /// Uses <c>RandomNumberGenerator.GetInt32</c> for cryptographically secure random number generation.
    /// This is more secure than <c>System.Random</c> and suitable for generating unique IDs.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// int uniqueID = IDGenerator.GenerateID(1000, 10000); // Generates ID between 1000-9999
    /// </code>
    /// </para>
    /// </remarks>
    public static int GenerateID(int from, int to)
    {
        return RandomNumberGenerator.GetInt32(from, to);
    }
}
