using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.AutomaticUserInterface;
using KamiLib.FileIO;
using Lumina.Excel.GeneratedSheets;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;
using MapData = Mappy.System.MapData;

namespace Mappy.Abstracts;

public abstract unsafe class ModuleBase {
    public abstract ModuleName ModuleName { get; }
    public abstract IModuleConfig Configuration { get; protected set; }
    protected T GetConfig<T>() where T : IModuleConfig => (T) Configuration;

    private readonly List<MappyMapIcon> mapIcons = new();
    private readonly List<MappyMapText> mapText = new();

    public IReadOnlyList<MappyMapIcon> MapIcons => mapIcons;
    public IReadOnlyList<MappyMapText> MapText => mapText;

    // Map Marker
    public virtual void ZoneChanged(uint territoryType) { }
    public virtual void LoadForMap(MapData mapData) { }
    protected virtual bool ShouldDrawMarkers(Map map) => Configuration.Enable;
    protected abstract void UpdateMarkers(Viewport viewport, Map map);
    public void Draw(Viewport viewport, Map map) {
        if (!ShouldDrawMarkers(map)) return;
        
        foreach (var icon in mapIcons) icon.Stale = true;
        foreach (var text in mapText) text.Stale = true;
        
        UpdateMarkers(viewport, map);

        mapIcons.RemoveAll(icon => icon.Stale);
        mapText.RemoveAll(text => text.Stale);
        
        foreach(var icon in mapIcons) DrawUtilities.DrawMapIcon(icon, Configuration, viewport, map);
        foreach(var text in mapText) DrawUtilities.DrawMapText(text, viewport, map);
    }

    protected void UpdateIcon(object markerId, Func<MappyMapIcon> makeNewIcon, Action<MappyMapIcon>? updateIcon = null) {
        if (mapIcons.FirstOrDefault(mapIcon => mapIcon.MarkerId.Equals(markerId)) is { } icon) {
            icon.Stale = false;
            updateIcon?.Invoke(icon);
        }
        else {
            mapIcons.Add(makeNewIcon());
        }
    }

    protected void UpdateText(object textId, Func<MappyMapText> makeNewText, Action<MappyMapText>? updateText = null) {
        if (mapText.FirstOrDefault(mapTextLabel => mapTextLabel.TextId.Equals(textId)) is { } text) {
            text.Stale = false;
            updateText?.Invoke(text);
        }
        else {
            mapText.Add(makeNewText());
        }
    }

    // File IO
    public virtual void Load() => Configuration = LoadConfig();
    public virtual void Unload() { }
    public virtual void Update() { }
    public void DrawConfig() => DrawableAttribute.DrawAttributes(Configuration, SaveConfig);
    private IModuleConfig LoadConfig() => FileController.LoadFile<IModuleConfig>($"{ModuleName}.config.json", Configuration);
    private void SaveConfig() => FileController.SaveFile($"{ModuleName}.config.json", Configuration.GetType(), Configuration);
    
    // Utilities
    protected static bool IsPlayerInCurrentMap(Map map) {
        if (AgentMap.Instance() is null) return false;
        if (AgentMap.Instance()->CurrentMapId != map.RowId) return false;

        return true;
    }

    protected static bool IsPlayerInCurrentTerritory(Map map) {
        if (AgentMap.Instance() is null) return false;

        var isSameMap = AgentMap.Instance()->CurrentMapId == map.RowId;
        var isSameTerritory = AgentMap.Instance()->CurrentTerritoryId == map.TerritoryType.Row;
        
        if (!isSameMap && !isSameTerritory) return false;

        return true;
    }

    protected static bool IsLocalPlayerValid() => Service.ClientState.LocalPlayer is not null;
}