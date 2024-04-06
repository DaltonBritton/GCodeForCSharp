// Ignore Spelling: gcode

using System.Text.RegularExpressions;
using GCodeParser;
using GCodeParser.Commands;

namespace GcodeParser.Commands
{
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
            if (command.Contains("F") || command.Contains("B")) { throw new InvalidGCode($"Invalid HeaterTempCommand {command} - Does not support auto temp"); }
            if (command.Contains("I") || command.Contains("T")) { throw new InvalidGCode($"Invalid HeaterTempCommand {command} - Does not support multi index bed, hot end or materials"); }
            
            if (Regex.IsMatch(command, "^M140"))
            {
                _heater = Heater.bed;
                if (GetStringAfterChar('S', command, out var sValue)) _temp = float.Parse(sValue);

            }
            else if (Regex.IsMatch(command, "^M104"))
            {
                _heater = Heater.hotend;
                if (GetStringAfterChar('S', command, out var sValue)) _temp = float.Parse(sValue);
            }
            else if (Regex.IsMatch(command, "^M141"))
            {
                _heater = Heater.chamber;
                if (GetStringAfterChar('S', command, out var sValue)) _temp = float.Parse(sValue);
            }
            else
            {
                throw new InvalidGCode($"Invalid HeaterTempCommand {command}");
            }

        }
        /// <inheritdoc />
        public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
        {
            string commandStart = "";
            switch (_heater)
            {
                case Heater.chamber: commandStart = "M141"; break;
                case Heater.bed: commandStart = "M140"; break;
                case Heater.hotend: commandStart = "M104"; break;
            }

            string command = $"{commandStart} S{_temp} ";
            
            return AddInlineComment(command, gcodeFlavor);
        }

        /// <inheritdoc />
        protected override void ApplyToState(PrinterState state)
        {
            switch (_heater)
            {
                case Heater.chamber: state.chamberTemp = _temp; break;
                case Heater.bed: state.bedTemp = _temp; break ;
                case Heater.hotend: state.hotEndTemp = _temp; break;
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
                (GCodeFlavor.Marlin) => SettingHeaterCommandRegex().IsMatch(command),
                _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
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

        [GeneratedRegex(@"(^M104)|(^M140)|(^M141)")]
        private static partial Regex SettingHeaterCommandRegex();
    }
}
