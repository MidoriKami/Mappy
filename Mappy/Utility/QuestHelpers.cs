using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using KamiLib.Game;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Utility;

public unsafe class QuestHelpers
{
    public static IEnumerable<Level>? GetActiveLevelsForQuest(string questName, uint mapId) => 
        (from quest in GetAcceptedQuests()
            let luminaData = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u)!
            where string.Equals(luminaData.Name.RawString, questName, StringComparison.InvariantCultureIgnoreCase)
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

        return list;
    }

    public static IEnumerable<Level> GetActiveLevelsForQuest(QuestWork quest, uint? madId = null)
    {
        var luminaQuest = LuminaCache<CustomQuestSheet>.Instance.GetRow(quest.QuestId + 65536u)!;
        var currentMapId = madId ?? AgentMap.Instance()->CurrentMapId;    
        
        return 
            from index in Enumerable.Range(0, 24).Where(index => luminaQuest.ToDoCompleteSeq[index] == quest.Sequence) // For each of the possible 24 sequence steps, get all active indexes
            from levelRow in Enumerable.Range(0, 8) // Check all 8 sub locations
            select LuminaCache<Level>.Instance.GetRow(luminaQuest.ToDoLocation[index, levelRow].Row) into level // Get each of the 8 levels
            where level?.Map.Row == currentMapId // If this level is for the current map
            select level;
    }
}