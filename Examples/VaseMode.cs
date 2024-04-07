using System.Diagnostics.Contracts;
using System.Numerics;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class VaseMode
{

    public static List<Command> SpiralVase(float layerHeight, float lineWidth, float filamentDiameter, float radius, float vaseHeight, float textureFrequency,
        float textureAmplitude, float resolution = 2.03f*float.Pi/100, int rotationsPerLayer = 1)
    {
        float currentHeight = layerHeight;
        float heightIncrementPerStep = layerHeight * resolution / (2 * float.Pi);
        heightIncrementPerStep /= rotationsPerLayer;
        float totalExtruded = 0;
        float angle = 0;
        
        Vector2 offset = new Vector2(radius+textureAmplitude, radius+textureAmplitude) * 1.25f;


        PrinterState printerState = new();
        List<Command> commands =
        [
            new UnrecognizedCommand("M104 S210", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G28", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G90", GCodeFlavor.Marlin),
            new UnrecognizedCommand("M82", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G92 E0", GCodeFlavor.Marlin),
            new UnrecognizedCommand("M109 S210", GCodeFlavor.Marlin)
        ];


        LinearMoveCommand startPos = new LinearMoveCommand(x: offset.X+10, y: offset.Y+10, z: currentHeight);
        startPos.ApplyToState(printerState);
        commands.Add(startPos);


        Vector3 linePos = new(offset.X - 10, offset.Y + 10, currentHeight);
        Command startCommand = Helpers.FillLine(printerState, linePos, currentHeight, lineWidth * 1.5f, filamentDiameter,
            ref totalExtruded);
        commands.Add(startCommand);
        
        linePos = new(offset.X - 10, offset.Y, currentHeight);
        startCommand = Helpers.FillLine(printerState, linePos, currentHeight, lineWidth * 1.5f, filamentDiameter,
            ref totalExtruded);
        commands.Add(startCommand);
        
        linePos = new(offset.X, offset.Y, currentHeight);
        startCommand = Helpers.FillLine(printerState, linePos, currentHeight, lineWidth * 1.5f, filamentDiameter,
            ref totalExtruded);
        commands.Add(startCommand);

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
            Command command;
            if(IsFirstLayer(angle, rotationsPerLayer))
                command = Helpers.FillLine(printerState, vasePosition3D, currentHeight, lineWidth*1.5f, filamentDiameter, ref totalExtruded);
            else
                command = Helpers.FillLine(printerState, vasePosition3D, layerHeight, lineWidth, filamentDiameter, ref totalExtruded);

            
            commands.Add(command);
        }

        return commands;
    }

    [Pure]
    private static bool IsFirstLayer(float angle, int rotationsPerLayer)
    {
        return angle < 2 * float.Pi * rotationsPerLayer;
    }
    
}