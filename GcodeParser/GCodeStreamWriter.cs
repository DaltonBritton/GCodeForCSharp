using GCodeParser.Commands;

namespace GCodeParser;

public class GCodeStreamWriter(Stream outputStream, GCodeFile.GCodeFlavor gcodeFlavor = GCodeFile.GCodeFlavor.Marlin)
    : IDisposable, IAsyncDisposable
{
    private readonly StreamWriter _backingStream = new(outputStream);

    private readonly PrinterState _printerState = new();

    public async ValueTask DisposeAsync()
    {
        await _backingStream.DisposeAsync();
        await outputStream.DisposeAsync();
    }

    public void Dispose()
    {
        _backingStream.Dispose();
        outputStream.Dispose();
    }

    public void SaveCommand(Command command)
    {
        _backingStream.WriteLine(command.ToGCode(_printerState, gcodeFlavor));
    }

    public async ValueTask SaveCommandAsync(Command command)
    {
        await _backingStream.WriteLineAsync(command.ToGCode(_printerState, gcodeFlavor));
    }

    public void SaveCommands(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            _backingStream.WriteLine(command.ToGCode(_printerState, gcodeFlavor));
        }
    }

    public async ValueTask SaveCommandsAsync(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            await _backingStream.WriteLineAsync(command.ToGCode(_printerState, gcodeFlavor));
        }
    }

    public async ValueTask SaveCommandsAsync(IAsyncEnumerable<Command> commands)
    {
        await foreach (Command command in commands)
        {
            await _backingStream.WriteLineAsync(command.ToGCode(_printerState, gcodeFlavor));
        }
    }
}