using System;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Classes;
using KamiLib.Extensions;
using Lumina.Excel.GeneratedSheets;
using Mappy.Classes;
using MapType = FFXIVClientStructs.FFXIV.Client.UI.Agent.MapType;
using SeString = Lumina.Text.SeString;

namespace Mappy.Controllers;

public unsafe class IntegrationsController : IDisposable {
	private delegate void OpenMapByMapIdDelegate(AgentMap* thisPtr, uint mapId, uint a3, bool a4);
    
	private readonly Hook<AgentMap.Delegates.ShowMap>? showMapHook;
	private readonly Hook<OpenMapByMapIdDelegate>? openMapByIdHook;
	private readonly Hook<AgentMap.Delegates.OpenMap>? openMapHook;
    
	public IntegrationsController() {
		showMapHook ??= Service.Hooker.HookFromAddress<AgentMap.Delegates.ShowMap>(AgentMap.MemberFunctionPointers.ShowMap, OnShowHook);
		openMapByIdHook ??= Service.Hooker.HookFromAddress<OpenMapByMapIdDelegate>(AgentMap.MemberFunctionPointers.OpenMapByMapId, OpenMapById);
		openMapHook ??= Service.Hooker.HookFromAddress<AgentMap.Delegates.OpenMap>(AgentMap.MemberFunctionPointers.OpenMap, OpenMap);
        
		if (Service.ClientState is { IsPvP: false }) {
			EnableIntegrations();
		}
        
		Service.ClientState.EnterPvP += DisableIntegrations;
		Service.ClientState.LeavePvP += EnableIntegrations;
	}

	public void Dispose() {
		DisableIntegrations();
        
		showMapHook?.Dispose();
		openMapByIdHook?.Dispose();
		openMapHook?.Dispose();
        
		Service.ClientState.EnterPvP -= DisableIntegrations;
		Service.ClientState.LeavePvP -= EnableIntegrations;
	}

	private void EnableIntegrations() {
		Service.Log.Debug("Enabling Integrations");
        
		showMapHook?.Enable();
		openMapByIdHook?.Enable();
		openMapHook?.Enable();
	}

	private void DisableIntegrations() {
		Service.Log.Debug("Disabling Integrations");
        
		showMapHook?.Disable();
		openMapByIdHook?.Disable();
		openMapHook?.Disable();
	}

	private void OnShowHook(AgentMap* agent, bool a1, bool a2)
		=> HookSafety.ExecuteSafe(() => {
			Service.Log.Debug("[OnShow] Beginning Show");

			if (!ShouldShowMap()) {
				Service.Log.Debug("[OnShow] Condition to open map is rejected, aborting.");
				return;
			}
			
			if (AgentMap.Instance()->AddonId is not 0 && AgentMap.Instance()->CurrentMapId != AgentMap.Instance()->SelectedMapId) {
				AgentMap.Instance()->Hide();
				Service.Log.Debug("[OnShow] Vanilla tried to return to current map, aborted.");
				return;
			}

			showMapHook!.Original(agent, a1, a2);
			Service.Log.Debug($"[OnShow] Called Original with A1 = {a1}, A2 = {a2}");
		}, Service.Log, "Exception during OnShowHook");

	private void OpenMapById(AgentMap* agent, uint mapId, uint a3, bool a4) 
		=> HookSafety.ExecuteSafe(() => {
			openMapByIdHook!.Original(agent, mapId, a3, a4);
			Service.Log.Debug($"[OpenMapById] Called Original with MapId = {mapId}, A3 = {a3}, A4 = {a4}");
		}, Service.Log, "Exception during OpenMapByMapId");
    
	private void OpenMap(AgentMap* agent, OpenMapInfo* mapInfo) 
		=> HookSafety.ExecuteSafe(() => {
			openMapHook!.Original(agent, mapInfo);
			Service.Log.Debug($"[OpenMap] Called Original with MapInfo [ " +
			                    $"MapId: {mapInfo->MapId}, " +
			                    $"MapType: {mapInfo->Type}, " +
			                    $"Title: {mapInfo->TitleString.ToString()}, " +
			                    $"PlaceNameId: {mapInfo->PlaceNameId}, " +
			                    $"AetheryteId: {mapInfo->AetheryteId}, " +
			                    $"FateId: {mapInfo->FateId}, " +
			                    $"Unknown 1C: {mapInfo->Unk1C}, " +
			                    $"Unknown 88: {mapInfo->Unk88}, " +
			                    $"Unknown 8C: {mapInfo->Unk8C}, " +
			                    $"Unknown 8D: {mapInfo->Unk8D} ]");

			switch (mapInfo->Type) {
				case MapType.QuestLog: {
					Service.Log.Debug("[OpenMap] Processing QuestLog Event");

					var targetMapId = mapInfo->MapId;

					if (GetMapIdForQuest(mapInfo) is { } foundMapId) {
						Service.Log.Debug($"[OpenMap] GetMapIdForQuest identified Quest Target Map as MapId: {foundMapId}");

						if (targetMapId is 0) {
							Service.Log.Debug($"[OpenMap] targetMapId was {targetMapId} using foundMapId: {foundMapId}");
							targetMapId = foundMapId;
						}
					}

					if (agent->SelectedMapId != targetMapId) {
						Service.Log.Debug($"[OpenMap] Opening MapId: {targetMapId}");
						OpenMap(targetMapId);
					}
					else {
						Service.Log.Debug($"[OpenMap] Already in MapId: {targetMapId}, aborting.");
					}

					if (System.SystemConfig.CenterOnQuest) {
						ref var targetMarker = ref agent->TempMapMarkers[0].MapMarker;
						CenterOnMarker(targetMarker);
						Service.Log.Debug($"[OpenMap] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
					}

					break;
				}

				case MapType.GatheringLog: {
					Service.Log.Debug("[OpenMap] Processing GatheringLog Event");
					
					if (System.SystemConfig.CenterOnGathering) {
						ref var targetMarker = ref agent->TempMapMarkers[0].MapMarker;
						
						CenterOnMarker(targetMarker);
						Service.Log.Debug($"[OpenMap] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
					}
					break;
				}

				case MapType.FlagMarker: {
					Service.Log.Debug("[OpenMap] Processing FlagMarker Event");
					
					if (System.SystemConfig.CenterOnFlag) {
						ref var targetMarker = ref agent->FlagMapMarker.MapMarker;
						
						CenterOnMarker(targetMarker);
						Service.Log.Debug($"[OpenMap] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
					}
					break;
				}
			}
			
			if (System.SystemConfig.AutoZoom) {
				System.MapRenderer.Scale = DrawHelpers.GetMapScaleFactor() * 0.33f;
			}
		}, Service.Log, "Exception during OpenMap");

	public void OpenMap(uint mapId)
		=> AgentMap.Instance()->OpenMapByMapId(mapId, 0, true);

	public void OpenOccupiedMap()
		=> OpenMap(AgentMap.Instance()->CurrentMapId);

	private static void CenterOnMarker(MapMarkerBase marker) {
		var coordinates = new Vector2(marker.X, marker.Y) / 16.0f * DrawHelpers.GetMapScaleFactor() - DrawHelpers.GetMapOffsetVector();

		System.SystemConfig.FollowPlayer = false;
		System.MapRenderer.DrawOffset = -coordinates;

		// If the map isn't open, we want to force it to not center on whatever the user decided and center on our markers instead.
		if (!System.MapWindow.IsOpen) {
			System.MapWindow.ProcessingCommand = true;
		}
	}

	public static bool ShouldShowMap() {
		if (Service.ClientState is { IsLoggedIn: false } or { IsPvP: true }) return false;
		if (System.SystemConfig.HideInDuties && Service.Condition.IsBoundByDuty()) return false;
		if (System.SystemConfig.HideInCombat && Service.Condition.IsInCombat()) return false;
		if (System.SystemConfig.HideBetweenAreas && Service.Condition.IsBetweenAreas()) return false;
		if (System.SystemConfig.HideWithGameGui && !IsNamePlateAddonVisible()) return false;
		if (System.SystemConfig.HideWithGameGui && Control.Instance()->TargetSystem.TargetModeIndex is 1) return false;

		return true;
	}
	
	private static bool IsNamePlateAddonVisible()
		=> !RaptureAtkUnitManager.Instance()->UiFlags.HasFlag(UIModule.UiFlags.Nameplates);

	private uint? GetMapIdForQuest(OpenMapInfo* mapInfo) {
		foreach (var leveQuest in QuestManager.Instance()->LeveQuests) {
			if (leveQuest.LeveId is 0) continue;

			var leveData = Service.DataManager.GetExcelSheet<Leve>()?.GetRow(leveQuest.LeveId)!;
			if (!IsNameMatch(leveData.Name, mapInfo)) continue;

			return leveData.LevelStart.Value?.Map.Row;
		}
		
		foreach (var quest in QuestManager.Instance()->NormalQuests) {
			if (quest.QuestId is 0) continue;
			
			// Is this the quest we are looking for?
			var questData = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets2.Quest>()?.GetRow(quest.QuestId + 65536u)!;
			if (!IsNameMatch(questData.Name, mapInfo)) continue;

			return questData
				.TodoParams.FirstOrDefault(param => param.ToDoCompleteSeq == quest.Sequence)
				.ToDoLocation.FirstOrDefault(location => location is not { Row: 0, Value: null })
				?.Value?.Map.Row;
		}

		return Service.DataManager.GetExcelSheet<Quest>()?.FirstOrDefault(quest =>
			IsNameMatch(quest.Name, mapInfo) &&
			quest is { IssuerLocation.Row: not 0 })
			?.IssuerLocation.Value?.Map.Row;
	}

	private static bool IsNameMatch(SeString name, OpenMapInfo* mapInfo) 
		=> string.Equals(name.ToString(), mapInfo->TitleString.ToString(), StringComparison.OrdinalIgnoreCase);
}