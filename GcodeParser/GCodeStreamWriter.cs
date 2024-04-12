using GCodeParser.Commands;

namespace GCodeParser;

/// <summary>
/// Writes GCode commands to an output stream.
/// </summary>
public class GCodeStreamWriter : IDisposable, IAsyncDisposable
{
    private readonly StreamWriter _backingStream;

    private readonly PrinterState _printerState = new();
    private readonly Stream _outputStream;
    private readonly GCodeFlavor _gcodeFlavor;

    /// <summary>
    /// Writes GCode commands to an output stream.
    /// </summary>
    /// <param name="outputStream">The stream to write to.</param>
    /// <param name="gcodeFlavor">The flavor of gcode to write commands as.</param>
    public GCodeStreamWriter(Stream outputStream, GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin)
    {
        _outputStream = outputStream;
        _gcodeFlavor = gcodeFlavor;
        _backingStream = new(outputStream);

        AddWaterMark();
        AddStartingGCode();
    }

    /// <summary>
    /// Saves a command to the output stream.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public void SaveCommand(Command command)
    {
        string gcodeLine = command.ToGCode(_printerState, _gcodeFlavor);

        command.ApplyToState(_printerState);

        if (gcodeLine != string.Empty)
            _backingStream.WriteLine(gcodeLine);
    }

    /// <summary>
    /// Saves a command to the output stream Asynchronously.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public async ValueTask SaveCommandAsync(Command command)
    {
        string gcodeLine = command.ToGCode(_printerState, _gcodeFlavor);

        command.ApplyToState(_printerState);

        if (gcodeLine != string.Empty)
            await _backingStream.WriteLineAsync(gcodeLine);
    }

    /// <summary>
    /// Saves all commands within the IEnumerable to the output stream.
    /// </summary>
    /// <param name="commands">A List of commands to save to the output stream</param>
    public void SaveCommands(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            SaveCommand(command);
        }
    }

    /// <summary>
    /// Saves all commands within the IEnumerable to the output stream Asynchronously.
    /// </summary>
    /// <param name="commands">A List of commands to save to the output stream</param>
    public async ValueTask SaveCommandsAsync(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
            await SaveCommandAsync(command);
        }
    }

    /// <inheritdoc cref="SaveCommandsAsync(System.Collections.Generic.IEnumerable{GCodeParser.Commands.Command})"/>
    public async ValueTask SaveCommandsAsync(IAsyncEnumerable<Command> commands)
    {
        await foreach (Command command in commands)
        {
            await SaveCommandAsync(command);
        }
    }

    /// <summary>
    /// Writes all buffered commands to the output stream, before clearing the buffer.
    /// </summary>
    public void Flush()
    {
        _backingStream.Flush();
    }

    /// <inheritdoc cref="Flush"/>
    public async Task FlushAsync()
    {
        await _backingStream.FlushAsync();
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _backingStream.Dispose();
        _outputStream.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _backingStream.DisposeAsync();
        await _outputStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private void AddWaterMark()
    {
        _backingStream.WriteLine("; GCode Generated/Modified by GCodeForCSharp");
        _backingStream.WriteLine("; For More Information Visit https://github.com/DaltonBritton/GCodeForCSharp");
    }

    private void AddStartingGCode()
    {
        _backingStream.WriteLine("G92 E0");
        _backingStream.WriteLine("G90");
        _backingStream.WriteLine("M83");
    }
}