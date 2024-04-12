// Ignore Spelling: gcode

using System.Text.RegularExpressions;
using GCodeParser;
using GCodeParser.Commands;

namespace GcodeParser.Commands;

/// <summary>
/// Class for creating HeaterTempCommands. Can construct based on an existing command or generate based
/// On temperature and heating element parameters. 
/// 
/// Does not handle auto temp or multi index heating elements, or material settings. 
/// </summary>
public sealed partial class HeaterTempCommand : Command
{
    private readonly float _temp;
    private readonly Heater _heater;


    /// <summary>
    /// Constructor for creating a write able heater command
    /// </summary>
    /// <param name="temp"></param>
    /// <param name="heater"></param>
    public HeaterTempCommand(float temp, Heater heater)
    {
        _temp = temp;
        _heater = heater;
    }

    /// <summary>
    /// Command for reading a heater command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="gcodeFlavor"></param>
    /// <exception cref="InvalidGCode"></exception>
    public HeaterTempCommand(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
    {
        Dictionary<string, double> arguments = CommandUtils.GetNumericArgumentsWithoutDuplicates(RawCommand, gcodeFlavor);

        if(arguments.ContainsKey("F") || arguments.ContainsKey("B"))
            throw new InvalidGCode($"Invalid HeaterTempCommand {command} - Does not support auto temp");

        if (arguments.ContainsKey("I") || arguments.ContainsKey("T"))
            throw new InvalidGCode(
                $"Invalid HeaterTempCommand {command} - Does not support multi index bed, hot end or materials");
        
        // Get Heater
        if (SetHotEndTempCommandRegex().IsMatch(command))
            _heater = Heater.Hotend;
        else if (SetBedTempCommandRegex().IsMatch(command)) 
            _heater = Heater.Bed; 
        else if (SetChamberTempCommandRegex().IsMatch(command))
            _heater = Heater.Chamber;
        else
            throw new InvalidGCode($"Invalid HeaterTempCommand {command}");
        
        // Get Temp
        if (!arguments.TryGetValue("S", out double sValue))
            throw new InvalidGCode($"Invalid HeaterTempCommand {command} - Does not include a set temp argument");

        _temp = (float) sValue;
    }

    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
    {
        string commandStart = _heater switch
        {
            Heater.Chamber => "M141",
            Heater.Bed => "M140",
            Heater.Hotend => "M104",
            _ => throw new ArgumentOutOfRangeException($"Unexpected Heater {_heater}")
        };

        string command = $"{commandStart} S{_temp}";

        return AddInlineComment(command, gcodeFlavor);
    }

    /// <inheritdoc />
    public override void ApplyToState(PrinterState state)
    {
        switch (_heater)
        {
            case Heater.Chamber:
                state.ChamberTemp = _temp;
                break;
            case Heater.Bed:
                state.BedTemp = _temp;
                break;
            case Heater.Hotend:
                state.HotEndTemp = _temp;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unexpected Heater {_heater}");
        }
    }

    /// <summary>
    /// Checks if the provided <paramref name="command"/> is a valid HeaterTempCommand.
    /// </summary>
    /// <param name="command">A single line of gcode, doesn't include any new line chars</param>
    /// <param name="gcodeFlavor">Dictates the syntax to be used.</param>
    /// <returns>True if the <paramref name="command"/> can be parsed as a HeaterTempCommand, False if otherwise.</returns>
    /// <exception cref="InvalidGCode">Thrown if an unrecognized gcodeFlavor is provided</exception>
    public static bool IsCommand(string command, GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            GCodeFlavor.Marlin => SettingHeaterCommandRegex().IsMatch(command),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    [GeneratedRegex("(^M104)|(^M140)|(^M141)")]
    private static partial Regex SettingHeaterCommandRegex();
    
    
    [GeneratedRegex("^M140")]
    private static partial Regex SetBedTempCommandRegex();
    
    [GeneratedRegex("^M104")]
    private static partial Regex SetHotEndTempCommandRegex();
    [GeneratedRegex("^M141")]
    private static partial Regex SetChamberTempCommandRegex();
}