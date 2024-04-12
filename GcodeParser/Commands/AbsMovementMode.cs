using System.Text.RegularExpressions;
using GcodeParser;

namespace GCodeParser.Commands;

/// <summary>
/// Sets the printer to Absolute/Relative Movement Mode.
/// Effects all future <see cref="LinearMoveCommand">LinearMoveCommands</see> coordinate systems.
///
/// Note to contributors:
///     AbsMovementMode is a special case, whenever movement occurs it is converted into Abs position for the axis X, Y, Z and relative for the E axis.
///     This produces different but equivalent gcode where there exists no AbsMovementMode Command.
///     This is done to simplify the number of edge cases.
/// </summary>
public sealed partial class AbsMovementMode : Command
{
    private readonly bool _isAbs;
    private readonly bool _extruderOnly;

    /// <summary>
    /// Creates a new Absolute Movement Mode Command.
    /// </summary>
    /// <param name="isAbs"></param>
    /// <param name="extruderOnly"></param>
    /// <param name="inlineComment"></param>
    /// <exception cref="InvalidGCode">Thrown if unable to parse line.</exception>
    public AbsMovementMode(bool isAbs, bool extruderOnly = false, string inlineComment = "") : base($";{inlineComment}",
        GCodeFlavor.Marlin)
    {
        _isAbs = isAbs;
        _extruderOnly = extruderOnly;
    }

    /// <summary>
    /// Creates a new Absolute Movement Mode Command.
    /// </summary>
    /// <param name="command">A single line of gcode containing the Absolute Movement Mode Command. Shouldn't contain any newline characters</param>
    /// <param name="gcodeFlavor">Dictates the syntax used to parse the Absolute Movement Mode Command</param>
    /// <exception cref="InvalidGCode">Thrown if unable to parse line.</exception>
    public AbsMovementMode(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
    {
        _extruderOnly = false;
        if (SetAbsCommandRegex().IsMatch(command))
            _isAbs = true;
        else if (SetRelCommandRegex().IsMatch(command))
            _isAbs = false;
        else if (SetExtruderAbsRegex().IsMatch(command))
        {
            _isAbs = true;
            _extruderOnly = true;
        }
        else if (SetExtruderRelRegex().IsMatch(command))
        {
            _isAbs = false;
            _extruderOnly = true;
        }
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
        if (_extruderOnly)
            state.AbsExtruderMode = _isAbs;
        else
            state.AbsMode = _isAbs;
    }

    /// <summary>
    /// Checks if the given <paramref name="gcodeLine"/> is a valid AbsMovementMode Command.
    /// </summary>
    /// <param name="gcodeLine">A single line of gcode without any newline chars</param>
    /// <param name="gcodeFlavor"></param>
    /// <returns>True if the given <paramref name="gcodeLine"/> is a valid AbsMovementMode Command, False otherwise.</returns>
    /// <exception cref="InvalidGCode">Thrown if an unsupported gcodeFlavor is provided</exception>
    public static bool IsCommand(string gcodeLine, GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            GCodeFlavor.Marlin => SettingAbsModeCommandRegex().IsMatch(gcodeLine),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    [GeneratedRegex("(^G91)|(^G90)|(^M82)|(^M83)")]
    private static partial Regex SettingAbsModeCommandRegex();
    
    [GeneratedRegex("^G90")]
    private static partial Regex SetAbsCommandRegex();
    
    [GeneratedRegex("^G91")]
    private static partial Regex SetRelCommandRegex();
    
    [GeneratedRegex("^M82")]
    private static partial Regex SetExtruderAbsRegex();
    
    [GeneratedRegex("^M83")]
    private static partial Regex SetExtruderRelRegex();
}