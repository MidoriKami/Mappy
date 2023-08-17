using KamiLib.AutomaticUserInterface;

namespace Mappy.Models.Enums;

public enum ModuleName
{
    [EnumLabel("Player")]
    Player,
    
    [EnumLabel("TemporaryMarkers")]
    TemporaryMarkers,
    
    [EnumLabel("Waymarks")]
    Waymarks,
    
    [EnumLabel("PartyMembers")]
    PartyMembers,
    
    [EnumLabel("Pets")]
    Pets,
    
    [EnumLabel("AllianceMembers")]
    AllianceMembers,
    
    [EnumLabel("TreasureMarkers")]
    TreasureMarkers,
    
    [EnumLabel("QuestMarkers")]
    QuestMarkers,
    
    [EnumLabel("GatheringPoints")]
    GatheringPoint,
    
    [EnumLabel("MapMarkers")]
    MapMarkers,
    
    [EnumLabel("HousingMarkers")]
    HousingMarkers,
    
    [EnumLabel("FATEs")]
    FATEs,
    
    [EnumLabel("MiscMarkers")]
    MiscMarkers,
    
    [EnumLabel("IslandSanctuary")]
    IslandSanctuary,
    
    [EnumLabel("PluginIntegrations")]
    PluginIntegrations,
    
    [EnumLabel("Hostiles")]
    Hostiles,
}