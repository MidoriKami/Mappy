using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;

namespace Mappy.Models;

/// <summary>
/// This container uses a 2-dimensional array to contain Map Markers that contain basic information.
/// If you need more advanced information, use the MapMarkerContainer fields instead if applicable.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe partial struct SimpleMapMarkerContainer
{
    public ulong CurrentSize;
    public nint InternalPointer;
    public SimpleMapMarkerData** DataArray;
    public ulong MaxSize;

    public Span<Pointer<SimpleMapMarkerData>> DataSpan => new(DataArray, (int)CurrentSize);
    
    public IEnumerable<SimpleMapMarkerData> GetEnumerable()
    {
        var results = new List<SimpleMapMarkerData>();

        foreach (var index in Enumerable.Range(0, (int) CurrentSize))
        {
            results.Add(*DataArray[index]);
        }

        return results;
    }
}

/// <summary>
/// This container uses a linked list internally to contain Map Markers that contain tooltip information.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe partial struct MapMarkerContainer
{
    public LinkedList* List;
    public int Size;

    public IEnumerable<MarkerInfo> GetAllMarkers()
    {
        var result = new List<MarkerInfo>();
        var current = List->First;
        
        foreach(var _ in Enumerable.Range(0, Size))
        {
            result.Add(current->Data);
            current = current->Next;
        }

        return result;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe partial struct LinkedList
{
    public Node* First;
    public Node* Last;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe partial struct Node
{
    public Node* Next;
    public Node* Previous;

    public MarkerInfo Data;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct ClientStructsMapData
{
    [FieldOffset(0x90)] public fixed byte QuestData[0x90 * 30];
    [FieldOffset(0x1170)] public fixed byte LevequestData[0x90 * 16];
    [FieldOffset(0x1AE8)] public StdVector<MapMarkerData> ActiveLevequestMarkerData;
    [FieldOffset(0x1B10)] public MapMarkerContainer QuestMarkerData;
    [FieldOffset(0x1B18)] public SimpleMapMarkerContainer SimpleQuestMarkerData;
    [FieldOffset(0x1B58)] public MapMarkerContainer GuildLeveAssignmentMapMarkerData;
    [FieldOffset(0x1BA0)] public MapMarkerContainer GuildOrderGuideMarkerData;
    [FieldOffset(0x3E90)] public MapMarkerContainer TripleTriadMarkerData;
    [FieldOffset(0x3EA0)] public MapMarkerContainer CustomTalkMarkerData;
    [FieldOffset(0x3EA8)] public SimpleMapMarkerContainer SimpleCustomTalkMarkerData;
    [FieldOffset(0x3F48)] public MapMarkerContainer GemstoneTraderMarkerData;
    [FieldOffset(0x3F50)] public SimpleMapMarkerContainer SimpleGemstoneTraderMarkerData;

    public Span<MarkerInfo> QuestDataSpan => new(Unsafe.AsPointer(ref QuestData[0]), QuestManager.Instance()->NumAcceptedQuests);

    public Span<MarkerInfo> LevequestDataSpan => new(Unsafe.AsPointer(ref LevequestData[0]), QuestManager.Instance()->NumAcceptedLeveQuests);
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe partial struct SimpleMapMarkerData
{
    [FieldOffset(0x00)] public uint IconId;
    [FieldOffset(0x04)] public uint LevelId; // RowId into the 'Level' sheet
    [FieldOffset(0x08)] public uint ObjectiveId; // RowId for whichever type of data this specific marker is representing, QuestId in the case of quests
    [FieldOffset(0x0C)] public int Flags;
}

[StructLayout(LayoutKind.Explicit, Size = 0x90)]
public unsafe partial struct MarkerInfo
{
    [FieldOffset(0x04)] public uint ObjectiveId;
    [FieldOffset(0x08)] public Utf8String Label;
    [FieldOffset(0x70)] public StdVector<MapMarkerData> MarkerData;
    [FieldOffset(0x8B)] public byte ShouldRender; // Only valid for Accepted Quests and Accepted Levequests
    [FieldOffset(0x88)] public ushort RecommendedLevel; // Only valid for Accepted Quests and Accepted Levequests
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public unsafe partial struct MapMarkerData
{
    [FieldOffset(0x00)] public uint LevelId;
    [FieldOffset(0x04)] public uint ObjectiveId;
    [FieldOffset(0x08)] public Utf8String* TooltipString;
    [FieldOffset(0x10)] public uint IconId;
    [FieldOffset(0x3C)] public ushort RecommendedLevel;
}

// [StructLayout(LayoutKind.Explicit, Size = 0x90)]
// public struct QuestInfo
// {
//     [FieldOffset(0x04)] public uint QuestID;
//     [FieldOffset(0x08)] public Utf8String Name;
//     [FieldOffset(0x70)] public StdVector<QuestMarkerInfo> MarkerData;
//     [FieldOffset(0x88)] public ushort RecommendedLevel;
//     [FieldOffset(0x8B)] public byte ShouldRender;
// }
//
// [StructLayout(LayoutKind.Explicit, Size = 0x48)]
// public unsafe partial struct QuestMarkerInfo
// {
//     [FieldOffset(0x00)] public uint LevelId;
//     [FieldOffset(0x08)] public Utf8String* Tooltip;
//     [FieldOffset(0x10)] public uint IconId;
// }