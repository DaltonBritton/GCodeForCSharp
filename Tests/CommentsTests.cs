using GCodeParser;
using GCodeParser.Commands;

namespace Tests;

[TestClass]
public class CommentsTests
{
    [TestMethod]
    public async Task TestComment1()
    {
        Stream inputStream = new MemoryStream("; this is a test Comment"u8.ToArray());

        IEnumerable<Command> commands = await GCodeFile.ReadGCode(inputStream);

        string gcode = commands.GCodeToString();

        Assert.AreEqual("; this is a test Comment\n", gcode);
    }

    [TestMethod]
    public async Task TestComment2()
    {
        Stream inputStream = new MemoryStream("G28; this is a test Comment"u8.ToArray());

        IEnumerable<Command> commands = await GCodeFile.ReadGCode(inputStream);

        string gcode = commands.GCodeToString();

        Assert.AreEqual("G28; this is a test Comment\n", gcode);
    }
}