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
    public class HeaterTempCommand : Command
    {

        private double temp;


        private string command;
        public enum heaters
        {
            /// <summary>
            /// 
            /// </summary>
            bed,
            /// <summary>
            /// 
            /// </summary>
            chamber,
            /// <summary>
            /// 
            /// </summary>
            hotend,
        }

        public HeaterTempCommand(string temp, heaters name)
        { 
            if (name == heaters.bed)
            {

            }
        }


        public HeaterTempCommand(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
        {
            
            this.command = command;
            if (Regex.IsMatch(command, "^M140"))
            {
                string SValue;
                if (getStringAfterChar("S", out SValue)) temp = double.Parse(SValue);

            }
            else if (Regex.IsMatch(command, "^M104"))
            {
                if(command.Contains("F") || command.Contains("B")) { throw new InvalidGCode($"Invalid HeaterTempCommand {command} - Does not support auto temp"); }

                string SValue;
                if (getStringAfterChar("S", out SValue)) temp = double.Parse(SValue);


            }
            else if (Regex.IsMatch(command, "^M141"))
            {
                string SValue;
                if (getStringAfterChar("S", out SValue)) temp = double.Parse(SValue);
            }
            else
            {
                throw new InvalidGCode($"Invalid HeaterTempCommand {command}");
            }

        }

        public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
        {
            //TODO add pathway for generative
            return AddInlineComment(this.command, gcodeFlavor);
        }

        protected override void ApplyToState(PrinterState state)
        {

        }

        public static bool IsCommand(string command, GCodeFlavor gcodeFlavor)
        {
            if (gcodeFlavor == GCodeFlavor.Marlin) { throw new InvalidGCode("Can only parse Marlin"); }


            return true;
        }

        private bool getStringAfterChar(string command, out string value)
        {
            if (command.Contains("I"))
            {
                int indexOfNum = command.IndexOf("I");

                value = command.Substring(indexOfNum, command.IndexOf(" ", indexOfNum) - indexOfNum);
                return true;
            }
            value = "";
            return false;
        }
    }
}
