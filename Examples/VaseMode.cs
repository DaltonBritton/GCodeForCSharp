using System.Numerics;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class VaseMode
{

    public static List<Command> SpiralVase(float layerHeight, float lineWidth, float filamentDiameter, float radius, float vaseHeight, float textureFrequency,
        float textureAmplitude, float resolution = 2.03f*float.Pi/100)
    {
        float currentHeight = layerHeight;
        float heightIncrementPerStep = layerHeight * resolution / (2 * float.Pi);
        float totalExtruded = 0;
        float angle = 0;

        PrinterState printerState = new();
        List<Command> commands = new();
        printerState.AbsExtruderMode = false;//TODO add absExtruderCommand
        
        commands.Add(new UnrecognizedCommand("M104 S210", GCodeFlavor.Marlin));
        commands.Add(new UnrecognizedCommand("G28", GCodeFlavor.Marlin));
        commands.Add(new UnrecognizedCommand("G90", GCodeFlavor.Marlin));
        commands.Add(new UnrecognizedCommand("M109 S210", GCodeFlavor.Marlin));

        Vector2 offset = new Vector2(radius, radius) * 1.25f;



        
        while (currentHeight < vaseHeight)
        {
            currentHeight += heightIncrementPerStep;
            angle += resolution;
            
            // Get next position on vase
            float vaseRadius = radius + textureAmplitude * float.Sin(textureFrequency * angle);

            Vector2 vasePosition2D = Helpers.PolarToCartesian(angle, vaseRadius) + offset;

            Vector3 vasePosition3D = new()
            {
                X = vasePosition2D.X,
                Y = vasePosition2D.Y,
                Z = currentHeight,
            };

            // create movement Command
            Command command = Helpers.FillLine(printerState, vasePosition3D, layerHeight, lineWidth, filamentDiameter, ref totalExtruded);
            
            commands.Add(command);
            
            
        }

        return commands;
    }
    
}