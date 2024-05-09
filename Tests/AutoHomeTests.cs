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
        Span<char> buffer = stackalloc char[100];
        
        ReadOnlySpan<char> generated = command.ToGCode(printerState, GCodeFlavor.Marlin, buffer);

        Assert.AreEqual("G28 X", generated.ToString());
    }

    [TestMethod]
    public void TestCreateCommand2()
    {
        PrinterState printerState = new PrinterState();
        List<Axis> list = new();
        list.Add(Axis.X);
        list.Add(Axis.Y);
        AutoHomeCommand command = new(list);
        Span<char> buffer = stackalloc char[100];

        
        ReadOnlySpan<char>  generated = command.ToGCode(printerState, GCodeFlavor.Marlin, buffer);

        Assert.AreEqual("G28 X Y", generated.ToString());
    }

    [TestMethod]
    public void TestCreateCommand3()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new();
        Span<char> buffer = stackalloc char[100];

        
        ReadOnlySpan<char>  generated = command.ToGCode(printerState, GCodeFlavor.Marlin, buffer);

        Assert.AreEqual("G28 X Y Z", generated.ToString());
    }

    [TestMethod]
    public void TestReadCommand()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new("G28 X ", GCodeFlavor.Marlin);
        
        Span<char> buffer = stackalloc char[100];

        ReadOnlySpan<char>  generated = command.ToGCode(printerState, GCodeFlavor.Marlin, buffer);

        Assert.AreEqual("G28 X", generated.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidGCode))]
    public void TestInvalidRead()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new("G28 X L", GCodeFlavor.Marlin);
    }


    [TestMethod]
    public void TestWithCommennt()
    {
        PrinterState printerState = new PrinterState();
        AutoHomeCommand command = new("G28 X Y; Lets Get more danger", GCodeFlavor.Marlin);
        
        Span<char> buffer = stackalloc char[100];

        ReadOnlySpan<char>  generated = command.ToGCode(printerState, GCodeFlavor.Marlin, buffer);

        Assert.AreEqual("G28 X Y; Lets Get more danger", generated.ToString());
    }
}