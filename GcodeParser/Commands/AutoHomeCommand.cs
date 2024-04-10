using GCodeParser;
using GCodeParser.Commands;
using System.Text;
using System.Text.RegularExpressions;

namespace GcodeParser.Commands
{
    /// <summary>
    /// Command for setting axes to Auto Home. 
    /// Does not support [L] [O] or [R] parameters
    /// </summary>
    public sealed class AutoHomeCommand : Command
    {
        

        
        private readonly Dictionary<Axis, bool> _axes = new();
        /// <summary>
        /// Constructor for generating an Auto Home command for all axes
        /// </summary>
        public AutoHomeCommand()
        {
            _axes[Axis.X] = true; 
            _axes[Axis.Y] = true; 
            _axes[Axis.Z] = true;
        }

        /// <summary>
        /// Constructor for generating an Auto Home command for multiple Axes
        /// </summary>
        /// <param name="inputAxes"></param>
        public AutoHomeCommand(IEnumerable<Axis> inputAxes)
        {
            var axisEnumerable = inputAxes.ToList();
            
            if (axisEnumerable.Contains(Axis.X)) { _axes[Axis.X] = true; }
            if (axisEnumerable.Contains(Axis.Y)) { _axes[Axis.Y] = true; }
            if (axisEnumerable.Contains(Axis.Z)) { _axes[Axis.Z] = true; }
            
        }

        /// <summary>
        /// Constructor for creating an AutoHome command for one Axis
        /// </summary>
        /// <param name="axis"></param>
        public AutoHomeCommand(Axis axis)
        {
            if (axis == (Axis.X)) { _axes[Axis.X] = true; }
            if (axis == (Axis.Y)) { _axes[Axis.Y] = true; }
            if (axis == (Axis.Z)) { _axes[Axis.Z] = true; }
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
                command = command.Substring(0, command.IndexOf(';'));
            }
            if (GCodeFlavor.Marlin != gcodeFlavor) throw new InvalidGCode($"Unsupported GCode flavor {gcodeFlavor}");

            if (!Regex.IsMatch(command, "^G28")) { throw new InvalidGCode($"Invalid Auto Home Command {command}"); }

            if (command.Contains("L") || command.Contains("O") || command.Contains("R")) { throw new InvalidGCode($"Unsupported Auto Home Command {command}"); }
            

            if (command.Contains("X")) { _axes[Axis.X] = true; }
            if (command.Contains("Y")) { _axes[Axis.Y] = true; }
            if (command.Contains("Z")) { _axes[Axis.Z] = true; }
        }
        /// <inheritdoc/>
        public override string ToGCode(PrinterState state, GCodeFlavor gcodeFlavor)
        {
            StringBuilder builder = new StringBuilder("G28");
            
            foreach(var axisPair in _axes)
            {
                Axis axis = axisPair.Key;
                bool isHomed = axisPair.Value;
                if(isHomed)
                    builder.Append($" {axis}");
            }

            string command = builder.ToString();
            
            return AddInlineComment(command, gcodeFlavor);
        }
        /// <inheritdoc/>
        public override void ApplyToState(PrinterState state)
        {
            if(_axes.TryGetValue(Axis.X, out bool isHomed))
                state.XHome = isHomed;
            
            if(_axes.TryGetValue(Axis.Y, out isHomed))
                state.XHome = isHomed;
            
            if(_axes.TryGetValue(Axis.Z, out isHomed))
                state.XHome = isHomed;
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
