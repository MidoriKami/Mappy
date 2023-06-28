using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using KamiLib.Caching;
using KamiLib.Hooking;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.System;

public record NetworkMarkerInfo(uint MapIcon, uint LevelRowId, uint ObjectiveId, byte Flags)
{
    public T? GetLuminaData<T>() where T : ExcelRow => LuminaCache<T>.Instance.GetRow(ObjectiveId);
    public Level? GetLevelData() => LuminaCache<Level>.Instance.GetRow(LevelRowId);
}

public unsafe class QuestController : IDisposable
{
    private delegate nint ReceiveMarkersDelegate(nint a1, nint a2, nint a3, nint a4, int a5);
    private delegate void ReceiveLevequestAreasDelegate(nint a1, uint a2);
    
    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 0F B6 43 10 4C 8D 4B 44", DetourName = nameof(ReceiveMarkers))]
    private readonly Hook<ReceiveMarkersDelegate>? receiveMarkersHook = null;

    [Signature("40 56 41 56 48 81 EC ?? ?? ?? ?? 48 8B F1", DetourName = nameof(ReceiveLevequestArea))]
    private readonly Hook<ReceiveLevequestAreasDelegate>? receiveLevequestAreasHook = null;
    
    private ConcurrentDictionary<uint, NetworkMarkerInfo> ReceivedMarkers { get; } = new();
    public HashSet<uint> ActiveLevequestLevels { get; } = new();

    public IEnumerable<NetworkMarkerInfo> CustomTalkMarkers => ReceivedMarkers.Values.Where(networkData => networkData.ObjectiveId is > 0xB0000 and < 0xC0000);
    public IEnumerable<NetworkMarkerInfo> TripleTriadMarkers => ReceivedMarkers.Values.Where(networkData => networkData.ObjectiveId is > 0x230000 and < 0x240000);
    public IEnumerable<NetworkMarkerInfo> QuestMarkers => ReceivedMarkers.Values.Where(networkData => networkData.ObjectiveId is > 0x10000 and < 0x20000);
    public IEnumerable<NetworkMarkerInfo> GuildleveAssignmentMarkers => ReceivedMarkers.Values.Where(networkData => networkData.ObjectiveId is > 0x60000 and < 0x70000);
    public IEnumerable<NetworkMarkerInfo> MiscNetworkMarkers => ReceivedMarkers.Values.Where(networkData => networkData.ObjectiveId is > 0x170000 and < 0x180000);

    public QuestController()
    {
        SignatureHelper.Initialise(this);
        receiveMarkersHook?.Enable();
        receiveLevequestAreasHook?.Enable();
    }

    public void Dispose()
    {
        receiveMarkersHook?.Dispose();
        receiveLevequestAreasHook?.Dispose();
    }

    public void ZoneChanged()
    {
        ReceivedMarkers.Clear();
    }
    
    private nint ReceiveMarkers(nint questMapIconIdArray, nint eventHandlerValueArray, nint questIdArray, nint unknownArray, int numEntries)
    {
        Safety.ExecuteSafe(() =>
        {
            PluginLog.Debug($"Received QuestMarkers: {numEntries}");
            
            foreach(var index in Enumerable.Range(0, numEntries))
            {
                var markerId = ((uint*) questMapIconIdArray)[index];
                var levelRowId = ((uint*) eventHandlerValueArray)[index];
                var questId = ((uint*) questIdArray)[index];
                var flags = ((byte*) unknownArray)[index];

                ReceivedMarkers.TryRemove(questId, out _);
                ReceivedMarkers.TryAdd(questId, new NetworkMarkerInfo(markerId, levelRowId, questId, flags));

                var location = LuminaCache<Level>.Instance.GetRow(levelRowId)!;

                PluginLog.Debug($"[{markerId, 5}] [{levelRowId, 7}] [{questId, 7}] [{flags, 4}] - {location.Territory.Value?.PlaceName.Value?.Name ?? "Null Name"}");
            }
        });

        return receiveMarkersHook!.Original(questMapIconIdArray, eventHandlerValueArray, questIdArray, unknownArray, numEntries);
    }
    
    private void ReceiveLevequestArea(nint a1, uint a2)
    {
        Safety.ExecuteSafe(() =>
        {
            ActiveLevequestLevels.Add(a2);
        });

        receiveLevequestAreasHook!.Original(a1, a2);
    }
}