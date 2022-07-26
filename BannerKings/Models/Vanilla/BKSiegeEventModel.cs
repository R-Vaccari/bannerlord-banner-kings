using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKSiegeEventModel : DefaultSiegeEventModel
    {

        public override IEnumerable<SiegeEngineType> GetPrebuiltSiegeEnginesOfSettlement(Settlement settlement)
        {
            IEnumerable<SiegeEngineType> baseResult = base.GetPrebuiltSiegeEnginesOfSettlement(settlement);

            return baseResult;
        }
    }
}
