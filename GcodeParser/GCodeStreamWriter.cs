﻿using GCodeParser.Commands;

namespace GCodeParser;

/// <summary>
/// Writes GCode commands to an output stream.
/// </summary>
/// <param name="outputStream">The stream to write to.</param>
/// <param name="gcodeFlavor">The flavor of gcode to write commands as.</param>
public class GCodeStreamWriter(Stream outputStream, GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin)
    : IDisposable, IAsyncDisposable
{
    private readonly StreamWriter _backingStream = new(outputStream);

    /// <summary>
    /// The Current State of the printer given all commands provided to the GCodeStreamWriter
    /// </summary>
    public readonly PrinterState PrinterState = new();

    /// <summary>
    /// Saves a command to the output stream.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public void SaveCommand(Command command)
    {
        string gcodeLine = command.ToGCode(PrinterState, gcodeFlavor);

        command.ApplyToState(PrinterState);
        
        if(gcodeLine != string.Empty)
            _backingStream.WriteLine(gcodeLine);
    }

    /// <summary>
    /// Saves a command to the output stream Asynchronously.
    /// </summary>
    /// <param name="command">The command to save.</param>
    public async ValueTask SaveCommandAsync(Command command)
    {
        string gcodeLine = command.ToGCode(PrinterState, gcodeFlavor);
        
        command.ApplyToState(PrinterState);
        
        if(gcodeLine != string.Empty)
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
        outputStream.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _backingStream.DisposeAsync();
        await outputStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}