namespace GCodeParser;

public class PrinterState
{
    private bool _absExtruderMode = true;

    private bool _absMode = true;
    private double _e;
    private bool _extruderAbsOverride;

    private double _x;
    private double _y;
    private double _z;

    public double X
    {
        get => _x;
        set => SetAxis(value, ref _x, ref _absMode);
    }

    public double Y
    {
        get => _y;
        set => SetAxis(value, ref _y, ref _absMode);
    }

    public double Z
    {
        get => _z;
        set => SetAxis(value, ref _z, ref _absMode);
    }

    public double E
    {
        get => _e;
        set => SetAxis(value, ref _e, ref _absExtruderMode);
    }

    public double F { get; set; }

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

    public bool AbsExtruderMode
    {
        get => _absExtruderMode;
        set
        {
            _absExtruderMode = value;
            _extruderAbsOverride = true;
        }
    }

    private void SetAxis(double value, ref double axis, ref bool isAbs)
    {
        if (isAbs)
            axis = value;
        else
            axis += value;
    }
}