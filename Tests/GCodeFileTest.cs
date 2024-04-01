// ReSharper disable ObjectCreationAsStatement

namespace Tests;

[TestClass]
public class GCodeFileTest
{
    /*
    [TestMethod]
    public void GCodeSaveTest()
    {
        const string gcode = "\ufeffG0 X10\n" +
                             "G0 X20\n";

        Stream inputStream = new MemoryStream(Encoding.UTF8.GetBytes(gcode));
        GCodeFile gcodeFile = new(inputStream);

        MemoryStream savedGCode = new();
        gcodeFile.Save(savedGCode, new UserInput());


        string fileContents = MemoryStreamToString(savedGCode);

        Assert.AreEqual(gcode, fileContents);

    }

    private string MemoryStreamToString(MemoryStream memoryStream)
    {
        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }
    */
}