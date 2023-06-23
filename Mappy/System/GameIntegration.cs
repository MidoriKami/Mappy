using System;
using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KamiLib;
using KamiLib.Caching;
using KamiLib.Hooking;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.System.Modules;
using Mappy.Utility;
using Mappy.Views.Windows;
using Map = Lumina.Excel.GeneratedSheets.Map;

namespace Mappy.System;

public unsafe class GameIntegration : IDisposable
{
    private delegate void OpenMapByIdDelegate(AgentMap* agent, uint mapID);
    private delegate void OpenMapDelegate(AgentMap* agent, OpenMapInfo* data);
    private delegate void SetFlagMarkerDelegate(AgentMap* agent, uint territoryId, uint mapId, float mapX, float mapY, uint iconId);
    private delegate void SetGatheringMarkerDelegate(AgentMap* agent, uint styleFlags, int mapX, int mapY, uint iconID, int radius, Utf8String* tooltip);
    private delegate void ShowMapDelegate(AgentInterface* agentMap, bool a1, bool a2);
    private delegate byte InsertTextCommand(AgentInterface* agent, uint paramID, byte a3 = 0);

    private readonly Hook<OpenMapByIdDelegate>? openMapByIdHook;
    private readonly Hook<OpenMapDelegate>? openMapHook;
    private readonly Hook<SetFlagMarkerDelegate>? setFlagMarkerHook;
    private readonly Hook<SetGatheringMarkerDelegate>? setGatheringMarkerHook;
    
    [Signature("E8 ?? ?? ?? ?? 40 B6 01 C7 44 24 ?? ?? ?? ?? ?? BA ?? ?? ?? ?? 48 8B CF E8 ?? ?? ?? ?? 84 C0 74 15", DetourName = nameof(OnShowHook))]
    private readonly Hook<ShowMapDelegate>? showHook = null;
    
    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 49 8D 8E")]
    private readonly InsertTextCommand? insertFlagTextCommand = null;

    private AgentInterface* ChatAgent => Framework.Instance()->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.ChatLog);
    private AgentInterface* GatheringNoteAgent => Framework.Instance()->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.GatheringNote);

    private bool integrationsEnabled;
    
    public GameIntegration()
    {
        SignatureHelper.Initialise(this);

        openMapByIdHook ??= Hook<OpenMapByIdDelegate>.FromAddress((nint)AgentMap.Addresses.OpenMapByMapId.Value, OpenMapById);
        openMapHook ??= Hook<OpenMapDelegate>.FromAddress((nint)AgentMap.Addresses.OpenMap.Value, OpenMap);
        setFlagMarkerHook ??= Hook<SetFlagMarkerDelegate>.FromAddress((nint)AgentMap.Addresses.SetFlagMapMarker.Value, SetFlagMarker);
        setGatheringMarkerHook ??= Hook<SetGatheringMarkerDelegate>.FromAddress((nint)AgentMap.Addresses.AddGatheringTempMarker.Value, SetGatheringMarker);

        if (MappySystem.SystemConfig.EnableIntegrations)
        {
            Enable();
        }
        
        Service.ClientState.EnterPvP += Disable;
        Service.ClientState.LeavePvP += TryEnable;
    }
    
    public void Dispose()
    {
        openMapByIdHook?.Dispose();
        openMapHook?.Dispose();
        setFlagMarkerHook?.Dispose();
        setGatheringMarkerHook?.Dispose();
        showHook?.Dispose();
        
        Service.ClientState.EnterPvP -= Disable;
        Service.ClientState.LeavePvP -= TryEnable;
    }

    public void Enable()
    {
        openMapByIdHook?.Enable();
        openMapHook?.Enable();
        setFlagMarkerHook?.Enable();
        setGatheringMarkerHook?.Enable();
        showHook?.Enable();

        integrationsEnabled = true;
    }

    public void Disable()
    {
        openMapByIdHook?.Disable();
        openMapHook?.Disable();
        setFlagMarkerHook?.Disable();
        setGatheringMarkerHook?.Disable();
        showHook?.Disable();
        
        integrationsEnabled = false;
    }

    private void TryEnable()
    {
        if (MappySystem.SystemConfig.EnableIntegrations)
        {
            Enable();
        }
    }

    public void Update()
    {
        if (integrationsEnabled && !MappySystem.SystemConfig.EnableIntegrations)
        {
            Disable();
        }

        if (!integrationsEnabled && MappySystem.SystemConfig.EnableIntegrations)
        {
            Enable();
        }
    }
    
    private void OpenMapById(AgentMap* agent, uint mapId) => Safety.ExecuteSafe(() =>
    {
        MappySystem.MapTextureController.LoadMap(mapId);
    }, "Exception during OpenMapByMapId");

    private void OpenMap(AgentMap* agent, OpenMapInfo* mapInfo) => Safety.ExecuteSafe(() =>
    {
        PluginLog.Debug("OpenMap");

        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is {} mapWindow)
        {
            ImGui.SetWindowFocus(mapWindow.WindowName);
            var map = LuminaCache<Map>.Instance.GetRow(mapInfo->MapId)!;

            MappySystem.SystemConfig.FollowPlayer = false;
            
            switch (mapInfo->Type)
            {
                case MapType.FlagMarker when Flag.TempMapMarker is { } flag:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    var flagPosition = Position.GetTextureOffsetPosition(flag.Position, map);
                    mapWindow.Viewport.SetViewportCenter(flagPosition);
                    break;
                
                case MapType.QuestLog:// when GetQuestLocation(mapInfo) is {} questLocation:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    break;
                
                case MapType.GatheringLog when GatheringArea.TempMapMarker is {} area:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    var gatherAreaPosition = Position.GetTextureOffsetPosition(area.Position, map);
                    mapWindow.Viewport.SetViewportCenter(gatherAreaPosition);
                    break;
                
                default:
                    MappySystem.MapTextureController.LoadMap(mapInfo->MapId);
                    break;
            }
        }

    }, "Exception during OpenMap");

    // private Vector2? GetQuestLocation(OpenMapInfo* mapInfo)
    // {
    //     var targetLevels = Service.QuestManager.GetActiveLevelsForQuest(mapInfo->TitleString.ToString(), mapInfo->MapId);
    //     var focusLevel = targetLevels?.Where(level => level.Map.Row == mapInfo->MapId && level.Map.Row != 0).FirstOrDefault();
    //
    //     if (focusLevel is not null)
    //     {
    //         return new Vector2(focusLevel.X, focusLevel.Z);
    //     }
    //
    //     return null;
    // }
    
    private void OnShowHook(AgentInterface* agent, bool a1, bool a2) => Safety.ExecuteSafe(() =>
    {
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is {} mapWindow)
        {
            ImGui.SetWindowFocus(mapWindow.WindowName);
            mapWindow.IsOpen = !mapWindow.IsOpen;
        }
    }, "Exception during OnShowHook");

    private void SetFlagMarker(AgentMap* agent, uint territoryId, uint mapId, float mapX, float mapY, uint iconId) => Safety.ExecuteSafe(() =>
    {
        PluginLog.Debug($"SetFlagMarker");
        
        Flag.SetFlagMarker( new TemporaryMapMarker
        {
            Type = MarkerType.Flag,
            MapID = mapId,
            IconID = iconId,
            Position = new Vector2(mapX, mapY)
        });

        setFlagMarkerHook!.Original(agent, territoryId, mapId, mapX, mapY, iconId);
    }, "Exception during SetFlagMarker");

    private void SetGatheringMarker(AgentMap* agent, uint styleFlags, int mapX, int mapY, uint iconID, int radius, Utf8String* tooltip) => Safety.ExecuteSafe(() =>
    {
        PluginLog.Debug("SetGatheringMarker");

        GatheringArea.SetGatheringAreaMarker(new TemporaryMapMarker
        {
            Type = MarkerType.Gathering,
            MapID = GetGatheringAreaMapInfo()->MapId,
            IconID = iconID,
            Radius = radius,
            Position = new Vector2(mapX, mapY),
            TooltipText = tooltip->ToString(),
        });
            
    }, "Exception during SetGatheringMarker");

    public void InsertFlagInChat() => insertFlagTextCommand?.Invoke(ChatAgent, 1048u, 0);

    private OpenMapInfo* GetGatheringAreaMapInfo()
    {
        // GatheringNoteAgent+184 is a pointer to where the OpenMapInfo block is roughly located
        var agentPointer = new IntPtr(GatheringNoteAgent);
        var agentOffsetPointer = agentPointer + 184;

        // OpenMapInfo is allocated 16bytes from this address
        var dataBlockPointer = new IntPtr(*(long*) agentOffsetPointer);
        var dataBlock = dataBlockPointer + 16;
        
        return (OpenMapInfo*) dataBlock;
    }
}