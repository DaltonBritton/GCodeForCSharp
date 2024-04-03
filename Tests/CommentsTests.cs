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

        GCodeStreamReader gcodeStream = new GCodeStreamReader(inputStream);

        await Helpers.AssertCommandsEqual(gcodeStream, ["; this is a test Comment"]);
    }

    [TestMethod]
    public async Task TestComment2()
    {
        Stream inputStream = new MemoryStream("G28; this is a test Comment"u8.ToArray());

        GCodeStreamReader gcodeStream = new GCodeStreamReader(inputStream);

        await Helpers.AssertCommandsEqual(gcodeStream, ["G28; this is a test Comment"]);
    }
}