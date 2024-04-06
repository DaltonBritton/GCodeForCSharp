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
    /// <summary>
    /// Command for setting axes to Auto Home. 
    /// Does not support [L] [O] or [R] parameters
    /// </summary>
    public sealed partial class AutoHomeCommand : Command
    {
        

        
        private bool[] axes;

        private string command;

        /// <summary>
        /// Constructor for generating an Auto Home command for all axes
        /// </summary>
        public AutoHomeCommand()
        {
            axes = new bool[3];
            axes[0] = true; 
            axes[1] = true; 
            axes[2] = true;

            command = "G28 X Y Z ";
        }

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

            StringBuilder builder = new StringBuilder("G28 ");
            
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

            command = "G28 " + axis.ToString() + " ";
        }

        /// <summary>
        /// Command for reading an Auto Home Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="gcodeFlavor"></param>
        /// <exception cref="InvalidGCode"></exception>
        public AutoHomeCommand(string command, GCodeFlavor gcodeFlavor) : base(command, gcodeFlavor)
        {
            if (command.Contains(";"))
            {
                command = command.Substring(0, command.IndexOf(";"));
            }
            if (GCodeFlavor.Marlin != gcodeFlavor) throw new InvalidGCode($"Unsupported GCode flavor {gcodeFlavor}");

            if (!Regex.IsMatch(command, "^G28")) { throw new InvalidGCode($"Invalid Auto Home Command {command}"); }

            if (command.Contains("L") || command.Contains("O") || command.Contains("R")) { throw new InvalidGCode($"Unsupported Auto Home Command {command}"); }

            this.command = command;
            axes = new bool[3];

            if (command.Contains("X")) { axes[0] = true; }
            if (command.Contains("Y")) { axes[1] = true; }
            if (command.Contains("Z")) { axes[2] = true; }
        }
        /// <inheritdoc/>
        public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
        {
            return AddInlineComment(this.command, gcodeFlavor);
        }
        /// <inheritdoc/>
        protected override void ApplyToState(PrinterState state)
        {
            if (axes[0]) { state.xHome = true; }
            if (axes[1]) { state.yHome = true; }
            if (axes[2]) { state.zHome = true; }
        }

        /// <summary>
        /// Checks if a given string is a valid Auto Home Command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="gcodeFlavor"></param>
        /// <returns></returns>
        /// <exception cref="InvalidGCode"></exception>
        public static bool IsCommand(string command, GCodeFlavor gcodeFlavor)
        {
            if (GCodeFlavor.Marlin != gcodeFlavor) throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}");

            return Regex.IsMatch(command, "^G28");
        }
    }
}
