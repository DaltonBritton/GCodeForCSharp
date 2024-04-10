using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using GcodeParser.Commands;

namespace GCodeParser.Commands;

/// <summary>
/// Moves the print head to a the given location. Represents the command G0, G1
/// </summary>
public partial class LinearMoveCommand : Command
{
    private double? _e;
    private double? _f;
    private double? _x;
    private double? _y;
    private double? _z;

    /// <summary>
    /// Creates a new Linear Move Command from the GCode representation of the command.
    /// </summary>
    /// <param name="command">The String Representation of a linear move command. (ex. G0, G1)</param>
    /// <param name="gcodeFlavor">Dictates the syntax which to read the command with.</param>
    /// <exception cref="InvalidGCode">Thrown if an unsupported gcodeFlavor is provided.</exception>
    public LinearMoveCommand(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
    {
        switch (gcodeFlavor)
        {
            case (GCodeFlavor.Marlin):
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
        _f = f;
        _x = x;
        _y = y;
        _z = z;
        _e = e;
    }

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");


        string gcode;
        if (_e != null)
        {
            if ((state.AbsExtruderMode && !ApproxEqual((double)_e, state.E)) ||
                (!state.AbsExtruderMode && !ApproxEqual((double)_e, 0)))
                gcode = "G1";
            else
                gcode = "G0";
        }
        else
            gcode = "G0";

        StringBuilder builder = new(gcode);

        WriteArgumentToGCode(builder, "X", _x, state.X, state.AbsMode);
        WriteArgumentToGCode(builder, "Y", _y, state.Y, state.AbsMode);
        WriteArgumentToGCode(builder, "Z", _z, state.Z, state.AbsMode);
        WriteArgumentToGCode(builder, "F", _f, state.F, true);

        if (_e != null)
        {
            switch (state.AbsExtruderMode)
            {
                case true when !ApproxEqual((double)_e, state.E):
                    builder.Append($" E{_e-state.E}");
                    break;
                case false when !ApproxEqual((double)_e, 0):
                    builder.Append($" E{_e}");
                    break;
            }
        }
        
        string commandString = builder.ToString();
        if (commandString == "G0")
            return string.Empty;

        commandString = AddInlineComment(commandString, gcodeFlavor);

        return commandString;
    }

    /// <inheritdoc />
    public override void ApplyToState(PrinterState printerState)
    {
        if (_x != null)
            printerState.X = (double) _x;

        if (_y != null)
            printerState.Y = (double) _y;

        if (_z != null)
            printerState.Z = (double) _z;

        if (_e != null)
            printerState.E = (double) _e;

        if (_f != null)
            printerState.F = (double) _f;
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
            (GCodeFlavor.Marlin) => MarlinLinearMoveCommand().IsMatch(gcodeLine),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    private void ParseMarlin(string command)
    {
        Dictionary<string, double> arguments =
            CommandUtils.GetNumericArgumentsWithoutDuplicates(command, GCodeFlavor.Marlin);

        if (arguments.Remove("X", out var argumentValue))
            _x = argumentValue;
        if (arguments.Remove("Y", out argumentValue))
            _y = argumentValue;
        if (arguments.Remove("Z", out argumentValue))
            _z = argumentValue;
        if (arguments.Remove("E", out argumentValue))
            _e = argumentValue;
        if (arguments.Remove("F", out argumentValue))
            _f = argumentValue;

        if (arguments.Count != 0)
            throw new InvalidGCode($"Unexpected Arguments in {command}");
    }

    [GeneratedRegex(@"^G[01]")]
    private static partial Regex MarlinLinearMoveCommand();

    private void WriteArgumentToGCode(StringBuilder builder, string argumentName, double? argumentValue,
        double printerAxisState, bool isAbs)
    {
        if(argumentValue == null)
            return;
        
        if (isAbs && !ApproxEqual((double) argumentValue, printerAxisState))
            builder.Append($" {argumentName}{argumentValue}");
        else if (!isAbs && !ApproxEqual((double)argumentValue, 0))
            builder.Append($" {argumentName}{printerAxisState+argumentValue}");

    }

    private static bool ApproxEqual(double n1, double n2)
    {
        return Math.Abs(n1 - n2) < 0.00001;
    }
}