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
        command.ApplyToState(_printerState);
        string createdCommand = command.ToGCode(_printerState, GCodeFlavor.Marlin);
        
        Assert.AreEqual("G92 X10 Y11 Z12 E13", createdCommand);
    }

    [TestMethod]
    public void TestReadCommand()
    {
        SetPosition command = new("G92 X11 E12", GCodeFlavor.Marlin);
        command.ApplyToState(_printerState);
        string createdCommand = command.ToGCode(_printerState, GCodeFlavor.Marlin);
        
        Assert.AreEqual("G92 X11 E12", createdCommand);
    }

    [TestMethod]
    public void TestOffset()
    {
        
    }
}