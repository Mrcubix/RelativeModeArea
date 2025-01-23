using System.Drawing;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace RelativeModeArea.Common;

public class RelativeModeAreaBase
{
    private readonly string _group;

    public RelativeModeAreaBase(string group) => _group = group;

    protected TabletReference _tablet;
    protected IDriver _driver;
    protected bool _initialized = false;

    // Lines per millimeter for each input type
    protected Vector2 _penLpmm;
    protected Vector2 _touchLpmm;

    // The rectangle representing the defined area depending on the input type
    protected RectangleF _penRect;
    protected RectangleF _touchRect;

    protected float _width;
    protected float _height;
    protected float _x;
    protected float _y;

    public event Action<IDeviceReport> Emit;

    [TabletReference]
    public TabletReference Tablet
    {
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

    public virtual void Initialize(TabletReference tablet, IDriver driver)
    {
        _tablet = tablet;
        _driver = driver;

        if (_tablet == null)
            return;

        var digitizer = _tablet.Properties.Specifications.Digitizer;

        // Lines per millimeter for each input type, their resolution are usually different
        _penLpmm = new Vector2(digitizer.MaxX / digitizer.Width, digitizer.MaxY / digitizer.Height);

        var topLeft = new Vector2(_x, _y) - new Vector2(_width / 2, _height / 2);

        // Rectangles for each input types as their resolution are usually different
        _penRect = new RectangleF(topLeft.X * _penLpmm.X, topLeft.Y * _penLpmm.Y, _width * _penLpmm.X, _height * _penLpmm.Y);

        if (_penRect.Left < 0 || _penRect.Top < 0 || _penRect.Right > digitizer.MaxX || _penRect.Bottom > digitizer.MaxY)
            Log.Write(_group, "Part of your defined area is outside of the tablet's digitizer area", LogLevel.Warning);

        _initialized = true;
    }

    /// <summary>
    ///   Handle inputs and emit in specific situations
    /// </summary>
    /// <param name="report">A Tablet report</param>
    /// <remarks>
    ///   This will return without emitting if the input is outside the defined area
    /// </remarks>
    public virtual void Consume(IDeviceReport report)
    {
        // The plugin may only initialize if the current output mode is relative
        Handle(report);
    }

    public void OnEmit(IDeviceReport report) 
        => Emit?.Invoke(report);

    protected virtual bool Handle(IDeviceReport report)
    {
        if (_initialized && report is IAbsolutePositionReport tablet)
        {
            HandlePosition(tablet);
            return true;
        }

        return false;
    }

    protected void HandlePosition(IAbsolutePositionReport positionReport)
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
}
