namespace GCodeParser.Commands;

/// <summary>
/// Creates a Unrecognized command
/// </summary>
public class UnrecognizedCommand(string command, GCodeFlavor gcodeFlavor) : Command(command, gcodeFlavor)
{
    private readonly string _command = command;

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gCodeFlavor)
    {
        return _command;
    }
    
    /// <inheritdoc cref="ToGCode(GCodeParser.PrinterState,GCodeParser.GCodeFlavor)"/>
    public string ToGCode()
    {
        return command;
    }

    /// <inheritdoc />
    public override void ApplyToState(PrinterState state)
    {
    }
}