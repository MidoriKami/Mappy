using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("ZoomOptions", 3)]
public interface IZoomConfig
{
    [BoolConfig("FocusObjective", "FocusObjectiveHelp")]
    public bool FocusObjective { get; set; }

    [FloatConfig("ZoomSpeed", 0.05f, 0.40f)]
    public float ZoomSpeed { get; set; }

    [BoolConfig("ZoomOnFlag", "ZoomOnFlagHelp")]
    public bool ZoomInOnFlag { get; set; }
}