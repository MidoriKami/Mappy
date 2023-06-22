using System;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using ImGuiScene;
using KamiLib.Caching;
using Lumina.Data.Files;

namespace Mappy.System;

public class PenumbraController
{
    private readonly ICallGateSubscriber<string, string> penumbraResolveDefaultSubscriber;
    private readonly ICallGateSubscriber<bool> penumbraGetEnabledState;

    public PenumbraController()
    {
        penumbraResolveDefaultSubscriber = Service.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveInterfacePath");
        penumbraGetEnabledState = Service.PluginInterface.GetIpcSubscriber<bool>("Penumbra.GetEnabledState");
        
        IconCache.Instance.SetAlternativeGetTextureFunc(GetTexture);
    }
    
    public TextureWrap? GetTexture(string path)
    {
        try
        {
            if (penumbraGetEnabledState.InvokeFunc())
            {
                var resolvedPath = ResolvePenumbraPath(path);
                PluginLog.Verbose($"Loading Texture from Penumbra: {path} -> {resolvedPath}");
                return GetTextureForPath(resolvedPath);
            }
        }
        catch (Exception)
        {
            // ignored
        }
        
        return Service.DataManager.GetImGuiTexture(path);
    }
    
    private string ResolvePenumbraPath(string filePath)
    {
        try
        {
            return penumbraResolveDefaultSubscriber.InvokeFunc(filePath);
        }
        catch
        {
            return filePath;
        }
    }

    private TextureWrap? GetTextureForPath(string path)
    {
        if (path[0] is '/' or '\\' || path[1] == ':')
        {
            var texFile = Service.DataManager.GameData.GetFileFromDisk<TexFile>(path);
            return Service.DataManager.GetImGuiTexture(texFile);
        }
        else
        {
            return Service.DataManager.GetImGuiTexture(path);
        }
    }
}