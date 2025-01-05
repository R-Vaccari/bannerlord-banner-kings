using BannerKings.Behaviours;
using BannerKings.Behaviours.PartyNeeds;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyMoraleModel : DefaultPartyMoraleModel
    {

        public override ExplainedNumber GetEffectivePartyMorale(MobileParty mobileParty, bool includeDescription = false)
        {
            var result = base.GetEffectivePartyMorale(mobileParty, includeDescription);

            if (mobileParty.IsLordParty && mobileParty.Owner == Hero.MainHero && TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>()
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

                /*float foreigners = 0f;
                foreach (TroopRosterElement element in mobileParty.MemberRoster.GetTroopRoster())
                {
                    if (element.Character.Culture != mobileParty.LeaderHero.Culture)
                    {
                        if (element.Character.Occupation == Occupation.Mercenary) foreigners += element.Number * 0.5f;
                        else foreigners += element.Number;
                    }
                }

                if (data.Perks.Contains(BKPerks.Instance.CommanderInspirer)) foreigners *= 0.5f;
                float foreignersRatio = foreigners / (float)mobileParty.MemberRoster.Count;
                result.AddFactor(foreignersRatio * -0.5f, new TextObject("{=fScrE9fp}Foreign troops")); */

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

                PartySupplies supplies = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKPartyNeedsBehavior>().GetPartySupplies(mobileParty);
                if (supplies != null)
                {
                    if (mobileParty.MemberRoster.TotalManCount > supplies.MinimumSoldiersThreshold)
                    {
                        float alcoholNeed = MathF.Max(supplies.GetAlcoholCurrentNeed().ResultNumber, 1f);
                        float alcohol = MathF.Min(supplies.AlcoholNeed / alcoholNeed, 
                            supplies.AlcoholNeed);
                        result.Add(-alcohol, new TextObject("{=Jph09YjR}Alcohol supplies"));

                        float animal = MathF.Min(supplies.AnimalProductsNeed / supplies.GetAnimalProductsCurrentNeed().ResultNumber, 
                            supplies.AnimalProductsNeed);
                        result.Add(-animal, new TextObject("{=EYGfTj7F}Animal products  supplies"));

                        float textiles = MathF.Min(supplies.ClothNeed / supplies.GetTextileCurrentNeed().ResultNumber, 
                            supplies.ClothNeed);
                        result.Add(-textiles, new TextObject("{=Zz8Op0OS}Textiles supplies"));

                        float wood = MathF.Min(supplies.WoodNeed / supplies.GetWoodCurrentNeed().ResultNumber,
                            supplies.WoodNeed);
                        result.Add(-wood, new TextObject("{=wtBW7t3v}Wood supplies"));
                    }
                }
            }

            return result;
        }
    }
}