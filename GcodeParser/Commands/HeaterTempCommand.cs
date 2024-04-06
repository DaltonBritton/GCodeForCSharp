// Ignore Spelling: gcode

using GCodeParser;
using GCodeParser.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        private float temp;
        private Heater heater;

        private string command;

        /// <summary>
        /// Enum for different heating elements
        /// </summary>
        public enum Heater
        {
            /// <summary>
            /// Heater for bed
            /// </summary>
            bed,
            /// <summary>
            /// Heater for chamber
            /// </summary>
            chamber,
            /// <summary>
            /// Heater for hot end
            /// </summary>
            hotend,
        }

        /// <summary>
        /// Constructor for creating a write able heater command
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="heater"></param>
        public HeaterTempCommand(float temp, Heater heater)
        {
            this.temp = temp;
            this.heater = heater;
            string commandStart = "";
            switch (heater)
            {
                case Heater.chamber: commandStart = "M141"; break;
                case Heater.bed: commandStart = "M140"; break;
                case Heater.hotend: commandStart = "M104"; break;
            }

            command = $"{commandStart} S{temp}";

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

            this.command = command;
            if (Regex.IsMatch(command, "^M140"))
            {
                heater = Heater.bed;
                string SValue;
                if (getStringAfterChar('S', command, out SValue)) temp = float.Parse(SValue);

            }
            else if (Regex.IsMatch(command, "^M104"))
            {
                heater = Heater.hotend;
                string SValue;
                if (getStringAfterChar('S', command, out SValue)) temp = float.Parse(SValue);
            }
            else if (Regex.IsMatch(command, "^M141"))
            {
                heater = Heater.chamber;
                string SValue;
                if (getStringAfterChar('S', command, out SValue)) temp = float.Parse(SValue);
            }
            else
            {
                throw new InvalidGCode($"Invalid HeaterTempCommand {command}");
            }

        }
        /// <inheritdoc />
        public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
        {
            return AddInlineComment(this.command, gcodeFlavor);
        }

        /// <inheritdoc />
        protected override void ApplyToState(PrinterState state)
        {
            switch (this.heater)
            {
                case Heater.chamber: state.chamberTemp = temp; break;
                case Heater.bed: state.bedTemp = temp; break ;
                case Heater.hotend: state.hotEndTemp = temp; break;
            }
        }
        /// <inheritdoc />
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
        private bool getStringAfterChar(char charAfter, string command, out string value)
        {
            if (command.Contains(charAfter))
            {
                int indexOfNum = command.IndexOf(charAfter) + 1;

                int valueLength = command.IndexOf(" ", indexOfNum) - indexOfNum;
                if (valueLength < 0) valueLength = command.Length - indexOfNum;

                value = command.Substring(indexOfNum, valueLength);
                return true;
            }
            value = "";
            return false;
        }

        [GeneratedRegex(@"(^M104)|(^M140)|(^M141)")]
        private static partial Regex SettingHeaterCommandRegex();
    }
}
