using System.Diagnostics.Contracts;
using System.Numerics;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class VaseMode
{

    public static void SpiralVase(GCodeStreamWriter gcodeWriter, float layerHeight, float lineWidth, float filamentDiameter, float radius, float vaseHeight, float textureFrequency,
        float textureAmplitude, float resolution = 2.03f*float.Pi/100, int rotationsPerLayer = 1)
    {
        float currentHeight = layerHeight;
        float heightIncrementPerStep = layerHeight * resolution / (2 * float.Pi);
        heightIncrementPerStep /= rotationsPerLayer;
        float angle = 0;
        
        Vector2 offset = new Vector2(radius+textureAmplitude, radius+textureAmplitude) * 1.25f;


        List<Command> commands =
        [
            new UnrecognizedCommand("M104 S210", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G28", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G90", GCodeFlavor.Marlin),
            new UnrecognizedCommand("M82", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G92 E0", GCodeFlavor.Marlin),
            new UnrecognizedCommand("M109 S180", GCodeFlavor.Marlin),
            new LinearMoveCommand(x: offset.X+10, y: offset.Y+10, z: currentHeight)
        ];

        gcodeWriter.SaveCommands(commands);


        Vector3 linePos = new(offset.X - 10, offset.Y + 10, currentHeight);
        Helpers.FillLine(gcodeWriter, linePos, currentHeight, lineWidth * 1.5f, filamentDiameter);
        
        linePos = new(offset.X - 10, offset.Y, currentHeight);
        Helpers.FillLine(gcodeWriter, linePos, currentHeight, lineWidth * 1.5f, filamentDiameter);
        
        linePos = new(offset.X, offset.Y, currentHeight);
        Helpers.FillLine(gcodeWriter, linePos, currentHeight, lineWidth * 1.5f, filamentDiameter);

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
            if(IsFirstLayer(angle, rotationsPerLayer))
                Helpers.FillLine(gcodeWriter, vasePosition3D, currentHeight, lineWidth*1.5f, filamentDiameter);
            else
                Helpers.FillLine(gcodeWriter, vasePosition3D, layerHeight, lineWidth, filamentDiameter);
        }

    }

    [Pure]
    private static bool IsFirstLayer(float angle, int rotationsPerLayer)
    {
        return angle < 2 * float.Pi * rotationsPerLayer;
    }
    
}