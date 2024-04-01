namespace Tests;

[TestClass]
public class MiscTests
{
    /*
    [TestMethod]
    public void TestHomeCommand()
    {
        Stream inputStream = new MemoryStream("G28"u8.ToArray());

        GCodeFile gCodeFile = new GCodeFile(inputStream);

        var commands = gCodeFile.GetCommands();

        Assert.AreEqual(commands.Count, 1);
        Assert.AreEqual("G28", commands[0].ToMarlin());
    }

    [TestMethod]
    public void TestMultiline()
    {
        Stream inputStream = new MemoryStream("G28\nG29"u8.ToArray());

        GCodeFile gCodeFile = new GCodeFile(inputStream);

        var commands = gCodeFile.GetCommands();

        Assert.AreEqual(2, commands.Count);
        Assert.AreEqual("G28", commands[0].ToMarlin());
        Assert.AreEqual("G29", commands[1].ToMarlin());
    }
    */
}