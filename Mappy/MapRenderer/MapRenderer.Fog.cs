using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib.Extensions;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Mappy.MapRenderer;

public unsafe partial class MapRenderer {
	private void DrawFogOfWar() {
		if (!System.SystemConfig.ShowFogOfWar) return;
		if (blendedTexture is null) return;
        
		var areaMapNumberArray = AtkStage.Instance()->GetNumberArrayData(NumberArrayType.AreaMap2);

		if (areaMapNumberArray->IntArray[2] != lastKnownDiscoveryFlags) {
			lastKnownDiscoveryFlags = areaMapNumberArray->IntArray[2];

			if (lastKnownDiscoveryFlags != -1 && lastKnownDiscoveryFlags != AgentMap.Instance()->SelectedMapDiscoveryFlag) {
				Service.Log.Debug("[Fog of War] Discovery Bits Changed, updating fog texture.");
				Task.Run(() => {
					fogTexture = LoadFogTexture();
				});
			}
		}
        
		if (fogTexture is not null && lastKnownDiscoveryFlags != -1) {
			ImGui.SetCursorPos(DrawPosition);
			ImGui.Image(fogTexture.ImGuiHandle, fogTexture.Size * Scale);
            
		} else if (fogTexture is null && lastKnownDiscoveryFlags != -1) {
			var defaultBackgroundTexture = Service.TextureProvider.GetFromGame($"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex").GetWrapOrEmpty();
            
			ImGui.SetCursorPos(DrawPosition);
			ImGui.Image(defaultBackgroundTexture.ImGuiHandle, defaultBackgroundTexture.Size * Scale);
		}
	}
	
	private IDalamudTextureWrap? LoadFogTexture() {
        var vanillaBgPath = $"{AgentMap.Instance()->SelectedMapBgPath.ToString()}.tex";
        var bgFile = GetTexFile(vanillaBgPath);

        if (bgFile is null) {
            Service.Log.Warning("Failed to load map textures");
            return null;
        }
        
        // Load non-transparent background texture
        var backgroundBytes = bgFile.GetRgbaImageData();
        
        // Load alpha maps
        var fogTextureBytes = GetPrebakedTextureBytes();
        if (fogTextureBytes is null) return null;
        
        var timer = Stopwatch.StartNew();
        
        // Make background texture fully invisible
        Parallel.For(0, 2048 * 2048, i => {
            backgroundBytes[i * 4 + 3] = 0;
        });
        
        // Make non-transparent any section that the player has not-already explored
        Parallel.For(0, 128, x => {
            Parallel.For(0, 128, y => {
                var pixelIndex = (x + y * 128) * 4;
                var targetPixel = (x + 2048 * y) * 4;
                
                var redAmount = fogTextureBytes[pixelIndex + 0] / 255.0f;
                var greenAmount = fogTextureBytes[pixelIndex + 1] / 255.0f;
                var blueAmount = fogTextureBytes[pixelIndex + 2] / 255.0f;
                
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
            });
        });

        Service.Log.Debug($"Fog of War Calculated in {timer.ElapsedMilliseconds} ms");

        return Service.TextureProvider.CreateFromRaw(RawImageSpecification.Rgba32(2048, 2048), backgroundBytes);
    }
	
	private static byte[]? GetPrebakedTextureBytes() {
		var addon = Service.GameGui.GetAddonByName<AddonAreaMap>("AreaMap");
		if (addon is null) return null;

		var componentMap = (void*) Marshal.ReadIntPtr((nint) addon, 0x430);
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
}