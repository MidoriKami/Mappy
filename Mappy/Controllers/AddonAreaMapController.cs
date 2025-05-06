using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiLib.Classes;
using KamiLib.CommandManager;
using KamiLib.Extensions;
using Mappy.Windows;

namespace Mappy.Controllers;

public unsafe class AddonAreaMapController :IDisposable {
	private Hook<AddonAreaMap.Delegates.Show>? showAreaMapHook;
	private Hook<AddonAreaMap.Delegates.Hide>? hideAreaMapHook;
	
	public AddonAreaMapController() {
		Service.Log.Debug("Beginning Listening for AddonAreaMap");
		Service.Framework.Update += AddonAreaMapListener;
		
		Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "AreaMap", OnAreaMapDraw);
		
		// Add a special error handler for the case that somehow the addon is stuck offscreen
		System.CommandManager.RegisterCommand(new CommandHandler {
			ActivationPath = "/areamap/reset",
			Delegate = _ => {
				var addon = Service.GameGui.GetAddonByName<AddonAreaMap>("AreaMap");
				if (addon is not null && addon->RootNode is not null) {
					addon->RootNode->SetPositionFloat(addon->X, addon->Y);
				}
			},
		});
	}

	private void AddonAreaMapListener(IFramework framework) {
		var addonAreaMap = Service.GameGui.GetAddonByName<AddonAreaMap>("AreaMap");
		if (addonAreaMap is not null) {
			Service.Log.Debug("AddonAreaMap Found, Hooking");
			
			HookAreaMapFunctions(addonAreaMap);
			
			Service.Log.Debug("Finished Listening for AddonAreaMap");
			Service.Framework.Update -= AddonAreaMapListener;
		}
	}

	public void Dispose() {
		Service.AddonLifecycle.UnregisterListener(OnAreaMapDraw);
		Service.Framework.Update -= AddonAreaMapListener;
		
		showAreaMapHook?.Dispose();
		hideAreaMapHook?.Dispose();
		
		// Reset windows root node position on dispose
		var addonAreaMap = Service.GameGui.GetAddonByName<AddonAreaMap>("AreaMap");
		if (addonAreaMap is not null) {
			addonAreaMap->RootNode->SetPositionFloat(addonAreaMap->X, addonAreaMap->Y);
		}
	}

	public void EnableIntegrations() {
		showAreaMapHook?.Enable();
		hideAreaMapHook?.Enable();
	}

	public void DisableIntegrations() {
		showAreaMapHook?.Disable();
		hideAreaMapHook?.Disable();
	}

	private void HookAreaMapFunctions(AddonAreaMap* areaMap) {
		showAreaMapHook = Service.Hooker.HookFromAddress<AddonAreaMap.Delegates.Show>(areaMap->VirtualTable->Show, OnAreaMapShow);
		hideAreaMapHook = Service.Hooker.HookFromAddress<AddonAreaMap.Delegates.Hide>(areaMap->VirtualTable->Hide, OnAreaMapHide);

		if (Service.ClientState is { IsPvP: false }) {
			EnableIntegrations();
		}
	}

	private void OnAreaMapShow(AddonAreaMap* thisPtr, bool silenceOpenSoundEffect, uint unsetShowHideFlags)
		=> HookSafety.ExecuteSafe(() => {
			Service.Log.Debug("[AreaMap] OnAreaMapShow");
		
			System.WindowManager.GetWindow<MapWindow>()?.Open();
			showAreaMapHook!.Original(thisPtr, silenceOpenSoundEffect, unsetShowHideFlags);
		}, Service.Log, "Exception during OnAreaMapShow");

	private void OnAreaMapHide(AddonAreaMap* thisPtr, bool unkBool, bool callHideCallback, uint setShowHideFlags) 		
		=> HookSafety.ExecuteSafe(() => {
			Service.Log.Debug("[AreaMap] OnAreaMapHide");
			
			System.WindowManager.GetWindow<MapWindow>()?.Close();
			hideAreaMapHook!.Original(thisPtr, unkBool, callHideCallback, setShowHideFlags);

			if (Service.GameGui.FindAgentInterface((nint) thisPtr) == nint.Zero) {
				System.WindowManager.GetWindow<MapWindow>()?.Close();
			}
			
		}, Service.Log, "Exception during OnAreaMapHide");

	private void OnAreaMapDraw(AddonEvent type, AddonArgs args) {
		if (Service.ClientState is { IsPvP: true }) return;

		var addon = (AddonAreaMap*) args.Addon;
		if (addon->RootNode is null) return;

		// Have to check for color, because it likes to animate a fadeout,
		// and we want the map to stay completely hidden until it's done.
		if (addon->IsVisible || addon->RootNode->Color.A is not 0x00) {
			addon->RootNode->SetPositionFloat(-9001.0f, -9001.0f);
		}
		else {
			addon->RootNode->SetPositionFloat(addon->X, addon->Y);
		}
	}
}