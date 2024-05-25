using System;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.Extensions;
using Lumina.Excel.GeneratedSheets;
using Mappy.Classes;
using MapType = FFXIVClientStructs.FFXIV.Client.UI.Agent.MapType;

namespace Mappy.Controllers;

public unsafe class IntegrationsController : IDisposable {

    private delegate void ShowMapDelegate(AgentMap* agentMap, bool a1, bool a2);
    private delegate void OpenMapByIdDelegate(AgentMap* agent, uint mapID);
    private delegate void OpenMapDelegate(AgentMap* agent, OpenMapInfo* data);
    
    private readonly Hook<ShowMapDelegate>? showMapHook;
    private readonly Hook<OpenMapByIdDelegate>? openMapByIdHook;
    private readonly Hook<OpenMapDelegate>? openMapHook;
    
    public IntegrationsController() {
        showMapHook ??= Service.Hooker.HookFromAddress<ShowMapDelegate>(AgentMap.Addresses.ShowMap.Value, OnShowHook);
        openMapByIdHook ??= Service.Hooker.HookFromAddress<OpenMapByIdDelegate>((nint)AgentMap.Addresses.OpenMapByMapId.Value, OpenMapById);
        openMapHook ??= Service.Hooker.HookFromAddress<OpenMapDelegate>((nint)AgentMap.Addresses.OpenMap.Value, OpenMap);
        
        if (Service.ClientState is { IsPvP: false }) {
            Enable();
            
            if (AgentMap.Instance()->AgentInterface.AddonId is not 0) {
                TryYeetMap();
                System.MapWindow.Open();
            }
        }
        
        Service.ClientState.EnterPvP += Disable;
        Service.ClientState.LeavePvP += Enable;
    }

    public void Dispose() {
        Disable();
        
        showMapHook?.Dispose();
        openMapByIdHook?.Dispose();
        openMapHook?.Dispose();
        
        TryUnYeetMap();
        
        Service.ClientState.EnterPvP -= Disable;
        Service.ClientState.LeavePvP -= Enable;
    }
    
    public void Enable() {
        Service.Log.Debug("Enabling Integrations");
        
        showMapHook?.Enable();
        openMapByIdHook?.Enable();
        openMapHook?.Enable();
    }

    public void Disable() {
        Service.Log.Debug("Disabling Integrations");
        
        showMapHook?.Disable();
        openMapByIdHook?.Disable();
        openMapHook?.Disable();
    }

    private void OnShowHook(AgentMap* agent, bool a1, bool a2)
        => HookSafety.ExecuteSafe(() => {
            showMapHook!.Original(agent, a1, a2);
            TryMirrorGameState(agent);
            
            Service.Log.Debug($"Show Map: {a1}, {a2}");
        }, Service.Log, "Exception during OnShowHook");

    private void OpenMapById(AgentMap* agent, uint mapId) 
        => HookSafety.ExecuteSafe(() => {
            openMapByIdHook!.Original(agent, mapId);
            TryMirrorGameState(agent);
            
            Service.Log.Debug($"Open Map By ID: {mapId}");
        }, Service.Log, "Exception during OpenMapByMapId");
    
    private void OpenMap(AgentMap* agent, OpenMapInfo* mapInfo) 
        => HookSafety.ExecuteSafe(() => {
            openMapHook!.Original(agent, mapInfo);
            TryMirrorGameState(agent);

            switch (mapInfo->Type) {
                case MapType.QuestLog: {
                    // todo: make this function a lot more efficient, we only need the map data now, since we can get the marker data elsewhere.
                    if (GetQuestLocation(mapInfo) is {} targetLevelData ) {
                        agent->OpenMapByMapId(targetLevelData.Map.Row);
                    }
                    
                    CenterOnMarker(agent, agent->TempMapMarkerArraySpan[0].MapMarker);
                    break;
                }

                case MapType.GatheringLog: {
                    CenterOnMarker(agent, agent->TempMapMarkerArraySpan[0].MapMarker);
                    break;
                }

                case MapType.FlagMarker: {
                    CenterOnMarker(agent, agent->FlagMapMarker.MapMarker);
                    break;
                }
            }
            
            Service.Log.Debug($"Open Map With OpenMapInfo: {mapInfo->MapId}, {mapInfo->Type}, {mapInfo->TerritoryId}, {mapInfo->TitleString}");
        }, Service.Log, "Exception during OpenMap");
    
    private static void CenterOnMarker(AgentMap* agent, MapMarkerBase marker) {
        var coordinates = new Vector2(marker.X, marker.Y) / 16.0f * agent->SelectedMapSizeFactorFloat;
        Service.Log.Debug(coordinates.ToString());

        System.MapWindow.ProcessingCommand = true;
        System.MapRenderer.DrawOffset = -coordinates;
    }

    private Level? GetQuestLocation(OpenMapInfo* mapInfo) {
        var targetLevels = QuestHelpers.GetActiveLevelsForQuest(mapInfo->TitleString.ToString(), mapInfo->MapId);
        var focusLevel = targetLevels?.Where(level => level.Map.Row == mapInfo->MapId && level.Map.Row != 0).FirstOrDefault();
    
        if (focusLevel is not null) {
            return focusLevel;
        }
        
        // This quest isn't accepted yet
        else {
            bool MatchQuestName(Quest quest1, OpenMapInfo* mapData) => string.Equals(quest1.Name.RawString, mapData->TitleString.ToString(), StringComparison.OrdinalIgnoreCase);

            if (Service.DataManager.GetExcelSheet<Quest>()?.FirstOrDefault(quest => 
                    MatchQuestName(quest, mapInfo) && 
                    quest is {IssuerLocation.Row: not 0 }) is { IssuerLocation.Value: { } issuerLocation }) {
                return issuerLocation;
            }
        }
    
        return null;
    }
    
    private void TryMirrorGameState(AgentMap* agent) {
        if (agent->AgentInterface.AddonId is not 0) {
            System.MapWindow.Open();
            ImGui.SetWindowFocus(System.MapWindow.WindowName);
            TryYeetMap();
        }
        else {
            System.MapWindow.Close();
            TryUnYeetMap();
        }
    }
    
    public void TryYeetMap() {
        var areaMap = (AtkUnitBase*)Service.GameGui.GetAddonByName("AreaMap");
        if (areaMap is not null && areaMap->RootNode is not null) {
            areaMap->RootNode->SetPositionFloat(-9000.1f, -9000.1f);
        }
    }

    public void TryUnYeetMap() {
        var areaMap = (AtkUnitBase*)Service.GameGui.GetAddonByName("AreaMap");
        if (areaMap is not null && areaMap->RootNode is not null) {
            areaMap->Close(true);
            
            // Little hacky, but we need to wait just a little bit before setting the position, or else we'll see the vanilla map flash closed really quick.
            Service.Framework.RunOnTick(() => areaMap->RootNode->SetPositionFloat(areaMap->X, areaMap->Y), delayTicks: 6);
        }
    }
}