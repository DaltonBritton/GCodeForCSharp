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
        HeaterTempCommand command = new(97.7f, HeaterTempCommand.Heater.bed);
        string generated = command.ToGCode(printerState, GCodeFlavor.Marlin);

        Assert.AreEqual("M140 S97.7", generated);
    }

    [TestMethod]
    public void TestReadCommand()
    {

    }
}