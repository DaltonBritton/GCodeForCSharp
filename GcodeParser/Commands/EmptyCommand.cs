﻿using System.Text.RegularExpressions;

namespace GCodeParser.Commands;

public partial class EmptyCommand(string comment = "") : Command($";{comment}", GCodeFile.GCodeFlavor.Marlin)
{
    /// <inheritdoc />
    public override string ToGCode(PrinterState state, GCodeFile.GCodeFlavor gcodeFlavor)
    {
        return AddInlineComment("", gcodeFlavor);
    }

    public static bool IsCommand(string line, GCodeFile.GCodeFlavor gcodeFlavor)
    {
        return gcodeFlavor switch
        {
            (GCodeFile.GCodeFlavor.Marlin) => EmptyCommandRegex().IsMatch(line),
            _ => throw new InvalidGCode($"Unsupported gcode flavor {gcodeFlavor}")
        };
    }

    protected override void ApplyToState(PrinterState state)
    {
    }

    [GeneratedRegex("^\\s*(?:;.*)?$")]
    private static partial Regex EmptyCommandRegex();
}