using GCodeParser;
using GCodeParser.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GcodeParser.Commands.AutoHomeCommand;

namespace GcodeParser.Commands
{
    internal class AutoHomeCommand : Command
    {
        public enum Axis
        {
            /// <summary>
            /// X Axis
            /// </summary>
            X,
            /// <summary>
            /// Y Axis
            /// </summary>
            Y,
            /// <summary>
            /// Z Axis
            /// </summary>
            Z,
        }


        private bool[] axes;

        private string command;

        /// <summary>
        /// Constructor for generating an Auto Home command for multiple Axes
        /// </summary>
        /// <param name="inputAxes"></param>
        public AutoHomeCommand(IEnumerable<Axis> inputAxes)
        {
            axes = new bool[3];
            if (inputAxes.Contains(Axis.X)) { axes[0] = true; }
            if (inputAxes.Contains(Axis.Y)) { axes[1] = true; }
            if (inputAxes.Contains(Axis.Z)) { axes[2] = true; }

            StringBuilder builder = new StringBuilder("G28");
            
            foreach(Axis curr in inputAxes)
            { 
                builder.Append(curr.ToString() + " ");
            }

            command = builder.ToString();
        }

        /// <summary>
        /// Constructor for creating an AutoHome command for one Axis
        /// </summary>
        /// <param name="axis"></param>
        public AutoHomeCommand(Axis axis)
        {
            axes = new bool[3];

            if (axis == (Axis.X)) { axes[0] = true; }
            if (axis == (Axis.Y)) { axes[1] = true; }
            if (axis == (Axis.Z)) { axes[2] = true; }

            command = "G28 " + axis.ToString();
        }

        /// <summary>
        /// Command for reading a Auto Home Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="gcodeFlavor"></param>
        /// <exception cref="InvalidGCode"></exception>
        public AutoHomeCommand(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
        {
            if (GCodeFlavor.Marlin != gcodeFlavor) throw new InvalidGCode($"Unsupported GCode flavor {gcodeFlavor}");

            if (!Regex.IsMatch(command, "^G28")) { throw new InvalidGCode($"Invalid Auto Home Command {command}"); }

            if (command.Contains("L") || command.Contains("O") || command.Contains("R")) { throw new InvalidGCode($"Unsupported Auto Home Command {command}"); }

            this.command = command;
            axes = new bool[3];

            if (command.Contains("X")) { axes[0] = true; }
            if (command.Contains("Y")) { axes[1] = true; }
            if (command.Contains("Z")) { axes[2] = true; }
        }
        public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
        {
            return AddInlineComment(this.command, gcodeFlavor);
        }

        protected override void ApplyToState(PrinterState state)
        {
            if (axes[0]) { state.xHome = true; }
            if (axes[1]) { state.yHome = true; }
            if (axes[2]) { state.zHome = true; }
        }


        public static bool IsCommand(string command, GCodeFlavor gcodeFlavor)
        {
            if (GCodeFlavor.Marlin != gcodeFlavor) throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");

            return Regex.IsMatch(command, "^G28");
        }
    }
}
