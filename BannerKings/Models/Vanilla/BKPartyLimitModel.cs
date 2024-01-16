using BannerKings.Behaviours;
using BannerKings.Behaviours.PartyNeeds;
using BannerKings.Components;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    internal class BKPartyLimitModel : DefaultPartySizeLimitModel
    {
        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            var baseResult = base.GetPartyMemberSizeLimit(party, includeDescriptions);
            if (party.MobileParty == null)
            {
                return baseResult;
            }

            var leader = party.MobileParty.LeaderHero;
            if (leader != null)
            {
                if (leader.IsClanLeader()) baseResult.AddFactor(BannerKingsSettings.Instance.PartySizes - 1f, 
                    new TextObject("{=mSLQa207}Party Size Scaling"));
                else baseResult.AddFactor((BannerKingsSettings.Instance.PartySizes -1f) * 0.5f, 
                    new TextObject("{=mSLQa207}Party Size Scaling"));

                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (data.Perks.Contains(BKPerks.Instance.AugustCommander))
                {
                    baseResult.Add(5f, BKPerks.Instance.AugustCommander.Name);
                }

                if (data.Perks.Contains(BKPerks.Instance.CommanderLogistician))
                {
                    baseResult.Add(5f, BKPerks.Instance.CommanderLogistician.Name);
                }

                if (data.Perks.Contains(BKPerks.Instance.CommanderWarband))
                {
                    baseResult.AddFactor(0.08f, BKPerks.Instance.CommanderWarband.Name);
                }

                if (leader.Clan == Clan.PlayerClan && TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>().HasDebuff(DefaultStartOptions.Instance.Gladiator))
                {
                    baseResult.AddFactor(-0.4f, DefaultStartOptions.Instance.Gladiator.Name);
                }

                if (data.Lifestyle != null)
                {
                    if (data.Lifestyle.Equals(DefaultLifestyles.Instance.CivilAdministrator))
                    {
                        baseResult.AddFactor(-0.15f, DefaultLifestyles.Instance.CivilAdministrator.Name);
                    }

                    if (data.Lifestyle.Equals(DefaultLifestyles.Instance.Kheshig))
                    {
                        baseResult.AddFactor(0.15f, DefaultLifestyles.Instance.Kheshig.Name);
                    }
                }

                if (party.MobileParty.IsBandit && party.MobileParty.PartyComponent is BanditHeroComponent)
                {
                    baseResult.Add(150f, new TextObject("{=C0MCMXZ1}Bandit horde"));
                    baseResult.Add(party.MobileParty.LeaderHero.GetSkillValue(DefaultSkills.Roguery) * 1.5f,
                        DefaultSkills.Roguery.Name);
                }

                PartySupplies supplies = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKPartyNeedsBehavior>().GetPartySupplies(party.MobileParty);
                if (supplies != null)
                {
                    if (party.MobileParty.MemberRoster.TotalManCount > supplies.MinimumSoldiersThreshold)
                    {
                        baseResult.Add(-supplies.WeaponsNeed / 1f, new TextObject("{=7Y1M7b0R}Lacking weapon supplies"));
                        baseResult.Add(-supplies.ArrowsNeed / 1f, new TextObject("{=2Luts26h}Lacking ammunition supplies"));
                        baseResult.Add(-supplies.HorsesNeed / 1f, new TextObject("{=Ps0ugfFQ}Lacking mount supplies"));
                        baseResult.Add(-supplies.ShieldsNeed / 1f, new TextObject("{=ut6PVJ40}Lacking shield supplies"));
                    }
                }

                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);
                if (title != null)
                {
                    float type = (float)title.TitleType + 1;
                    baseResult.AddFactor(0.4f / type, new TextObject("{=!}Highest title of rank {RANK}")
                        .SetTextVariable("RANK", DefaultTitleNames.Instance.GetTitleName(leader.Culture, title.TitleType).Name));
                }
            }

            if (party.MobileParty.PartyComponent != null && party.MobileParty.PartyComponent is PopulationPartyComponent)
                baseResult.Add(50f);

            return baseResult;
        }

        public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            return base.GetPartyPrisonerSizeLimit(party, includeDescriptions);
        }

        public override int GetTierPartySizeEffect(int tier)
        {
            return base.GetTierPartySizeEffect(tier);
        }
    }
}