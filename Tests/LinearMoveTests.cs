using System.Numerics;
using GcodeParser;
using GCodeParser;
using GCodeParser.Commands;

// ReSharper disable ObjectCreationAsStatement

namespace Tests;

[TestClass]
public class LinearMoveTests
{
    [TestMethod]
    public void TestLinearMove()
    {
        using Stream inputStream = new MemoryStream("G0 X5"u8.ToArray());

        using GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);

        List<ICommand> commands = new();
        foreach (ICommand command in gcodeReader)
        {
            commands.Add(command);
        }

        PrinterState printerState = new();
        Span<char> buffer = new char[100];

        Assert.AreEqual(1, commands.Count);
        Assert.AreEqual("G0 X5", commands[0].ToGCode(printerState, GCodeFlavor.Marlin, buffer).ToString());
    }

    [TestMethod]
    public async Task TestLinearMove2X()
    {
        await using Stream inputStream = new MemoryStream("G0 X5\nG1 X10"u8.ToArray());

        await using GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);
        string[] expectedCommands = ["G0 X5", "G0 X10"]; // G1 Converted to G0 bc no filament was extruded

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestLinearMoveRelative()
    {
        await using Stream inputStream = new MemoryStream("G91\nG0 X10"u8.ToArray());

        await using GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // Relative command gets removed and replaced with abs pos movements
        string[] expectedCommands = ["G0 X10"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestLinearMoveRelative2X()
    {
        await using Stream inputStream = new MemoryStream("G91\nG0 X10\nG0 X10"u8.ToArray());
        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // Relative command gets removed and replaced with abs pos movements
        string[] expectedCommands = ["G0 X10", "G0 X20"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestRelativeLinearMoveAbsLinearMove()
    {
        await using Stream inputStream = new MemoryStream("G91\nG0 X10\nG90\nG0 X5"u8.ToArray());
        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // Relative command gets removed and replaced with abs pos movements
        string[] expectedCommands = ["G0 X10", "G0 X5"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestExtruderOverrideAbs()
    {
        await using Stream inputStream = new MemoryStream("G91\nM82\nG0 X10 E10\nG0 X5 E5\n"u8.ToArray());
        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        string[] expectedCommands = ["G1 X10 E10", "G1 X15 E-5"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestExtruderOverrideRel()
    {
        await using Stream inputStream = new MemoryStream("M83\nG0 X10 E10\nG0 X5 E5\n"u8.ToArray());
        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // E is exported relative
        string[] expectedCommands = ["G1 X10 E10", "G1 X5 E5"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestAbsNoMovement()
    {
        await using Stream inputStream = new MemoryStream("G0 X10 Y10 Z10 E5\nG1 X10 Y10 Z10 E5"u8.ToArray());
        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // G0 converted to G1 for commands that extrude filament
        // Second command gets removed because no movement is preformed
        string[] expectedCommands = ["G1 X10 Y10 Z10 E5"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestRelNoMovement()
    {
        await using Stream inputStream = new MemoryStream("G91\nG0 X10 Y10 Z10 E5\nG1 X0 Y0 Z0 E0"u8.ToArray());

        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // G0 converted to G1 for commands that extrude filament
        // Second command gets removed because no movement is preformed
        string[] expectedCommands = ["G1 X10 Y10 Z10 E5"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestAbsPartialMovement()
    {
        await using Stream inputStream = new MemoryStream("G90\nM82\nG0 X10 Y10 Z10 E5\nG1 X5 Y10 Z15 E5"u8.ToArray());
        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // G0 converted to G1 for commands that extrude filament
        // Second command gets removed because no movement is preformed
        string[] expectedCommands = ["G1 X10 Y10 Z10 E5", "G0 X5 Z15"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    public async Task TestRelPartialMovement()
    {
        await using Stream inputStream = new MemoryStream("G91\nG0 X10 Y10 Z10 E5\nG1 X5 Y0 Z15 E0"u8.ToArray());
        GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);


        // G0 converted to G1 for commands that extrude filament
        // Second command gets removed because no movement is preformed
        string[] expectedCommands = ["G1 X10 Y10 Z10 E5", "G0 X15 Z25"];

        await Helpers.AssertCommandsEqual(gcodeReader, expectedCommands);
    }

    [TestMethod]
    [ExpectedException(typeof(DuplicateArgumentException))]
    public async Task TestDuplicateArgument()
    {
        await using Stream inputStream = new MemoryStream("G0 X10 X10"u8.ToArray());
        await using GCodeStreamReader gcodeStream = new GCodeStreamReader(inputStream);
        await gcodeStream.ReadNextCommandAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidGCode))]
    public async Task TestLinearMoveInvalidParam()
    {
        await using Stream inputStream = new MemoryStream("G0 A10"u8.ToArray());
        await using GCodeStreamReader gcodeStream = new GCodeStreamReader(inputStream);
        await gcodeStream.ReadNextCommandAsync();
    }

    [TestMethod]
    public void TestGetResultingPosAbs()
    {
        GCodeStreamWriter gcodeStreamWriter = new GCodeStreamWriter(Stream.Null);

        LinearMoveCommand command1 = new(x: 10, y: 20, z: 30);
        Vector3 result1 = command1.GetResultingPos(gcodeStreamWriter.PrinterState);
        
        Assert.AreEqual(new(x: 10, y: 20, z: 30), result1);
        gcodeStreamWriter.SaveCommand(command1);

        LinearMoveCommand command2 = new(x: 40, y: 50, z: 60);
        Vector3 result2 = command2.GetResultingPos(gcodeStreamWriter.PrinterState);
        
        Assert.AreEqual(new(x: 40, y: 50, z: 60), result2);
    }
    
    [TestMethod]
    public void TestGetResultingPosRel()
    {
        GCodeStreamWriter gcodeStreamWriter = new GCodeStreamWriter(Stream.Null);
        gcodeStreamWriter.SaveCommand(new AbsMovementMode(false));

        LinearMoveCommand command1 = new(x: 10, y: 20, z: 30);
        Vector3 result1 = command1.GetResultingPos(gcodeStreamWriter.PrinterState);
        
        Assert.AreEqual(new(x: 10, y: 20, z: 30), result1);
        gcodeStreamWriter.SaveCommand(command1);

        LinearMoveCommand command2 = new(x: 40, y: 50, z: 60);
        Vector3 result2 = command2.GetResultingPos(gcodeStreamWriter.PrinterState);
        
        Assert.AreEqual(new(x: 50, y: 70, z: 90), result2);
    }
    
    [TestMethod]
    public void TestGetResultingExtruderMovementAbs()
    {
        using GCodeStreamWriter gcodeStreamWriter = new GCodeStreamWriter(Stream.Null);
        gcodeStreamWriter.SaveCommand(new AbsMovementMode(true, true));

        LinearMoveCommand command1 = new(e: 10);
        double result1 = command1.GetResultingExtruderPos(gcodeStreamWriter.PrinterState);
        
        Assert.AreEqual(10,result1);
        gcodeStreamWriter.SaveCommand(command1);

        LinearMoveCommand command2 = new(e: 20);
        double result2 = command2.GetResultingExtruderPos(gcodeStreamWriter.PrinterState);

        
        Assert.AreEqual(10,result2);
    }
    
    [TestMethod]
    public void TestGetResultingExtruderMovementRel()
    {
        using GCodeStreamWriter gcodeStreamWriter = new GCodeStreamWriter(Stream.Null);

        LinearMoveCommand command1 = new(e: 10);
        double result1 = command1.GetResultingExtruderPos(gcodeStreamWriter.PrinterState);
        
        Assert.AreEqual(10, result1);
        gcodeStreamWriter.SaveCommand(command1);

        LinearMoveCommand command2 = new(e: 20);
        double result2 = command2.GetResultingExtruderPos(gcodeStreamWriter.PrinterState);

        
        Assert.AreEqual(20, result2);
    }
    
    [TestMethod]
    public void TestGetResultingExtruderMovementNoMovement()
    {
        using GCodeStreamWriter gcodeStreamWriter = new GCodeStreamWriter(Stream.Null);

        LinearMoveCommand command1 = new(e: 10);
        double result1 = command1.GetResultingExtruderPos(gcodeStreamWriter.PrinterState);
        
        Assert.AreEqual(10, result1);
        gcodeStreamWriter.SaveCommand(command1);

        LinearMoveCommand command2 = new(x: 20, y: 100);
        double result2 = command2.GetResultingExtruderPos(gcodeStreamWriter.PrinterState);

        
        Assert.AreEqual(0, result2);
    }
}