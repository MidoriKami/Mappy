using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
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
}

public unsafe class Quest : ModuleBase
{
    private record AllowedQuestInfo(uint MapIcon, uint Level, uint QuestId, byte Flags);
    
    public override ModuleName ModuleName => ModuleName.QuestMarkers;
    public override ModuleConfigBase Configuration { get; protected set; } = new QuestConfig();

    private delegate nint ReceiveMarkersDelegate(nint a1, nint a2, nint a3, nint a4, int a5);

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 0F B6 43 10 4C 8D 4B 44", DetourName = nameof(ReceiveMarkers))]
    private readonly Hook<ReceiveMarkersDelegate>? receiveMarkersHook = null;
    
    private readonly ConcurrentDictionary<uint, AllowedQuestInfo> allowedQuests = new();

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

                allowedQuests.TryRemove(questId, out _);
                allowedQuests.TryAdd(questId, new AllowedQuestInfo(markerId, levelRowId, questId, flags));
            }
        });

        return receiveMarkersHook!.Original(questMapIconIdArray, eventHandlerValueArray, questIdArray, unknownArray, numEntries);
    }
    
    public override void ZoneChanged(uint territoryType) => allowedQuests.Clear();

    public override void LoadForMap(MapData mapData)
    {
        
    }
    
    protected override void DrawMarkers(Viewport viewport, Map map)
    {
        var config = GetConfig<QuestConfig>();

        if (!config.HideUnacceptedQuests) DrawUnacceptedQuests(map);

        if (!config.HideAcceptedQuests) DrawAcceptedQuests(viewport, map);

        if (!config.HideLeveQuests) DrawLeveQuests(viewport, map);
    }
    
    private void DrawLeveQuests(Viewport viewport, Map map)
    {
        foreach (var quest in QuestManager.Instance()->LeveQuestsSpan)
        {
            if(quest.LeveId is 0) continue;
            
            var luminaData = LuminaCache<Leve>.Instance.GetRow(quest.LeveId)!;
            var level = LuminaCache<Level>.Instance.GetRow(luminaData.LevelLevemete.Row)!;
            var journalGenre = LuminaCache<JournalGenre>.Instance.GetRow(luminaData.JournalGenre.Row)!;
            if(level.Map.Row != map.RowId) continue;
            
            var name = luminaData.Name.RawString;
            var position = Position.GetTextureOffsetPosition(new Vector2(level.X, level.Z), map);
            
            DrawCircle(position, 50.0f, new Vector4(104, 43, 176, 45));
            DrawObjective(level, (uint)journalGenre.Icon, name, viewport, map);
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
                case > 0x10000 and < 0x20000:
                    var questInfo = LuminaCache<CustomQuestSheet>.Instance.GetRow(questId)!;
                    DrawUtilities.DrawTooltip($"Lv. {questInfo.ClassJobLevel0} {questInfo.Name.RawString}", KnownColor.White.AsVector4());
                    break;
                
                case > 0x60000 and < 0x70000:
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

                DrawObjective(level, (uint)journalIcon, luminaQuest.Name.RawString, viewport, map);
            }
        }
    }
    
    private void DrawObjective(Level level, uint iconId, string label, Viewport viewport, Map map)
    {
        DrawRing(level, viewport, map);
        DrawIcon(level, iconId, map);
        
        DrawUtilities.DrawTooltip(label, KnownColor.White.AsVector4());
    }
    
    private void DrawIcon(Level level, uint iconId, Map map)
    {
        var position = Position.GetTextureOffsetPosition(new Vector2(level.X, level.Z), map);
        var config = GetConfig<QuestConfig>();

        DrawUtilities.DrawIcon(iconId, position, config.IconScale);
    }

    private void DrawRing(Level positionInfo, Viewport viewport, Map map)
    {
        var config = GetConfig<QuestConfig>();
        
        var position = Position.GetTextureOffsetPosition(new Vector2(positionInfo.X, positionInfo.Z), map);
        var drawPosition = viewport.GetImGuiWindowDrawPosition(position);
        var radius = positionInfo.Radius * viewport.Scale / 7.0f;

        DrawCircle(drawPosition, radius, config.InProgressColor);
    }

    private void DrawCircle(Vector2 position, float radius, Vector4 color)
    {
        var imGuiColor = ImGui.GetColorU32(color);
        
        ImGui.BeginGroup();
        ImGui.GetWindowDrawList().AddCircleFilled(position, radius, imGuiColor);
        ImGui.GetWindowDrawList().AddCircle(position, radius, imGuiColor, 0, 4);
        ImGui.EndGroup();
    }

    public static IEnumerable<Level>? GetActiveLevelsForQuest(string questName, uint mapId)
    {
        return 
            (from quest in GetAcceptedQuests()
            let luminaData = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u)!
            where luminaData.Name.ToDalamudString().TextValue == questName
            select GetActiveLevelsForQuest(quest, mapId))
            .FirstOrDefault();
    }
    
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

        return list;
    }

    private static IEnumerable<Level> GetActiveLevelsForQuest(QuestWork quest, uint? madId = null)
    {
        var luminaQuest = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u)!;
        var currentMapId = madId ?? AgentMap.Instance()->CurrentMapId;    
        
        return 
            from index in Enumerable.Range(0, 24).Where(index => luminaQuest.ToDoCompleteSeq[index] == quest.Sequence) // For each of the possible 24 sequence steps, get all active indexes
            from levelRow in Enumerable.Range(0, 8) // Check all 8 sub locations
            select luminaQuest.ToDoLocation[index, levelRow] into level // Get each of the 8 levels
            where level.Value?.Map.Row == currentMapId // If this level is for the current map
            select level.Value;
    }
}