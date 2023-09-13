using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiScene;
using KamiLib.Caching;
using KamiLib.Hooking;
using Lumina.Excel.GeneratedSheets;
using Mappy.Utility;

namespace Mappy.System;
public record MapData(Map Map);

public unsafe class MapTextureController : IDisposable
{
    public List<Map>? MapLayers { get; private set; }
    public Map? CurrentMap { get; private set; }
    public TextureWrap? MapTexture { get; private set; }

    [MemberNotNullWhen(true, "MapLayers", "CurrentMap", "MapTexture")]
    public bool Ready => MapTexture is not null && CurrentMap is not null && MapLayers is not null;

    public event EventHandler<MapData>? MapLoaded;

    private uint lastMapId;

    public void Dispose()
    {
        MapTexture?.Dispose();
    }

    public void Update()
    {
        var agent = AgentMap.Instance();
        if (agent is null) return;

        var currentMapId = agent->CurrentMapId;
        if (lastMapId != currentMapId)
        {
            PluginLog.Debug($"Map ID Updated: {currentMapId}");
            LoadMap(currentMapId);

            lastMapId = currentMapId;
        }
    }

    public void LoadMap(uint mapId)
    {
        if (CurrentMap?.RowId != mapId)
        {
            InternalLoadMap(mapId);
        }
    }

    public void MoveMapToPlayer()
    {
        var agent = AgentMap.Instance();
        if (agent is null) return;

        LoadMap(agent->CurrentMapId);
    }

    private void InternalLoadMap(uint mapId) => Safety.ExecuteSafe(() =>
    {
        CurrentMap = LuminaCache<Map>.Instance.GetRow(mapId)!;

        PluginLog.Debug($"Loading Map: {mapId} - {CurrentMap.GetName()}");
        PluginLog.Debug($"Map Data: {CurrentMap.Id.RawString}");

        MapLayers = Service.DataManager.GetExcelSheet<Map>()!
            .Where(eachMap => eachMap.PlaceName.Row == CurrentMap.PlaceName.Row)
            .Where(eachMap => eachMap.MapIndex != 0)
            .OrderBy(eachMap => eachMap.MapIndex)
            .ToList();

        MapLoaded?.Invoke(this, new MapData(CurrentMap));

        MapTexture?.Dispose();
        MapTexture = Service.TextureProvider.GetTextureFromGame(GetPathFromMap(CurrentMap));
    });
    
    private static string GetPathFromMap(Map map)
    {
        var mapKey = map.Id.RawString;
        var rawKey = mapKey.Replace("/", "");
        return $"ui/map/{mapKey}/{rawKey}_m.tex";
    }
}