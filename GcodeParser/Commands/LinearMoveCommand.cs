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


        string gcode = _e != null ? "G1" : "G0";

        StringBuilder builder = new(gcode);

        WriteArgumentToGCode(builder, "X", _x, state.X);
        WriteArgumentToGCode(builder, "Y", _y, state.Y);
        WriteArgumentToGCode(builder, "Z", _z, state.Z);
        WriteArgumentToGCode(builder, "E", _e, state.E);
        WriteArgumentToGCode(builder, "F", _f, state.F);

        ApplyToState(state);

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

    
    public static bool IsCommand(string command, GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            (GCodeFlavor.Marlin) => MarlinLinearMoveCommand().IsMatch(command),
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

    private void GetNewPositions(PrinterState state)
    {
        _x = state.X;
        _y = state.Y;
        _z = state.Z;
        _e = state.E;
        _f = state.F;
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
        double printerState)
    {
        if(argumentValue == null)
            return;
        
        if (Math.Abs((double) argumentValue - printerState) > 0.00001)
            builder.Append($" {argumentName}{argumentValue}");
    }
}