using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;

namespace GCodeParser.Commands;

public partial class LinearMoveCommand : Command
{
    private double? _e;
    private double? _f;
    private double? _x;
    private double? _y;
    private double? _z;

    public LinearMoveCommand(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
    {
        switch (gcodeFlavor)
        {
            case (GCodeFlavor.Marlin):
                ParseMarlin(command);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gcodeFlavor), gcodeFlavor,
                    $"Unsupported GCode Flavor {gcodeFlavor}");
        }
    }

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
        IEnumerable<string> tokens = GetTokens(command);

        using IEnumerator<string> tokensEnumerator = tokens.GetEnumerator();

        // Advance tokensEnumerator to G0/G1
        if (!tokensEnumerator.MoveNext())
            throw new InvalidGCode("Expected 'G0' or 'G1' but didn't find valid token at beginning of line");

        string commandCode = tokensEnumerator.Current;

        if (commandCode is not ("G0" or "G1"))
            throw new InvalidGCode($"Marlin Linear Moves must start with G0 or G1, got {commandCode}");

        ProcessTokens(tokensEnumerator);
    }

    private void ProcessTokens(IEnumerator<string> tokens)
    {
        // Initialize duplicate check flags
        bool x = false, y = false, z = false, e = false, f = false;
        while (tokens.MoveNext())
        {
            string token = tokens.Current;

            // Extract the argument name and axis position from the token
            char argumentName = token[0];
            if (!double.TryParse(token[1..], out double axisPosition))
                throw new InvalidGCode($"Unable to parse argument, get {token}");

            // Update the corresponding printerState based on the argument name
            switch (argumentName)
            {
                case ('X'):
                    CheckAndUpdateDuplicateArgumentFlag('X', ref x);
                    _x = axisPosition;
                    break;
                case ('Y'):
                    CheckAndUpdateDuplicateArgumentFlag('Y', ref y);
                    _y = axisPosition;
                    break;
                case ('Z'):
                    CheckAndUpdateDuplicateArgumentFlag('Z', ref z);
                    _z = axisPosition;
                    break;
                case ('E'):
                    CheckAndUpdateDuplicateArgumentFlag('E', ref e);
                    _e = axisPosition;
                    break;
                case ('F'):
                    CheckAndUpdateDuplicateArgumentFlag('F', ref f);
                    _f = axisPosition;
                    break;
                default:
                    throw new InvalidGCode($"Unable to parse argument, get{token}");
            }
        }
    }

    private void CheckAndUpdateDuplicateArgumentFlag(char axisName, ref bool duplicateArgumentFlag)
    {
        if (duplicateArgumentFlag)
            throw new InvalidGCode($"Got duplicate argument {axisName} for linear move command");

        duplicateArgumentFlag = true;
    }

    [Pure]
    private static IEnumerable<string> GetTokens(string command)
    {
        int commaIndex = command.IndexOf(';');
        if (commaIndex != -1)
            command = command.Substring(0, commaIndex);

        foreach (var token in command.Split(" "))
        {
            if (token != string.Empty)
                yield return token;
        }
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