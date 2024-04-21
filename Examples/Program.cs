

using Examples;
using GCodeParser;

string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
string gcodeFilePath = $"{documentsPath}{Path.DirectorySeparatorChar}fancyStuff.gcode";
File.Delete(gcodeFilePath);

Stream debugStream = File.OpenWrite(gcodeFilePath);

GCodeStreamWriter gcodeWriter = new(debugStream);

VaseMode.SpiralVase(gcodeWriter,0.2f, 0.8f, 1.75f, 210, 25f, 75f, 3.502f, 5f, resolution: float.Pi/100, rotationsPerLayer: 2);


gcodeWriter.Flush();