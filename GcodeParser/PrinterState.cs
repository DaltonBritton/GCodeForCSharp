using System.Diagnostics.Contracts;
using System.Numerics;
using GCodeParser.Commands;

namespace GCodeParser;

/// <summary>
/// The state of a 3D printer at some point while parsing or saving GCode.
/// <para>
///      IMPORTANT:
///             Setting fields of the printer state DOES NOT generate gcode,
///             fields of the printer state should ONLY be set from WITHIN a <see cref="ICommand"/> class.
///             Setting a field of the printer state without properly managing it within a <see cref="ICommand"/> class
///             may CORRUPT the printer state and lead to errors when <see cref="ICommand">Commands</see> are parsed or
///             converted to gcode.
/// </para>
/// </summary>
public class PrinterState
{
    private bool _absExtruderMode = true;

    private bool _absMode = true;
    private double _e;
    private bool _extruderAbsOverride;

    private double _x;
    private double _y;
    private double _z;

    private readonly Dictionary<string, object> _externalProperties = new();

    /// <summary>
    /// The Current Absolute Position of the X Axis of the 3D Printer.
    /// </summary>
    public double X
    {
        get => _x;
        set => SetAxis(value, ref _x, _absMode);
    }

    /// <summary>
    /// The Current Absolute Position of the Y Axis of the 3D Printer.
    /// </summary>
    public double Y
    {
        get => _y;
        set => SetAxis(value, ref _y, _absMode);
    }

    /// <summary>
    /// The Current Absolute Position of the Z Axis of the 3D Printer.
    /// </summary>
    public double Z
    {
        get => _z;
        set => SetAxis(value, ref _z, _absMode);
    }

    /// <summary>
    /// The Current Absolute Position of the E Axis of the 3D Printer.
    /// </summary>
    public double E
    {
        get => _e;
        set => SetAxis(value, ref _e, _absExtruderMode);
    }

    /// <summary>
    /// The Current Maximum flow rate of the 3D Printer.
    /// </summary>
    public double F { get; set; }

    /// <summary>
    /// The Current AbsMode of the 3D printer
    /// </summary>
    public bool AbsMode
    {
        get => _absMode;
        set
        {
            _absMode = value;
            if (!_extruderAbsOverride)
                _absExtruderMode = value;
        }
    }

    /// <summary>
    /// The Current AbsMode of the extruder of the 3D printer.
    /// Setting the AbsExtruderMode will override the AbsMode of the 3D printer
    /// </summary>
    public bool AbsExtruderMode
    {
        get => _absExtruderMode;
        set
        {
            _absExtruderMode = value;
            _extruderAbsOverride = true;
        }
    }

    /// <summary>
    /// Used to add properties to the PrinterState when injecting custom parsers or saving custom commands.
    ///
    /// Ideally would only be modified within the <see cref="ICommand.ApplyToState"/> method.
    /// </summary>
    public object this[string property]
    {
        get => _externalProperties[property];
        set => _externalProperties[property] = value;
    }

    /// <summary>
    /// Gets the resulting position after moving to a new location.
    /// Note: <paramref name="newPos"/> will be interpreted as abs/relative depending on the current state.
    /// Important: Doesn't update the state
    /// </summary>
    /// <param name="newPos">The movement command to execute</param>
    /// <returns>The resulting position after moving</returns>
    [Pure]
    public Vector3 GetPrinterPosAfterMovement(double? x, double? y, double? z)
    {
        double xResult = _x;
        double yResult = _y;
        double zResult = _z;
        
        if(x != null)
            SetAxis((double) x, ref xResult, _absMode);
        if(y != null)
            SetAxis((double) y, ref yResult, _absMode);
        if(z != null)
            SetAxis((double) z, ref zResult, _absMode);

        return new()
        {
            X = (float) xResult,
            Y = (float) yResult,
            Z = (float) zResult,
        };
    }

    /// <summary>
    /// Gets the resulting ExtruderPos after moving to a new location.
    /// Note: <paramref name="newExtruderPos"/> will be interpreted as abs/relative depending on the current state.
    /// Important: Doesn't update the state
    /// </summary>
    /// <param name="newExtruderPos">The amount of fillement extruded in movement</param>
    /// <returns>The resulting ExtruderPos after moving</returns>
    public double GetExtruderPosAfterMovement(double? newExtruderPos)
    {
        double e = _e;
        
        if(newExtruderPos != null)
            SetAxis((double) newExtruderPos, ref e, _absExtruderMode);

        return e;
    }
    
    /// <summary>
    /// Sets the temp of the hotEnd
    /// </summary>
    public float HotEndTemp = 0;

    /// <summary>
    /// Sets the temp of the bed
    /// </summary>
    public float BedTemp = 0;

    /// <summary>
    /// Sets the temp of the chamber
    /// </summary>
    public float ChamberTemp = 0;

    /// <summary>
    /// True if the x axis has been homed
    /// </summary>
    public bool XHome = false;

    /// <summary>
    /// True if the y axis has been homed
    /// </summary>
    public bool YHome = false;

    /// <summary>
    /// True if the z axis has been homed
    /// </summary>
    public bool ZHome = false;

    private static void SetAxis(double value, ref double axis, bool isAbs)
    {
        if (isAbs)
            axis = value;
        else
            axis += value;
    }
}