using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.RegularExpressions;
using GCodeParser;
using GCodeParser.Commands;

namespace Examples;

public static class VariableInfill
{
    private class Line(Vector3 lineStart, Vector3 lineEnd)
    {
        public readonly Vector3 LineStart = lineStart;
        public readonly Vector3 LineEnd = lineEnd;
    }

    

    public static void VariableInfillParser(PrusaSlicerParser gcodeReader, GCodeStreamWriter gcodeWriter, float stepSize)
    {
        GCodeStreamWriter tempWriter = new GCodeStreamWriter(new MemoryStream());
        
        List<Line> perimeters = new();

        float totalExtruded = 0;
        
        gcodeWriter.SaveCommand(new UnrecognizedCommand("G92 E0", GCodeFlavor.Marlin));
        gcodeWriter.SaveCommand(new UnrecognizedCommand("M82", GCodeFlavor.Marlin));

        PrinterState printerState = gcodeReader.PrinterState;
        printerState["LineType"] = string.Empty;
        
        // Read Commands
        foreach (var cmd in gcodeReader)
        {
            Command command = cmd;
            
            if (command is PerimeterCommand perimeterCommand)
            {
                if (perimeterCommand.E != null)
                    totalExtruded += (float) perimeterCommand.E;
                
                command = new LinearMoveCommand(x: perimeterCommand.X, y: perimeterCommand.Y,
                    z: perimeterCommand.Z, e: totalExtruded, f: perimeterCommand.F);
                
                Vector3 lineStart = new()
                {
                    X = (float)tempWriter.PrinterState.X,
                    Y = (float)tempWriter.PrinterState.Y,
                    Z = (float)tempWriter.PrinterState.Z,
                };
                
                tempWriter.SaveCommand(command);
                gcodeWriter.SaveCommand(command);

                Vector3 lineEnd = new()
                {
                    X = (float)tempWriter.PrinterState.X,
                    Y = (float)tempWriter.PrinterState.Y,
                    Z = (float)tempWriter.PrinterState.Z,
                };
                
                perimeters.Add(new Line(lineStart, lineEnd));
                
                continue;
            }
            
            if (command is InternalInfillCommand internalInfill)
            {
                Vector3 lineStart = new()
                {
                    X = (float)tempWriter.PrinterState.X,
                    Y = (float)tempWriter.PrinterState.Y,
                    Z = (float)tempWriter.PrinterState.Z,
                };
                
                tempWriter.SaveCommand(internalInfill);

                Vector3 lineEnd = new()
                {
                    X = (float)tempWriter.PrinterState.X,
                    Y = (float)tempWriter.PrinterState.Y,
                    Z = (float)tempWriter.PrinterState.Z,
                };

                float lineLength = Vector3.Distance(lineStart, lineEnd);
                Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);

                int numSteps = (int)float.Ceiling(lineLength / stepSize);
                for (int step = 1; step < numSteps+1; step++)
                {
                    float interpolation = step / (float) numSteps;
                    float distFromStart = interpolation * lineLength;

                    Vector3 newPoint = lineStart + lineDirection * distFromStart;

                    float distToPerimeter = newPoint.MinDist(perimeters);

                    float volumeScalar = -0.125f*distToPerimeter + 3;
                    volumeScalar = float.Max(1f, volumeScalar);
                    volumeScalar = float.Min(3f, volumeScalar);

                    float volume = 0;
                    if(internalInfill.E != null)
                        volume = (float)(internalInfill.E * volumeScalar / numSteps);

                    totalExtruded += volume;

                    LinearMoveCommand newMovementCommand = new(x: newPoint.X, y: newPoint.Y, z: newPoint.Z, e: totalExtruded, f: internalInfill.F);

                    gcodeWriter.SaveCommand(newMovementCommand);
                }
                
                
                continue;
            }

            if (command is LinearMoveCommand otherLinearMoveCommand)
            {
                if (otherLinearMoveCommand.E != null)
                    totalExtruded += (float) otherLinearMoveCommand.E;
                
                command = new LinearMoveCommand(x: otherLinearMoveCommand.X, y: otherLinearMoveCommand.Y,
                    z: otherLinearMoveCommand.Z, e: totalExtruded, f: otherLinearMoveCommand.F);
            }
            
            if(command is LineTypeCommand lineTypeCommand)
            {
                printerState["LineType"] = lineTypeCommand.LineType;
                if((string) printerState["LineType"] == "Perimeter")
                    perimeters.Clear();
            }
            
            if(command is EmptyCommand)
                continue;

            if (command is UnrecognizedCommand unrecognizedCommand)
            {
                string commandString = unrecognizedCommand.ToGCode();
                if(commandString.Contains("M83") || commandString.Contains("G92"))
                    continue;
            }
            
            tempWriter.SaveCommand(command);
            gcodeWriter.SaveCommand(command);
        }
    }

    private static float MinDist(this Vector3 point, IEnumerable<Line> lines)
    {
        float minDist = float.MaxValue;

        foreach (var line in lines)
        {
            minDist = MathF.Min(minDist, point.DistToLine(line));
        }

        return minDist;
    }
    
    private static float DistToLine(this Vector3 point, Line line)
    {
        
        // Return minimum distance between line segment vw and point p
        float l2 = Vector3.DistanceSquared(line.LineStart, line.LineEnd);  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2 == 0.0) return Vector3.Distance(point, line.LineStart);   // v == w case
        // Consider the line extending the segment, parameterized as v + t (w - v).
        // We find projection of point p onto the line. 
        // It falls where t = [(p-v) . (w-v)] / |w-v|^2
        // We clamp t from [0,1] to handle points outside the segment vw.
        float t = MathF.Max(0, MathF.Min(1, Vector3.Dot(point - line.LineStart, line.LineEnd - line.LineStart) / l2));
        Vector3 projection = line.LineStart + t * (line.LineEnd - line.LineStart);  // Projection falls on the segment
        return Vector3.Distance(point, projection);
    }
}