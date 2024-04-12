using GCodeParser;
using GCodeParser.Commands;

namespace Tests;

public static class Helpers
{
    public static async ValueTask AssertCommandsEqual(IAsyncEnumerable<Command> gcodeFile,
        IEnumerable<string> expectedCommands)
    {
        MemoryStream memoryStream = new MemoryStream();

        await using GCodeStreamWriter gcodeWriter = new(memoryStream);

        await gcodeWriter.SaveCommandsAsync(gcodeFile);

        await gcodeWriter.FlushAsync();
        memoryStream.Position = 0;


        using StreamReader reader = new(memoryStream);


        // Starting GCode
        expectedCommands =
        [
            "; GCode Generated/Modified by GCodeForCSharp",
            "; For More Information Visit https://github.com/DaltonBritton/GCodeForCSharp",
            "G92 E0",
            "G90",
            "M83",
            ..expectedCommands,
        ];
        
        using IEnumerator<string> expectedCommandsIterator = expectedCommands.GetEnumerator();


        string? gcodeLine = await reader.ReadLineAsync();
        while (gcodeLine != null && expectedCommandsIterator.MoveNext())
        {
            string expectedLine = expectedCommandsIterator.Current;
            Assert.AreEqual(expectedLine, gcodeLine);

            gcodeLine = await reader.ReadLineAsync();
        }

        Assert.IsNull(gcodeLine);
        Assert.IsFalse(expectedCommandsIterator.MoveNext());
    }
}