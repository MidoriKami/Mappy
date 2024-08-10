using System;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Text.Json.Serialization;
using Dalamud.Interface;
using KamiLib.Configuration;

namespace Mappy.Data;

public enum CenterTarget {
    [Description("Disabled")]
    Disabled = 0,

    [Description("Player")]
    Player = 1,

    [Description("Map")]
    Map = 2
}

[Flags]
public enum FadeMode {
    [Description("Always")]
    Always = 1 << 0,
    
    [Description("When Moving")]
    WhenMoving = 1 << 2,
    
    [Description("When Focused")]
    WhenFocused = 1 << 3,
    
    [Description("When Unfocused")]
    WhenUnFocused = 1 << 4,
}

public class SystemConfig : CharacterConfiguration {
    public bool UseLinearZoom = false;
    public float ZoomSpeed = 0.25f;
    public float IconScale = 0.50f;
    public bool ShowMiscTooltips = true;
    public bool HideWithGameGui = true;
    public bool HideBetweenAreas = false;
    public bool HideInDuties = false;
    public bool HideInCombat = false;
    public bool KeepOpen = false;
    public bool FollowOnOpen = false;
    public bool FollowPlayer = true;
    public CenterTarget CenterOnOpen = CenterTarget.Disabled;
    public bool ScalePlayerCone = false;
    public float ConeSize = 150.0f;
    public bool ShowRadar = true;
    public Vector4 RadarColor = KnownColor.Gray.Vector() with { W = 0.10f };
    public Vector4 RadarOutlineColor = KnownColor.Gray.Vector() with { W = 0.30f };
    public bool HideWindowFrame = false;
    public bool EnableShiftDragMove = false;
    public bool IgnoreEscapeKey = false;
    public bool LockWindow = false;
    public float FadePercent = 0.60f;
    public FadeMode FadeMode = FadeMode.WhenUnFocused | FadeMode.WhenMoving;
    public Vector2 WindowPosition = new(1024.0f, 700.0f);
    public Vector2 WindowSize = new(500.0f, 500.0f);
    public bool AlwaysShowToolbar = false;
    public bool ShowToolbarOnHover = true;
    public bool ScaleWithZoom = true;
    public bool AcceptedSpoilerWarning = false;
    public Vector4 AreaColor = KnownColor.CornflowerBlue.Vector() with { W = 0.33f };
    public Vector4 AreaOutlineColor = KnownColor.CornflowerBlue.Vector() with { W = 0.30f };
    public Vector4 PlayerConeColor = KnownColor.CornflowerBlue.Vector() with { W = 0.33f };
    public Vector4 PlayerConeOutlineColor = KnownColor.CornflowerBlue.Vector() with { W = 1.0f };
    public bool CenterOnFlag = true;
    public bool CenterOnGathering = true;
    public bool CenterOnQuest = true;
    public bool LockCenterOnMap = false;
    public bool ShowCoordinateBar = true;
    public float ToolbarFade = 0.33f;
    public float CoordinateBarFade = 0.66f;
    public Vector4 CoordinateTextColor = KnownColor.White.Vector();
    public bool ZoomLocked = false;
    public bool ShowPlayers = true;
    public bool SetFlagOnFateClick = false;
    public bool ShowPlayerIcon = true;
    public float PlayerIconScale = 1.0f;
    public float MapScale = 1.0f;
    public bool AutoZoom = false;

    // Do not persist this setting
    [JsonIgnore] public bool DebugMode = false;

    public static SystemConfig Load() 
        => Service.PluginInterface.LoadConfigFile("System.config.json", () => new SystemConfig());

    public void Save() 
        => Service.PluginInterface.SaveConfigFile("System.config.json", System.SystemConfig);
}