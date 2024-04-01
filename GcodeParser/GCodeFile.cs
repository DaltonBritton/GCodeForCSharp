using System.Text;
using GCodeParser.Commands;

namespace GCodeParser;

public static class GCodeFile
{
    public enum GCodeFlavor
    {
        Marlin,
    }

    public static async Task<IEnumerable<Command>> ReadGCode(Stream inputStream,
        GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin)
    {
        using StreamReader reader = new(inputStream);
        List<Command> commands = [];

        PrinterState printerState = new();

        while (await reader.ReadLineAsync() is { } line)
        {
            commands.Add(ReadLine(printerState, gcodeFlavor, line));
        }

        return commands;
    }

    public static string GCodeToString(this IEnumerable<Command> commands, GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin)
    {
        StringBuilder builder = new();
        PrinterState printerState = new();

        foreach (Command command in commands)
        {
            string commandString = command.ToGCode(printerState, gcodeFlavor);
            if (commandString != String.Empty)
                builder.Append($"{commandString}\n");
        }

        return builder.ToString();
    }

    public static async Task SaveGCode(this IEnumerable<Command> commands, Stream outputStream,
        GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin)
    {
        PrinterState printerState = new();
        await using StreamWriter writer = new(outputStream);

        foreach (Command command in commands)
        {
            await writer.WriteLineAsync(command.ToGCode(printerState, gcodeFlavor));
        }
    }

    private static Command ReadLine(PrinterState printerState, GCodeFlavor gcodeFlavor, string line)
    {
        if (LinearMoveCommand.IsCommand(line, gcodeFlavor))
            return new LinearMoveCommand(line, gcodeFlavor, printerState);

        if (AbsMovementMode.IsCommand(line, gcodeFlavor))
            return new AbsMovementMode(line, printerState);

        if (EmptyCommand.IsCommand(line, gcodeFlavor))
            return new EmptyCommand(line);

        return new UnrecognizedCommand(line, gcodeFlavor);
    }
}