

using Examples;
using GCodeParser;
using GCodeParser.Commands;

string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
string gcodeFilePath = $"{documentsPath}{Path.DirectorySeparatorChar}Vase.gcode";
File.Delete(gcodeFilePath);

Stream debugStream = File.OpenWrite(gcodeFilePath);

GCodeStreamWriter gcodeWriter = new(debugStream);

//VaseMode.SpiralVase(gcodeWriter,0.2f, 0.8f, 1.75f, 25f, 75f, 2.2002f, 23f, resolution: float.Pi/100, rotationsPerLayer: 5);

DotTexture.CircleOutline(gcodeWriter, 25, 5, 0.5f, 1.75f, offset: new(100, 100));
gcodeWriter.Flush();