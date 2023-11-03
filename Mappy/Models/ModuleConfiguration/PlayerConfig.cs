using System.Numerics;
using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.ModuleConfiguration; 

[Category("ModuleColors")]
public interface IPlayerColorConfig {
    [ColorConfig("OutlineColor", 0, 0, 0, 88)]
    public Vector4 OutlineColor { get; set; }
    
    [ColorConfig("FillColor", 163, 219, 255, 80)]
    public Vector4 FillColor { get; set; }
}

[Category("ModuleConfig")]
public class PlayerConfig : IModuleConfig, IIconConfig, IPlayerColorConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 10;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.40f;
    public Vector4 OutlineColor { get; set; } = new(0.0f, 0.0f, 0.0f, 0.345f);
    public Vector4 FillColor { get; set; } = new(0.639f, 0.858f, 1.0f, 0.313f);
    
    [BoolConfig("ShowCone")]
    public bool ShowCone { get; set; } = true;

    [BoolConfig("ScaleCone")]
    public bool ScaleCone { get; set; } = true;

    [FloatConfig("ConeRadius", 0.0f, 360.0f)]
    public float ConeRadius { get; set; } = 90.0f;

    [FloatConfig("ConeAngle", 0.0f, 180.0f)]
    public float ConeAngle { get; set; } = 90.0f;

    [FloatConfig("OutlineThickness", 0.5f, 5.0f)]
    public float OutlineThickness { get; set; } = 2.0f;
}