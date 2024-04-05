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
    internal class HeaterTempCommand : Command
    {

        private double? hotendTemp;
        private double? bedTemp;
        private double? chamerTemp;

        private int? hotendMaterialIndex;
        private int? bedIndex;
        private int? hotendIndex;


        private string command;


        public HeaterTempCommand(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
        {
            
            this.command = command;
            if (Regex.IsMatch(command, "^M140"))
            {
                string SValue;
                if (getStringAfterChar("S", out SValue)) bedTemp = double.Parse(SValue);

                string IValue;
                if (getStringAfterChar("I", out IValue)) bedIndex = int.Parse(IValue);


            }
            else if (Regex.IsMatch(command, "^M104"))
            {
                if(command.Contains("F") || command.Contains("B")) { throw new InvalidGCode($"Invalid HeaterTempCommand {command} - Does not support auto temp")}

                string SValue;
                if (getStringAfterChar("S", out SValue)) hotendTemp = double.Parse(SValue);

                string IValue;
                if (getStringAfterChar("I", out IValue)) hotendMaterialIndex = int.Parse(IValue);

                string TValue;
                if (getStringAfterChar("T", out TValue)) hotendMaterialIndex = int.Parse(TValue);

            }
            else if (Regex.IsMatch(command, "^M141"))
            {
                string SValue;
                if (getStringAfterChar("S", out SValue)) chamerTemp = double.Parse(SValue);
            }
            else
            {
                throw new InvalidGCode($"Invalid HeaterTempCommand {command}");
            }

        }

        public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
        {
            throw new NotImplementedException();
        }

        protected override void ApplyToState(PrinterState state)
        {
            throw new NotImplementedException();
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
