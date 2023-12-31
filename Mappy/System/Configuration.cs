﻿using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Mappy.DataModels;
using Mappy.Modules;

namespace Mappy.System;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public Setting<bool> KeepOpen = new(true);
    public Setting<bool> FollowPlayer = new(false); 
    public Setting<bool> LockWindow = new(false);
    public Setting<bool> HideWindowFrame = new(false);
    public Setting<bool> FadeWhenUnfocused = new(true);
    public Setting<bool> HideInDuties = new(false);
    public Setting<float> FadePercent = new(0.6f);
    public Setting<bool> AlwaysShowToolbar = new(false);
    public Setting<bool> EnableIntegrations = new(true);
    public Setting<bool> HideBetweenAreas = new(false);
    public Setting<bool> HideInCombat = new(false);

    public PlayerMapComponentSettings PlayerSettings = new();
    public AllianceMemberSettings AllianceSettings = new();
    public FateSettings FateSettings = new();
    public GatheringPointSettings GatheringPoints = new();
    public MapMarkersSettings MapMarkers = new();
    public PartyMemberSettings PartyMembers = new();
    public PetSettings Pet = new();
    public WaymarkSettings Waymarks = new();
    public QuestSettings QuestMarkers = new();
    public TreasureSettings Treasure = new();
    public HousingSettings Housing = new();
    public FlagMarkerSettings Flag = new();
    public GatheringAreaSettings GatheringArea = new();

    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;
    public void Initialize(DalamudPluginInterface inputPluginInterface) => pluginInterface = inputPluginInterface;
    public void Save() => pluginInterface!.SavePluginConfig(this);
}
