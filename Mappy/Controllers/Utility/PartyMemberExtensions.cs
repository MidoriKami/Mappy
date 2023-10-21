using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace Mappy.Utility; 

public static class PartyMemberExtensions {
    public static unsafe string GetName(this PartyMember member)
        => MemoryHelper.ReadStringNullTerminated((nint) member.Name);
}