using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using KamiLib;
using Mappy.Models;
using Mappy.System;
using Mappy.Utility;
using Mappy.Views.Windows;

namespace Mappy.Views.General;

public class QuestListView {
    private MappyMapIcon? focusedMarker;
    public bool IsQuestListHovered;

    public void Draw() {
        ImGui.SetCursorPos(new Vector2(0.0f, 38.5f * ImGuiHelpers.GlobalScale));

        if (ImGui.BeginTable("##QuestListTable", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.NoPadInnerX, ImGui.GetContentRegionAvail())) {
            ImGui.TableSetupColumn("##QuestList", ImGuiTableColumnFlags.WidthFixed, 250.0f * ImGuiHelpers.GlobalScale);
            
            ImGui.TableNextColumn();
            DrawBackground();
            ImGuiHelpers.ScaledDummy(10.0f);
            DrawQuests();
            
            ImGui.EndTable();
        }
    }

    public void TryStopAnimation() {
        if (focusedMarker is not null) {
            focusedMarker.Animate = false;
            focusedMarker = null;
        }
    }
    
    private void DrawBackground() {
        var backgroundColor = ImGui.GetColorU32(Vector4.Zero with { W = 0.8f });

        ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + ImGui.GetContentRegionMax(), backgroundColor);
    }
    
    private unsafe void DrawQuests() {
        if (ImGui.BeginChild("##QuestScrollable", ImGui.GetContentRegionAvail())) {
            if (Map.Instance()->QuestMarkerData.Size > 0) {
                var questsData = Map.Instance()->QuestMarkerData.GetAllMarkers()
                    .Where(quest => quest.MarkerData.First is not null)
                    .OrderByDescending(quest => quest.MarkerData.First->IconId)
                    .ThenBy(quest => quest.MarkerData.First->RecommendedLevel)
                    .ThenBy(quest => quest.MarkerData.First->TooltipString->ToString())
                    .GroupBy(quest => quest.MarkerData.First->IconId);
                
                foreach (var questGroup in questsData) {
                    foreach (var quest in questGroup) {
                        foreach (var marker in quest.MarkerData.Span) {
                            if (ImGui.Selectable($"##{quest.ObjectiveId}")) {
                                if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is not { Viewport: var viewport }) continue;
                                if (MappySystem.MapTextureController is not { Ready: true } textureController) continue;

                                MappySystem.SystemConfig.FollowPlayer = false;
                                textureController.MoveMapToPlayer();
                    
                                var objectPosition = Position.GetTexturePosition(new Vector2(marker.X, marker.Z), textureController.CurrentMap);
                                viewport.SetViewportCenter(objectPosition);
                                viewport.SetViewportZoom(1.20f);

                                AnimateMarker(marker);
                            }
            
                            ImGui.SameLine();
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2.0f * ImGuiHelpers.GlobalScale);
                            var icon = Service.TextureProvider.GetIcon(marker.IconId)!;
                            ImGui.Image(icon.ImGuiHandle, ImGuiHelpers.ScaledVector2(24.0f, 24.0f));
            
                            ImGui.SameLine();
                            ImGui.Text($"Lv. {marker.RecommendedLevel} {marker.TooltipString->ToString()}");
            
                            ImGuiHelpers.ScaledDummy(3.0f);
                        }
                    }
                }
            } else {
                const string text = "No quests available";
                var textSize = ImGui.CalcTextSize(text);
                ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
                ImGui.TextColored(KnownColor.Orange.Vector(), text);
            }
        }
        ImGui.EndChild();
        IsQuestListHovered = ImGui.IsItemHovered();
    }
    
    private void AnimateMarker(MapMarkerData location) {
        if (focusedMarker is not null) {
            focusedMarker.Animate = false;
        }
        
        focusedMarker = MappySystem.ModuleController.GetMapMarker((location.ObjectiveId, location.LevelId));
        
        if (focusedMarker is not null) {
            focusedMarker.Animate = true;
            focusedMarker.AnimationRadius = 75.0f;
        }
    }
}