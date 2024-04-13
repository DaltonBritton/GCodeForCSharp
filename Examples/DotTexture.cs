using System.Numerics;
using GcodeParser;
using GCodeParser;
using GcodeParser.Commands;
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

        float currentHeight = supportingRingLayerHeight;
        float currentAngle = 0;

        List<Command> commands =
        [
            new HeaterTempCommand(180, Heater.Hotend),
            new HeaterTempCommand(60, Heater.Bed),
            new UnrecognizedCommand("G28", GCodeFlavor.Marlin),
            new LinearMoveCommand(x: offset.X+10, y: offset.Y+10, z: supportingRingLayerHeight, f: 3600)
        ];
        gcodeWriter.SaveCommands(commands);

        gcodeWriter.PrinterState.AbsExtruderMode = false;

        // Create Starting movement to wipe nozzle
        Vector3 linePos = new(offset.X, offset.Y+ 10, supportingRingLayerHeight);
        Helpers.FillLine(gcodeWriter, linePos, supportingRingLayerHeight, dotDiameter, filamentDiameter);
        
        linePos = new(offset.X, offset.Y, supportingRingLayerHeight);
        Helpers.FillLine(gcodeWriter, linePos, supportingRingLayerHeight, dotDiameter, filamentDiameter);
        

        
        // Generate Supporting Ring Bottom
        for (int supportingRingLayerIndex = 0; supportingRingLayerIndex < supportingRingLayers; supportingRingLayerIndex++)
        {
            Helpers.DrawCircle(gcodeWriter, radius, currentHeight, supportingRingLayerHeight, dotDiameter, filamentDiameter, offset, resolution: resolution);

            currentHeight += supportingRingLayerHeight;
        }

        // Draw dots
        for (int layerIndex = 0; layerIndex < numLayers; layerIndex++)
        {
            for (int dotIndex = 0; dotIndex < resolution; dotIndex++)
            {
                Vector2 circleOutline2D = Helpers.PolarToCartesian(currentAngle, radius);
                circleOutline2D += offset;

                Vector3 dotPos = new(circleOutline2D, currentHeight);
                
                Helpers.DrawDot(gcodeWriter, filamentDiameter, dotDiameter, layerHeight, dotPos);
                
                currentAngle += angleStep;
            }

            currentAngle += angleStep / 2;
            currentHeight += layerHeight;
        }
        
        // Generate Supporting Ring Top
        for (int supportingRingLayerIndex = 0; supportingRingLayerIndex < supportingRingLayers; supportingRingLayerIndex++)
        {
            Helpers.DrawCircle(gcodeWriter, radius, currentHeight, supportingRingLayerHeight, dotDiameter, filamentDiameter, offset, resolution: resolution);

            currentHeight += supportingRingLayerHeight;
        }
        
        gcodeWriter.SaveCommand(new LinearMoveCommand(z: currentHeight + 30));
    }


    public static void Texturize(PrusaSlicerParser gcodeReader, GCodeStreamWriter gcodeWriter)
    {
        
    }
}