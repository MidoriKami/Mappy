using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Classes;
using Mappy.Data;

namespace Mappy.Controllers;

public unsafe class FlagController : IDisposable {
	private readonly Hook<AgentMap.Delegates.SetFlagMapMarker>? setFlagMapMarkerHook;
	
	public FlagController() {
		setFlagMapMarkerHook ??= Service.Hooker.HookFromAddress<AgentMap.Delegates.SetFlagMapMarker>(AgentMap.MemberFunctionPointers.SetFlagMapMarker, OnSetFlagMapMarker);
	}

	public void Dispose() {
		setFlagMapMarkerHook?.Dispose();
	}

	public void EnableIntegrations() {
		setFlagMapMarkerHook?.Enable();
	}

	public void DisableIntegrations() {
		setFlagMapMarkerHook?.Disable();
	}

	private void OnSetFlagMapMarker(AgentMap* thisPtr, uint territoryId, uint mapId, float x, float y, uint iconId)
		=> HookSafety.ExecuteSafe(() => {

			var newFlagData = new Flag(territoryId, mapId, x, y, iconId);
			var dataFile = System.FlagConfig;

			if (!dataFile.FlagHistory.Contains(newFlagData)) {
				dataFile.FlagHistory.AddFirst(new Flag(territoryId, mapId, x, y, iconId));

				if (dataFile.FlagHistory.Count > dataFile.HistoryLimit) {
					dataFile.FlagHistory.RemoveLast();
				}
			
				dataFile.Save();
			}
			
			setFlagMapMarkerHook!.Original.Invoke(thisPtr, territoryId, mapId, x, y, iconId);
		}, Service.Log, "Exception during OnSetFlagMapMarker");
}