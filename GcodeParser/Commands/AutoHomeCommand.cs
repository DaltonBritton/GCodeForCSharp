﻿using GCodeParser;
using GCodeParser.Commands;
using System.Text;
using System.Text.RegularExpressions;

namespace GcodeParser.Commands;

/// <summary>
/// Command for setting axes to Auto Home. 
/// Does not support [L] [O] or [R] parameters
/// </summary>
public partial struct AutoHomeCommand : ICommand
{
    private readonly Dictionary<Axis, bool> _axes = new();

    private readonly string _inlineComment = string.Empty;

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

        if (axisEnumerable.Contains(Axis.X))
        {
            _axes[Axis.X] = true;
        }

        if (axisEnumerable.Contains(Axis.Y))
        {
            _axes[Axis.Y] = true;
        }

        if (axisEnumerable.Contains(Axis.Z))
        {
            _axes[Axis.Z] = true;
        }
    }

    /// <summary>
    /// Constructor for creating an AutoHome command for one Axis
    /// </summary>
    /// <param name="axis"></param>
    public AutoHomeCommand(Axis axis)
    {
        _axes[axis] = true;
        _inlineComment = "";
    }

    /// <summary>
    /// Command for reading an Auto Home Command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="gcodeFlavor"></param>
    /// <exception cref="InvalidGCode"></exception>
    public AutoHomeCommand(ReadOnlySpan<char> command, GCodeFlavor gcodeFlavor)
    {
        if (GCodeFlavor.Marlin != gcodeFlavor) throw new InvalidGCode($"Unsupported GCode flavor {gcodeFlavor}");

        ReadOnlySpan<char> rawCommand = ICommand.GetRawCommand(command, gcodeFlavor);

        _inlineComment = ICommand.GetInlineComment(command, gcodeFlavor).ToString();
        
        if (!AutoHomeRegex().IsMatch(rawCommand))
        {
            throw new InvalidGCode($"Invalid Auto Home Command {command}");
        }

        HashSet<string> arguments = CommandUtils.GetBooleanArgumentsWithoutDuplicates(rawCommand.ToString(), gcodeFlavor);
        
        
        if (arguments.Contains("L") || arguments.Contains("O") || arguments.Contains("R"))
            throw new InvalidGCode($"Unsupported Auto Home Command {command}");
        
        
        if (arguments.Remove("X"))
            _axes[Axis.X] = true;

        if (arguments.Remove("Y"))
            _axes[Axis.Y] = true;

        if (arguments.Remove("Z")) 
            _axes[Axis.Z] = true;

        if (arguments.Count != 0)
            throw new InvalidGCode($"Unexpected Argument in command {command}");
    }

    /// <inheritdoc/>
    public ReadOnlySpan<char> ToGCode(PrinterState state, GCodeFlavor gcodeFlavor, Span<char> buffer)
    {
        StringBuilder builder = new StringBuilder("G28");

        foreach (var (axis, isHomed) in _axes)
        {
            if (isHomed)
                builder.Append($" {axis}");
        }

        // Add comment
        if(_inlineComment != string.Empty)
        {
            builder.Append(';');
            builder.Append(_inlineComment);
        }
        
        string command = builder.ToString();

        return command;
    }

    /// <inheritdoc/>
    public void ApplyToState(PrinterState state)
    {
        if (_axes.TryGetValue(Axis.X, out bool isHomed))
            state.XHome = isHomed;

        if (_axes.TryGetValue(Axis.Y, out isHomed))
            state.XHome = isHomed;

        if (_axes.TryGetValue(Axis.Z, out isHomed))
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

        return AutoHomeRegex().IsMatch(command);
    }

    [GeneratedRegex("^G28")]
    private static partial Regex AutoHomeRegex();
}