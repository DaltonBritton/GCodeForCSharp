namespace GCodeParser.Commands;

public class InvalidGCode(string message) : Exception(message);

public abstract class Command
{
    protected Command(string command)
    {
        int commaLocation = command.IndexOf(';');
        InlineComment = (commaLocation != -1) ? command.Substring(commaLocation + 1) : string.Empty;
    }

    public string InlineComment { get; }

    /// <summary>
    /// Gets a string representing the command as gcode.
    /// Return an empty string if command should be omitted from gcode.
    /// </summary>
    /// <param name="state">The current state of the printer. The state should be updated to reflect changes to the printer state after this command is executed.</param>
    /// <param name="gcodeFlavor">The flavor of gcode to output.</param>
    /// <returns>A string representing the command as gcode.</returns>
    public abstract string ToGCode(PrinterState state, GCodeFile.GCodeFlavor gcodeFlavor);

    protected abstract void ApplyToState(PrinterState state);

    protected string AddInlineComment(string command, GCodeFile.GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFile.GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");
        
        return InlineComment != string.Empty ? $"{command};{InlineComment}" : command;
    }
}