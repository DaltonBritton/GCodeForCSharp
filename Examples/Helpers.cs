using System.Numerics;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class Helpers
{
    public static LinearMoveCommand FillLine(PrinterState printerState, Vector3 movePosition, float layerHeight, float lineWidth, float filamentDiameter, ref float totalExtruded)
    {
        Vector3 currentPosition = new()
        {
            X = (float) printerState.X,
            Y = (float) printerState.Y,
            Z = (float) printerState.Z
        };

        float moveDist = (movePosition - currentPosition).Length();

        float lineVolume = layerHeight * lineWidth * moveDist;

        float extruderMovement = GetExtrudeDistFromVolume(lineVolume, filamentDiameter);

        totalExtruded += extruderMovement;
        
        LinearMoveCommand command = new(x: movePosition.X, y: movePosition.Y, z: movePosition.Z, totalExtruded);
        command.ApplyToState(printerState);

        return command;
    }

    private static float GetExtrudeDistFromVolume(float volume, float filamentDiameter)
    {
        return volume / (filamentDiameter * filamentDiameter);
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