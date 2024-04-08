using System.Runtime.InteropServices.Marshalling;
using System.Text.RegularExpressions;
using GcodeParser.Commands;

namespace GCodeParser.Commands;

/// <summary>
/// 
/// </summary>
public partial class SetFanSpeed : Command
{
    private float _fanSpeed;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="gCodeFlavor"></param>
    /// <exception cref="InvalidGCode"></exception>
    public SetFanSpeed(string message, GCodeFlavor gCodeFlavor)
    {
        if (gCodeFlavor != GCodeFlavor.Marlin)
            throw new InvalidGCode($"Unsupported GCode flavor {gCodeFlavor}");

        if (Regex.IsMatch(message, "^M107"))
        {
            _fanSpeed = 0;
            if (message.Contains("P"))
                throw new InvalidGCode($"Invalid FanOffCommand - Does not support multi fan");

        }
        
        else if (Regex.IsMatch(message, "^106"))
        {
            if (message.Contains("I"))
            {
                if (GetStringAfterChar('I', message, out var speed)) 
                    _fanSpeed = speed == "" ? 255 : float.Parse(speed);
            }
            else if (message.Contains("S"))
            {
                if (GetStringAfterChar('S', message, out var speed)) 
                    _fanSpeed = speed == "" ? 255 : float.Parse(speed);
            }

            if (message.Contains("P"))
            {
                GetStringAfterChar('p', message, out string output);
                throw new InvalidGCode($"Invalid SetFanCommand P{output} - Does not support multi fan");
            }
            if (message.Contains("T"))
            {
                GetStringAfterChar('T', message, out string output);
                throw new InvalidGCode($"Invalid SetFanCommand P{output} - Does not support multi fan speeds");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fanSpeed"></param>
    public SetFanSpeed(float fanSpeed)
    {
        _fanSpeed = fanSpeed;
    }

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        if (state.FanSpeed == 0)
            return "M107";

        return $"M106 S{state.FanSpeed}";
    }

    /// <inheritdoc />
    public override void ApplyToState(PrinterState state)
    {
        state.FanSpeed = _fanSpeed;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="gCodeFlavor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidGCode"></exception>
    public static bool IsCommand(string message, GCodeFlavor gCodeFlavor)
    {
        return gCodeFlavor switch
        {
            (GCodeFlavor.Marlin) => SettingFanSpeedCommandRegex().IsMatch(message),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gCodeFlavor}")
        };
    }
    
    /// <summary>
    /// Returns the value of the parameter that begins with the given character
    /// </summary>
    /// <param name="charAfter"></param>
    /// <param name="command"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool GetStringAfterChar(char charAfter, string command, out string value)
    {
        if (command.Contains(charAfter))
        {
            int indexOfNum = command.IndexOf(charAfter) + 1;

            int valueLength = command.IndexOf(' ', indexOfNum) - indexOfNum;
            if (valueLength < 0) valueLength = command.Length - indexOfNum;

            value = command.Substring(indexOfNum, valueLength);
            return true;
        }
        value = string.Empty;
        return false;
    }
    
    [GeneratedRegex(@"(^M106)|(^M107)")]
    private static partial Regex SettingFanSpeedCommandRegex();
}