namespace GCodeParser.Commands;

/// <summary>
/// Creates a Unrecognized command
/// </summary>
public struct UnrecognizedCommand(string command, GCodeFlavor gcodeFlavor) : ICommand
{
    
    /// <inheritdoc />
    public ReadOnlySpan<char> ToGCode(PrinterState state, GCodeFlavor gCodeFlavor, Span<char> buffer)
    {
        return command;
    }
    
    /// <inheritdoc cref="ToGCode(GCodeParser.PrinterState,GCodeParser.GCodeFlavor)"/>
    public string ToGCode()
    {
        return command;
    }

    /// <inheritdoc />
    public void ApplyToState(PrinterState state) { }
}