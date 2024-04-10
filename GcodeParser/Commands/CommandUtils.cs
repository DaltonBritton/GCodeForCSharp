

using System.Diagnostics.Contracts;
using GCodeParser;
using GCodeParser.Commands;

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

    /// <summary>
    /// Gets all arguments within a <paramref name="command"/> given the <paramref name="gcodeFlavor"/>
    /// </summary>
    /// <param name="command">A single line of gcode, doesn't include any new line chars.</param>
    /// <param name="gcodeFlavor">Dictates the syntax to get arguments</param>
    /// <returns>A Dictionary containing all argument given to the <paramref name="command"/></returns>
    /// <exception cref="InvalidGCode">Thrown if unable to get numeric value for argument or if a duplicate argument is found.</exception>
    [Pure]
    public static Dictionary<string, double> GetNumericArgumentsWithoutDuplicates(string command,
        GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode("Unsupported GCodeFlavor");
        
        IEnumerable<string> tokens = GetTokens(command);
        Dictionary<string, double> arguments = new();
        bool isFirst = true;

        foreach (var token in tokens)
        {
            if (isFirst)
            {
                isFirst = false;
                continue;
            }

            string argumentName = token[0].ToString();
            if (!double.TryParse(token[1..], out double argumentValue))
                throw new InvalidGCode($"Unable to parse argument {argumentName}, in command {command} as a numeric value");

            if (!arguments.TryAdd(argumentName, argumentValue))
                throw new InvalidGCode($"Duplicate argument {argumentName}, in command {command}");
        }

        return arguments;
    }
}