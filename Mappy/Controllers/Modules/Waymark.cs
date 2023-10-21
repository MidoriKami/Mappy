using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using KamiLib.Game;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Models.ModuleConfiguration;
using FieldMarker = Lumina.Excel.GeneratedSheets.FieldMarker;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.System.Modules;

public unsafe class Waymark : ModuleBase {
    public override ModuleName ModuleName => ModuleName.Waymarks;
    public override IModuleConfig Configuration { get; protected set; } = new WaymarkConfig();

    private readonly List<FieldMarker> fieldMarkers = LuminaCache<FieldMarker>.Instance.Where(marker => marker.MapIcon is not 0).ToList();

    protected override bool ShouldDrawMarkers(Map map) {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }
    
    protected override void UpdateMarkers(Viewport viewport, Map map) {
        var markerSpan = MarkingController.Instance()->FieldMarkerArraySpan;
        
        foreach (var index in Enumerable.Range(0, 8)) {
            if (markerSpan[index] is { Active: true } marker) {
                UpdateIcon(index, () => new MappyMapIcon {
                    MarkerId = index,
                    IconId = GetIconForMarkerIndex(index),
                    ObjectPosition = new Vector2(marker.X, marker.Z) / 1000.0f,
                }, icon => {
                    icon.ObjectPosition = new Vector2(marker.X, marker.Z) / 1000.0f;
                });
            }
        }
    }
    
    private uint GetIconForMarkerIndex(int index) => fieldMarkers[index].MapIcon;
}