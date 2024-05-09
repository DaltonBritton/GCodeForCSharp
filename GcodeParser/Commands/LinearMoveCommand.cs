using System.Diagnostics.Contracts;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using GcodeParser;
using GcodeParser.Commands;
using GcodeParser.Utils;

namespace GCodeParser.Commands;

/// <summary>
/// Moves the print head to a the given location. Represents the command G0, G1
/// </summary>
public partial struct LinearMoveCommand : ICommand
{
    public double? E { get; private set; }
    public double? F { get; private set; }
    public double? X { get; private set; }
    public double? Y { get; private set; }
    public double? Z { get; private set; }

    private string _inlineComment = string.Empty;

    /// <summary>
    /// Creates a new Linear Move Command from the GCode representation of the command.
    /// </summary>
    /// <param name="command">The String Representation of a linear move command. (ex. G0, G1)</param>
    /// <param name="gcodeFlavor">Dictates the syntax which to read the command with.</param>
    /// <exception cref="InvalidGCode">Thrown if an unsupported gcodeFlavor is provided.</exception>
    public LinearMoveCommand(string command, GCodeFlavor gcodeFlavor)
    {
        switch (gcodeFlavor)
        {
            case GCodeFlavor.Marlin:
                ParseMarlin(command);
                break;
            default:
                throw new InvalidGCode($"Unsupported GCode Flavor {gcodeFlavor}");
        }
    }

    /// <summary>
    /// Creates an new Linear Move Command, used for gcode generation.
    /// </summary>
    public LinearMoveCommand(double? x = null, double? y = null, double? z = null,
        double? e = null, double? f = null)
    {
        F = f;
        X = x;
        Y = y;
        Z = z;
        E = e;
    }

    /// <inheritdoc />
    public ReadOnlySpan<char> ToGCode(PrinterState state, GCodeFlavor gcodeFlavor, Span<char> buffer)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");
        
        StringBuilderStackAlloc builder = new(buffer);
        
        string gcode = GetCommandCodeFromPrinterState(state);
        builder.Append(gcode);
        WriteArgumentToGCode(ref builder, "X", X, state.X, state.AbsMode);
        WriteArgumentToGCode(ref builder, "Y", Y, state.Y, state.AbsMode);
        WriteArgumentToGCode(ref builder, "Z", Z, state.Z, state.AbsMode);
        WriteArgumentToGCode(ref builder, "F", F, state.F, true);

        if (E != null)
        {
            double? extruderMovement = GetExtruderMovementBasedOnPrinterState(state, (double)E);
            if (extruderMovement != null)
                builder.Append($" E{extruderMovement}");
        }


        builder.Append(_inlineComment);
        
        ReadOnlySpan<char> commandString = builder.GetReadOnlySpan();
        if (commandString == "G0")
            return ReadOnlySpan<char>.Empty;

        return commandString;
    }


    /// <inheritdoc cref="PrinterState.GetPrinterPosAfterMovement"/>
    [Pure]
    public Vector3 GetResultingPos(PrinterState state)
    {
        return state.GetPrinterPosAfterMovement(X, Y, Z);
    }
    
    /// <inheritdoc cref="PrinterState.GetExtruderPosAfterMovement"/>
    [Pure]
    public double GetResultingExtruderPos(PrinterState state)
    {
        return state.GetExtruderPosAfterMovement(E);
    }

    [Pure]
    private static double? GetExtruderMovementBasedOnPrinterState(PrinterState state, double e)
    {
        return state.AbsExtruderMode switch
        {
            true when !ApproxEqual(e, state.E) => e - state.E,
            false when !ApproxEqual(e, 0) => e,
            _ => null
        };
    }

    private string GetCommandCodeFromPrinterState(PrinterState printerState)
    {
        string gcode;
        if (E != null)
        {
            if ((printerState.AbsExtruderMode && !ApproxEqual((double)E, printerState.E)) ||
                (!printerState.AbsExtruderMode && !ApproxEqual((double)E, 0)))
                gcode = "G1";
            else
                gcode = "G0";
        }
        else
            gcode = "G0";

        return gcode;
    }

    /// <inheritdoc />
    public void ApplyToState(PrinterState printerState)
    {
        if (X != null)
            printerState.X = (double) X;

        if (Y != null)
            printerState.Y = (double) Y;

        if (Z != null)
            printerState.Z = (double) Z;

        if (E != null)
            printerState.E = (double) E;

        if (F != null)
            printerState.F = (double) F;
    }


    /// <summary>
    /// Checks if the given <paramref name="gcodeLine"/> is a valid LinearMoveCommand.
    /// </summary>
    /// <param name="gcodeLine">A single line of gcode without any newline chars</param>
    /// <param name="gcodeFlavor"></param>
    /// <returns>True if the given <paramref name="gcodeLine"/> is a valid LinearMoveCommand, False otherwise.</returns>
    /// <exception cref="InvalidGCode">Thrown if an unsupported gcodeFlavor is provided</exception>
    [Pure]
    public static bool IsCommand(string gcodeLine, GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            GCodeFlavor.Marlin => MarlinLinearMoveCommand().IsMatch(gcodeLine),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    private void ParseMarlin(ReadOnlySpan<char> command)
    {
        Span<char> argumentNames = stackalloc char[5];
        Span<double> argumentValues = stackalloc double[5];
        
        StackAllocDictionary<char, double> arguments =
            CommandUtils.GetNumericArgumentsWithoutDuplicatesStackAlloc(command, GCodeFlavor.Marlin, argumentNames, argumentValues);

        int numArguments = 0;
        
        if (arguments.TryGet('X', out var argumentValue))
        {
            X = argumentValue;
            numArguments++;
        }
        if (arguments.TryGet('Y', out argumentValue))
        {
            Y = argumentValue;
            numArguments++;
        }
        if (arguments.TryGet('Z', out argumentValue))
        {
            Z = argumentValue;
            numArguments++;
        }
        if (arguments.TryGet('E', out argumentValue))
        {
            E = argumentValue;
            numArguments++;
        }
        if (arguments.TryGet('F', out argumentValue))
        {
            F = argumentValue;
            numArguments++;
        }

        if (arguments.Count != numArguments)
            throw new InvalidGCode($"Unexpected Arguments in {command}");
    }

    [GeneratedRegex("^G[01]")]
    private static partial Regex MarlinLinearMoveCommand();

    private static void WriteArgumentToGCode(ref StringBuilderStackAlloc builder, ReadOnlySpan<char> argumentName, double? argumentValue,
        double printerAxisState, bool isAbs)
    {
        if (argumentValue == null)
            return;

        switch (isAbs)
        {
            case true when !ApproxEqual((double)argumentValue, printerAxisState):
                builder.Append(" ");
                builder.Append(argumentName);
                builder.Append(argumentValue.ToString());
                break;
            case false when !ApproxEqual((double)argumentValue, 0):
                builder.Append(" ");
                builder.Append(argumentName);
                builder.Append((printerAxisState + argumentValue).ToString());
                break;
        }
    }

    private static bool ApproxEqual(double n1, double n2)
    {
        return Math.Abs(n1 - n2) < 0.00001;
    }
}