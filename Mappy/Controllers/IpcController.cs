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
    Texture,
    Map,
}

public enum IpcMarkerType {
    Image,
    Shape
}

public class IpcMapMarker {
    public required PositionType PositionType { get; init; }
    public uint IconId { get; init; }
    public required Vector2 Position { get; set; }
    public required uint MapId { get; init; }
    public string Tooltip { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public required IpcMarkerType Type { get; init; }
    public Vector4 OutlineColor { get; init; }
    public Vector4 FillColor { get; init; }
    public int Segments { get; init; }
    public float Radius { get; init; }
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
    
    // Adds a Marker to the world. Using World Coordinates
    //
    // Function signature: string AddWorldShape(Vector2 WorldPosition, uint MapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius, string Tooltip, string Description);
    //
    // WorldPosition          Vector2       Position (GameObject Position) to draw icon at
    // MapId                  uint          id representing a mapId, a value of 0 will place the marker in the map the player is currently located in
    // OutlineColor           Vector4       color of the shape
    // FillColor              Vector4       fill color for shape
    // Segments               int           number of segments for shape
    // Radius                 float         size for shape
    //
    // Returns a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<Vector2, uint, Vector4, Vector4, int, float, string>? _addWorldShape;
    
    // Adds a Marker to the world. Using Texture Coordinates
    //
    // Function signature: string AddWorldShape(Vector2 TexturePosition, uint MapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius, string Tooltip, string Description);
    //
    // TexturePosition        Vector2       Position (Image Texture Position) to draw icon at
    // MapId                  uint          id representing a mapId, a value of 0 will place the marker in the map the player is currently located in
    // OutlineColor           Vector4       color of the shape
    // FillColor              Vector4       fill color for shape
    // Segments               int           number of segments for shape
    // Radius                 float         size for shape
    //
    // Returns a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<Vector2, uint, Vector4, Vector4, int, float, string>? _addTextureShape;
    
    // Adds a Marker to the world. Using Map Coordinates (12.3, 10.0)
    //
    // Function signature: string AddWorldShape(Vector2 MapPosition, uint MapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius, string Tooltip, string Description);
    //
    // TexturePosition        Vector2       MapCoordinates Map Coordinates ex (12.3, 10.0) to draw icon at
    // MapId                  uint          id representing a mapId, a value of 0 will place the marker in the map the player is currently located in
    // OutlineColor           Vector4       color of the shape
    // FillColor              Vector4       fill color for shape
    // Segments               int           number of segments for shape
    // Radius                 float         size for shape
    //
    // Returns a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<Vector2, uint, Vector4, Vector4, int, float, string>? _addMapShape;
    
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
    // Function signature: string AddLineMarker(Vector2 Start, Vector2 Stop, uint MapId, Vector4 Color, float Thickness);
    //
    // Start                  Vector2       The Start Coordinates of the Arrow, uses Texture Coordinates
    // Stop                   Vector2       The Stop Coordinates of the Arrow, uses Texture Coordinates
    // MapId                  uint          id representing a mapId, a value of 0 will place the line in the map the player is currently located in
    // Color                  Vector4       The color of the line
    // Thickness              float         The thickness of the line
    //
    // Return a unique identifier for the added marker, string.Empty if it failed to add the marker
    private static ICallGateProvider<Vector2, Vector2, uint, Vector4, float, string>? _addWorldLine;
    
    // Adds a line to the map
    //
    // Function signature: string AddLineMarker(Vector2 Start, Vector2 Stop, uint MapId, Vector4 Color, float Thickness);
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

    public static Dictionary<string, IpcMapMarker> Markers = new();
    public static Dictionary<string, IpcArrowMarker> LineMarkers = new();
    
    public IpcController() {
        
        _isReady = Service.PluginInterface.GetIpcProvider<bool>("Mappy.IsReady");
        _isReady.RegisterFunc(IsReady);

        _addWorldMarker = Service.PluginInterface.GetIpcProvider<uint, Vector2, uint, string, string, string>("Mappy.World.AddMarker");
        _addTextureMarker = Service.PluginInterface.GetIpcProvider<uint, Vector2, uint, string, string, string>("Mappy.Texture.AddMarker");
        _addMapCoordinateMarker = Service.PluginInterface.GetIpcProvider<uint, Vector2, uint, string, string, string>("Mappy.MapCoord.AddMarker");
        _addWorldMarker.RegisterFunc(AddWorldMarker);
        _addTextureMarker.RegisterFunc(AddTextureMarker);
        _addMapCoordinateMarker.RegisterFunc(AddMapCoordinateMarker);
        
        _addTextureLine = Service.PluginInterface.GetIpcProvider<Vector2, Vector2, uint, Vector4, float, string>("Mappy.Texture.AddLine");
        _addWorldLine = Service.PluginInterface.GetIpcProvider<Vector2, Vector2, uint, Vector4, float, string>("Mappy.World.AddLine");
        _addMapCoordLine = Service.PluginInterface.GetIpcProvider<Vector2, Vector2, uint, Vector4, float, string>("Mappy.MapCoord.AddLine");
        _addTextureLine.RegisterFunc(AddTextureLine);
        _addWorldLine.RegisterFunc(AddWorldLine);
        _addMapCoordLine.RegisterFunc(AddMapCoordLine);
        
        _addWorldShape = Service.PluginInterface.GetIpcProvider<Vector2, uint, Vector4, Vector4, int, float, string>("Mappy.World.AddShape");
        _addTextureShape = Service.PluginInterface.GetIpcProvider<Vector2, uint, Vector4, Vector4, int, float, string>("Mappy.Texture.AddShape");
        _addMapShape = Service.PluginInterface.GetIpcProvider<Vector2, uint, Vector4, Vector4, int, float, string>("Mappy.MapCoord.AddShape");
        _addWorldShape.RegisterFunc(AddWorldShape);
        _addTextureShape.RegisterFunc(AddTextureShape);
        _addMapShape.RegisterFunc(AddMapShape);

        _updateMarker = Service.PluginInterface.GetIpcProvider<string, Vector2, bool>("Mappy.UpdateMarker");
        _updateMarker.RegisterFunc(UpdateMarker);

        _removeMarker = Service.PluginInterface.GetIpcProvider<string, bool>("Mappy.Remove");
        _removeMarker.RegisterFunc(RemoveMarker);
    }

    private static bool IsReady()
        => MappySystem.MapTextureController is { Ready: true };
    
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
    
    private static string AddMapCoordLine(Vector2 start, Vector2 stop, uint mapId, Vector4 color, float thickness)
        => AddLine(start, stop, mapId, color, thickness, PositionType.Map);
    
    private static string AddTextureLine(Vector2 start, Vector2 stop, uint mapId, Vector4 color, float thickness)
        => AddLine(start, stop, mapId, color, thickness, PositionType.Texture);
    
    private static string AddWorldLine(Vector2 start, Vector2 stop, uint mapId, Vector4 color, float thickness)
        => AddLine(start, stop, mapId, color, thickness, PositionType.World);

    private static string AddWorldShape(Vector2 location, uint mapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius)
        => AddShape(location, mapId, outlineColor, fillColor, segments, radius, PositionType.World);
    
    private static string AddTextureShape(Vector2 location, uint mapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius)
        => AddShape(location, mapId, outlineColor, fillColor, segments, radius, PositionType.Texture);
    
    private static string AddMapShape(Vector2 location, uint mapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius)
        => AddShape(location, mapId, outlineColor, fillColor, segments, radius, PositionType.Map);

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
            Type = IpcMarkerType.Image,
        }) ? newId : string.Empty;
    }

    private static unsafe string AddLine(Vector2 start, Vector2 stop, uint mapId, Vector4 color, float thickness, PositionType positionType) {
        if (AgentMap.Instance() is null) return string.Empty;
        
        mapId = mapId is 0 ? AgentMap.Instance()->CurrentMapId : mapId;
        if (LuminaCache<Map>.Instance.GetRow(mapId) is not { } map) return string.Empty;

        var newId = Guid.NewGuid().ToString("N");

        var startPosition = positionType switch {
            PositionType.World => Position.GetTexturePosition(start, map),
            PositionType.Map => Position.GetTexturePosition(Position.MapToWorld(start, map), map),
            PositionType.Texture => start,
            _ => throw new ArgumentOutOfRangeException(nameof(positionType), positionType, null)
        };

        var stopPosition = positionType switch {
            PositionType.World => Position.GetTexturePosition(stop, map),
            PositionType.Map => Position.GetTexturePosition(Position.MapToWorld(stop, map), map),
            PositionType.Texture => stop,
            _ => throw new ArgumentOutOfRangeException(nameof(positionType), positionType, null)
        };
        
        return LineMarkers.TryAdd(newId, new IpcArrowMarker
        {
            Start = startPosition,
            End = stopPosition,
            MapId = mapId,
            Color = color,
            Thickness = thickness,
        }) ? newId : string.Empty;
    }
    
    private static unsafe string AddShape(Vector2 location, uint mapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius, PositionType type) {
        if (AgentMap.Instance() is null) return string.Empty;
        
        mapId = mapId is 0 ? AgentMap.Instance()->CurrentMapId : mapId;

        var newId = Guid.NewGuid().ToString("N");

        return Markers.TryAdd(newId, new IpcMapMarker {
            Position = location,
            PositionType = type,
            MapId = mapId,
            Type = IpcMarkerType.Shape,
            OutlineColor = outlineColor,
            FillColor = fillColor,
            Segments = segments,
            Radius = radius,
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
        _addWorldLine?.UnregisterFunc();
        _addMapCoordLine?.UnregisterFunc();
        _addWorldShape?.UnregisterFunc();
        _addTextureShape?.UnregisterFunc();
        _addMapShape?.UnregisterFunc();
        _isReady?.UnregisterFunc();
    }
}