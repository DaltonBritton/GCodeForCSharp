using System.Diagnostics.CodeAnalysis;
using GCodeParser.Commands;

namespace GCodeParser;

public class GCodeStreamReader(Stream inputStream, GCodeFile.GCodeFlavor gcodeFlavor = GCodeFile.GCodeFlavor.Marlin)
    : IDisposable, IAsyncDisposable, IAsyncEnumerable<Command>
{
    
    /// <summary>
    /// Generates a Command given a line of Gcode. Used to inject custom command parsers into the GcodeStreamReader.
    /// </summary>
    /// <param name="gcodeLine">A single line of a gcodefile. Note: Doesn't include new line chars.</param>
    /// <param name="command">
    /// The command that was found when parsing the <paramref name="gcodeLine"/>.
    /// Null if no command was recognized.
    /// </param>
    /// <returns>True if the command was recognized, false if otherwise.</returns>
    public delegate bool CustomCommandGenerator(string gcodeLine, GCodeFile.GCodeFlavor gcodeFlavor, PrinterState printerState, [NotNullWhen(true)] out Command? command);
    
    private readonly StreamReader _backingStream = new(inputStream);

    private readonly PrinterState _printerState = new();
    private readonly List<CustomCommandGenerator> _customCommandGenerators = new();


    public bool HasNextCommand => !_backingStream.EndOfStream;

    public ValueTask DisposeAsync()
    {
        _backingStream.Dispose();
        inputStream.Dispose();
        return ValueTask.CompletedTask;
    }

    public async IAsyncEnumerator<Command> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken())
    {
        while (HasNextCommand)
        {
            yield return await ReadNextCommandAsync();
        }
    }

    public void Dispose()
    {
        _backingStream.Dispose();
        inputStream.Dispose();
    }

    public Command ReadNextCommand()
    {
        string? line = _backingStream.ReadLine();
        if (line == null)
            throw new Exception("Reached end of stream, no more commands exist");

        Command command = ReadLine(_printerState, line);

        return command;
    }

    public async Task<Command> ReadNextCommandAsync()
    {
        string? line = await _backingStream.ReadLineAsync();
        if (line == null)
            throw new Exception("Reached end of stream, no more commands exist");

        Command command = ReadLine(_printerState, line);

        return command;
    }
    
    public void AddCustomGCodeParser(CustomCommandGenerator customCommandGenerator)
    {
        _customCommandGenerators.Add(customCommandGenerator);
    }
    
    

    private Command ReadLine(PrinterState printerState, string line)
    {
        if (EvaluateCustomCommandGenerators(line, printerState, out Command? customCommand))
            return customCommand;
        
        if (LinearMoveCommand.IsCommand(line, gcodeFlavor))
            return new LinearMoveCommand(line, gcodeFlavor, printerState);

        if (AbsMovementMode.IsCommand(line, gcodeFlavor))
            return new AbsMovementMode(line, printerState);

        if (EmptyCommand.IsCommand(line, gcodeFlavor))
            return new EmptyCommand(line);

        return new UnrecognizedCommand(line, gcodeFlavor);
    }

    private bool EvaluateCustomCommandGenerators(string gcodeLine, PrinterState printerState, [NotNullWhen(true)] out Command? command)
    {
        foreach (var customCommandGenerator in _customCommandGenerators)
        {
            if (customCommandGenerator(gcodeLine, gcodeFlavor, printerState, out command))
                return true;
        }

        command = null;
        return false;
    }
}