using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;

namespace Mappy.Models;

[StructLayout(LayoutKind.Sequential)]
public unsafe partial struct MapDataContainer<T> where T : unmanaged
{
    public ulong CurrentSize;
    public nint Unused;
    public T** DataMap;
    public ulong MaxSize;

    public Span<Pointer<T>> DataSpan => new(DataMap, (int)CurrentSize);
}
    
[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct ClientStructsMapData
{
    [FieldOffset(0x90)] public fixed byte QuestData[0x90 * 30];
    [FieldOffset(0x1170)] public fixed byte LevequestData[0x90 * 16];
    [FieldOffset(0x1AE8)] public StdVector<QuestMarkerInfo> ActiveLevequestMarkerData;
    [FieldOffset(0x1B18)] public MapDataContainer<StandardMapMarkerData> QuestMarkerData;
    [FieldOffset(0x1BA0)] public NonstandardMapMarkerContainer GuildOrderGuideMarkerData;
    [FieldOffset(0x1B60)] public MapDataContainer<StandardMapMarkerData> GuildLeveAssignmentMarkerData;
    [FieldOffset(0x3EA8)] public MapDataContainer<StandardMapMarkerData> CustomTalkMarkerData;
    [FieldOffset(0x3E90)] public NonstandardMapMarkerContainer TripleTriadMarkerData;
    [FieldOffset(0x3F50)] public MapDataContainer<StandardMapMarkerData> BicolorGemstoneVendorMarkerData;

    public Span<QuestInfo> QuestDataSpan => new(Unsafe.AsPointer(ref QuestData[0]), 30);

    public Span<QuestInfo> LevequestDataSpan => new(Unsafe.AsPointer(ref LevequestData[0]), 16);
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe partial struct StandardMapMarkerData
{
    [FieldOffset(0x00)] public uint IconId;
    [FieldOffset(0x04)] public uint LevelId; // RowId into the 'Level' sheet
    [FieldOffset(0x08)] public uint ObjectiveId; // RowId for whichever type of data this specific marker is representing, QuestId in the case of quests
    [FieldOffset(0x0C)] public int Flags;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct NonstandardMapMarkerContainer
{
    [FieldOffset(0x00)] public NonstandardMarker** GuildOrderGuideData;
    [FieldOffset(0x08)] public int DataCount;
        
    public Span<Pointer<NonstandardMarker>> DataSpan => new(GuildOrderGuideData, DataCount);
}

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct NonstandardMarker
{
    [FieldOffset(0x14)] public uint ObjectiveId;
    [FieldOffset(0x18)] public Utf8String Tooltip;
    [FieldOffset(0x80)] public NonstandardMarkerData* MarkerData;
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public unsafe partial struct NonstandardMarkerData
{
    [FieldOffset(0x00)] public uint LevelId;
    [FieldOffset(0x04)] public uint ObjectiveId;
    [FieldOffset(0x10)] public uint IconId;
}

[StructLayout(LayoutKind.Explicit, Size = 0x90)]
public struct QuestInfo
{
    [FieldOffset(0x04)] public uint QuestID;
    [FieldOffset(0x08)] public Utf8String Name;
    [FieldOffset(0x70)] public StdVector<QuestMarkerInfo> MarkerData;
    [FieldOffset(0x8B)] public byte ShouldRender;
    [FieldOffset(0x88)] public ushort RecommendedLevel;
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public unsafe partial struct QuestMarkerInfo
{
    [FieldOffset(0x00)] public uint LevelId;
    [FieldOffset(0x08)] public Utf8String* Tooltip;
    [FieldOffset(0x10)] public uint IconId;
}