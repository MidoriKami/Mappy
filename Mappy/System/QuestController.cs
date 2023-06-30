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
    private delegate nint ReceiveMarkersDelegate(uint* mapIconArray, uint* levelArray, uint* questIdArray, byte* flagArray, int numEntries);
    private delegate void ReceiveLevequestAreasDelegate(nint a1, uint levelId);
    
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
    
    private nint ReceiveMarkers(uint* mapIconArray, uint* levelArray, uint* questIdArray, byte* flagArray, int numEntries)
    {
        Safety.ExecuteSafe(() =>
        {
            PluginLog.Debug($"Received QuestMarkers: {numEntries}");

            var markers = GenerateMarkerList(mapIconArray, levelArray, questIdArray, flagArray, numEntries).ToList();
            markers.ForEach(LogMarker);

            if (!markers.All(marker => marker.Flags is 6))
            {
                ReceivedMarkers.Clear();

                foreach (var marker in markers)
                {
                    ReceivedMarkers.TryAdd(marker.ObjectiveId, marker);
                }
            }
        });

        return receiveMarkersHook!.Original(mapIconArray, levelArray, questIdArray, flagArray, numEntries);
    }

    private IEnumerable<NetworkMarkerInfo> GenerateMarkerList(uint* mapIconArray, uint* levelArray, uint* questIdArray, byte* flagArray, int numEntries) 
        => Enumerable.Range(0, numEntries).Select(index => new NetworkMarkerInfo(mapIconArray[index], levelArray[index], questIdArray[index], flagArray[index]));

    private void LogMarker(NetworkMarkerInfo marker)
    {
        var location = LuminaCache<Level>.Instance.GetRow(marker.LevelRowId);

        PluginLog.Debug($"[{marker.MapIcon, 5}] [{marker.LevelRowId, 7}] [{marker.ObjectiveId, 7}] [{marker.Flags, 4}] - {location?.Territory.Value?.PlaceName.Value?.Name ?? "Null Name"}");
    }
    
    private void ReceiveLevequestArea(nint a1, uint a2)
    {
        Safety.ExecuteSafe(() =>
        {
            PluginLog.Debug("Received Levequest Area");
            
            ActiveLevequestLevels.Add(a2);
        });

        receiveLevequestAreasHook!.Original(a1, a2);
    }
}