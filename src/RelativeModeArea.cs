using System.Drawing;
using System.Numerics;
using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Lib.Tablet;

namespace RelativeModeArea;

[PluginName(PLUGIN_NAME)]
public class RelativeModeArea : IPositionedPipelineElement<IDeviceReport>
{
    public const string PLUGIN_NAME = "Relative Mode Area";

    private TabletReference _tablet;
    private IDriver _driver;
    private bool _initialized = false;

    // Lines per millimeter for each input type
    private Vector2 _penLpmm;
    private Vector2 _touchLpmm;

    // The rectangle representing the defined area depending on the input type
    private RectangleF _penRect;
    private RectangleF _touchRect;

    public event Action<IDeviceReport> Emit;

    [TabletReference]
    public TabletReference Tablet { 
        get => _tablet;
        set
        {
            _tablet = value;
            Initialize(_tablet, _driver);
        }
    }

    [Resolved]
    public IDriver Driver
    {
        get => _driver;
        set
        {
            _driver = value;
            Initialize(_tablet, _driver);
        }
    }

    public PipelinePosition Position => PipelinePosition.PreTransform;

    public void Initialize(TabletReference tablet, IDriver driver)
    {
        _tablet = tablet;
        _driver = driver;

        if (_tablet == null || _driver == null)
            return;

        if (_driver is not Driver oDriver)
            return;

        // Obtain the device (there may be multiple of the same device)
        var deviceTree = oDriver.InputDevices.Where(x => x.Properties == Tablet.Properties)
                                             .Where(x => x.OutputMode is RelativeOutputMode)
                                             .FirstOrDefault();

        if (deviceTree == null)
        {
            Log.Write(PLUGIN_NAME, "Could not find relative mode tablet", LogLevel.Error);
            return;
        }

        var digitizer = _tablet.Properties.Specifications.Digitizer;

        // Lines per millimeter
        _penLpmm = new Vector2(digitizer.MaxX / digitizer.Width, digitizer.MaxY / digitizer.Height);
        _touchLpmm = new Vector2(TouchMaxX / digitizer.Width, TouchMaxX / digitizer.Height);

        var topLeft = new Vector2(X, Y) - new Vector2(Width / 2, Height / 2);

        // Rectangles
        _penRect = new RectangleF(topLeft.X * _penLpmm.X, topLeft.Y * _penLpmm.Y, Width * _penLpmm.X, Height * _penLpmm.Y);
        _touchRect = new RectangleF(topLeft.X * _touchLpmm.X, topLeft.Y * _touchLpmm.Y, Width * _touchLpmm.X, Height * _touchLpmm.Y);

        _initialized = true;
    }

    /// <summary>
    ///   Handle inputs and emit in specific situations
    /// </summary>
    /// <param name="report">A Tablet report</param>
    /// <remarks>
    ///   This will return without emitting if the input is outside the defined area
    /// </remarks>
    public void Consume(IDeviceReport report)
    {
        // The plugin may only initialize if the current output mode is relative
        if (_initialized)
        {
            if (report is TouchConvertedReport touchConverted)
                HandleTouch(touchConverted);
            else if (report is IAbsolutePositionReport tablet)
                HandlePosition(tablet);
        }
        else
            Emit?.Invoke(report);
    }

    private void HandlePosition(IAbsolutePositionReport positionReport)
    {
        var position = positionReport.Position;

        if (position.X < _penRect.Left)
            return;
        else if (position.X > _penRect.Right)
            return;

        if (position.Y < _penRect.Top)
            return;
        else if (position.Y > _penRect.Bottom)
            return;

        Emit?.Invoke(positionReport);
    }

    private void HandleTouch(TouchConvertedReport touchReport)
    {
        var position = touchReport.Position;

        if (position.X < _touchRect.Left)
            return;
        else if (position.X > _touchRect.Right)
            return;

        if (position.Y < _touchRect.Top)
            return;
        else if (position.Y > _touchRect.Bottom)
            return;

        Emit?.Invoke(touchReport);
    }

    [Property("Touch Max X"),
     DefaultPropertyValue(4095),
     ToolTip("Relative Mode Area:\n\n" +
             "The maximum value of the X axis. \n" +
             "Check the debugger to obtain this value.")]
    public int TouchMaxX { get; set; }

    [Property("Touch Max Y"),
     DefaultPropertyValue(4095),
     ToolTip("Relative Mode Area:\n\n" +
             "The maximum value of the Y axis. \n" +
             "Check the debugger to obtain this value.")]
    public int TouchMaxY { get; set; }

    [Property("Width"),
     DefaultPropertyValue(1),
     ToolTip("Relative Mode Area:\n\n" +
             "The width of the relative mode area. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float Width { get; set; }

    [Property("Height"),
     DefaultPropertyValue(1),
     ToolTip("Relative Mode Area:\n\n" +
             "The height of the relative mode area. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float Height { get; set; }

    [Property("X"),
     DefaultPropertyValue(1),
     ToolTip("Relative Mode Area:\n\n" +
             "The centered position on the X axis. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float X { get; set; }

    [Property("Y"),
     DefaultPropertyValue(1),
     ToolTip("Relative Mode Area:\n\n" +
             "The centered position on the Y axis. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float Y { get; set; }
}
