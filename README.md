# GCodeForCSharp

This C# library provides functionality to parse and generate GCode, a common language used in 3D Printer for controlling their movements and operations.

## Features

- **Parsing**: Easily parse GCode commands into structured data for further processing or manipulation.
- **Generation**: Generate GCode commands programmatically, allowing dynamic creation of 3D printing instructions.

## Installation

You can install the GCode Parser/Generator library via NuGet Package Manager:


```
Install-Package GCodeForCSharp
```

## Usage

### Parsing GCode

``` CSharp
using GCodeParser;  
  
using GCodeStreamReader gcodeReader = new(File.OpenRead("Path/To/File.gcode"));  
  
foreach (var command in gcodeReader)  
{  
    //Do something  
}
```

### Generating GCode

``` CSharp
using GCodeParser;  
using GCodeParser.Commands;  
  
using GCodeStreamWriter gcodeWriter = new(File.OpenWrite("Path/To/Save/File.gcode"));  
  
for (int i = 0; i < 100; i++)  
{  
    float angle = 2 * float.Pi * i / 100;  
    LinearMoveCommand command = new(x: MathF.Cos(angle), y: MathF.Sin(angle));  
  
    gcodeWriter.SaveCommand(command);  
}
```

## Contributing

Contributions to this project are welcome! If you encounter any issues or have suggestions for improvements, feel free to open an issue or submit a pull request on [GitHub](https://github.com/DaltonBritton/GCodeForCSharp).

## License

This project is licensed under the MIT License - see the LICENSE file for details.
