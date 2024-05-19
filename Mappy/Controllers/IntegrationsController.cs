using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.Extensions;

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
        showMapHook?.Dispose();
        openMapByIdHook?.Dispose();
        openMapHook?.Dispose();
        
        TryUnYeetMap();
        
        Service.ClientState.EnterPvP -= Disable;
        Service.ClientState.LeavePvP -= Enable;
    }
    
    public void Enable() {
        Service.Log.Debug("Enabling Integrations");
        
        // setFlagMarkerHook?.Enable();
        // setGatheringMarkerHook?.Enable();
        showMapHook?.Enable();
        openMapByIdHook?.Enable();
        openMapHook?.Enable();
    }

    public void Disable() {
        Service.Log.Debug("Disabling Integrations");
        
        // setFlagMarkerHook?.Disable();
        // setGatheringMarkerHook?.Disable();
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
                case MapType.QuestLog:
                    break;
                
                case MapType.GatheringLog:
                    break;
            }
            
            
            Service.Log.Debug($"Open Map By ID: {mapInfo->MapId}, {mapInfo->Type}, {mapInfo->TerritoryId}, {mapInfo->TitleString}");

            // if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { Viewport: var viewport } mapWindow) {
            //     ImGui.SetWindowFocus(mapWindow.WindowName);
            //     
            //     if (MappySystem.SystemConfig.IntegrationsUnCollapse) {
            //         mapWindow.UnCollapseOrToggle();
            //     }
            //     
            //     mapWindow.IsOpen = true;
            //     mapWindow.ProcessingCommand = true;
            //
            //     if (temporaryGatheringMarkerSet && TemporaryMarkers.GatheringMarker is {} tempMarker) {
            //         tempMarker.MapID = mapInfo->MapId;
            //         temporaryGatheringMarkerSet = false;
            //     }
            //
            //     var map = LuminaCache<Map>.Instance.GetRow(mapInfo->MapId)!;
            //
            //     MappySystem.SystemConfig.FollowPlayer = false;
            //
            //     switch (mapInfo->Type) {
            //         case MapType.FlagMarker when TemporaryMarkers.FlagMarker is { Type: MarkerType.Flag } flag:
            //             MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
            //             var flagPosition = Position.GetTexturePosition(flag.Position, map);
            //             if (MappySystem.SystemConfig.FocusObjective) {
            //                 viewport.SetViewportCenter(flagPosition);
            //                 if (MappySystem.SystemConfig.ZoomInOnFlag) viewport.SetViewportZoom(1.0f);
            //             }
            //             break;
            //     
            //         case MapType.QuestLog when GetQuestLocation(mapInfo) is {} questLocation:
            //             MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
            //             if (MappySystem.SystemConfig.FocusObjective) {
            //                 var questPosition = Position.GetTexturePosition(questLocation, map);
            //                 viewport.SetViewportCenter(questPosition);
            //                 viewport.SetViewportZoom(1.00f);
            //             }
            //             break;
            //     
            //         case MapType.GatheringLog when TemporaryMarkers.GatheringMarker is { Type: MarkerType.Gathering } area:
            //             MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
            //             if (MappySystem.SystemConfig.FocusObjective) {
            //                 var gatherAreaPosition = Position.GetTexturePosition(area.Position, map);
            //                 viewport.SetViewportCenter(gatherAreaPosition);
            //                 viewport.SetViewportZoom(0.50f);
            //             }
            //             break;
            //     
            //         default:
            //             MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
            //             break;
            //     }
            // }
            
            
        }, Service.Log, "Exception during OpenMap");
    
    private void TryMirrorGameState(AgentMap* agent) {
        if (agent->AgentInterface.AddonId is not 0) {
            System.MapWindow.Open();
            ImGui.SetWindowFocus(System.MapWindow.WindowName);
        }
        else {
            System.MapWindow.Close();
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