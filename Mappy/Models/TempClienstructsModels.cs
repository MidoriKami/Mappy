using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.Interop;

namespace Mappy.Models;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct StdVector<T> where T : unmanaged
{
    public T* First;
    public T* Last;
    public T* End;

    public ReadOnlySpan<T> Span
    {
        get
        {
            var size = Size();
            if (size >= 0x7FEFFFFF)
                throw new IndexOutOfRangeException($"Size exceeds max. Array index. (Size={size})");
            return new ReadOnlySpan<T>(First, (int) size);
        }
    }

    public ulong Size()
    {
        if (First == null || Last == null)
            return 0;

        return ((ulong) Last - (ulong) First) / (ulong) sizeof(T);
    }

    public ulong Capacity()
    {
        if (End == null || First == null)
            return 0;

        return ((ulong) End - (ulong) First) / (ulong) sizeof(T);
    }

    public T Get(ulong index)
    {
        if (index >= Size())
            throw new IndexOutOfRangeException($"Index out of Range: {index}");

        return First[index];
    }
}
    
[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct ClientStructsMapData
{
    [FieldOffset(0x90)] public MapMarkerInfo QuestData;
    [FieldOffset(0x1170)] public MapMarkerInfo LevequestData;
    [FieldOffset(0x1AE8)] public StdVector<QuestMarkerInfo> LevequestMarkerData;
    [FieldOffset(0x1B20)] public MapMarkerDataContainer QuestMarkerData;
    [FieldOffset(0x1BA0)] public SpecialMarkerContainer GuildOrderGuideMarkerData;
    [FieldOffset(0x1B68)] public MapMarkerDataContainer GuildLeveAssignmentMarkerData;
    [FieldOffset(0x3EB0)] public MapMarkerDataContainer CustomTalkMarkerData;
    [FieldOffset(0x3E90)] public SpecialMarkerContainer TripleTriadMarkerData;

    public Span<MapMarkerInfo> QuestDataSpan
    {
        get
        {
            fixed (MapMarkerInfo* pointer = &QuestData)
            {
                return new Span<MapMarkerInfo>(pointer, 30);
            }
        }
    }

    public Span<MapMarkerInfo> LevequestDataSpan
    {
        get
        {
            fixed (MapMarkerInfo* pointer = &LevequestData)
            {
                return new Span<MapMarkerInfo>(pointer, 30);
            }
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct MapMarkerDataContainer
{
    [FieldOffset(0x08)] public MapMarkerData** MarkerData;
    [FieldOffset(0x10)] public int MarkerCount;

    public Span<Pointer<MapMarkerData>> MarkerDataSpan => new(MarkerData, MarkerCount);
}
    
[StructLayout(LayoutKind.Explicit, Size = 0x10)]
public unsafe partial struct MapMarkerData
{
    [FieldOffset(0x00)] public uint IconId;
    [FieldOffset(0x04)] public uint LevelId; // RowId into the 'Level' sheet
    [FieldOffset(0x08)] public uint ObjectiveId; // RowId for whichever type of data this specific marker is representing, QuestId in the case of quests
    [FieldOffset(0x0C)] public int Flags;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct SpecialMarkerContainer
{
    [FieldOffset(0x00)] public SpecialMarker** GuildOrderGuideData;
    [FieldOffset(0x08)] public int DataCount;
        
    public Span<Pointer<SpecialMarker>> DataSpan => new(GuildOrderGuideData, DataCount);
}

[StructLayout(LayoutKind.Explicit)]
public unsafe partial struct SpecialMarker
{
    [FieldOffset(0x14)] public uint ObjectiveId;
    [FieldOffset(0x18)] public Utf8String Tooltip;
    [FieldOffset(0x80)] public SpecialMarkerData* MarkerData;
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public unsafe partial struct SpecialMarkerData
{
    [FieldOffset(0x00)] public uint LevelId;
    [FieldOffset(0x04)] public uint ObjectiveId;
    [FieldOffset(0x10)] public uint IconId;
}

[StructLayout(LayoutKind.Explicit, Size = 0x48)]
public unsafe partial struct QuestMarkerInfo
{
    [FieldOffset(0x00)] public uint LevelId;
    [FieldOffset(0x08)] public Utf8String* Tooltip; // Yes this is a pointer, to a tooltip
    [FieldOffset(0x10)] public uint IconId;
}

[StructLayout(LayoutKind.Explicit, Size = 0x90)]
public struct MapMarkerInfo
{
    [FieldOffset(0x04)] public uint QuestID;
    [FieldOffset(0x08)] public Utf8String Name;
    [FieldOffset(0x70)] public StdVector<QuestMarkerInfo> MarkerData;
    [FieldOffset(0x8B)] public byte ShouldRender;
    [FieldOffset(0x88)] public ushort RecommendedLevel;
}