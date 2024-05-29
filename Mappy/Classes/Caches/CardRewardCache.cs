using System.Linq;
using KamiLib.Classes;
using Lumina.Excel.GeneratedSheets;

namespace Mappy.Classes.Caches;

public class CardRewardCache : Cache<uint, string>{
    protected override string LoadValue(uint key) {
        if (Service.DataManager.GetExcelSheet<TripleTriad>()!.GetRow(key) is { } triadInfo) {
            var cardRewards = triadInfo.ItemPossibleReward
                .Where(reward => reward.Row is not 0)
                .Select(reward => reward.Value)
                .OfType<Item>()
                .Select(item => item.Name.RawString);

            return string.Join("\n", cardRewards);
        }

        return string.Empty;
    }
}