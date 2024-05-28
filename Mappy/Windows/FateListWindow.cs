using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ImGuiNET;
using KamiLib.Window;

namespace Mappy.Windows;

public class FateListWindow : Window {
    private const float ElementHeight = 48.0f;
    
    public FateListWindow() : base("Mappy Fate List Window", new Vector2(300.0f, 400.0f)) {
        AdditionalInfoTooltip = "Shows Fates for the zone you are currently in";
    }

    protected override unsafe void DrawContents() {
        if (Service.FateTable.Length > 0) {
            foreach (var index in Enumerable.Range(0, Service.FateTable.Length)) {
                var fate = FateManager.Instance()->Fates.Span[index].Value;
                
                var cursorStart = ImGui.GetCursorScreenPos();
                if (ImGui.Selectable($"##{fate->FateId}_Selectable", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, ElementHeight * ImGuiHelpers.GlobalScale))) {
                    AgentMap.Instance()->OpenMap(AgentMap.Instance()->CurrentMapId);
                    System.SystemConfig.FollowPlayer = false;
                    System.MapRenderer.DrawOffset = -new Vector2(fate->Location.X, fate->Location.Z);
                }

                ImGui.SetCursorScreenPos(cursorStart);
                if (Service.TextureProvider.GetIcon(fate->IconId) is { } icon) {
                    ImGui.Image(icon.ImGuiHandle, ImGuiHelpers.ScaledVector2(ElementHeight, ElementHeight));
                        
                    ImGui.SameLine();
                    var text = $"Lv. {fate->Level} {fate->Name}";
                        
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ElementHeight * ImGuiHelpers.GlobalScale / 2.0f - ImGui.CalcTextSize(text).Y / 2.0f);
                    ImGui.Text(text);
                }
                
            }
        }
        else {
            const string text = "No FATE's available";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.SetCursorPosY(ImGui.GetContentRegionAvail().Y / 2.0f - textSize.Y / 2.0f);
            ImGui.TextColored(KnownColor.Orange.Vector(), text);
        }
    }
}