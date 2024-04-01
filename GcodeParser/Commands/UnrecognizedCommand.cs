namespace GCodeParser.Commands;

public class UnrecognizedCommand(string command, GCodeFile.GCodeFlavor gcodeFlavor) : Command(command)
{
    public override string ToGCode(PrinterState state, GCodeFile.GCodeFlavor gCodeFlavor)
    {
        return command;
    }

    protected override void ApplyToState(PrinterState state)
    {
    }
}