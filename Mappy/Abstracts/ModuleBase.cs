using DailyDuty.System;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.AutomaticUserInterface;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.System;

namespace Mappy.Abstracts;

public abstract unsafe class ModuleBase
{
    public abstract ModuleName ModuleName { get; }
    public abstract ModuleConfigBase Configuration { get; protected set; }
    protected T GetConfig<T>() where T : ModuleConfigBase => (T) Configuration;

    // Map Marker
    public virtual void ZoneChanged(uint territoryType) { }
    public abstract void LoadForMap(MapData mapData);
    protected abstract void DrawMarkers(Viewport viewport, Map map);
    protected virtual bool ShouldDrawMarkers(Map map)
    {
        if (!Configuration.Enable) return false;

        return true;
    }
    public void Draw(Viewport viewport, Map map)
    {
        if (!ShouldDrawMarkers(map)) return;
        
        DrawMarkers(viewport, map);
    }

    // File IO
    public virtual void Load() => Configuration = LoadConfig();
    public virtual void Unload() { }
    public void DrawConfig() => DrawableAttribute.DrawAttributes(Configuration, SaveConfig);
    private ModuleConfigBase LoadConfig() => FileController.LoadFile<ModuleConfigBase>($"{ModuleName}.config.json", Configuration);
    public void SaveConfig() => FileController.SaveFile($"{ModuleName}.config.json", Configuration.GetType(), Configuration);
    
    // Utilities
    protected bool IsPlayerInCurrentMap(Map map)
    {
        if (AgentMap.Instance() is null) return false;
        if (AgentMap.Instance()->CurrentMapId != map.RowId) return false;

        return true;
    }

    protected bool IsLocalPlayerValid() => Service.ClientState.LocalPlayer is not null;
}