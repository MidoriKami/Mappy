using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;
using Mappy.Utility;

namespace Mappy.System;

public enum PositionType {
    World,
    Texture
}

public class IpcMapMarker {
    public required PositionType PositionType { get; init; }
    public required uint IconId { get; init; }
    public required Vector2 Position { get; set; }
    public required uint MapId { get; init; }
    public string Tooltip { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public class IpcArrowMarker {
    public required Vector2 Start { get; init; }
    public required Vector2 End { get; init; }
    public required uint MapId { get; init; }
    public required Vector4 Color { get; init; }
    public required float Thickness { get; init; }
}

public class IpcController : IDisposable {
    // Mappy uses three different coordinate systems
    // "World" positions are game object positions
    // "Texture" positions are locations on the map image itself, all maps are 2048 x 2048 in size, with (1024, 1024) being center
    // "Map Coordinate" positions are the user friendly coordinates displayed by the game, typically some value like: (12.3, 10.0)

    // Adds a Marker to the world. Using World Coordinates
    //
    // Function signature: string AddWorldMarker(uint IconId, Vector2 WorldPosition, uint MapId, string Tooltip, string Description);
    //
    // IconId                 uint          id representing a game icon
    // WorldPosition          Vector2       Position (GameObject Position) to draw icon at
    // MapId                  uint          id representing a mapId, a value of 0 will place the marker in the map the player is currently located in
    // Tooltip                string        label to display when the user hovers their cursor on the icon, string.Empty will disable this feature
    // Description            string        extra text to display when the user hovers their cursor on the icon, string.Empty will disable this feature
    //
    // Returns a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<uint, Vector2, uint, string, string, string>? _addWorldMarker;
    
    // Adds a Marker to the world. Using Texture Coordinates
    //
    // Function signature: string AddWorldMarker(uint IconId, Vector2 TexturePosition, uint MapId, string Tooltip, string Description);
    //
    // IconId                 uint          id representing a game icon
    // TexturePosition        Vector2       Position (Image Texture Position) to draw icon at
    // MapId                  uint          id representing a mapId, a value of 0 will place the marker in the map the player is currently located in
    // Tooltip                string        label to display when the user hovers their cursor on the icon, string.Empty will disable this feature
    // Description            string        extra text to display when the user hovers their cursor on the icon, string.Empty will disable this feature
    //
    // Returns a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<uint, Vector2, uint, string, string, string>? _addTextureMarker;
    
    // Adds a Marker to the world. Using Map Coordinates (12.3, 10.0)
    //
    // Function signature: string AddWorldMarker(uint IconId, Vector2 MapCoordinates, uint MapId, string Tooltip, string Description);
    //
    // IconId                 uint          id representing a game icon
    // TexturePosition        Vector2       MapCoordinates Map Coordinates ex (12.3, 10.0) to draw icon at
    // MapId                  uint          id representing a mapId, a value of 0 will place the marker in the map the player is currently located in
    // Tooltip                string        label to display when the user hovers their cursor on the icon, string.Empty will disable this feature
    // Description            string        extra text to display when the user hovers their cursor on the icon, string.Empty will disable this feature
    //
    // Returns a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<uint, Vector2, uint, string, string, string>? _addMapCoordinateMarker;
    
    // Removes the specified marker
    //
    // Function signature: bool RemoveMarker(string MarkerId);
    //
    // MarkerId               string        id of the marker to remove
    //
    // Returns whether the marker was successfully removed or not
    private static ICallGateProvider<string, bool>? _removeMarker;
    
    // Updates the specified marker
    //
    // Function signature: bool UpdateMarker(string MarkerId, Vector2 Position);
    //
    // MarkerId               string        id of the marker to remove
    // Position               Vector2       Updates the position of the marker, it will continue to use its original coordinate system for positioning
    //
    // Returns whether the marker was successfully updated or not
    private static ICallGateProvider<string, Vector2, bool>? _updateMarker;

    // Returns true if the IPC is ready
    private static ICallGateProvider<bool>? _isReady;

    // Adds a line to the map
    //
    // Function signature: string AddArrowMarker(Vector2 Start, Vector2 Stop, uint MapId, Vector4 Color, float Thickness);
    //
    // Start                  Vector2       The Start Coordinates of the Arrow, uses Texture Coordinates
    // Stop                   Vector2       The Stop Coordinates of the Arrow, uses Texture Coordinates
    // MapId                  uint          id representing a mapId, a value of 0 will place the line in the map the player is currently located in
    // Color                  Vector4       The color of the line
    // Thickness              float         The thickness of the line
    //
    // Return a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<Vector2, Vector2, uint, Vector4, float, string>? _addTextureLine;
    
    // Adds a line to the map
    //
    // Function signature: string AddArrowMarker(Vector2 Start, Vector2 Stop);
    //
    // Start                  Vector2       The Start Coordinates of the Arrow, uses Map Coordinates
    // Stop                   Vector2       The Stop Coordinates of the Arrow, uses Map Coordinates
    // MapId                  uint          id representing a mapId, a value of 0 will place the line in the map the player is currently located in
    // Color                  Vector4       The color of the line
    // Thickness              float         The thickness of the line
    //
    // Return a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<Vector2, Vector2, uint, Vector4, float, string>? _addMapCoordLine;

    // Removes the specified arrow marker
    //
    // Function signature: bool RemoveArrowMarker(string MarkerId);
    //
    // MarkerId               string        id of the marker to remove
    //
    // Returns whether the marker was successfully removed or not
    private static ICallGateProvider<string, bool>? _removeLine;

    public static Dictionary<string, IpcMapMarker> Markers = new();
    public static Dictionary<string, IpcArrowMarker> LineMarkers = new();

    // // Copy/Paste this to subscribe to these functions, be sure to check for IPCNotReady exceptions ;)
    // public ICallGateSubscriber<uint, Vector2, uint, string, string, string>? AddWorldMarkerIpcFunction = null;
    // public ICallGateSubscriber<uint, Vector2, uint, string, string, string>? AddTextureMarkerIpcFunction = null;
    // public ICallGateSubscriber<uint, Vector2, uint, string, string, string>? AddMapCoordinateMarkerIpcFunction = null;
    // public ICallGateSubscriber<string, bool>? RemoveMarkerIpcFunction = null;
    // public ICallGateSubscriber<string, Vector2, bool>? UpdateMarkerIpcFunction = null;
    // public ICallGateSubscriber<Vector2, Vector2, uint, Vector4, float, string>? AddTextureLineIpcFunction = null;
    // public ICallGateSubscriber<Vector2, Vector2, uint, Vector4, float, string>? AddMapCoordLineIpcFunction = null;
    // public ICallGateSubscriber<string, bool>? RemoveLineIpcFunction = null;
    // public ICallGateSubscriber<bool>? IsReadyIpcFunction = null;

    // Todo: Add Tooltips
    public IpcController() {
        // // Copy/Paste this to subscribe to these functions, be sure to check for IPCNotReady exceptions ;)
        // AddWorldMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.World.AddMarker");
        // AddTextureMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.Texture.AddMarker");
        // AddMapCoordinateMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.MapCoord.AddMarker");
        // RemoveMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, bool>("Mappy.RemoveMarker");
        // UpdateMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, Vector2, bool>("Mappy.UpdateMarker");
        // AddTextureLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, Vector2, uint, Vector4, float, string>("Mappy.Texture.AddLine");
        // AddMapCoordLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, Vector2, uint, Vector4, float, string>("Mappy.MapCoord.AddLine");
        // RemoveLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, bool>("Mappy.RemoveLine");
        // IsReadyIpcFunction = Service.PluginInterface.GetIpcSubscriber<bool>("Mappy.IsReady");
        
        _addWorldMarker = Service.PluginInterface.GetIpcProvider<uint, Vector2, uint, string, string, string>("Mappy.World.AddMarker");
        _addTextureMarker = Service.PluginInterface.GetIpcProvider<uint, Vector2, uint, string, string, string>("Mappy.Texture.AddMarker");
        _addMapCoordinateMarker = Service.PluginInterface.GetIpcProvider<uint, Vector2, uint, string, string, string>("Mappy.MapCoord.AddMarker");
        _removeMarker = Service.PluginInterface.GetIpcProvider<string, bool>("Mappy.RemoveMarker");
        _updateMarker = Service.PluginInterface.GetIpcProvider<string, Vector2, bool>("Mappy.UpdateMarker");
        _addTextureLine = Service.PluginInterface.GetIpcProvider<Vector2, Vector2, uint, Vector4, float, string>("Mappy.Texture.AddLine");
        _addMapCoordLine = Service.PluginInterface.GetIpcProvider<Vector2, Vector2, uint, Vector4, float, string>("Mappy.MapCoord.AddLine");
        _removeLine = Service.PluginInterface.GetIpcProvider<string, bool>("Mappy.RemoveLine");
        _isReady = Service.PluginInterface.GetIpcProvider<bool>("Mappy.IsReady");
        
        _addWorldMarker.RegisterFunc(AddWorldMarker);
        _addTextureMarker.RegisterFunc(AddTextureMarker);
        _addMapCoordinateMarker.RegisterFunc(AddMapCoordinateMarker);
        _removeMarker.RegisterFunc(RemoveMarker);
        _updateMarker.RegisterFunc(UpdateMarker);
        _addTextureLine.RegisterFunc(AddTextureLine);
        _addMapCoordLine.RegisterFunc(AddMapCoordLine);
        _removeLine.RegisterFunc(RemoveLine);
        _isReady.RegisterFunc(IsReady);
    }

    private static string AddWorldMarker(uint iconId, Vector2 worldLocation, uint mapId, string tooltip, string secondaryTooltip)
        => AddMarker(iconId, worldLocation, mapId, tooltip, secondaryTooltip, PositionType.World);

    private static string AddTextureMarker(uint iconId, Vector2 textureLocation, uint mapId, string tooltip, string secondaryTooltip)
        => AddMarker(iconId, textureLocation, mapId, tooltip, secondaryTooltip, PositionType.Texture);

    private static unsafe string AddMapCoordinateMarker(uint iconId, Vector2 mapCoordinateLocation, uint mapId, string tooltip, string secondaryTooltip) {
        if (AgentMap.Instance() is null) return string.Empty;
        
        mapId = mapId is 0 ? AgentMap.Instance()->CurrentMapId : mapId;

        if (LuminaCache<Map>.Instance.GetRow(mapId) is not { } map) return string.Empty;

        var scaledObjectPosition = Position.MapToWorld(mapCoordinateLocation, map);
        
        return AddWorldMarker(iconId, scaledObjectPosition, mapId, tooltip, secondaryTooltip);
    }
    
    private static bool RemoveMarker(string markerId)
        => LineMarkers.Remove(markerId) || Markers.Remove(markerId);

    private static unsafe string AddMapCoordLine(Vector2 start, Vector2 stop, uint mapId, Vector4 color, float thickness) {
        if (AgentMap.Instance() is null) return string.Empty;
        
        mapId = mapId is 0 ? AgentMap.Instance()->CurrentMapId : mapId;
        if (LuminaCache<Map>.Instance.GetRow(mapId) is not { } map) return string.Empty;

        var newId = Guid.NewGuid().ToString("N");
        
        var adjustedStart = Position.MapToWorld(start, map);
        var adjustedStop = Position.MapToWorld(stop, map);
        
        return LineMarkers.TryAdd(newId, new IpcArrowMarker
        {
            Start = Position.GetTexturePosition(adjustedStart, map),
            End = Position.GetTexturePosition(adjustedStop, map),
            MapId = mapId,
            Color = color,
            Thickness = thickness,
        }) ? newId : string.Empty;
    }
    
    private static unsafe string AddTextureLine(Vector2 start, Vector2 stop, uint mapId, Vector4 color, float thickness) {
        if (AgentMap.Instance() is null) return string.Empty;
        
        mapId = mapId is 0 ? AgentMap.Instance()->CurrentMapId : mapId;
        
        var newId = Guid.NewGuid().ToString("N");

        return LineMarkers.TryAdd(newId, new IpcArrowMarker
        {
            Start = start,
            End = stop,
            MapId = mapId,
            Color = color,
            Thickness = thickness,
        }) ? newId : string.Empty;
    }

    private static bool RemoveLine(string id) 
        => LineMarkers.Remove(id) || Markers.Remove(id);

    private static bool IsReady()
        => MappySystem.MapTextureController is { Ready: true };
    
    private static unsafe string AddMarker(uint iconId, Vector2 location, uint mapId, string tooltip, string secondaryTooltip, PositionType type) {
        if (AgentMap.Instance() is null) return string.Empty;
        
        mapId = mapId is 0 ? AgentMap.Instance()->CurrentMapId : mapId;

        var newId = Guid.NewGuid().ToString("N");

        return Markers.TryAdd(newId, new IpcMapMarker {
            Position = location,
            PositionType = type,
            IconId = iconId,
            Description = secondaryTooltip,
            Tooltip = tooltip,
            MapId = mapId,
        }) ? newId : string.Empty;
    }

    private static bool UpdateMarker(string markerId, Vector2 location) {
        if (!Markers.TryGetValue(markerId, out var marker)) return false;

        marker.Position = location;
        return true;
    }

    public void Dispose() {
        _addWorldMarker?.UnregisterFunc();
        _addTextureMarker?.UnregisterFunc();
        _addMapCoordinateMarker?.UnregisterFunc();
        _removeMarker?.UnregisterFunc();
        _updateMarker?.UnregisterFunc();
        _addTextureLine?.UnregisterFunc();
        _addMapCoordLine?.UnregisterFunc();
        _removeLine?.UnregisterFunc();
        _isReady?.UnregisterFunc();
    }
}