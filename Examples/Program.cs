

using Examples;
using GCodeParser;
using GCodeParser.Commands;

Stream debugStream = File.OpenWrite("C:/Users/unada/Downloads/vase.gcode");

GCodeStreamWriter gcodeWriter = new(debugStream);

List<Command> vase = VaseMode.SpiralVase(0.2f, 0.6f, 1.75f, 50f, 150f, 5.004f, 5f);

gcodeWriter.SaveCommands(vase);
gcodeWriter.Flush();