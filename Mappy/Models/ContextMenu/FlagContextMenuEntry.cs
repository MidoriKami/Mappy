using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Mappy.Abstracts;
using Mappy.System;
using Mappy.System.Localization;

namespace Mappy.Models.ContextMenu;

public unsafe class FlagContextMenuEntry : IContextMenuEntry
{
    public ContextMenuType[] MenuTypes => new[]
    {
        ContextMenuType.General
    };
    
    public bool Visible => AgentMap.Instance() is not null;

    public string Label => AgentMap.Instance()->IsFlagMarkerSet is 0 ? Strings.AddFlag : Strings.MoveFlag;
    
    public void ClickAction(Vector2 clickPosition)
    {
        if(MappySystem.MapTextureController is {Ready: true, CurrentMap: var map})
        {
            var agent = AgentMap.Instance();
            agent->IsFlagMarkerSet = 0;
            agent->SetFlagMapMarker(map.TerritoryType.Row, map.RowId, clickPosition.X, clickPosition.Y);

            if (MappySystem.SystemConfig.InsertFlagInChat)
            {
                AgentChatLog.Instance()->InsertTextCommandParam(1048, false);
            }
        }
    }
}