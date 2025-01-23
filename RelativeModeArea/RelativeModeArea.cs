using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using RelativeModeArea.Common;

namespace RelativeModeArea;

[PluginName(PLUGIN_NAME)]
public class RelativeModeArea : RelativeModeAreaBase, IPositionedPipelineElement<IDeviceReport>
{
    public const string PLUGIN_NAME = "Relative Mode Area";

    public RelativeModeArea() : base(PLUGIN_NAME) { }

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
        if (_initialized == false || Handle(report) == false)
            OnEmit(report);
    }

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
