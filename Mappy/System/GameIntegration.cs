using System;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib;
using KamiLib.Caching;
using KamiLib.Hooking;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.System.Modules;
using Mappy.Utility;
using Mappy.Views.Windows;
using Map = Lumina.Excel.GeneratedSheets.Map;
using Quest = Lumina.Excel.GeneratedSheets.Quest;

namespace Mappy.System;

public unsafe class GameIntegration : IDisposable
{
    private delegate void OpenMapByIdDelegate(AgentMap* agent, uint mapID);
    private delegate void OpenMapDelegate(AgentMap* agent, OpenMapInfo* data);
    private delegate void SetFlagMarkerDelegate(AgentMap* agent, uint territoryId, uint mapId, float mapX, float mapY, uint iconId);
    private delegate void SetGatheringMarkerDelegate(AgentMap* agent, uint styleFlags, int mapX, int mapY, uint iconID, int radius, Utf8String* tooltip);
    private delegate void ShowMapDelegate(AgentInterface* agentMap, bool a1, bool a2);

    private readonly Hook<OpenMapByIdDelegate>? openMapByIdHook;
    private readonly Hook<OpenMapDelegate>? openMapHook;
    private readonly Hook<SetFlagMarkerDelegate>? setFlagMarkerHook;
    private readonly Hook<SetGatheringMarkerDelegate>? setGatheringMarkerHook;
    private readonly Hook<ShowMapDelegate>? showHook;
    
    private bool integrationsEnabled;
    private bool temporaryGatheringMarkerSet;
    
    public GameIntegration()
    {
        showHook ??= Hook<ShowMapDelegate>.FromAddress((nint) AgentMap.Addresses.ShowMap.Value, OnShowHook);
        openMapByIdHook ??= Hook<OpenMapByIdDelegate>.FromAddress((nint)AgentMap.Addresses.OpenMapByMapId.Value, OpenMapById);
        openMapHook ??= Hook<OpenMapDelegate>.FromAddress((nint)AgentMap.Addresses.OpenMap.Value, OpenMap);
        setFlagMarkerHook ??= Hook<SetFlagMarkerDelegate>.FromAddress((nint)AgentMap.Addresses.SetFlagMapMarker.Value, SetFlagMarker);
        setGatheringMarkerHook ??= Hook<SetGatheringMarkerDelegate>.FromAddress((nint)AgentMap.Addresses.AddGatheringTempMarker.Value, SetGatheringMarker);

        if (MappySystem.SystemConfig.EnableIntegrations)
        {
            Enable();
        }
        
        Service.ClientState.EnterPvP += Disable;
        Service.ClientState.LeavePvP += TryEnable;
    }
    
    public void Dispose()
    {
        openMapByIdHook?.Dispose();
        openMapHook?.Dispose();
        setFlagMarkerHook?.Dispose();
        setGatheringMarkerHook?.Dispose();
        showHook?.Dispose();
        
        Service.ClientState.EnterPvP -= Disable;
        Service.ClientState.LeavePvP -= TryEnable;
    }

    public void Enable()
    {
        Service.Log.Debug("Enabling Integrations");
        
        openMapByIdHook?.Enable();
        openMapHook?.Enable();
        setFlagMarkerHook?.Enable();
        setGatheringMarkerHook?.Enable();
        showHook?.Enable();

        integrationsEnabled = true;
    }

    public void Disable()
    {
        Service.Log.Debug("Disabling Integrations");
        
        openMapByIdHook?.Disable();
        openMapHook?.Disable();
        setFlagMarkerHook?.Disable();
        setGatheringMarkerHook?.Disable();
        showHook?.Disable();
        
        integrationsEnabled = false;
    }

    private void TryEnable()
    {
        if (MappySystem.SystemConfig.EnableIntegrations)
        {
            Enable();
        }
    }

    public void Update()
    {
        if (integrationsEnabled && !MappySystem.SystemConfig.EnableIntegrations)
        {
            Disable();
        }

        if (!integrationsEnabled && MappySystem.SystemConfig.EnableIntegrations)
        {
            Enable();
        }
    }
    
    private void OpenMapById(AgentMap* agent, uint mapId) => Safety.ExecuteSafe(() =>
    {
        MappySystem.MapTextureController.LoadMap(mapId);
    }, "Exception during OpenMapByMapId");

    private void OpenMap(AgentMap* agent, OpenMapInfo* mapInfo) => Safety.ExecuteSafe(() =>
    {
        Service.Log.Debug("OpenMap");

        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { Viewport: var viewport } mapWindow)
        {
            ImGui.SetWindowFocus(mapWindow.WindowName);
            mapWindow.IsOpen = true;
            mapWindow.ProcessingCommand = true;
            
            if (temporaryGatheringMarkerSet && TemporaryMarkers.GatheringMarker is {} tempMarker)
            {
                tempMarker.MapID = mapInfo->MapId;
                temporaryGatheringMarkerSet = false;
            }

            var map = LuminaCache<Map>.Instance.GetRow(mapInfo->MapId)!;

            MappySystem.SystemConfig.FollowPlayer = false;
            
            switch (mapInfo->Type)
            {
                case MapType.FlagMarker when TemporaryMarkers.FlagMarker is { Type: MarkerType.Flag } flag:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    var flagPosition = Position.GetTexturePosition(flag.Position, map);
                    if (MappySystem.SystemConfig.FocusObjective)
                    {
                        viewport.SetViewportCenter(flagPosition);
                        if (MappySystem.SystemConfig.ZoomInOnFlag) viewport.SetViewportZoom(1.0f);
                    }
                    break;
                
                case MapType.QuestLog when GetQuestLocation(mapInfo) is {} questLocation:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    if (MappySystem.SystemConfig.FocusObjective)
                    {
                        var questPosition = Position.GetTexturePosition(questLocation, map);
                        viewport.SetViewportCenter(questPosition);
                        viewport.SetViewportZoom(1.00f);
                    }
                    break;
                
                case MapType.GatheringLog when TemporaryMarkers.GatheringMarker is { Type: MarkerType.Gathering } area:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    if (MappySystem.SystemConfig.FocusObjective)
                    {
                        var gatherAreaPosition = Position.GetTexturePosition(area.Position, map);
                        viewport.SetViewportCenter(gatherAreaPosition);
                        viewport.SetViewportZoom(0.50f);
                    }
                    break;
                
                default:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    break;
            }
        }
    }, "Exception during OpenMap");

    private Vector2? GetQuestLocation(OpenMapInfo* mapInfo)
    {
        var targetLevels = QuestHelpers.GetActiveLevelsForQuest(mapInfo->TitleString.ToString(), mapInfo->MapId);
        var focusLevel = targetLevels?.Where(level => level.Map.Row == mapInfo->MapId && level.Map.Row != 0).FirstOrDefault();
    
        if (focusLevel is not null)
        {
            return new Vector2(focusLevel.X, focusLevel.Z);
        }
        else // This quest isn't accepted yet
        {
            bool MatchQuestName(Quest quest1, OpenMapInfo* mapData) => string.Equals(quest1.Name.RawString, mapData->TitleString.ToString(), StringComparison.OrdinalIgnoreCase);

            if (LuminaCache<Quest>.Instance.FirstOrDefault(quest => MatchQuestName(quest, mapInfo)) is { IssuerLocation.Value: { } issuerLocation, JournalGenre.Value: { } journalInfo })
            {
                var levelLocation = new Vector2(issuerLocation.X, issuerLocation.Z);
                
                TemporaryMarkers.SetGatheringMarker(new TemporaryMapMarker
                {
                    Position = levelLocation,
                    TooltipText = mapInfo->TitleString.ToString(),
                    IconID = (uint) journalInfo.Icon,
                    Radius = 50.0f,
                    Type = MarkerType.Quest,
                    MapID = mapInfo->MapId,
                });

                return levelLocation; 
            }
        }
    
        return null;
    }
    
    private void OnShowHook(AgentInterface* agent, bool a1, bool a2) => Safety.ExecuteSafe(() =>
    {
#if DEBUG
        if (ImGui.GetIO().KeyAlt)
        {
            showHook!.Original(agent, a1, a2);
            return;
        }
#endif
        
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is {} mapWindow)
        {
            ImGui.SetWindowFocus(mapWindow.WindowName);
            mapWindow.IsOpen = !mapWindow.IsOpen;
        }
    }, "Exception during OnShowHook");

    private void SetFlagMarker(AgentMap* agent, uint territoryId, uint mapId, float mapX, float mapY, uint iconId) => Safety.ExecuteSafe(() =>
    {
        Service.Log.Debug($"SetFlagMarker : {mapX} {mapY}");
        
        TemporaryMarkers.SetFlagMarker( new TemporaryMapMarker
        {
            Type = MarkerType.Flag,
            MapID = mapId,
            IconID = iconId,
            Position = new Vector2(mapX, mapY)
        });

        setFlagMarkerHook!.Original(agent, territoryId, mapId, mapX, mapY, iconId);
    }, "Exception during SetFlagMarker");

    private void SetGatheringMarker(AgentMap* agent, uint styleFlags, int mapX, int mapY, uint iconID, int radius, Utf8String* tooltip) => Safety.ExecuteSafe(() =>
    {
        Service.Log.Debug("SetGatheringMarker");

        if (AgentGatheringNote.Instance()->GatheringAreaInfo is not null)
        {
            TemporaryMarkers.SetGatheringMarker(new TemporaryMapMarker
            {
                Type = MarkerType.Gathering,
                MapID = AgentGatheringNote.Instance()->GatheringAreaInfo->OpenMapInfo.MapId,
                IconID = iconID,
                Radius = radius,
                Position = new Vector2(mapX, mapY),
                TooltipText = tooltip->ToString(),
            });
        }
        else
        {
            TemporaryMarkers.SetGatheringMarker(new TemporaryMapMarker
            {
                Type = MarkerType.Gathering,
                IconID = iconID,
                Radius = radius,
                Position = new Vector2(mapX, mapY),
                TooltipText = tooltip->ToString(),
            });
        }

        temporaryGatheringMarkerSet = true;

    }, "Exception during SetGatheringMarker");
}