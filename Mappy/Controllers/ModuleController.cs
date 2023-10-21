using System.Collections.Generic;
using System.Linq;
using KamiLib.Utility;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;

namespace Mappy.System;

public class ModuleController {
    public readonly List<ModuleBase> Modules;
    
    public ModuleController() {
        Modules = Reflection.ActivateOfType<ModuleBase>().ToList();

        MappySystem.MapTextureController.MapLoaded += MapLoaded;
    }

    public void Load() {
        foreach (var module in Modules) {
            module.Load();
        }
    }

    public void Draw(Viewport viewport, Map map) {
        foreach (var module in Modules.OrderBy(module => module.Configuration.Layer)) {
            module.Draw(viewport, map);
        }
    }

    private void MapLoaded(object? sender, MapData mapData) {
        foreach (var module in Modules) {
            module.LoadForMap(mapData);
        }
    }

    public void Unload() {
        foreach (var module in Modules) {
            module.Unload();
        }
        
        MappySystem.MapTextureController.MapLoaded -= MapLoaded;
    }
    
    public void ZoneChanged(ushort newZone) {
        foreach (var module in Modules) {
            module.ZoneChanged(newZone);
        }
    }

    public void Update() {
        foreach (var module in Modules) {
            module.Update();
        }
    }

    public MappyMapIcon? GetMapMarker(object markerId) {
        foreach (var module in Modules) {
            if (module.MapIcons.FirstOrDefault(mapMarker => mapMarker.MarkerId.Equals(markerId)) is { } marker) {
                return marker;
            }
        }

        return null;
    }

    public MappyMapText? GetMapText(object textId) {
        foreach (var module in Modules) {
            if (module.MapText.FirstOrDefault(mapMarker => mapMarker.TextId.Equals(textId)) is { } marker) {
                return marker;
            }
        }

        return null;
    }
}