using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;
using GcodeParser.Commands;

namespace GCodeParser.Commands;

public partial class LinearMoveCommand : Command
{
    public double? E { get; private set; }
    public double? F { get; private set; }
    public double? X { get; private set; }
    public double? Y { get; private set; }
    public double? Z { get; private set; }

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
        F = f;
        X = x;
        Y = y;
        Z = z;
        E = e;
    }

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        if (gcodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");


        string gcode;
        if (E != null)
        {
            if ((state.AbsExtruderMode && !ApproxEqual((double)E, state.E)) ||
                (!state.AbsExtruderMode && !ApproxEqual((double)E, 0)))
                gcode = "G1";
            else
                gcode = "G0";
        }
        else
            gcode = "G0";

        StringBuilder builder = new(gcode);

        WriteArgumentToGCode(builder, "X", X, state.X, state.AbsMode);
        WriteArgumentToGCode(builder, "Y", Y, state.Y, state.AbsMode);
        WriteArgumentToGCode(builder, "Z", Z, state.Z, state.AbsMode);
        WriteArgumentToGCode(builder, "E", E, state.E, state.AbsExtruderMode);
        WriteArgumentToGCode(builder, "F", F, state.F, state.AbsMode);
        
        string commandString = builder.ToString();
        if (commandString == "G0")
            return string.Empty;

        commandString = AddInlineComment(commandString, gcodeFlavor);

        return commandString;
    }

    /// <inheritdoc />
    public override void ApplyToState(PrinterState printerState)
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
        IEnumerable<string> tokens = CommandUtils.GetTokens(command);

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
                    X = axisPosition;
                    break;
                case ('Y'):
                    CheckAndUpdateDuplicateArgumentFlag('Y', ref y);
                    Y = axisPosition;
                    break;
                case ('Z'):
                    CheckAndUpdateDuplicateArgumentFlag('Z', ref z);
                    Z = axisPosition;
                    break;
                case ('E'):
                    CheckAndUpdateDuplicateArgumentFlag('E', ref e);
                    E = axisPosition;
                    break;
                case ('F'):
                    CheckAndUpdateDuplicateArgumentFlag('F', ref f);
                    F = axisPosition;
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