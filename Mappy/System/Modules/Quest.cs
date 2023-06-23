using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.Caching;
using KamiLib.Hooking;
using KamiLib.Utilities;
using Lumina.Excel.GeneratedSheets;
using Mappy.Abstracts;
using Mappy.Models;
using Mappy.Models.Enums;
using Mappy.Utility;

namespace Mappy.System.Modules;

public class QuestConfig : IconModuleConfigBase
{
    
}

public unsafe class Quest : ModuleBase
{
    private record AllowedQuestInfo(uint MapIcon, uint QuestId);
    
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new QuestConfig();

    private delegate nint ReceiveMarkersDelegate(nint a1, nint a2, nint a3, nint a4, nint a5);

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 0F B6 43 10 4C 8D 4B 44", DetourName = nameof(ReceiveMarkers))]
    private readonly Hook<ReceiveMarkersDelegate>? receiveMarkersHook = null;
    
    private readonly ConcurrentBag<AllowedQuestInfo> allowedQuests = new();

    public Quest()
    {
        SignatureHelper.Initialise(this);
        receiveMarkersHook?.Enable();
    }

    public override void Unload()
    {
        receiveMarkersHook?.Dispose();
        base.Unload();
    }
    
    private nint ReceiveMarkers(nint questMapIconIdArray, nint eventHandlerValueArray, nint questIdArray, nint unknownArray, nint numEntries)
    {
        Safety.ExecuteSafe(() =>
        {
            foreach(var index in Enumerable.Range(0, (int)numEntries))
            {
                var markerId = ((uint*) questMapIconIdArray)[index];
                var questId = ((uint*) questIdArray)[index];

                if (!allowedQuests.Any(quest => quest.QuestId == questId))
                {
                    allowedQuests.Add(new AllowedQuestInfo(markerId, questId));
                }
            }
        });

        return receiveMarkersHook!.Original(questMapIconIdArray, eventHandlerValueArray, questIdArray, unknownArray, numEntries);
    }

    protected override bool ShouldDrawMarkers(Map map)
    {
        if (!IsPlayerInCurrentMap(map)) return false;
        
        return base.ShouldDrawMarkers(map);
    }

    public override void ZoneChanged(uint territoryType) => allowedQuests.Clear();

    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        foreach (var quest in allowedQuests)
        {
            var icon = IconCache.Instance.GetIcon(quest.MapIcon);
            var questInfo = LuminaCache<Lumina.Excel.GeneratedSheets.Quest>.Instance.GetRow(quest.QuestId);

            if (questInfo is not { IssuerLocation.Value: var location }) continue;
            var position = Position.GetObjectPosition(new Vector2(location.X, location.Z), map);
            
            DrawUtilities.DrawIcon(icon, position);
            DrawUtilities.DrawTooltip($"Lv. {questInfo.ClassJobLevel0} {questInfo.Name.RawString}", KnownColor.White.AsVector4());
        }
    }
}