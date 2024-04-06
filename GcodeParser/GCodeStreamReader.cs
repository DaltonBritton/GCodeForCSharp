using System.Collections;
using System.Diagnostics.CodeAnalysis;
using GcodeParser.Commands;
using GCodeParser.Commands;

namespace GCodeParser;

/// <summary>
/// Reads GCode from an <paramref name="inputStream"/>.
/// </summary>
/// <param name="inputStream">The stream to read from</param>
/// <param name="gcodeFlavor">The expected flavor of the gcode.</param>
public class GCodeStreamReader(Stream inputStream, GCodeFlavor gcodeFlavor = GCodeFlavor.Marlin)
    : IDisposable, IAsyncDisposable, IEnumerable<Command>, IAsyncEnumerable<Command>
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
    public delegate bool CustomCommandGenerator(string gcodeLine, GCodeFlavor gcodeFlavor, PrinterState printerState, [NotNullWhen(true)] out Command? command);
    
    private readonly StreamReader _backingStream = new(inputStream);

    private readonly PrinterState _printerState = new();
    private readonly List<CustomCommandGenerator> _customCommandGenerators = new();


    /// <summary>
    /// Reads the next command in the GCode File.
    /// </summary>
    /// <returns>A Command representing the next command in a file, null if end of file is reached</returns>
    public Command? ReadNextCommand()
    {
        string? line = _backingStream.ReadLine();
        if (line == null)
            return null;

        Command command = ReadLine(_printerState, line);

        return command;
    }

    /// <inheritdoc cref="ReadNextCommand"/>
    public async Task<Command?> ReadNextCommandAsync()
    {
        string? line = await _backingStream.ReadLineAsync();
        if (line == null)
            return null;

        Command command = ReadLine(_printerState, line);

        return command;
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    /// <inheritdoc />
    public IEnumerator<Command> GetEnumerator()
    {
        Command? command = ReadNextCommand();
        while (command != null)
        {
            yield return command;
            
            command = ReadNextCommand();
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerator<Command> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
    {
        Command? command = await ReadNextCommandAsync();
        while (command != null)
        {
            yield return command;
            
            command = await ReadNextCommandAsync();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _backingStream.Dispose();
        inputStream.Dispose();
    }
    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _backingStream.Dispose();
        inputStream.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Injects a GCode Parser into the parsing tree.
    /// <para>
    /// Used to Parse Commands unique to a specific printer or to Parse Slicer Generated Comments such as line type changes, or layer changes.
    /// </para>
    /// <para>
    /// CustomCommandGenerators are called in the order they were added before any internal CommandGenerators are called, for each line in the GCodeFile.
    /// </para>
    /// </summary>
    /// <param name="customCommandGenerator">A Command Generator used to parse commands in a GCode File.</param>
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
            return new AbsMovementMode(line, gcodeFlavor, printerState);

        if (HeaterTempCommand.IsCommand(line, gcodeFlavor))
            return new HeaterTempCommand(line, gcodeFlavor);

        if (AutoHomeCommand.IsCommand(line, gcodeFlavor))
            return new AutoHomeCommand(line, gcodeFlavor);

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