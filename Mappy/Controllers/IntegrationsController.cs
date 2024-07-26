using System;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Classes;
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
			Service.Log.Debug("[OnShowHook] Hook Start");
			
			if (AgentMap.Instance()->AddonId is not 0 && AgentMap.Instance()->CurrentMapId != AgentMap.Instance()->SelectedMapId) {
				AgentMap.Instance()->Hide();
				Service.Log.Debug("[OnShowHook] Vanilla tried to return to current map, aborted.");
				return;
			}

			showMapHook!.Original(agent, a1, a2);
			Service.Log.Verbose($"[OnShowHook] Called Original with A1 = {a1}, A2 = {a2}");
		}, Service.Log, "Exception during OnShowHook");

	private void OpenMapById(AgentMap* agent, uint mapId, uint a3, bool a4) 
		=> HookSafety.ExecuteSafe(() => {
			openMapByIdHook!.Original(agent, mapId, a3, a4);
			Service.Log.Debug($"[OpenMapByIdHook] Called Original with MapId = {mapId}, A3 = {a3}, A4 = {a4}");
		}, Service.Log, "Exception during OpenMapByMapId");
    
	private void OpenMap(AgentMap* agent, OpenMapInfo* mapInfo) 
		=> HookSafety.ExecuteSafe(() => {
			openMapHook!.Original(agent, mapInfo);
			Service.Log.Debug($"[OpenMapHook] Called Original with MapInfo [ " +
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
					Service.Log.Debug($"[OpenMapHook] Processing QuestLog Event");

					if (GetMapIdForQuest(mapInfo) is {} targetMapId ) {
						
						Service.Log.Debug($"[OpenMapHook] Identified Quest Target Map as MapId: {targetMapId}");

						if (agent->SelectedMapId != targetMapId) {
							OpenMap(targetMapId);
							Service.Log.Debug($"[OpenMapHook] Opening MapId: {targetMapId}");
						}
						else {
							Service.Log.Debug($"[OpenMapHook] Already in MapId: {targetMapId}, aborting.");
						}
					}

					if (System.SystemConfig.CenterOnQuest) {
						ref var targetMarker = ref agent->TempMapMarkers[0].MapMarker;
						
						CenterOnMarker(targetMarker);
						Service.Log.Debug($"[OpenMapHook] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
					}
					break;
				}

				case MapType.GatheringLog: {
					Service.Log.Debug($"[OpenMapHook] Processing GatheringLog Event");
					
					if (System.SystemConfig.CenterOnGathering) {
						ref var targetMarker = ref agent->TempMapMarkers[0].MapMarker;
						
						CenterOnMarker(targetMarker);
						Service.Log.Debug($"[OpenMapHook] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
					}
					break;
				}

				case MapType.FlagMarker: {
					Service.Log.Debug($"[OpenMapHook] Processing FlagMarker Event");
					
					if (System.SystemConfig.CenterOnFlag) {
						ref var targetMarker = ref agent->TempMapMarkers[0].MapMarker;
						
						CenterOnMarker(targetMarker);
						Service.Log.Debug($"[OpenMapHook] Centering Map on X = {targetMarker.X}, Y = {targetMarker.Y}");
					}
					break;
				}
			}
		}, Service.Log, "Exception during OpenMap");

	public void OpenMap(uint mapId)
		=> AgentMap.Instance()->OpenMapByMapId(mapId, 0, true);

	public void OpenOccupiedMap()
		=> OpenMap(AgentMap.Instance()->CurrentMapId);

	private static void CenterOnMarker(AgentMap* agent, MapMarkerBase marker) {
		var coordinates = new Vector2(marker.X, marker.Y) / 16.0f * agent->SelectedMapSizeFactorFloat;

		System.MapWindow.ProcessingCommand = true;
		System.MapRenderer.DrawOffset = -coordinates;
	}

	private uint? GetMapIdForQuest(OpenMapInfo* mapInfo) {
		foreach (var leveQuest in QuestManager.Instance()->LeveQuests) {
			if (leveQuest.IsHidden || leveQuest.LeveId is 0) continue;

			var leveData = Service.DataManager.GetExcelSheet<Leve>()?.GetRow(leveQuest.LeveId)!;
			if (!IsNameMatch(leveData.Name, mapInfo)) continue;

			return leveData.LevelStart.Value?.Map.Row;
		}
		
		foreach (var quest in QuestManager.Instance()->NormalQuests) {
			if (quest.IsHidden || quest.QuestId is 0) continue;
			
			// Is this the quest we are looking for?
			var questData = Service.DataManager.GetExcelSheet<CustomQuestSheet>()?.GetRow(quest.QuestId + 65536u)!;
			if (!IsNameMatch(questData.Name, mapInfo)) continue;

			var todoPrimaryIndex = 0;
			if (quest.Sequence is 0xFF) {
				// For each of the possible steps check if out sequence matches that index
				foreach(var index in Enumerable.Range(0, 24)) {
					if (questData.ToDoCompleteSeq[index] == quest.Sequence) {
						todoPrimaryIndex = index;
						break;
					}
				}
			}
			else {
				todoPrimaryIndex = quest.Sequence;
			}
			
			// Iterate the level data for markers for the current sequence number
			foreach (var index in Enumerable.Range(0, 8)) {
				var levelData = questData.ToDoLocation[todoPrimaryIndex, index];
				if (levelData.Row is 0) continue;
				if (levelData.Value is null) continue;

				return levelData.Value.Map.Row;
			}
		}

		return Service.DataManager.GetExcelSheet<Quest>()?.FirstOrDefault(quest =>
			IsNameMatch(quest.Name, mapInfo) &&
			quest is { IssuerLocation.Row: not 0 })
			?.IssuerLocation.Value?.Map.Row;
	}

	private bool IsNameMatch(SeString name, OpenMapInfo* mapInfo) 
		=> string.Equals(name.ToString(), mapInfo->TitleString.ToString(), StringComparison.OrdinalIgnoreCase);
}