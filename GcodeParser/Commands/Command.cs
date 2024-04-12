using GcodeParser;

namespace GCodeParser.Commands;



/// <summary>
/// Base class for all gcode commands.
/// Represents a single line of gcode or a single gcode command
/// </summary>
public abstract class Command
{
    /// <summary>
    /// Gets the command without any inline comments
    /// </summary>
    protected string RawCommand { get; }

    /// <summary>
    /// Gets the inline comment contained within the line.
    /// </summary>
    private string InlineComment { get; }

    /// <summary>
    /// Constructs a new Command.
    /// </summary>
    protected Command()
    {
        RawCommand = string.Empty;
        InlineComment = string.Empty;
    }


    /// <summary>
    /// Constructs a new Command and extracts any inline comments that exist on the same line according to the <paramref name="gcodeFlavor"/>.
    /// </summary>
    /// <param name="command">A single line of gcode, used to extract comments.</param>
    /// <param name="gcodeFlavor">Dictates the syntax used to extract any inline comments.</param>
    protected Command(string command, GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");

        int commaLocation = command.IndexOf(';');
        InlineComment = commaLocation != -1 ? command[(commaLocation + 1)..] : string.Empty;

        RawCommand = commaLocation != -1 ? command[..(commaLocation + 1)] : command;
    }

    /// <summary>
    /// Gets a string representing the command as gcode.
    /// Return an empty string if command should be omitted from gcode.
    /// </summary>
    /// <param name="state">The current state of the printer. The state should be updated to reflect changes to the printer state after this command is executed.</param>
    /// <param name="gcodeFlavor">The flavor of gcode to output.</param>
    /// <returns>A string representing the command as gcode.</returns>
    public abstract string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor);

    /// <summary>
    /// Applies all changes to the printer state executing this command may produce.
    /// </summary>
    /// <param name="state">The printer state before this command has been executed.</param>
    public abstract void ApplyToState(PrinterState state);

    /// <summary>
    /// Adds an inline comment with the appropriate syntax as required by the <paramref name="gcodeFlavor"/>.
    /// </summary>
    /// <param name="command">A string representation of a command to which the comment will be added.</param>
    /// <param name="gcodeFlavor">Dictates the syntax for the comment to be applied.</param>
    /// <returns>A string containing an inline comment(if exists) and the original <paramref name="command"/></returns>
    /// <exception cref="InvalidGCode">Thrown in the event a gcode flavor isn't supported.</exception>
    protected string AddInlineComment(string command, GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");

        return InlineComment != string.Empty ? $"{command};{InlineComment}" : command;
    }
}