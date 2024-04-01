using GCodeParser.Commands;

namespace GCodeParser;

public class GCodeStreamWriter(Stream outputStream, GCodeFile.GCodeFlavor gcodeFlavor = GCodeFile.GCodeFlavor.Marlin)
    : IDisposable, IAsyncDisposable
{
    private readonly StreamWriter _backingStream = new(outputStream);

    private readonly PrinterState _printerState = new();

    public void SaveCommand(Command command)
    {
        string gcodeLine = command.ToGCode(_printerState, gcodeFlavor);
        
        if(gcodeLine != string.Empty)
            _backingStream.WriteLine(gcodeLine);
    }

    public async ValueTask SaveCommandAsync(Command command)
    {
        string gcodeLine = command.ToGCode(_printerState, gcodeFlavor);
        
        if(gcodeLine != string.Empty)
            await _backingStream.WriteLineAsync(gcodeLine);
    }

    public void SaveCommands(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            SaveCommand(command);
        }
    }

    public async ValueTask SaveCommandsAsync(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            await SaveCommandAsync(command);
        }
    }

    public async ValueTask SaveCommandsAsync(IAsyncEnumerable<Command> commands)
    {
        await foreach (Command command in commands)
        {
            await SaveCommandAsync(command);
        }
    }

    public void Flush()
    {
        _backingStream.Flush();
    }
    public async Task FlushAsync()
    {
        await _backingStream.FlushAsync();
    }
    
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
    
}