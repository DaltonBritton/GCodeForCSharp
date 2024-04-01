using GCodeParser.Commands;

namespace GCodeParser;

public class GCodeStreamReader(Stream inputStream, GCodeFile.GCodeFlavor gcodeFlavor = GCodeFile.GCodeFlavor.Marlin)
    : IDisposable, IAsyncDisposable, IAsyncEnumerable<Command>
{
    private readonly StreamReader _backingStream = new(inputStream);

    private readonly PrinterState _printerState = new();

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

        Command command = ReadLine(_printerState, gcodeFlavor, line);

        return command;
    }

    public async Task<Command> ReadNextCommandAsync()
    {
        string? line = await _backingStream.ReadLineAsync();
        if (line == null)
            throw new Exception("Reached end of stream, no more commands exist");

        Command command = ReadLine(_printerState, gcodeFlavor, line);

        return command;
    }

    private static Command ReadLine(PrinterState printerState, GCodeFile.GCodeFlavor gcodeFlavor, string line)
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