using System.Numerics;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class DotTexture
{
    public static void CircleOutline(GCodeStreamWriter gcodeWriter, float radius, float height, float layerHeight, float filamentDiameter, int resolution = 100, int supportingRingLayers = 5, float supportingRingLayerHeight = 0.2f, Vector2 offset = new())
    {
        int numLayers = (int)(height / layerHeight);
        float angleStep = 2 * float.Pi / resolution;


        float circleCircumference = 2 * float.Pi * radius;
        float dotDiameter = circleCircumference / resolution;

        float currentHeight = layerHeight;
        float currentAngle = 0;
        float totalExtruded = 0;
        
        List<Command> commands =
        [
            new UnrecognizedCommand("M104 S210", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G28", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G90", GCodeFlavor.Marlin),
            new UnrecognizedCommand("M82", GCodeFlavor.Marlin),
            new UnrecognizedCommand("G92 E0", GCodeFlavor.Marlin),
            new UnrecognizedCommand("M109 S210", GCodeFlavor.Marlin),
            new LinearMoveCommand(x: offset.X+10, y: offset.Y+10, z: currentHeight)
        ];
        gcodeWriter.SaveCommands(commands);

        
        // Create Starting movement to wipe nozzle
        Vector3 linePos = new(offset.X, offset.Y+ 10, currentHeight);
        Helpers.FillLine(gcodeWriter, linePos, currentHeight, dotDiameter * 1.5f, filamentDiameter,
            ref totalExtruded);
        
        linePos = new(offset.X, offset.Y, currentHeight);
        Helpers.FillLine(gcodeWriter, linePos, currentHeight, dotDiameter * 1.5f, filamentDiameter,
            ref totalExtruded);
        

        
        // Generate Supporting Ring Bottom
        for (int supportingRingLayerIndex = 0; supportingRingLayerIndex < supportingRingLayers; supportingRingLayerIndex++)
        {
            Helpers.DrawCircle(gcodeWriter, radius, currentHeight, supportingRingLayerHeight, dotDiameter, filamentDiameter, offset,
                ref totalExtruded, resolution: resolution);

            currentHeight += layerHeight;
        }

        // Draw dots
        for (int layerIndex = 0; layerIndex < numLayers; layerIndex++)
        {
            for (int dotIndex = 0; dotIndex < resolution; dotIndex++)
            {
                Vector2 circleOutline2D = Helpers.PolarToCartesian(currentAngle, radius);
                circleOutline2D += offset;

                Vector3 dotPos = new(circleOutline2D, currentHeight);
                
                Helpers.DrawDot(gcodeWriter, filamentDiameter, dotDiameter, layerHeight, dotPos, ref totalExtruded);
                
                currentAngle += angleStep;
            }

            currentAngle += angleStep / 2;
            currentHeight += layerHeight;
        }
        
        // Generate Supporting Ring Top
        for (int supportingRingLayerIndex = 0; supportingRingLayerIndex < supportingRingLayers; supportingRingLayerIndex++)
        {
            Helpers.DrawCircle(gcodeWriter, radius, currentHeight, supportingRingLayerHeight, dotDiameter, filamentDiameter, offset,
                ref totalExtruded, resolution: resolution);

            currentHeight += layerHeight;
        }
    }
}