using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Hooking;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib.Classes;
using KamiLib.Extensions;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Mappy.MapRenderer;

public unsafe partial class MapRenderer {
	private delegate void ImmediateContextProcessCommands(ImmediateContext* commands, RenderCommandBufferGroup* bufferGroup, uint a3);
	
	[Signature("E8 ?? ?? ?? ?? 48 8B 4B 30 FF 15 ?? ?? ?? ??", DetourName = nameof(OnImmediateContextProcessCommands))]
	private readonly Hook<ImmediateContextProcessCommands>? immediateContextProcessCommandsHook = null;

	private bool requestUpdatedMaskingTexture;
	private byte[]? maskingTextureBytes;

	private byte[]? blockyFogBytes;
	private IDalamudTextureWrap? fogTexture;
	private int lastKnownDiscoveryFlags;
	private readonly Stopwatch textureLoadStopwatch = new();

	private static int CurrentDiscoveryFlags => AtkStage.Instance()->GetNumberArrayData(NumberArrayType.AreaMap2)->IntArray[2];
	
	private void LoadFogHooks() {
		Service.Hooker.InitializeFromAttributes(this);
		immediateContextProcessCommandsHook?.Enable();
	}

	private void UnloadFogHooks() {
		immediateContextProcessCommandsHook?.Dispose();
	}
	
	private void OnImmediateContextProcessCommands(ImmediateContext* commands, RenderCommandBufferGroup* bufferGroup, uint a3) 
	=> HookSafety.ExecuteSafe(() => {

		// Delay by a certain number of frames because the game hasn't loaded the new texture yet.
		if (requestUpdatedMaskingTexture && textureLoadStopwatch is { IsRunning: true, ElapsedMilliseconds: > 100 }) {
			maskingTextureBytes = null;
			maskingTextureBytes = GetPrebakedTextureBytes();
			requestUpdatedMaskingTexture = false;
			textureLoadStopwatch.Stop();

			Task.Run(LoadFogTexture);
		}
		
		immediateContextProcessCommandsHook!.Original(commands, bufferGroup, a3);
	}, Service.Log, "Exception during OnImmediateContextProcessCommands");
	
	private void DrawFogOfWar() {
		if (!System.SystemConfig.ShowFogOfWar) return;
		if (CurrentDiscoveryFlags == AgentMap.Instance()->SelectedMapDiscoveryFlag) return;
		if (CurrentDiscoveryFlags == -1) return;
		
		var flagsChanged = lastKnownDiscoveryFlags != CurrentDiscoveryFlags;
		lastKnownDiscoveryFlags = CurrentDiscoveryFlags;

		if (flagsChanged) {
			Service.Log.Debug("[Fog of War] Discovery Bits Changed, updating fog texture.");
			requestUpdatedMaskingTexture = true;
			textureLoadStopwatch.Restart();
			fogTexture = null;
		}
		
		if (fogTexture is not null) {
			ImGui.SetCursorPos(DrawPosition);
			ImGui.Image(fogTexture.ImGuiHandle, fogTexture.Size * Scale);
            
		} else {
			var defaultBackgroundTexture = Service.TextureProvider.GetFromGame($"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex").GetWrapOrEmpty();
            
			ImGui.SetCursorPos(DrawPosition);
			ImGui.Image(defaultBackgroundTexture.ImGuiHandle, defaultBackgroundTexture.Size * Scale);
		}
	}
	
	private void LoadFogTexture() {
		var vanillaBgPath = $"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex";
		var bgFile = GetTexFile(vanillaBgPath);
		
		if (bgFile is null) {
		    Service.Log.Warning("Failed to load map textures");
		    return;
		}
		
		// Load non-transparent background texture
		var backgroundBytes = bgFile.GetRgbaImageData();
		
		// Load alpha mapping
		if (maskingTextureBytes is null) return;
		
		var timer = Stopwatch.StartNew();
		
		// Make background texture fully invisible
		for (var index = 0; index < 2048 * 2048; index++) {
			backgroundBytes[index * 4 + 3] = 0;
		}
		
		// Make non-transparent any section that the player has not-already explored
		for (var x = 0; x < 128; x++) 
		for (var y = 0; y < 128; y++) {
			var pixelIndex = (x + y * 128) * 4;
			var targetPixel = (x + 2048 * y) * 4;
		        
			var redAmount = maskingTextureBytes[pixelIndex + 0] / 255.0f;
			var greenAmount = maskingTextureBytes[pixelIndex + 1] / 255.0f;
			var blueAmount = maskingTextureBytes[pixelIndex + 2] / 255.0f;
		        
			var maxAlpha = Math.Max(redAmount, Math.Max(greenAmount, blueAmount));
			var alphaSum = (byte) ( maxAlpha * 255 );
		        
			if (alphaSum is not 0) {
				const int scaleFactor = 16;
				foreach (var xScalar in Enumerable.Range(0, scaleFactor))
				foreach (var yScalar in Enumerable.Range(0, scaleFactor)) {
					var scalingPixelTarget = targetPixel * scaleFactor + xScalar * 4 + yScalar * 2048 * 4;
					backgroundBytes[scalingPixelTarget + 3] = alphaSum;
				}
			}
		}

		Service.Log.Debug($"Fog of War Calculated in {timer.ElapsedMilliseconds} ms");

		blockyFogBytes = backgroundBytes;
		fogTexture = Service.TextureProvider.CreateFromRaw(RawImageSpecification.Rgba32(2048, 2048), backgroundBytes);

		Task.Run(CleanupFogTexture);
	}
	
	private static byte[]? GetPrebakedTextureBytes() {
		var addon = Service.GameGui.GetAddonByName<AddonAreaMap>("AreaMap");
		if (addon is null) return null;

		var componentMap =   (void*) Marshal.ReadIntPtr((nint) addon, 0x430);
		if (componentMap is null) return null;
        
		var texturePointer = (Texture*) Marshal.ReadIntPtr((nint) componentMap, 0x270);
		if (texturePointer is null) return null;

		var device = Service.PluginInterface.UiBuilder.Device;
			
		var texture = CppObject.FromPointer<Texture2D>((nint)texturePointer->D3D11Texture2D);
		var desc = new Texture2DDescription {
			ArraySize = 1,
			BindFlags = BindFlags.None,
			CpuAccessFlags = CpuAccessFlags.Read,
			Format = texture.Description.Format,
			Height = texture.Description.Height,
			Width = texture.Description.Width,
			MipLevels = 1,
			OptionFlags = texture.Description.OptionFlags,
			SampleDescription = new SampleDescription(1, 0),
			Usage = ResourceUsage.Staging
		};

		using var stagingTexture = new Texture2D(device, desc);
		var context = device.ImmediateContext;

		context.CopyResource(texture, stagingTexture);
		device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out var dataStream);

		using var pixelDataStream = new MemoryStream();
		dataStream.CopyTo(pixelDataStream);

		return pixelDataStream.ToArray();
	}
	
	private void CleanupFogTexture() {
		if (blockyFogBytes is null) return;
        
		var timer = Stopwatch.StartNew();
        
		// Because we had to scale a 128x128 texture mapping onto a 2048x2048, it'll look very blurry, lets blend the alpha channel
		const int blurRadius = 8;
        
		for(var x = 0; x < 2048; x++)
		for(var y = 0; y < 2048; y++) {
			var pixelIndex = (x + y * 2048) * 4;

			var alphaAverage = 0.0f;
			var numAveraged = 0;

			if (blockyFogBytes[pixelIndex + 3] == 255) continue;

			for (var xBlur = -blurRadius; xBlur < -blurRadius + blurRadius * 2; ++xBlur) {
				var currentX = x + xBlur;
				if (currentX is < 0 or >= 2048) continue;
				var currentPixelIndex = (currentX + y * 2048) * 4;

				alphaAverage += blockyFogBytes[currentPixelIndex + 3];
				numAveraged++;
			}
			
			for(var yBlur = -blurRadius; yBlur < -blurRadius + blurRadius * 2; ++yBlur) {
				var currentY = y + yBlur;

				if (currentY is < 0 or >= 2048) continue;
				var currentPixelIndex = (x + currentY * 2048) * 4;

				alphaAverage += blockyFogBytes[currentPixelIndex + 3];
				numAveraged++;
			}

			var newAlpha = (byte) (alphaAverage / numAveraged);
			blockyFogBytes[pixelIndex + 3] = newAlpha;
		}

		fogTexture = Service.TextureProvider.CreateFromRaw(RawImageSpecification.Rgba32(2048, 2048), blockyFogBytes);
        
		Service.Log.Debug($"Texture Cleanup completed in {timer.ElapsedMilliseconds} ms");
	}
}