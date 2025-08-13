using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Configuration;
using Lumina.Excel.Sheets;
using Mappy.Classes.SelectionWindowComponents;

namespace Mappy.Data;

public unsafe record Flag(uint Territory, uint Map, float X, float Y, uint IconId)
{
    public IDalamudTextureWrap? GetMapTexture() => MapDrawableOption.GetMapTexture(Map);

    public Map GetMap() => Service.DataManager.GetExcelSheet<Map>().GetRow(Map);

    public TerritoryType GetTerritoryType() => Service.DataManager.GetExcelSheet<TerritoryType>().GetRow(Territory);

    public void PlaceFlag()
    {
        AgentMap.Instance()->FlagMarkerCount = 0;
        AgentMap.Instance()->SetFlagMapMarker(Territory, Map, X, Y, IconId);

        if (System.SystemConfig.CenterOnFlag) {
            Focus();
        }
    }

    public string GetIdString() => $"{Territory}_{Map}_{X}_{Y}_{IconId}";

    public Vector2 GetCoordinate() => new(X, Y);

    public Vector2 GetMapCoordinate() => MapUtil.WorldToMap(GetCoordinate());

    public void Focus()
    {
        System.SystemConfig.FollowPlayer = false;
        System.IntegrationsController.OpenMap(Map);
        System.MapRenderer.CenterOnCoordinate(GetCoordinate());
    }

    public bool IsFlagSet()
    {
        if (AgentMap.Instance()->FlagMarkerCount is 0) return false;
        ref var setMarker = ref AgentMap.Instance()->FlagMapMarkers[0];

        if (setMarker.TerritoryId != Territory) return false;
        if (setMarker.MapId != Map) return false;
        if (Math.Abs(setMarker.XFloat - X) > 0.01f) return false;
        if (Math.Abs(setMarker.YFloat - Y) > 0.01f) return false;

        return true;
    }
}

public class FlagConfig
{
    public LinkedList<Flag> FlagHistory = [];

    // Not exposed to users, might be in the future.
    public int HistoryLimit = 10;

    public static FlagConfig Load() => Service.PluginInterface.LoadConfigFile("Flags.data.json", () => new FlagConfig());

    public void Save() => Service.PluginInterface.SaveConfigFile("Flags.data.json", System.FlagConfig);
}