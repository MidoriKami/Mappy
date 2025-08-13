using System.Linq;
using KamiLib.Classes;
using Lumina.Excel.Sheets;

namespace Mappy.Classes.Caches;

public class CardRewardCache : Cache<uint, string>
{
    protected override string LoadValue(uint key)
    {
        if (Service.DataManager.GetExcelSheet<TripleTriad>().GetRow(key) is { RowId: not 0 } triadInfo) {
            var cardRewards = triadInfo.ItemPossibleReward
                .Where(reward => reward.RowId is not 0)
                .Select(reward => reward.Value)
                .Where(item => item.RowId is not 0)
                .Select(item => item.Name.ExtractText());

            return string.Join("\n", cardRewards);
        }

        return string.Empty;
    }
}