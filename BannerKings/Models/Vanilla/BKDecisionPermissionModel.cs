using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKDecisionPermissionModel : DefaultKingdomDecisionPermissionModel
    {

        public override bool IsPolicyDecisionAllowed(PolicyObject policy)
        {
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
            }
            return base.IsPolicyDecisionAllowed(policy);
        }
    }
}
