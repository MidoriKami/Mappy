using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("ZoomOptions", 4)]
public interface IZoomConfig
{
    [BoolConfig("AllowZoomOnHover")]
    public bool AllowZoomOnHover { get; set; }
    
    [FloatConfig("ZoomSpeed", 0.05f, 0.40f)]
    public float ZoomSpeed { get; set; }
}