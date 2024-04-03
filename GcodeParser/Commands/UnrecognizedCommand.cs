namespace GCodeParser.Commands;

public class UnrecognizedCommand(string command, GCodeFlavor gcodeFlavor) : Command(command, gcodeFlavor)
{
    
    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gCodeFlavor)
    {
        return command;
    }

    protected override void ApplyToState(PrinterState state)
    {
    }
}