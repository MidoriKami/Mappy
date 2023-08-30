using DailyDuty.System;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.AutomaticUserInterface;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.Enums;
using MapData = Mappy.System.MapData;

namespace Mappy.Abstracts;

public abstract unsafe class ModuleBase
{
    public abstract ModuleName ModuleName { get; }
    public abstract IModuleConfig Configuration { get; protected set; }
    protected T GetConfig<T>() where T : IModuleConfig => (T) Configuration;

    // Map Marker
    public virtual void ZoneChanged(uint territoryType) { }
    public virtual void LoadForMap(MapData mapData) { }
    protected abstract void DrawMarkers(Viewport viewport, Map map);
    protected virtual bool ShouldDrawMarkers(Map map) => Configuration.Enable;
    public void Draw(Viewport viewport, Map map)
    {
        if (!ShouldDrawMarkers(map)) return;
        
        DrawMarkers(viewport, map);
    }

    // File IO
    public virtual void Load() => Configuration = LoadConfig();
    public virtual void Unload() { }
    public virtual void Update() { }
    public void DrawConfig() => DrawableAttribute.DrawAttributes(Configuration, SaveConfig);
    private IModuleConfig LoadConfig() => FileController.LoadFile<IModuleConfig>($"{ModuleName}.config.json", Configuration);
    private void SaveConfig() => FileController.SaveFile($"{ModuleName}.config.json", Configuration.GetType(), Configuration);
    
    // Utilities
    protected static bool IsPlayerInCurrentMap(Map map)
    {
        if (AgentMap.Instance() is null) return false;
        if (AgentMap.Instance()->CurrentMapId != map.RowId) return false;

        return true;
    }

    protected static bool IsPlayerInCurrentTerritory(Map map)
    {
        if (AgentMap.Instance() is null) return false;

        var isSameMap = AgentMap.Instance()->CurrentMapId == map.RowId;
        var isSameTerritory = AgentMap.Instance()->CurrentTerritoryId == map.TerritoryType.Row;
        
        if (!isSameMap && !isSameTerritory) return false;

        return true;
    }

    protected static bool IsLocalPlayerValid() => Service.ClientState.LocalPlayer is not null;
}