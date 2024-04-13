using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public class PrusaSlicerParser : GCodeStreamReader
{
    public PrusaSlicerParser(Stream inputStream, GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin) : base(inputStream, gcodeFlavor)
    {
        AddCustomGCodeParser(PerimeterCommand.CommandGenerator);
        AddCustomGCodeParser(ExternalPerimeterCommand.CommandGenerator);
        AddCustomGCodeParser(InternalInfillCommand.CommandGenerator);
        
        AddCustomGCodeParser(LineTypeCommand.CommandGenerator);
    }

    public new Command? ReadNextCommand()
    {

        return null;
    }
}