using BannerKings.Behaviours;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyMoraleModel : DefaultPartyMoraleModel
    {
        public override ExplainedNumber GetEffectivePartyMorale(MobileParty mobileParty, bool includeDescription = false)
        {
            var result = base.GetEffectivePartyMorale(mobileParty, includeDescription);

            if (mobileParty.Owner == Hero.MainHero && Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>()
                    .HasDebuff(DefaultStartOptions.Instance.Mercenary))
            {
                result.Add(-20f, DefaultStartOptions.Instance.Mercenary.Name);
            }

            if (mobileParty.LeaderHero != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.LeaderHero);
                if (data.Perks.Contains(BKPerks.Instance.AugustCommander))
                {
                    result.Add(3f, BKPerks.Instance.AugustCommander.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(mobileParty.LeaderHero, 
                    DefaultDivinities.Instance.DarusosianSecondary2))
                {
                    result.Add(5f, DefaultDivinities.Instance.DarusosianSecondary2.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(mobileParty.LeaderHero,
                    DefaultDivinities.Instance.VlandiaSecondary2))
                {
                    float vlandians = 0;

                    foreach (var element in mobileParty.MemberRoster.GetTroopRoster())
                    {
                        if (element.Character.Culture != null && element.Character.Culture.StringId == "vlandia")
                        {
                            vlandians += element.Number;
                        }
                    }

                    result.Add(10f * (vlandians / (float)mobileParty.MemberRoster.Count), 
                        DefaultDivinities.Instance.VlandiaSecondary2.Name);
                }

                if (data.Lifestyle != null && data.Lifestyle.Equals(DefaultLifestyles.Instance.Kheshig))
                {
                    float nonKhuzaits = 0;

                    foreach (var element in mobileParty.MemberRoster.GetTroopRoster())
                    {
                        if (element.Character.Culture != null && element.Character.Culture.StringId != "khuzait")
                        {
                            nonKhuzaits += element.Number;
                        }
                    }

                    result.Add(nonKhuzaits * -0.05f, DefaultLifestyles.Instance.Kheshig.Name);
                }
            }

            return result;
        }
    }
}