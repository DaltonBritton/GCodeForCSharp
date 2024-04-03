using System.Text.RegularExpressions;

namespace GCodeParser.Commands;

public sealed partial class AbsMovementMode : Command
{
    private readonly bool _isAbs;

    public AbsMovementMode(string command, GCodeFlavor gcodeFlavor, PrinterState printerState) : base(command, gcodeFlavor)
    {
        if (Regex.IsMatch(command, "^G90"))
            _isAbs = true;
        else if (Regex.IsMatch(command, "^G91"))
            _isAbs = false;
        else
            throw new InvalidGCode($"Invalid AbsMovementMode command {command}");

        ApplyToState(printerState);
    }

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        return AddInlineComment(string.Empty, gcodeFlavor);
    }

    protected override void ApplyToState(PrinterState state)
    {
        state.AbsMode = _isAbs;
    }

    public static bool IsCommand(string line, GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            (GCodeFlavor.Marlin) => SettingAbsModeCommandRegex().IsMatch(line),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    [GeneratedRegex(@"(^G91)|(^G90)")]
    private static partial Regex SettingAbsModeCommandRegex();
}