using System.Numerics;
using System.Text.RegularExpressions;
using GcodeParser.Commands;

namespace GCodeParser.Commands;

/// <summary>
/// 
/// </summary>
public partial class SetPosition : Command
{
    private readonly Vector4 _offset;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="line"></param>
    /// <param name="gCodeFlavor"></param>
    public SetPosition(string line, GCodeFlavor gCodeFlavor)
    {
        if (Regex.IsMatch(line, "M107"))
        {
            _offset = Vector4.Zero;
            return;
        }
        IEnumerable<string> command = CommandUtils.GetTokens(line);
        foreach (string parameter in command)
        {
            if (double.TryParse(parameter.Substring(1), out double value))
            {
                switch (parameter.First())
                {
                    case 'X':
                        _offset.X = (float)value;
                        break;
                    case 'Y':
                        _offset.Y = (float)value;
                        break;
                    case 'Z':
                        _offset.Z = (float)value;
                        break;
                    case 'E':
                        _offset.W = (float)value;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="e"></param>
    public SetPosition(double? x = 0, double? y = 0,double? z = 0, double? e = 0)
    {
        _offset.X = x != null ? (float)x : 0;
        _offset.Y = y != null ? (float)y : 0;
        _offset.Z = z != null ? (float)z : 0;
        _offset.W = e != null ? (float)e : 0;
    }
    /// <inheritdoc/>
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        string command = "G92";
        if (state.Offset.X != 0)
            command += $" X{state.Offset.X}";
        if (state.Offset.Y != 0)
            command += $" Y{state.Offset.Y}";
        if (state.Offset.Z != 0)
            command += $" Z{state.Offset.Z}";
        if (state.Offset.W != 0)
            command += $" E{state.Offset.W}";
        return command;
    }

    /// <inheritdoc/>
    public override void ApplyToState(PrinterState state)
    {
        state.Offset = _offset;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="meesage"></param>
    /// <param name="gCodeFlavor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidGCode"></exception>
    public static bool IsCommand(string meesage, GCodeFlavor gCodeFlavor)
    {
        return gCodeFlavor switch
        {
            (GCodeFlavor.Marlin) => SettingPositionCommandRegex().IsMatch(meesage),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gCodeFlavor}")
        };
    }
    
    [GeneratedRegex(@"(^G92)")]
    private static partial Regex SettingPositionCommandRegex();
}