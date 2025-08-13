using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.SelectionWindowComponents;

public abstract class DrawableOption
{
    protected virtual string[] GetAdditionalFilterStrings() => [];

    public virtual Map Map { get; set; }

    protected static float Width => 133.5f * ImGuiHelpers.GlobalScale;

    protected static float Height => 75.0f * ImGuiHelpers.GlobalScale;

    protected abstract void DrawIcon();

    public virtual string ExtraLineLong => string.Empty;

    public virtual string ExtraLineShort => string.Empty;

    public virtual Vector2? MarkerLocation => null;

    public virtual string GetElementKey() => $"{Map.RowId}{MarkerLocation}{ExtraLineShort}{ExtraLineLong}";

    public string[] GetFilterStrings()
    {
        if (Map.RowId is 0) return [];

        var baseStrings = new[]
        {
            Map.PlaceNameRegion.ValueNullable?.Name.ExtractText() ?? string.Empty, Map.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty,
            Map.PlaceNameSub.ValueNullable?.Name.ExtractText() ?? string.Empty, Map.TerritoryType.ValueNullable?.Name.ExtractText() ?? string.Empty, Map.Id.ExtractText(),
        };

        return baseStrings.Concat(GetAdditionalFilterStrings()).ToArray();
    }

    public void Draw()
    {
        using var id = ImRaii.PushId(Map.RowId.ToString());

        DrawIcon();
        ImGui.SameLine();

        using var contentsFrame = ImRaii.Child($"contents_frame#{GetElementKey()}", new Vector2(ImGui.GetContentRegionAvail().X, Height), false, ImGuiWindowFlags.NoInputs);
        if (!contentsFrame) return;

        ImGuiHelpers.ScaledDummy(1.0f);

        using var table = ImRaii.Table("data_table", 2, ImGuiTableFlags.SizingStretchProp);
        if (!table) return;

        ImGui.TableSetupColumn("##column1", ImGuiTableColumnFlags.None, 2.0f);
        ImGui.TableSetupColumn("##column2", ImGuiTableColumnFlags.None, 1.0f);

        var placeName = Map.PlaceName.ValueNullable?.Name.ExtractText() ?? string.Empty;
        var zoneName = Map.PlaceNameSub.ValueNullable?.Name.ExtractText() ?? string.Empty;
        var regionName = Map.PlaceNameRegion.ValueNullable?.Name.ExtractText() ?? string.Empty;

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(placeName);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(Map.RowId.ToString());

        ImGui.TableNextRow();
        ImGui.TableNextColumn();

        using var grayColor = ImRaii.PushColor(ImGuiCol.Text, KnownColor.DarkGray.Vector());
        if (!zoneName.IsNullOrEmpty() && !regionName.IsNullOrEmpty()) {
            ImGui.TextUnformatted($"{regionName}, {zoneName}");
        }
        else if (!zoneName.IsNullOrEmpty()) {
            ImGui.TextUnformatted($"{zoneName}");
        }
        else if (!regionName.IsNullOrEmpty()) {
            ImGui.TextUnformatted($"{regionName}");
        }

        ImGui.TableNextColumn();
        ImGui.TextUnformatted($"{Map.Id}");

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(ExtraLineLong);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(ExtraLineShort);
    }
}