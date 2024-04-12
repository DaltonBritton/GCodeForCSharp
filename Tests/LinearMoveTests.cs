using GcodeParser;
using GCodeParser;
using GCodeParser.Commands;

// ReSharper disable ObjectCreationAsStatement

namespace Tests;

[TestClass]
public class LinearMoveTests
{
    [TestMethod]
    public async Task TestLinearMove()
    {
        await using Stream inputStream = new MemoryStream("G0 X5"u8.ToArray());

        await using GCodeStreamReader gcodeReader = new GCodeStreamReader(inputStream);

        List<Command> commands = new();
        await foreach (Command command in gcodeReader)
        {
            commands.Add(command);
        }

        PrinterState printerState = new();

        Assert.AreEqual(1, commands.Count);
        Assert.AreEqual("G0 X5", commands[0].ToGCode(printerState, GCodeFlavor.Marlin));
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
}