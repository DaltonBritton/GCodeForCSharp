namespace Tests;

[TestClass]
public class SetPositionTests
{
    /*
    [TestMethod]
    public void TestSetPos1()
    {
        string gcode = "G0 X10\n" +
                       "G92 X0\n" +
                       "G0 X10";

        Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(gcode));
        GCodeFile gcodeFile = new(inputStream);

        IEnumerable<string> commands = gcodeFile.GetCommands().Select(command => command.ToMarlin());

        string[] expectedCommands =
        [
            "G0 X10",
            "G0 X20"
        ];

        Assert.IsTrue(commands.SequenceEqual(expectedCommands));
    }

    [TestMethod]
    public void TestSetPos2()
    {
        string gcode = "G0 X10\n" +
                       "G92 X0\n" +
                       "G92 X10\n" +
                       "G0 X10";

        Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(gcode));
        GCodeFile gcodeFile = new(inputStream);

        IEnumerable<string> commands = gcodeFile.GetCommands().Select(command => command.ToMarlin());

        string[] expectedCommands =
        [
            "G0 X10"
        ];

        Assert.IsTrue(commands.SequenceEqual(expectedCommands));
    }

    [TestMethod]
    public void TestSetPos3()
    {
        string gcode = "G1 E10\n" +
                       "G92 E4\n" +
                       "G1 E10\n";

        Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(gcode));
        GCodeFile gcodeFile = new(inputStream);

        IEnumerable<string> commands = gcodeFile.GetCommands().Select(command => command.ToMarlin());

        string[] expectedCommands =
        [
            "G1 E10",
            "G1 E6",
        ];

        Assert.IsTrue(commands.SequenceEqual(expectedCommands));
    }

    [TestMethod]
    public void TestSetPos4()
    {
        string gcode = "G1 X10 Y20 Z30 E40\n" +
                       "G92 X1 Y2 Z3 E4\n" +
                       "G1 X10 Y20 Z30 E40\n";

        Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(gcode));
        GCodeFile gcodeFile = new(inputStream);

        IEnumerable<string> commands = gcodeFile.GetCommands().Select(command => command.ToMarlin());

        string[] expectedCommands =
        [
            "G1 X10 Y20 Z30 E40",
            "G1 X19 Y38 Z57 E36",
        ];

        Assert.IsTrue(commands.SequenceEqual(expectedCommands));
    }

    [TestMethod]
    public void TestSetPos5()
    {
        string gcode = "G91\n" +
                       "G1 X10 Y20 Z30 E40\n" +
                       "G92 X1 Y2 Z3 E4\n" +
                       "G1 X10 Y20 Z30 E40\n";

        Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(gcode));
        GCodeFile gcodeFile = new(inputStream);

        IEnumerable<string> commands = gcodeFile.GetCommands().Select(command => command.ToMarlin());

        string[] expectedCommands =
        [
            "G1 X10 Y20 Z30 E40",
            "G1 X20 Y40 Z60 E40",
        ];

        Assert.IsTrue(commands.SequenceEqual(expectedCommands));
    }

    [TestMethod]
    public void TestSetPos6()
    {
        string gcode = "M82\n" +
                       "G1 E10\n" +
                       "G92 E4\n" +
                       "G1 E10\n";

        Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(gcode));
        GCodeFile gcodeFile = new(inputStream);

        IEnumerable<string> commands = gcodeFile.GetCommands().Select(command => command.ToMarlin());

        string[] expectedCommands =
        [
            "G1 E10",
            "G1 E6",
        ];

        Assert.IsTrue(commands.SequenceEqual(expectedCommands));
    }
    */
}