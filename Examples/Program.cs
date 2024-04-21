

using Examples;
using GCodeParser;

string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
string gcodeFilePath = $"{documentsPath}{Path.DirectorySeparatorChar}fancyStuff.gcode";
File.Delete(gcodeFilePath);

Stream debugStream = File.OpenWrite(gcodeFilePath);

GCodeStreamWriter gcodeWriter = new(debugStream);

// VaseMode.SpiralVase(gcodeWriter,0.2f, 0.8f, 1.75f, 25f, 75f, 3.502f, 5f, resolution: float.Pi/100, rotationsPerLayer: 2);

// DotTexture.CircleOutline(gcodeWriter, 25, 30, 0.75f, 1.75f, offset: new(100, 100));


gcodeWriter.Flush();