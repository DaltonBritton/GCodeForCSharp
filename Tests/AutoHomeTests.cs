using GcodeParser;
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
        AutoHomeCommand command = new(Axis.X);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("G28 X ", generated);
    }

    [TestMethod]
    public void TestCreateCommand2()
    {
        PrinterState printerState = new PrinterState();
        List<Axis> list = new();
        list.Add(Axis.X);
        list.Add(Axis.Y);
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

    [TestMethod]
    [ExpectedException(typeof(InvalidGCode))]
    public void TestInvalidRead()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new("G28 X L ", GCodeFlavor.Marlin);
        
    }


    [TestMethod]
    public void TestWithCommennt()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new("G28 X Y ; Lets Get more danger", GCodeFlavor.Marlin);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("G28 X Y ; Lets Get more danger", generated);

    }
}