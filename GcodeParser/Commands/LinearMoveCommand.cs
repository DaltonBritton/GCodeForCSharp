using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;

namespace GCodeParser.Commands;

public partial class LinearMoveCommand : Command
{
    private double _e;
    private double _f;
    private double _x;
    private double _y;
    private double _z;

    public LinearMoveCommand(string command, GCodeFile.GCodeFlavor gcodeFlavor, PrinterState state) : base(command)
    {
        switch (gcodeFlavor)
        {
            case (GCodeFile.GCodeFlavor.Marlin):
                ParseMarlin(command, state);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gcodeFlavor), gcodeFlavor,
                    $"Unsupported GCode Flavor {gcodeFlavor}");
        }
    }

    public LinearMoveCommand(PrinterState printerState, double? x = null, double? y = null, double? z = null,
        double? e = null, double? f = null) : base("")
    {
        if (x != null)
            printerState.X = (double)x;

        if (y != null)
            printerState.Y = (double)y;

        if (z != null)
            printerState.Z = (double)z;

        if (e != null)
            printerState.E = (double)e;

        if (f != null)
            printerState.F = (double)f;

        GetNewPositions(printerState);
    }

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFile.GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFile.GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");

        string gcode = Math.Abs(_e - state.E) > 0.00001 ? "G1" : "G0";

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

    protected override void ApplyToState(PrinterState printerState)
    {
        printerState.X = _x;
        printerState.Y = _y;
        printerState.Z = _z;
        printerState.E = _e;
        printerState.F = _f;
    }

    
    public static bool IsCommand(string command, GCodeFile.GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            (GCodeFile.GCodeFlavor.Marlin) => MarlinLinearMoveCommand().IsMatch(command),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    private void ParseMarlin(string command, PrinterState state)
    {
        IEnumerable<string> tokens = GetTokens(command);

        using IEnumerator<string> tokensEnumerator = tokens.GetEnumerator();

        // Advance tokensEnumerator to G0/G1
        if (!tokensEnumerator.MoveNext())
            throw new InvalidGCode("Expected 'G0' or 'G1' but didn't find valid token at beginning of line");

        string commandCode = tokensEnumerator.Current;

        if (commandCode is not ("G0" or "G1"))
            throw new InvalidGCode($"Marlin Linear Moves must start with G0 or G1, got {commandCode}");

        ProcessTokens(tokensEnumerator, state);
    }

    private void ProcessTokens(IEnumerator<string> tokens, PrinterState state)
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
                    state.X = axisPosition;
                    break;
                case ('Y'):
                    CheckAndUpdateDuplicateArgumentFlag('Y', ref y);
                    state.Y = axisPosition;
                    break;
                case ('Z'):
                    CheckAndUpdateDuplicateArgumentFlag('Z', ref z);
                    state.Z = axisPosition;
                    break;
                case ('E'):
                    CheckAndUpdateDuplicateArgumentFlag('E', ref e);
                    state.E = axisPosition;
                    break;
                case ('F'):
                    CheckAndUpdateDuplicateArgumentFlag('F', ref f);
                    state.F = axisPosition;
                    break;
                default:
                    throw new InvalidGCode($"Unable to parse argument, get{token}");
            }
        }

        GetNewPositions(state);
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

    private void WriteArgumentToGCode(StringBuilder builder, string argumentName, double argumentValue,
        double printerState)
    {
        if (Math.Abs(argumentValue - printerState) > 0.00001)
            builder.Append($" {argumentName}{argumentValue}");
    }
}