using BannerKings.Behaviours;
using BannerKings.Behaviours.PartyNeeds;
using BannerKings.Components;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
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

                PartySupplies supplies = Campaign.Current.GetCampaignBehavior<BKPartyNeedsBehavior>().GetPartySupplies(party.MobileParty);
                if (supplies != null)
                {
                    if (party.MobileParty.MemberRoster.TotalManCount > supplies.MinimumSoldiersThreshold)
                    {
                        float weapons = MathF.Min(supplies.WeaponsNeed / supplies.GetWeaponsCurrentNeed().ResultNumber,
                            supplies.WeaponsNeed);
                        float minValue = -baseResult.ResultNumber * 0.15f;
                        baseResult.Add(MathF.Clamp(weapons, minValue, 0),
                            new TextObject("{=7Y1M7b0R}Lacking weapon supplies"));

                        float ammo = MathF.Min(supplies.ArrowsNeed / supplies.GetArrowsCurrentNeed().ResultNumber,
                            supplies.ArrowsNeed);
                        baseResult.Add(MathF.Clamp(ammo, minValue, 0), new TextObject("{=2Luts26h}Lacking ammunition supplies"));

                        float mounts = MathF.Min(supplies.HorsesNeed / supplies.GetMountsCurrentNeed().ResultNumber,
                            supplies.HorsesNeed);
                        baseResult.Add(MathF.Clamp(mounts, minValue, 0), new TextObject("{=Ps0ugfFQ}Lacking mount supplies"));

                        float shields = MathF.Min(supplies.ShieldsNeed / supplies.GetShieldsCurrentNeed().ResultNumber,
                            supplies.ShieldsNeed);
                        baseResult.Add(MathF.Clamp(shields, minValue, 0), new TextObject("{=ut6PVJ40}Lacking shield supplies"));
                    }
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