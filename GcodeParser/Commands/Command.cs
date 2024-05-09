using GcodeParser;
using GcodeParser.Utils;

namespace GCodeParser.Commands;

/// <summary>
/// Base class for all gcode commands.
/// Represents a single line of gcode or a single gcode command
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the command without any inline comments
    /// </summary>
    protected static ReadOnlySpan<char> GetRawCommand(ReadOnlySpan<char> command, GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");

        int commaLocation = command.IndexOf(';');
        
        return commaLocation != -1 ? command[..(commaLocation + 1)] : command;
    }

    /// <summary>
    /// Gets the inline comment contained within the line.
    /// </summary>
    protected static ReadOnlySpan<char> GetInlineComment(ReadOnlySpan<char> command, GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");

        int commaLocation = command.IndexOf(';');
        
        return commaLocation != -1 ? command[(commaLocation + 1)..] : ReadOnlySpan<char>.Empty;
    }
    
    /// <summary>
    /// Gets a string representing the command as gcode.
    /// Return an empty string if command should be omitted from gcode.
    /// </summary>
    /// <param name="state">The current state of the printer. The state should be updated to reflect changes to the printer state after this command is executed.</param>
    /// <param name="gcodeFlavor">The flavor of gcode to output.</param>
    /// <returns>A string representing the command as gcode.</returns>
    public ReadOnlySpan<char> ToGCode(PrinterState state, GCodeFlavor gcodeFlavor, Span<char> buffer);

    /// <summary>
    /// Applies all changes to the printer state executing this command may produce.
    /// </summary>
    /// <param name="state">The printer state before this command has been executed.</param>
    public void ApplyToState(PrinterState state);
}