using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.Components;
using KamiLib.Window;
using Map = FFXIVClientStructs.FFXIV.Client.Game.UI.Map;
using Quest = Lumina.Excel.GeneratedSheets.Quest;

namespace Mappy.Windows;

public class QuestListWindow : Window {
    private readonly TabBar tabBar = new("questListTabBar", [
        new AcceptedQuestsTabItem(),
        new UnacceptedQuestsTabItem(),
    ]);
    
    public QuestListWindow() : base("Mappy Quest List Window", new Vector2(300.0f, 500.0f)) {
        AdditionalInfoTooltip = "Shows Quests for the zone you are currently in";
    }
    
    protected override void DrawContents() {
        using var child = ImRaii.Child("quest_list_scrollable", ImGui.GetContentRegionAvail());
        if (!child) return;

        tabBar.Draw();
    }

    public override void OnClose() {
        System.WindowManager.RemoveWindow(this);
    }
}

public unsafe class UnacceptedQuestsTabItem : ITabItem {
    private const float ElementHeight = 48.0f;

    public string Name => "Unaccepted Quests";
    
    public bool Disabled => false;
    
    public void Draw() {
        if (Map.Instance()->UnacceptedQuestMarkers.Count > 0) {
            foreach (var quest in Map.Instance()->UnacceptedQuestMarkers) {
                var questData = Service.DataManager.GetExcelSheet<Quest>()!.GetRow(quest.ObjectiveId + 65536u);
                
                foreach (var marker in quest.MarkerData) {
                    var cursorStart = ImGui.GetCursorScreenPos();
                    if (ImGui.Selectable($"##{quest.ObjectiveId}_Selectable_{marker.LevelId}", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, ElementHeight * ImGuiHelpers.GlobalScale))) {
                        System.IntegrationsController.OpenMap(marker.MapId);
                        System.SystemConfig.FollowPlayer = false;
                        System.MapRenderer.DrawOffset = -new Vector2(marker.X, marker.Z);
                    }

                    ImGui.SetCursorScreenPos(cursorStart);
                    ImGui.Image(Service.TextureProvider.GetFromGameIcon(marker.IconId).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(ElementHeight, ElementHeight));
                    
                    ImGui.SameLine();
                    var text = $"Lv. {questData?.ClassJobLevel0} {quest.Label}";
                    
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ElementHeight * ImGuiHelpers.GlobalScale / 2.0f - ImGui.CalcTextSize(text).Y / 2.0f);
                    ImGui.Text(text);
                }
            }
        }
        else {
            const string text = "No quests available";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f - textSize.Y / 2.0f);
            ImGui.TextColored(KnownColor.Orange.Vector(), text);
        }
    }
}

public unsafe class AcceptedQuestsTabItem : ITabItem {
    private const float ElementHeight = 48.0f;

    public string Name => "Accepted Quests";
    
    public bool Disabled => false;
    
    public void Draw() {
        if (Map.Instance()->QuestMarkers.Length > 0) {
            foreach (var quest in Map.Instance()->QuestMarkers) {
                if (quest.ObjectiveId is 0) continue;
                
                var questData = Service.DataManager.GetExcelSheet<Quest>()!.GetRow(quest.ObjectiveId + 65536u);
                ImGui.Text($"Lv. {questData?.ClassJobLevel0} {quest.Label}");
                
                var index = 0;
                foreach (var marker in quest.MarkerData) {
                    using var indent = ImRaii.PushIndent();

                    var cursorStart = ImGui.GetCursorScreenPos();
                    if (ImGui.Selectable($"##{quest.ObjectiveId}_Selectable_{marker.LevelId}_{index++}", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, ElementHeight * ImGuiHelpers.GlobalScale))) {
                        System.IntegrationsController.OpenMap(marker.MapId);
                        System.SystemConfig.FollowPlayer = false;
                        System.MapRenderer.DrawOffset = -new Vector2(marker.X, marker.Z);
                    }

                    var iconId = marker.IconId switch {
                        >= 60483 and <= 60494 => 60071u,
                        _ => marker.IconId,
                    };
                    
                    ImGui.SetCursorScreenPos(cursorStart);
                    ImGui.Image(Service.TextureProvider.GetFromGameIcon(iconId).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(ElementHeight, ElementHeight));
                    
                    ImGui.SameLine();
                    var text = $"Lv. {questData?.ClassJobLevel0} {quest.Label}";
                    
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ElementHeight * ImGuiHelpers.GlobalScale / 2.0f - ImGui.CalcTextSize(text).Y / 2.0f);
                    ImGui.Text(text);
                }
            }
        }
        else {
            const string text = "No quests available";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f - textSize.Y / 2.0f);
            ImGui.TextColored(KnownColor.Orange.Vector(), text);
        }
    }
}


