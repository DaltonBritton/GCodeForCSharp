using GCodeParser;
using GCodeParser.Commands;

namespace Tests;
[TestClass]
public class SetFanSpeedTests
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
        SetFanSpeed command = new(125);
        string createdCommand = command.ToGCode(_printerState, GCodeFlavor.Marlin);
        Assert.AreEqual("M106 S125", createdCommand);
        
        SetFanSpeed command2 = new(0);
        string createdCommand2 = command2.ToGCode(_printerState, GCodeFlavor.Marlin);
        Assert.AreEqual("M107", createdCommand2);
    }

    [TestMethod]
    public void TestReadCommand()
    {
        SetFanSpeed command = new("M107", GCodeFlavor.Marlin);
        string createdCommand = command.ToGCode(_printerState, GCodeFlavor.Marlin);
        
        Assert.AreEqual("M107", createdCommand);

        SetFanSpeed command2 = new("M106 S111", GCodeFlavor.Marlin);
        string createdCommand2 = command2.ToGCode(_printerState, GCodeFlavor.Marlin);
        
        Assert.AreEqual("M106 S111", createdCommand2);
    }

    [TestMethod]
    public void TestSetFanSpeed()
    {
        SetFanSpeed command = new("M106 S120", GCodeFlavor.Marlin);
        command.ApplyToState(_printerState);
        
        Assert.AreEqual(120, _printerState.FanSpeed);
    }

    [TestMethod]
    public void TestSetFanSpeedIOverride()
    {
        SetFanSpeed command = new("M106 I10 S125", GCodeFlavor.Marlin);
        command.ApplyToState(_printerState);
        
        Assert.AreEqual(10, _printerState.FanSpeed);
    }

    [TestMethod]
    public void TestSetFanSpeedOff()
    {
        SetFanSpeed command = new("M107", GCodeFlavor.Marlin);
        command.ApplyToState(_printerState);
        
        Assert.AreEqual(0, _printerState.FanSpeed);
    }

    [TestMethod]
    public void TestSetFannSpeedMax()
    {
        SetFanSpeed command = new("M106", GCodeFlavor.Marlin);
        command.ApplyToState(_printerState);
        
        Assert.AreEqual(255, _printerState.FanSpeed);
    }
}