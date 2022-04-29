﻿using TaleWorlds.CampaignSystem;
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
