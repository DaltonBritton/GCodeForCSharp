using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

class LineTypeCommand : Command
{
    public readonly string LineType;
    public static bool CommandGenerator(string gcodeLine, GCodeFlavor gcodeFlavor, PrinterState _,
        [NotNullWhen(true)] out Command? command)
    {
        command = null;
        if (!Regex.IsMatch(gcodeLine, "^;TYPE:.+$"))
            return false;

        string lineType = gcodeLine.Replace(";TYPE:", "");

        command = new LineTypeCommand(lineType);
        return true;
    }

    private LineTypeCommand(string lineType)
    {
        LineType = lineType;
    }
        
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        return ";TYPE:" + LineType;
    }

    public override void ApplyToState(PrinterState state)
    {
        state["LineType"] = LineType;
    }
}