using System.Text.RegularExpressions;

namespace GCodeParser.Commands;

/// <summary>
/// Sets the printer to Absolute/Relative Movement Mode.
/// Effects all future <see cref="LinearMoveCommand">LinearMoveCommands</see> coordinate systems.
/// </summary>
public sealed partial class AbsMovementMode : Command
{
    private readonly bool _isAbs;

    /// <summary>
    /// Creates a new Absolute Movement Mode Command.
    /// </summary>
    /// <param name="isAbs"></param>
    /// <param name="printerState">The state of the printer before applying the Absolute Movement Mode Command</param>
    /// <param name="inlineComment"></param>
    /// <exception cref="InvalidGCode">Thrown if unable to parse line.</exception>
    public AbsMovementMode(bool isAbs , string inlineComment = ""): base($";{inlineComment}", GCodeFlavor.Marlin)
    {
        _isAbs = isAbs;
    }
    
    /// <summary>
    /// Creates a new Absolute Movement Mode Command.
    /// </summary>
    /// <param name="command">A single line of gcode containing the Absolute Movement Mode Command. Shouldn't contain any newline characters</param>
    /// <param name="gcodeFlavor">Dictates the syntax used to parse the Absolute Movement Mode Command</param>
    /// <exception cref="InvalidGCode">Thrown if unable to parse line.</exception>
    public AbsMovementMode(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
    {
        if (Regex.IsMatch(command, "^G90"))
            _isAbs = true;
        else if (Regex.IsMatch(command, "^G91"))
            _isAbs = false;
        else
            throw new InvalidGCode($"Invalid AbsMovementMode command {command}");
    }

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        return AddInlineComment(string.Empty, gcodeFlavor);
    }

    /// <inheritdoc />
    public override void ApplyToState(PrinterState state)
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