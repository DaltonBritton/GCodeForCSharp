using System.Numerics;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class Helpers
{
    public static void FillLine(GCodeStreamWriter gcodeWriter, Vector3 movePosition, float layerHeight, float lineWidth, float filamentDiameter, ref float totalExtruded)
    {
        Vector3 currentPosition = new()
        {
            X = (float) gcodeWriter.PrinterState.X,
            Y = (float) gcodeWriter.PrinterState.Y,
            Z = (float) gcodeWriter.PrinterState.Z
        };

        float moveDist = (movePosition - currentPosition).Length();

        float lineVolume = layerHeight * lineWidth * moveDist;

        float extruderMovement = GetExtrudeDistFromVolume(lineVolume, filamentDiameter);

        totalExtruded += extruderMovement;
        
        LinearMoveCommand command = new(x: movePosition.X, y: movePosition.Y, z: movePosition.Z, totalExtruded);
        gcodeWriter.SaveCommand(command);
    }

    public static void DrawDot(GCodeStreamWriter gcodeWriter,  float filamentDiameter, float dotDiameter, float dotHeight, Vector3 dotLocation, ref float totalExtruded)
    {
        float dotRadius = dotDiameter / 2;
        float dotVolume = float.Pi * (dotRadius * dotRadius) * dotHeight;

        float extruderMovement = GetExtrudeDistFromVolume(dotVolume, filamentDiameter);
        totalExtruded += extruderMovement;
        
        // Move to position to extrude
        gcodeWriter.SaveCommand(new LinearMoveCommand(dotLocation.X, dotLocation.Y, dotLocation.Z));
        
        // Extrude
        gcodeWriter.SaveCommand(new LinearMoveCommand(e: totalExtruded));
    }

    public static void DrawCircle(GCodeStreamWriter gcodeWriter, float radius, float currentHeight, float layerHeight, float lineWidth, float filamentDiameter, Vector2 center, ref float totalExtruded, float startAngle = 0,
        int resolution = 100)
    {
        float angleStep = 2 * float.Pi / resolution;
        
        for (int pointIndex = 0; pointIndex < resolution; pointIndex++)
        {
            Vector2 circleOutline2D = PolarToCartesian(startAngle, radius);
            circleOutline2D += center;

            Vector3 pointPos = new(circleOutline2D, currentHeight);

            FillLine(gcodeWriter, pointPos, layerHeight, lineWidth, filamentDiameter, ref totalExtruded);

            startAngle += angleStep;
        }
    }

    private static float GetExtrudeDistFromVolume(float volume, float filamentDiameter)
    {
        float filamentRadius = filamentDiameter / 2;
        return volume / (float.Pi * filamentRadius * filamentRadius);
    }

    public static Vector2 PolarToCartesian(float angle, float radius)
    {
        return new Vector2()
        {
            X = radius * float.Cos(angle),
            Y = radius * float.Sin(angle),
        };
    }
}