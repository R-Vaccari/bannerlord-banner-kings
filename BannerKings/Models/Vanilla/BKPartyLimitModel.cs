using BannerKings.Behaviours;
using BannerKings.Components;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
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
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (data.Perks.Contains(BKPerks.Instance.AugustCommander))
                {
                    baseResult.Add(5f, BKPerks.Instance.AugustCommander.Name);
                }

                if (leader.Clan == Clan.PlayerClan && Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>().HasDebuff(DefaultStartOptions.Instance.Gladiator))
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
            }

            if (BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
            {
                if (party.MobileParty.PartyComponent is PopulationPartyComponent)
                {
                    baseResult.Add(50f);
                }
            }

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