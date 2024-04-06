using GcodeParser.Commands;
using GCodeParser;
using GCodeParser.Commands;

namespace Tests;

[TestClass]
public class AutoHomeTests
{
    [TestMethod]
    public void TestCreateCommand1()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new(AutoHomeCommand.Axis.X);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("G28 X ", generated);
    }

    [TestMethod]
    public void TestCreateCommand2()
    {
        PrinterState printerState = new PrinterState();
        List<AutoHomeCommand.Axis> list = new();
        list.Add(AutoHomeCommand.Axis.X);
        list.Add(AutoHomeCommand.Axis.Y);
        AutoHomeCommand command = new(list);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("G28 X Y ", generated);
    }

    [TestMethod]
    public void TestCreateCommand3()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new();
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("G28 X Y Z ", generated);
    }

    [TestMethod]
    public void TestReadCommand()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new("G28 X ", GCodeFlavor.Marlin);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("G28 X ", generated);
    }
}