using GCodeParser.Commands;

namespace GCodeParser;

/// <summary>
/// Writes GCode commands to an output stream.
/// </summary>
public class GCodeStreamWriter : IDisposable, IAsyncDisposable
{
    private readonly StreamWriter _backingStream;

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
    /// The Current State of the printer given all commands provided to the GCodeStreamWriter
    /// </summary>
    public readonly PrinterState PrinterState = new();

    /// <summary>
    /// Saves a command to the output stream.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public virtual void SaveCommand(ICommand command)
    {
        Span<char> buffer = stackalloc char[500];
        ReadOnlySpan<char> gcodeLine = command.ToGCode(PrinterState, _gcodeFlavor, buffer);

        command.ApplyToState(PrinterState);
        
        if(gcodeLine != string.Empty)
            _backingStream.WriteLine(gcodeLine);
    }
    
    /// <summary>
    /// Saves a LinearMoveCommand to the output stream.
    /// Do to high frequency of linear move commands this method exists to prevent boxing allocations and improve preformace.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public virtual void SaveCommand(LinearMoveCommand command)
    {
        Span<char> buffer = stackalloc char[500];
        ReadOnlySpan<char> gcodeLine = command.ToGCode(PrinterState, _gcodeFlavor, buffer);

        command.ApplyToState(PrinterState);
        
        if(gcodeLine != string.Empty)
            _backingStream.WriteLine(gcodeLine);
    }

    /// <summary>
    /// Saves a command to the output stream Asynchronously.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public virtual async ValueTask SaveCommandAsync(ICommand command)
    {
        string gcodeLine = GetGCodeAsString(command);
        
        command.ApplyToState(PrinterState);
        
        if(gcodeLine != string.Empty)
            await _backingStream.WriteLineAsync(gcodeLine);
    }
    
    /// <summary>
    /// Saves a command to the output stream Asynchronously.
    /// Do to high frequency of linear move commands this method exists to prevent boxing allocations and improve preformace.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public virtual async ValueTask SaveCommandAsync(LinearMoveCommand command)
    {
        string gcodeLine = GetGCodeAsString(command);
        
        command.ApplyToState(PrinterState);
        
        if(gcodeLine != string.Empty)
            await _backingStream.WriteLineAsync(gcodeLine);
    }

    /// <summary>
    /// Saves all commands within the IEnumerable to the output stream.
    /// </summary>
    /// <param name="commands">A List of commands to save to the output stream</param>
    public void SaveCommands(IEnumerable<ICommand> commands)
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
    public async ValueTask SaveCommandsAsync(IEnumerable<ICommand> commands)
    {
        foreach (var command in commands)
        {
            await SaveCommandAsync(command);
        }
    }

    /// <inheritdoc cref="SaveCommandsAsync(System.Collections.Generic.IEnumerable{ICommand})"/>
    public async ValueTask SaveCommandsAsync(IAsyncEnumerable<ICommand> commands)
    {
        await foreach (ICommand command in commands)
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

    private string GetGCodeAsString(ICommand command)
    {
        Span<char> buffer = stackalloc char[500];
        ReadOnlySpan<char> gcodeLine = command.ToGCode(PrinterState, _gcodeFlavor, buffer);

        return gcodeLine.ToString();
    }
    
    private string GetGCodeAsString(LinearMoveCommand command)
    {
        Span<char> buffer = stackalloc char[500];
        ReadOnlySpan<char> gcodeLine = command.ToGCode(PrinterState, _gcodeFlavor, buffer);

        return gcodeLine.ToString();
    }
    
    
    private void AddWaterMark()
    {
        _backingStream.WriteLine("; GCode Generated/Modified by GCodeForCSharp");
        _backingStream.WriteLine("; For More Information Visit https://github.com/DaltonBritton/GCodeForCSharp");
    }

    private void AddStartingGCode()
    {
        _backingStream.WriteLine("G92 E0");
        
        PrinterState.AbsMode = true;
        _backingStream.WriteLine("G90");

        PrinterState.AbsExtruderMode = false;
        _backingStream.WriteLine("M83");
    }
}