using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("IconConfig", -5)]
public interface IIconConfig
{
    [BoolConfig("ShowIcon")]
    public bool ShowIcon { get; set; }
    
    [FloatConfig("IconScale", 0.25f, 2.0f)]
    public float IconScale { get; set; }
}