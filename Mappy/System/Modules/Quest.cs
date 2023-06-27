using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.AutomaticUserInterface;
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
    [BoolConfigOption("HideUnacceptedQuests", "ModuleConfig", 1)]
    public bool HideUnacceptedQuests = false;

    [BoolConfigOption("HideAcceptedQuests", "ModuleConfig", 1)]
    public bool HideAcceptedQuests = false;

    [BoolConfigOption("HideLeveQuests", "ModuleConfig", 1)]
    public bool HideLeveQuests = false;

    [ColorConfigOption("InProgressColor", "ModuleColors", 2, 255, 69, 0, 45)]
    public Vector4 InProgressColor = KnownColor.OrangeRed.AsVector4() with { W = 0.33f };
    
    [ColorConfigOption("LeveQuestColor", "ModuleColors", 2, 0, 133, 5, 97)]
    public Vector4 LeveQuestColor = new Vector4(0, 133, 5, 97) / 255.0f;
}

public unsafe class Quest : ModuleBase
{
    private record AllowedQuestInfo(uint MapIcon, uint Level, uint QuestId, byte Flags);
    
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new QuestConfig();
    private delegate nint ReceiveMarkersDelegate(nint a1, nint a2, nint a3, nint a4, int a5);
    private delegate void ReceiveLevequestAreasDelegate(nint a1, uint a2);
    
    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 0F B6 43 10 4C 8D 4B 44", DetourName = nameof(ReceiveMarkers))]
    private readonly Hook<ReceiveMarkersDelegate>? receiveMarkersHook = null;

    [Signature("40 56 41 56 48 81 EC ?? ?? ?? ?? 48 8B F1", DetourName = nameof(ReceiveLevequestArea))]
    private readonly Hook<ReceiveLevequestAreasDelegate>? receiveLevequestAreasHook = null;
    
    private readonly ConcurrentDictionary<uint, AllowedQuestInfo> allowedQuests = new();
    private readonly HashSet<uint> activeLevequestLevels = new();

    public Quest()
    {
        SignatureHelper.Initialise(this);
        receiveMarkersHook?.Enable();
        receiveLevequestAreasHook?.Enable();
    }
    
    public override void Unload()
    {
        receiveMarkersHook?.Dispose();
        receiveLevequestAreasHook?.Disable();
        base.Unload();
    }
    
    private nint ReceiveMarkers(nint questMapIconIdArray, nint eventHandlerValueArray, nint questIdArray, nint unknownArray, int numEntries)
    {
        Safety.ExecuteSafe(() =>
        {
            // PluginLog.Debug($"Received QuestMarkers: {numEntries}");
            
            foreach(var index in Enumerable.Range(0, numEntries))
            {
                var markerId = ((uint*) questMapIconIdArray)[index];
                var levelRowId = ((uint*) eventHandlerValueArray)[index];
                var questId = ((uint*) questIdArray)[index];
                var flags = ((byte*) unknownArray)[index];

                allowedQuests.TryRemove(questId, out _);
                allowedQuests.TryAdd(questId, new AllowedQuestInfo(markerId, levelRowId, questId, flags));

                // var location = LuminaCache<Level>.Instance.GetRow(levelRowId)!;

                // PluginLog.Debug($"[{markerId, 5}] [{levelRowId, 7}] [{questId, 7}] [{flags, 4}] - {location.Territory.Value?.PlaceName.Value?.Name ?? "Null Name"}");
            }
        });

        return receiveMarkersHook!.Original(questMapIconIdArray, eventHandlerValueArray, questIdArray, unknownArray, numEntries);
    }
    
    private void ReceiveLevequestArea(nint a1, uint a2)
    {
        Safety.ExecuteSafe(() =>
        {
            activeLevequestLevels.Add(a2);
        });

        receiveLevequestAreasHook!.Original(a1, a2);
    }
    
    public override void ZoneChanged(uint territoryType) => allowedQuests.Clear();

    public override void LoadForMap(MapData mapData) { }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<QuestConfig>();

        if (!config.HideUnacceptedQuests) DrawUnacceptedQuests(map);
        if (!config.HideAcceptedQuests) DrawAcceptedQuests(viewport, map);
        if (!config.HideLeveQuests) DrawLeveQuests(viewport, map);
    }
    
    private void DrawLeveQuests(Viewport viewport, Map map)
    {
        var anyActive = false;
        
        foreach (var quest in QuestManager.Instance()->LeveQuestsSpan)
        {
            if (quest.LeveId is 0) continue;
            if (quest.Flags is 44) continue; // Complete
            
            var luminaData = LuminaCache<Leve>.Instance.GetRow(quest.LeveId)!;
            var level = LuminaCache<Level>.Instance.GetRow(luminaData.LevelStart.Row)!;
            var journalGenre = LuminaCache<JournalGenre>.Instance.GetRow(luminaData.JournalGenre.Row)!;
            if (level.Map.Row != map.RowId) continue;
            
            var icon = (uint) journalGenre.Icon;
            var tooltip = luminaData.Name.RawString;
            var ringColor = GetConfig<QuestConfig>().LeveQuestColor;
            var tooltipColor = KnownColor.White.AsVector4();
            var scale = GetConfig<QuestConfig>().IconScale;
                
            DrawUtilities.DrawLevelObjective(level, icon, tooltip, ringColor, tooltipColor, viewport, map, scale, 50.0f);

            if (quest.Flags is not 32) continue; // InProgress
            anyActive = true;
            foreach (var activeLevel in activeLevequestLevels)
            {
                var activeLevelData = LuminaCache<Level>.Instance.GetRow(activeLevel);
                if (activeLevelData is null) continue;

                DrawUtilities.DrawLevelObjective(activeLevelData, icon, tooltip, ringColor, tooltipColor, viewport, map, scale, 50.0f);
            }
        }

        if (!anyActive && activeLevequestLevels.Count > 0)
        {
            activeLevequestLevels.Clear();
        }
    }

    private void DrawUnacceptedQuests(Map map)
    {
        foreach (var (_ ,(mapIcon, level, questId, flags)) in allowedQuests)
        {
            if(flags is 6) continue;
            
            var levelInfo = LuminaCache<Level>.Instance.GetRow(level)!;
            if(levelInfo.Map.Row != map.RowId) continue;
            
            var position = Position.GetObjectPosition(new Vector2(levelInfo.X, levelInfo.Z), map);

            DrawUtilities.DrawIcon(mapIcon, position);

            switch (questId)
            {
                case > 0x10000 and < 0x20000: // Quest
                    var questInfo = LuminaCache<CustomQuestSheet>.Instance.GetRow(questId)!;
                    DrawUtilities.DrawTooltip($"Lv. {questInfo.ClassJobLevel0} {questInfo.Name.RawString}", KnownColor.White.AsVector4());
                    break;
                
                case > 0x60000 and < 0x70000: // Levequest Icon (vendor not active task)
                    var leveInfo = LuminaCache<GuildleveAssignment>.Instance.GetRow(questId)!;
                    DrawUtilities.DrawTooltip($"{leveInfo.Type.RawString}", KnownColor.White.AsVector4());
                    break;
            }
        }
    }

    private void DrawAcceptedQuests(Viewport viewport, Map map)
    {
        foreach (var quest in QuestManager.Instance()->NormalQuestsSpan)
        {
            if (quest.QuestId is 0) continue;

            foreach (var level in GetActiveLevelsForQuest(quest, map.RowId))
            {
                var luminaQuest = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u)!;
                var journalIcon = luminaQuest.JournalGenre.Value?.Icon;
                if (journalIcon is null) continue;

                var icon = (uint) journalIcon;
                var tooltip = luminaQuest.Name.RawString;
                var ringColor = GetConfig<QuestConfig>().InProgressColor;
                var tooltipColor = KnownColor.White.AsVector4();
                var scale = GetConfig<QuestConfig>().IconScale;
                
                DrawUtilities.DrawLevelObjective(level, icon, tooltip, ringColor, tooltipColor, viewport, map, scale);
            }
        }
    }

    public static IEnumerable<Level>? GetActiveLevelsForQuest(string questName, uint mapId) => 
        (from quest in GetAcceptedQuests()
            let luminaData = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u)!
            where luminaData.Name.ToDalamudString().TextValue == questName
            select GetActiveLevelsForQuest(quest, mapId))
        .FirstOrDefault();

    private static IEnumerable<QuestWork> GetAcceptedQuests()
    {
        var list = new List<QuestWork>();
        
        foreach (var quest in QuestManager.Instance()->NormalQuestsSpan)
        {
            if (quest is { IsHidden: false, QuestId: > 0 })
            {
                list.Add(quest);
            }
        }
    }
}