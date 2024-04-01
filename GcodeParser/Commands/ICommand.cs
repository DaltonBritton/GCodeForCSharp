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

    public abstract string ToGCode(PrinterState state, GCodeFile.GCodeFlavor gcodeFlavor);

    protected abstract void ApplyToState(PrinterState state);

    protected string AddInlineComment(string command)
    {
        return InlineComment != string.Empty ? $"{command};{InlineComment}" : command;
    }
}