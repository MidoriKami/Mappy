using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.ModuleConfiguration; 

[Category("ModuleColors")]
public interface IFateColorsConfig {
    [ColorConfig("CircleColor", 0.58f, 0.388f, 0.827f, 0.33f)]
    public Vector4 CircleColor { get; set; }
    
    [ColorConfig("ExpiringColor", 1.0f, 0.0f, 0.0f, 0.33f)]
    public Vector4 ExpiringColor { get; set; }
}

[Category("DirectionalMarker", 1)]
public interface IFateDistanceMarkerConfig {
    [BoolConfig("DirectionalMarker")]
    public bool EnableDirectionalMarker { get; set; }
    
    [FloatConfig("DistanceThreshold", 0.0f, 50.0f)]
    public float DistanceThreshold { get; set; }
}

[Category("ModuleConfig")]
public class FateConfig : IModuleConfig, IIconConfig, ITooltipConfig, IFateColorsConfig, IFateDistanceMarkerConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 3;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
    public bool ShowTooltip { get; set; } = true;
    public Vector4 TooltipColor { get; set; } = KnownColor.White.Vector();
    public Vector4 CircleColor { get; set; } = new(0.58f, 0.388f, 0.827f, 0.33f);
    public Vector4 ExpiringColor { get; set; } = KnownColor.Red.Vector() with { W = 0.33f };
    
    [BoolConfig("ShowRing")]
    public bool ShowRing { get; set; } = true;
    
    [BoolConfig("ExpiringWarning")]
    public bool ExpiringWarning { get; set; } = false;

    [IntCounterConfig("EarlyWarningTime", false)]
    public int EarlyWarningTime { get; set; } = 300;

    public bool EnableDirectionalMarker { get; set; } = true;
    public float DistanceThreshold { get; set; } = 20.0f;
}