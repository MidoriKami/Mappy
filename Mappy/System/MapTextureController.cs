using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiScene;
using KamiLib.Caching;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.System;
public record MapData(Map Map, List<Map> Layers, TextureWrap Texture);

public unsafe class MapTextureController : IDisposable
{
    public List<Map>? MapLayers { get; protected set; }
    public Map? CurrentMap { get; protected set; }
    public TextureWrap? MapTexture { get; protected set; }

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

    public void LoadMap(uint mapId) => Task.Run(() => InternalLoadMap(mapId));
    
    public void MoveMapToPlayer()
    {
        var agent = AgentMap.Instance();
        if (agent is null) return;
        if (this is not { Ready: true, CurrentMap: var map }) return;

        if (agent->CurrentMapId != map.RowId)
        {
            LoadMap(agent->CurrentMapId);
        }
    }
    
    private void InternalLoadMap(uint mapId)
    {
        PluginLog.Debug($"Loading Map: {mapId}");

        CurrentMap = LuminaCache<Map>.Instance.GetRow(mapId)!;
        
        MapLayers = Service.DataManager.GetExcelSheet<Map>()!
            .Where(eachMap => eachMap.PlaceName.Row == CurrentMap.PlaceName.Row)
            .Where(eachMap => eachMap.MapIndex != 0)
            .OrderBy(eachMap => eachMap.MapIndex)
            .ToList();

        MapTexture = MappySystem.PenumbraController.GetTexture(GetPathFromMap(CurrentMap))!;

        MapLoaded?.Invoke(this, new MapData(CurrentMap, MapLayers, MapTexture));
    }
    
    private static string GetPathFromMap(Map map)
    {
        var mapKey = map.Id.RawString;
        var rawKey = mapKey.Replace("/", "");
        return $"ui/map/{mapKey}/{rawKey}_m.tex";
    }
}