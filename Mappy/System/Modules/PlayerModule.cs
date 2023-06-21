using KamiLib.AutomaticUserInterface;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.System.Modules;

public class PlayerConfig : ModuleConfigBase
{
    [FloatConfigOption("IconScale", "ModuleConfig", 0)]
    public float IconScale = 0.60f;
}

public class PlayerModule : ModuleBase
{
    public override ModuleName ModuleName { get; init; } = ModuleName.Player;
    public override ModuleConfigBase Configuration { get; protected set; } = new PlayerConfig();
}