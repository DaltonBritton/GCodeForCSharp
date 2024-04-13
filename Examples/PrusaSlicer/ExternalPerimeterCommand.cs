using System.Diagnostics.CodeAnalysis;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public class ExternalPerimeterCommand(string command, GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin)
    : LinearMoveCommand(command, gcodeFlavor)
{
    
    public static bool CommandGenerator(string gcodeLine, GCodeFlavor gcodeFlavor, PrinterState printerState,
        [NotNullWhen(true)] out Command? command)
    {
        command = null;
        if ((string)printerState["LineType"] != "External perimeter" || !IsCommand(gcodeLine, gcodeFlavor))
            return false;

        command = new InternalInfillCommand(gcodeLine, gcodeFlavor);
        
        return true;
    }
    
    public override void ApplyToState(PrinterState printerState)
    {
        base.ApplyToState(printerState);

        printerState["LineType"] = "External perimeter";
    }
}