using GCodeParser;
using GCodeParser.Commands;

namespace Tests;

public static class Helpers
{
    public static async ValueTask AssertCommandsEqual(IAsyncEnumerable<ICommand> gcodeFile,
        IEnumerable<string> expectedCommands)
    {
        MemoryStream memoryStream = new MemoryStream();

        await using GCodeStreamWriter gcodeWriter = new(memoryStream);

        await gcodeWriter.SaveCommandsAsync(gcodeFile);

        await gcodeWriter.FlushAsync();
        memoryStream.Position = 0;
        
        var expectedCommandsIterator = AddStartedCommand(expectedCommands);

        foreach (var (expectedLine, actualLine) in expectedCommandsIterator.Zip(GetLines(memoryStream)))
        {
            Assert.AreEqual(expectedLine, actualLine);
        }
    }

    private static IEnumerable<string> AddStartedCommand(IEnumerable<string> expectedCommands)
    {
        expectedCommands =
        [
            "; GCode Generated/Modified by GCodeForCSharp",
            "; For More Information Visit https://github.com/DaltonBritton/GCodeForCSharp",
            "G92 E0",
            "G90",
            "M83",
            ..expectedCommands,
        ];

        return expectedCommands;
    }

    private static IEnumerable<string> GetLines(Stream stream)
    {
        StreamReader reader = new(stream);

        string? line = reader.ReadLine();

        while (line != null)
        {
            yield return line;
            
            line = reader.ReadLine();
        }
    }
}