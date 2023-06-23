using System.Collections.Concurrent;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using KamiLib.Caching;
using KamiLib.Hooking;
using KamiLib.Utilities;
using KamiLib.Windows;
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

    [ColorConfigOption("InProgressColor", "ColorConfig", 2, 255, 69, 0, 45)]
    public Vector4 InProgressColor = KnownColor.OrangeRed.AsVector4() with { W = 0.33f };
}

public unsafe class Quest : ModuleBase
{
    private record AllowedQuestInfo(uint MapIcon, uint QuestId);
    
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
                var location = ((uint*) eventHandlerValueArray)[index];
                var questId = ((uint*) questIdArray)[index];
                var unknown = ((byte*) unknownArray)[index];

                allowedQuests.TryRemove(questId, out _);
                allowedQuests.TryAdd(questId, new AllowedQuestInfo(markerId, questId));
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
        var config = GetConfig<QuestConfig>();

        if (!config.HideUnacceptedQuests) DrawUnacceptedQuests(map);

        if (!config.HideAcceptedQuests) DrawAcceptedQuests(viewport, map);
    }
    
    private void DrawUnacceptedQuests(Map map)
    {
        foreach (var (_, quest) in allowedQuests)
        {
            var icon = IconCache.Instance.GetIcon(quest.MapIcon);
            var questInfo = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId);

            if (questInfo?.IssuerLocation.Value is not {} location) continue;
            var position = Position.GetObjectPosition(new Vector2(location.X, location.Z), map);

            DrawUtilities.DrawIcon(icon, position);
            DrawUtilities.DrawTooltip($"Lv. {questInfo.ClassJobLevel0} {questInfo.Name.RawString}", KnownColor.White.AsVector4());
        }
    }

    private void DrawAcceptedQuests(Viewport viewport, Map map)
    {
        foreach (var quest in QuestManager.Instance()->NormalQuestsSpan)
        {
            if (quest.QuestId is 0) continue;
            DebugWindow.Print(quest.QuestId.ToString());
            
            var luminaQuest = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u)!;
            
            var activeLevels = Enumerable.Range(0, 24).Where(index => luminaQuest.ToDoCompleteSeq[index] == quest.Sequence);

            foreach (var index in activeLevels)
            {
                foreach (var levelRow in Enumerable.Range(0, 8))
                {
                    var level = luminaQuest.ToDoLocation[index, levelRow];
                    if (level.Value is null) continue;
                    
                    if(level.Value.Map.Row != AgentMap.Instance()->CurrentMapId) continue;
                    
                    DrawObjective(level.Value, luminaQuest, viewport, map);
                }
            }
        }
    }
    
    private void DrawObjective(Level level, CustomQuestSheet quest, Viewport viewport, Map map)
    {
        var questData = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.RowId)!;
        
        DrawRing(level, viewport, map);
        DrawIcon(level, quest, map);
        
        DrawUtilities.DrawTooltip(questData.Name.ToDalamudString().TextValue, KnownColor.White.AsVector4());
    }
    
    private void DrawIcon(Level level, CustomQuestSheet quest, Map map)
    {
        var position = Position.GetTextureOffsetPosition(new Vector2(level.X, level.Z), map);
        var config = GetConfig<QuestConfig>();
        
        DrawUtilities.DrawIcon(quest.Icon, position, config.IconScale);
    }

    private void DrawRing(Level positionInfo, Viewport viewport, Map map)
    {
        var config = GetConfig<QuestConfig>();
        
        var position = Position.GetTextureOffsetPosition(new Vector2(positionInfo.X, positionInfo.Z), map);
        var drawPosition = viewport.GetImGuiWindowDrawPosition(position);

        var radius = positionInfo.Radius * viewport.Scale / 7.0f;
        var color = ImGui.GetColorU32(config.InProgressColor);
                
        ImGui.BeginGroup();
        ImGui.GetWindowDrawList().AddCircleFilled(drawPosition, radius, color);
        ImGui.GetWindowDrawList().AddCircle(drawPosition, radius, color, 0, 4);
        ImGui.EndGroup();
    }
}