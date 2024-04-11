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