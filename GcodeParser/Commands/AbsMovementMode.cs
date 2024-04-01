using System.Text.RegularExpressions;

namespace GCodeParser.Commands;

public sealed partial class AbsMovementMode : Command
{
    private readonly bool _isAbs;

    public AbsMovementMode(string command, PrinterState printerState) : base(command)
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
    public override string ToGCode(PrinterState state, GCodeFile.GCodeFlavor gcodeFlavor)
    {
        return string.Empty;
    }

    protected override void ApplyToState(PrinterState state)
    {
        state.AbsMode = _isAbs;
    }

    public static bool IsCommand(string line, GCodeFile.GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            (GCodeFile.GCodeFlavor.Marlin) => SettingAbsModeCommandRegex().IsMatch(line),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    [GeneratedRegex(@"(^G91)|(^G90)")]
    private static partial Regex SettingAbsModeCommandRegex();
}