namespace Mappy.Models.ModuleConfiguration; 

public class WaymarkConfig : IModuleConfig, IIconConfig {
    public bool Enable { get; set; } = true;
    public int Layer { get; set; } = 9;
    public bool ShowIcon { get; set; } = true;
    public float IconScale { get; set; } = 0.50f;
}