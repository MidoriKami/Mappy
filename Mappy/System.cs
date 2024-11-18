using System.Collections.Generic;
using KamiLib.Classes;
using KamiLib.CommandManager;
using KamiLib.Window;
using Mappy.Classes.Caches;
using Mappy.Controllers;
using Mappy.Data;
using Mappy.Modules;
using Mappy.Windows;

namespace Mappy;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public static class System {
    public static SystemConfig SystemConfig { get; set; }
    public static IconConfig IconConfig { get; set; }
    public static WindowManager WindowManager { get; set; }
    public static MapWindow MapWindow { get; set; }
    public static ConfigurationWindow ConfigWindow { get; set; }
    public static MapRenderer.MapRenderer MapRenderer { get; set; }
    public static IntegrationsController IntegrationsController { get; set; }
    public static CommandManager CommandManager { get; set; }
    public static Teleporter Teleporter { get; set; }

    public static List<ModuleBase> Modules { get; set; } = [
        new TripleTriadModule(),
        new FateModule(),
    ];
    
    public static TooltipCache TooltipCache { get; set; } = new();
    public static CardRewardCache CardRewardCache { get; set; } = new();
    public static GatheringPointNameCache GatheringPointNameCache { get; set; } = new();
    public static GatheringPointIconCache GatheringPointIconCache { get; set; } = new();
    public static TripleTriadCache TripleTriadCache { get; set; } = new();
    public static AetheryteAethernetCache AetheryteAethernetCache { get; set; } = new();
}