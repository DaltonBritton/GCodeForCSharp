using System.Numerics;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class Helpers
{
    public static Command FillLine(PrinterState printerState, Vector3 movePosition, double layerHeight, double lineWidth, double filamentDiameter)
    {
        Vector3 currentPosition = new Vector3()
        {
            X = (float) printerState.X,
            Y = (float) printerState.Y,
            Z = (float) printerState.Z
        };

        double moveDist = (movePosition - currentPosition).Length();

        double lineVolume = layerHeight * lineWidth * moveDist;

        double extruderMovement = GetExtrudeDistFromVolume(lineVolume, filamentDiameter);

        //TODO: Create new Linear move Command

    }

    public static double GetExtrudeDistFromVolume(double volume, double filamentDiameter)
    {
        return volume / (filamentDiameter * filamentDiameter);
    }
}