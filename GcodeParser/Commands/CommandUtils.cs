

using System.Diagnostics.Contracts;

namespace GcodeParser.Commands;

/// <summary>
/// Provides helper methods to be used by commands
/// </summary>
public static class CommandUtils
{
    
    /// <summary>
    /// Gets all tokens within a command.
    /// <example>
    ///     GetTokens("G0 X1 Y2; hi this is a command");
    ///     Returns: ["G0", "X1", "Y2"]
    /// </example>
    /// </summary>
    /// <param name="command">A single line of gcode, doesn't include any new line chars.</param>
    /// <returns>An Iterator listing all tokens within a command.</returns>
    [Pure]
    public static IEnumerable<string> GetTokens(string command)
    {
        int commaIndex = command.IndexOf(';');
        
        if (commaIndex != -1)
            command = command.Substring(0, commaIndex);

        foreach (var token in command.Split(' '))
        {
            if (token != string.Empty)
                yield return token;
        }
    }
}