using System.Drawing;
using System.Numerics;
using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OTD.EnhancedOutputMode.Lib.Tablet;
using RelativeModeArea.Common;

namespace RelativeModeArea.Touch;

[PluginName(PLUGIN_NAME)]
public class RelativeModeTouchArea : RelativeModeAreaBase, IPositionedPipelineElement<IDeviceReport>
{
    public const string PLUGIN_NAME = "Relative Mode Area (+ Touch)";

    public RelativeModeTouchArea() : base(PLUGIN_NAME) { }

    public override void Initialize(TabletReference tablet, IDriver driver)
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

        // Lines per millimeter for each input type, their resolution are usually different
        _touchLpmm = new Vector2(TouchMaxX / digitizer.Width, TouchMaxY / digitizer.Height);

        var topLeft = new Vector2(_x, _y) - new Vector2(_width / 2, _height / 2);

        _touchRect = new RectangleF(topLeft.X * _touchLpmm.X, topLeft.Y * _touchLpmm.Y, _width * _touchLpmm.X, _height * _touchLpmm.Y);

        if (_touchRect.Left < 0 || _touchRect.Top < 0 || _touchRect.Right > TouchMaxX || _touchRect.Bottom > TouchMaxY)
            Log.Write(PLUGIN_NAME, "Part of your defined area is outside of the touch tablet's digitizer area", LogLevel.Warning);

        _initialized = true;
    }

    /// <summary>
    ///   Handle inputs and emit in specific situations
    /// </summary>
    /// <param name="report">A Tablet report</param>
    /// <remarks>
    ///   This will return without emitting if the input is outside the defined area
    /// </remarks>
    public override void Consume(IDeviceReport report)
    {
        // The plugin may only initialize if the current output mode is relative
        if (_initialized)
        {
            if (report is TouchConvertedReport touchConverted)
                HandleTouch(touchConverted);
            //else if (Handle(report) == false)
            else
                OnEmit(report);
        }
        else
            OnEmit(report);
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

        OnEmit(touchReport);
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
     Unit("mm"),
     DefaultPropertyValue(1),
     ToolTip("Relative Mode Area:\n\n" +
             "The width of the relative mode area. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float Width
    {
        get => _width;
        set => _width = value;
    }

    [Property("Height"),
     Unit("mm"),
     DefaultPropertyValue(1),
     ToolTip("Relative Mode Area:\n\n" +
             "The height of the relative mode area. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float Height
    {
        get => _height;
        set => _height = value;
    }

    [Property("X"),
     Unit("mm"),
     DefaultPropertyValue(0),
     ToolTip("Relative Mode Area:\n\n" +
             "The centered position on the X axis. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float X
    {
        get => _x;
        set => _x = value;
    }

    [Property("Y"),
     Unit("mm"),
     DefaultPropertyValue(0),
     ToolTip("Relative Mode Area:\n\n" +
             "The centered position on the Y axis. \n" +
             "Switch to Absolute Mode, create your area and copy the value to here.")]
    public float Y
    {
        get => _y;
        set => _y = value;
    }
}
