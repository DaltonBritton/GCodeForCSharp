using GCodeParser;
using GCodeParser.Commands;

namespace Tests;

[TestClass]
public class SetPositionCommandTests
{
    private PrinterState _printerState;
    [TestInitialize]
    public void TestInitialize()
    {
        _printerState = new PrinterState();
    }
    [TestMethod]
    public void TestCreateCommand()
    {
        SetPosition command = new(10,11, 12,13);
        string createdCommand = command.ToGCode(_printerState, GCodeFlavor.Marlin);
        
        Assert.AreEqual("G92 X10 Y11 Z12 E13", createdCommand);
    }

    [TestMethod]
    public void TestReadCommand()
    {
        SetPosition command = new("G92 X11 E12", GCodeFlavor.Marlin);
        string createdCommand = command.ToGCode(_printerState, GCodeFlavor.Marlin);
        
        Assert.AreEqual("G92 X11 E12", createdCommand);
    }

    [TestMethod]
    public void TestOffset()
    {
        SetPosition command = new("G92 Y100", GCodeFlavor.Marlin);
        _printerState.Y = 45;
        command.ApplyToState(_printerState);
        _printerState.Y = 0;
        Assert.AreEqual(-55, _printerState.Y);
    }
}