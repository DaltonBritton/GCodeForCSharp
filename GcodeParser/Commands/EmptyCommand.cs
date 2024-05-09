using System.Text.RegularExpressions;
using GcodeParser;

namespace GCodeParser.Commands;

public partial struct EmptyCommand(string command = "") : ICommand
{
    
    /// <inheritdoc />
    public ReadOnlySpan<char> ToGCode(PrinterState state, GCodeFlavor gcodeFlavor, Span<char> buffer)
    {
        ReadOnlySpan<char> InlineComment = ICommand.GetInlineComment(command, GCodeFlavor.Marlin);
        
        return $";{InlineComment}";
    }

    public static bool IsCommand(ReadOnlySpan<char> line, GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            GCodeFlavor.Marlin => EmptyCommandRegex().IsMatch(line),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    /// <inheritdoc />
    public void ApplyToState(PrinterState state)
    {
    }

    [GeneratedRegex("^\\s*(?:;.*)?$")]
    private static partial Regex EmptyCommandRegex();
}