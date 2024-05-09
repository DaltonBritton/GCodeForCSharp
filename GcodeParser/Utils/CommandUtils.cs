using System.Diagnostics.Contracts;
using GCodeParser;
using GcodeParser.Utils;

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
            command = command[..commaIndex];

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
    /// <exception cref="InvalidGCode">Thrown if unable to get numeric value for argument</exception>
    /// <exception cref="DuplicateArgumentException">Thrown if a duplicate argument is found</exception>
    public static HashSet<string> GetBooleanArgumentsWithoutDuplicates(string command, GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode("Unsupported GCodeFlavor");
        
        IEnumerable<string> tokens = GetTokens(command);
        HashSet<string> arguments = [];
        bool isFirst = true;

        foreach (var token in tokens)
        {
            if (isFirst)
            {
                isFirst = false;
                continue;
            }
            
            if(!arguments.Add(token))
                throw new DuplicateArgumentException($"Duplicate argument {token}, in command {command}");
        }

        return arguments;
    }

    /// <summary>
    /// Gets all arguments within a <paramref name="command"/> given the <paramref name="gcodeFlavor"/>
    /// </summary>
    /// <param name="command">A single line of gcode, doesn't include any new line chars.</param>
    /// <param name="gcodeFlavor">Dictates the syntax to get arguments</param>
    /// <returns>A Dictionary containing all argument given to the <paramref name="command"/></returns>
    /// <exception cref="InvalidGCode">Thrown if unable to get numeric value for argument</exception>
    /// <exception cref="DuplicateArgumentException">Thrown if a duplicate argument is found</exception>
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
                throw new InvalidGCode(
                    $"Unable to parse argument {argumentName}, in command {command} as a numeric value");

            if (!arguments.TryAdd(argumentName, argumentValue))
                throw new DuplicateArgumentException($"Duplicate argument {argumentName}, in command {command}");
        }
        
        return arguments;
    }

    /// <summary>
    /// Gets all arguments within a <paramref name="command"/> given the <paramref name="gcodeFlavor"/>.
    /// This is a stack allocated version of GetNumericArgumentsWithoutDuplicates used to reduce stack allocations when parsing Linear Move Commands
    /// </summary>
    public static StackAllocDictionary<char, double> GetNumericArgumentsWithoutDuplicatesStackAlloc(
        ReadOnlySpan<char> command, GCodeFlavor gcodeFlavor, Span<char> argumentsNames, Span<double> argumentsValues)
    {
        if(gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode("Unsupported GCodeFlavor");

        StackAllocDictionary<char, double> arguments = new(argumentsNames, argumentsValues);

        bool hasNextToken = TryGetNextToken(command, 0, out ReadOnlySpan<char> token, out int tokenEnd, out bool isArgumentName);

        char argumentName = default;
        bool isWaitingOnDouble = false;
        bool isCommandName = true;// ie. G1 or G28
        
        while (hasNextToken)
        {
            if (isArgumentName && isWaitingOnDouble)
                throw new Exception($"Expected Argument Name got <{token}>.");

            if (!isArgumentName && !isWaitingOnDouble)
                throw new Exception($"Expected Number got <{token}>.");
            
            
            if (isArgumentName && !isWaitingOnDouble)
            {
                if (token.Length != 1)
                    throw new Exception($"Argument Name <{token}> cannot have multiple chars");

                argumentName = token[0];
                isWaitingOnDouble = true;
            }

            if (!isArgumentName && isWaitingOnDouble)
            {
                isWaitingOnDouble = false;

                if (!isCommandName)
                {
                    if (arguments.TryGet(argumentName, out _))
                        throw new DuplicateArgumentException($"Duplicate Argument {argumentName} in command {command}");
                    arguments[argumentName] = double.Parse(token);
                }

                isCommandName = false;
            }
            
            
            hasNextToken = TryGetNextToken(command, tokenEnd, out token, out tokenEnd, out isArgumentName);
        }

        return arguments;
    }

    private static bool TryGetNextToken(ReadOnlySpan<char> command, int startIndex, out ReadOnlySpan<char> token, out int tokenEnd, out bool isArgumentName)
    {
        bool parsingWord = false;
        bool parsingDouble = false;
        int tokenStart = -1;
        int tokenLength = 0;
        
        for (int i = startIndex; i < command.Length; i++)
        {
            char character = command[i];


            if (character == ' ')
            {
                if(tokenStart == -1)
                    continue;
                else
                    break;
            }
            
            if (character == ';')
                break;

            if (char.IsLetter(character))
            {
                if(parsingDouble)
                    break;

                if (tokenStart == -1)
                    tokenStart = i;

                parsingWord = true;
            }

            if (char.IsNumber(character) || character == '.' || character == '-')
            {
                if (parsingWord)
                    break;

                if (tokenStart == -1)
                    tokenStart = i;

                parsingDouble = true;
            }
            
            tokenLength++;
        }

        if (tokenStart != -1)
        {
            token = command.Slice(tokenStart, tokenLength);
            tokenEnd = tokenStart + tokenLength;
            isArgumentName = parsingWord;
            return true;
        }

        token = default;
        tokenEnd = -1;
        isArgumentName = false;
        return false;
    }
}