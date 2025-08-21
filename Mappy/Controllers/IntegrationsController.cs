using System;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Classes;
using KamiLib.Extensions;
using Lumina.Excel.Sheets;
using Mappy.Classes;
using Mappy.Extensions;
using MapType = FFXIVClientStructs.FFXIV.Client.UI.Agent.MapType;

namespace Mappy.Controllers;

public unsafe class IntegrationsController : IDisposable
{
    private readonly Hook<AgentMap.Delegates.ShowMap>? showMapHook;
    private readonly Hook<AgentMap.Delegates.OpenMap>? openMapHook;

    public IntegrationsController()
    {
        showMapHook ??= Service.Hooker.HookFromAddress<AgentMap.Delegates.ShowMap>(AgentMap.MemberFunctionPointers.ShowMap, OnShowHook);
        openMapHook ??= Service.Hooker.HookFromAddress<AgentMap.Delegates.OpenMap>(AgentMap.MemberFunctionPointers.OpenMap, OnOpenMapHook);

        if (Service.ClientState is { IsPvP: false })
        {
            EnableIntegrations();
        }

        Service.ClientState.EnterPvP += DisableIntegrations;
        Service.ClientState.LeavePvP += EnableIntegrations;
    }

    public void Dispose()
    {
        DisableIntegrations();

        showMapHook?.Dispose();
        openMapHook?.Dispose();

        Service.ClientState.EnterPvP -= DisableIntegrations;
        Service.ClientState.LeavePvP -= EnableIntegrations;
    }

    private void EnableIntegrations()
    {
        Service.Log.Debug("Enabling Integrations");

        showMapHook?.Enable();
        openMapHook?.Enable();

        System.AreaMapController.EnableIntegrations();
        System.FlagController.EnableIntegrations();
    }

    private void DisableIntegrations()
    {
        Service.Log.Debug("Disabling Integrations");

        showMapHook?.Disable();
        openMapHook?.Disable();

        System.AreaMapController.DisableIntegrations();
        System.FlagController.DisableIntegrations();
    }

    private void OnShowHook(AgentMap* agent, bool a1, bool a2) =>
        HookSafety.ExecuteSafe(() =>
        {
            Service.Log.Debug("[OnShow] Beginning Show");

            if (!ShouldShowMap())
            {
                Service.Log.Debug("[OnShow] Condition to open map is rejected, aborting.");
                return;
            }

            if (AgentMap.Instance()->AddonId is not 0 && AgentMap.Instance()->CurrentMapId != AgentMap.Instance()->SelectedMapId)
            {
                if (!System.SystemConfig.KeepOpen)
                {
                    AgentMap.Instance()->Hide();
                }

                Service.Log.Debug("[OnShow] Vanilla tried to return to current map, aborted.");
                return;
            }

            if (System.SystemConfig.KeepOpen)
            {
                Service.Log.Debug("[OnShow] Keeping Open");
                return;
            }

            showMapHook!.Original(agent, a1, a2);
        }, Service.Log, "Exception during OnShowHook");

    private void OnOpenMapHook(AgentMap* agent, OpenMapInfo* mapInfo) =>
        HookSafety.ExecuteSafe(() =>
        {
            openMapHook!.Original(agent, mapInfo);

            switch (mapInfo->Type)
            {
                case MapType.QuestLog:
                    ProcessQuestLink(agent, mapInfo);
                    break;

                case MapType.GatheringLog:
                    ProcessGatheringLink(agent);
                    break;

                case MapType.FlagMarker:
                    ProcessFlagLink(agent);
                    break;

                case MapType.Bozja:
                    ProcessForayLink(agent, mapInfo);
                    break;

                case MapType.MobHunt:
                case MapType.SharedFate:
                case MapType.Teleport:
                case MapType.Treasure:
                    ProcessTeleportLink(agent, mapInfo);
                    break;

                // This appears to get triggered after a Teleport/Shared Fate teleport event.
                case MapType.Centered:

                case MapType.AetherCurrent:
                default:
                    Service.Log.Debug($"[OpenMap] Ignoring MapType: {mapInfo->Type}");
                    break;
            }

            if (System.SystemConfig.AutoZoom)
            {
                MapRenderer.MapRenderer.Scale = DrawHelpers.GetMapScaleFactor() * System.SystemConfig.AutoZoomScaleFactor;
            }
        }, Service.Log, "Exception during OpenMap");

    private void ProcessQuestLink(AgentMap* agent, OpenMapInfo* mapInfo)
    {
        Service.Log.Debug("[OpenMap] Processing QuestLog Event");

        var targetMapId = mapInfo->MapId;

        if (GetMapIdForQuest(mapInfo) is { } foundMapId)
        {
            Service.Log.Debug($"[OpenMap] GetMapIdForQuest identified Quest Target Map as MapId: {foundMapId}");

            if (targetMapId is 0)
            {
                Service.Log.Debug($"[OpenMap] targetMapId was {targetMapId} using foundMapId: {foundMapId}");
                targetMapId = foundMapId;
            }
        }

        if (agent->SelectedMapId != targetMapId)
        {
            Service.Log.Debug($"[OpenMap] Opening MapId: {targetMapId}");
            OpenMap(targetMapId);
        }
        else
        {
            Service.Log.Debug($"[OpenMap] Already in MapId: {targetMapId}, aborting.");
        }

        if (System.SystemConfig.CenterOnQuest)
        {
            ref var targetMarker = ref agent->TempMapMarkers[0].MapMarker;
            CenterOnMarker(targetMarker);
            Service.Log.Debug($"[OpenMap] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
        }

        System.MapWindow.ProcessingCommand = true;
    }

    private static void ProcessGatheringLink(AgentMap* agent)
    {
        Service.Log.Debug("[OpenMap] Processing GatheringLog Event");

        if (System.SystemConfig.CenterOnGathering)
        {
            ref var targetMarker = ref agent->TempMapMarkers[0].MapMarker;

            CenterOnMarker(targetMarker);
            Service.Log.Debug($"[OpenMap] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
        }

        System.MapWindow.ProcessingCommand = true;
    }

    private static void ProcessFlagLink(AgentMap* agent)
    {
        Service.Log.Debug("[OpenMap] Processing FlagMarker Event");

        if (System.SystemConfig.CenterOnFlag)
        {
            ref var targetMarker = ref agent->FlagMapMarkers[0].MapMarker;

            CenterOnMarker(targetMarker);
            Service.Log.Debug($"[OpenMap] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
        }

        System.MapWindow.ProcessingCommand = true;
    }

    private static void ProcessForayLink(AgentMap* agent, OpenMapInfo* mapInfo)
    {
        Service.Log.Debug("[OpenMap] Processing Bozja Event");

        var eventMarker = agent->EventMarkers.FirstOrNull(marker => marker.DataId == mapInfo->FateId && marker.Flags == 0x40);
        if (eventMarker is not null)
        {
            CenterOnMarker(eventMarker.Value);
        }

        System.MapWindow.ProcessingCommand = true;
    }

    private void ProcessTeleportLink(AgentMap* agent, OpenMapInfo* mapInfo)
    {
        Service.Log.Debug("[OpenMap] Processing Teleport Event");

        var targetMapId = mapInfo->MapId;

        if (agent->CurrentMapId != targetMapId)
        {
            Service.Log.Debug($"[OpenMap] Opening MapId: {targetMapId}");

            OpenMap(mapInfo->MapId);

            System.MapWindow.ProcessingCommand = true;

            return;
        }

        Service.Log.Debug($"[OpenMap] Already in MapId: {targetMapId}, aborting.");
        System.MapWindow.ProcessingCommand = true;
    }

    public void OpenMap(uint mapId)
    {
        AgentMap.Instance()->OpenMapByMapId(mapId, 0, true);

        // Since this is effecting state, we need to keep an eye on it for potential issues.
        AgentMap.Instance()->ResetMapMarkers();
    }

    public void OpenOccupiedMap() => OpenMap(AgentMap.Instance()->CurrentMapId);

    private static void CenterOnMarker(MapMarkerBase marker)
    {
        var coordinates = new Vector2(marker.X, marker.Y) / 16.0f * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector();

        System.SystemConfig.FollowPlayer = false;
        System.MapRenderer.DrawOffset = -coordinates;
    }

    private static void CenterOnMarker(MapMarkerData marker)
    {
        var coordinates = marker.Position.AsMapVector() * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector();

        System.SystemConfig.FollowPlayer = false;
        System.MapRenderer.DrawOffset = -coordinates;
    }

    public static bool ShouldShowMap()
    {
        if (Service.ClientState is { IsLoggedIn: false } or { IsPvP: true }) return false;
        if (System.SystemConfig.HideInCombat && Service.Condition.IsInCombat()) return false;
        if (System.SystemConfig.HideBetweenAreas && Service.Condition.IsBetweenAreas()) return false;
        if (System.SystemConfig.HideWithGameGui && !IsNamePlateAddonVisible()) return false;
        if (System.SystemConfig.HideWithGameGui && Control.Instance()->TargetSystem.TargetModeIndex is 1) return false;

        return true;
    }

    private static bool IsNamePlateAddonVisible() => !RaptureAtkUnitManager.Instance()->UiFlags.HasFlag(UIModule.UiFlags.Nameplates);

    private uint? GetMapIdForQuest(OpenMapInfo* mapInfo)
    {
        foreach (var leveQuest in QuestManager.Instance()->LeveQuests)
        {
            if (leveQuest.LeveId is 0) continue;

            var leveData = Service.DataManager.GetExcelSheet<Leve>().GetRow(leveQuest.LeveId);
            if (!IsNameMatch(leveData.Name.ExtractText(), mapInfo)) continue;

            return leveData.LevelStart.Value.Map.RowId;
        }

        foreach (var quest in QuestManager.Instance()->NormalQuests)
        {
            if (quest.QuestId is 0) continue;

            // Is this the quest we are looking for?
            var questData = Service.DataManager.GetExcelSheet<Quest>().GetRow(quest.QuestId + 65536u);
            if (!IsNameMatch(questData.Name.ExtractText(), mapInfo)) continue;

            return questData
                .TodoParams.FirstOrDefault(param => param.ToDoCompleteSeq == quest.Sequence)
                .ToDoLocation.FirstOrDefault(location => location is not { RowId: 0, ValueNullable: null })
                .Value.Map.RowId;
        }

        var possibleQuests = Service.DataManager.GetExcelSheet<Quest>()
            .Where(quest => quest is { IssuerLocation: { IsValid: true, RowId: not 0 } }).FirstOrNull(quest => IsNameMatch(quest.Name.ExtractText(), mapInfo));

        return possibleQuests?.IssuerLocation.Value.Map.RowId ?? null;
    }

    private static bool IsNameMatch(string name, OpenMapInfo* mapInfo) => string.Equals(name, mapInfo->TitleString.ToString(), StringComparison.OrdinalIgnoreCase);

    // public Vector3 GetLgbEventObjPos(uint lgbEventObjId) {
    // 	var layout = LayoutWorld.Instance()->ActiveLayout;
    //
    // 	if (layout == null) return default;
    //
    // 	if (!layout->InstancesByType.TryGetValue(InstanceType.EventObject, out var map, false))
    // 		return default;
    //
    // 	if (!map.Value->TryGetValue((ulong)lgbEventObjId << 32, out var pInstance, false))
    //
    // 		return default;
    // 	return pInstance.Value->GetTransformImpl()->Translation;
    // }
}