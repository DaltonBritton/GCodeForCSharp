using GcodeParser;
using GcodeParser.Commands;
using GCodeParser;
using GCodeParser.Commands;

namespace Tests;

[TestClass]
public class HeaterTempTests
{
    [TestMethod]
    public void TestCreateCommand()
    {
        PrinterState printerState = new PrinterState();
        HeaterTempCommand command = new(97.7f, Heater.Bed);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("M140 S97.7 ", generated);
    }

    [TestMethod]
    public void TestReadCommand()
    {
        PrinterState printerState = new PrinterState();
        HeaterTempCommand command = new("M140 S97.7", GCodeFlavor.Marlin);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("M140 S97.7", generated);
    }
}