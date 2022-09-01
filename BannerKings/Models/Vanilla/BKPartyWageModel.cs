using BannerKings.Managers.Education;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyWageModel : DefaultPartyWageModel
    {
        public override int GetCharacterWage(int tier)
        {
            var result = tier switch
            {
                0 => 3,
                1 => 6,
                2 => 10,
                3 => 15,
                4 => 26,
                5 => 38,
                6 => 50,
                _ => 60
            };

            return result;
        }

        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
            var result = base.GetTotalWage(mobileParty, includeDescriptions);

            var leader = mobileParty.LeaderHero ?? mobileParty.Owner;
            if (leader != null)
            {
                var totalCulture = 0f;
                var mountedTroops = 0f;
                for (var i = 0; i < mobileParty.MemberRoster.Count; i++)
                {
                    var elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
                    if (elementCopyAtIndex.Character.Culture == leader.Culture)
                    {
                        totalCulture += elementCopyAtIndex.Number;
                    }

                    if (elementCopyAtIndex.Character.HasMount())
                    {
                        mountedTroops += elementCopyAtIndex.Number;
                    }

                    if (elementCopyAtIndex.Character.IsHero)
                    {
                        if (elementCopyAtIndex.Character.HeroObject == mobileParty.LeaderHero)
                        {
                            continue;
                        }

                        var skills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
                        var companionModel = new BKCompanionPrices();
                        var totalCost = 0f;
                        foreach (var skill in skills)
                        {
                            float skillValue = elementCopyAtIndex.Character.GetSkillValue(skill);
                            if (skillValue > 30)
                            {
                                totalCost += skillValue * companionModel.GetCostFactor(skill);
                            }
                        }

                        result.Add(totalCost * 0.005f, elementCopyAtIndex.Character.Name);
                    }
                }

                var proportion = MBMath.ClampFloat(totalCulture / mobileParty.MemberRoster.TotalManCount, 0f, 1f);
                if (proportion > 0f)
                {
                    result.AddFactor(proportion * -0.1f, GameTexts.FindText("str_culture"));
                }

                if (mobileParty.IsGarrison)
                {
                    result.Add(result.ResultNumber * -0.5f);
                }


                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.HasPerk(BKPerks.Instance.CataphractEquites) && mountedTroops > 0f)
                {
                    result.AddFactor(mountedTroops * -0.1f, BKPerks.Instance.CataphractEquites.Name);
                }

                if (mobileParty.SiegeEvent != null && education.Lifestyle != null && 
                    education.Lifestyle.Equals(DefaultLifestyles.Instance.SiegeEngineer))
                {
                    result.AddFactor(-0.3f, DefaultLifestyles.Instance.SiegeEngineer.Name);
                }
            }

            if (mobileParty.IsCaravan && mobileParty.Owner != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.Owner);
                if (education.HasPerk(BKPerks.Instance.CaravaneerDealer))
                {
                    result.AddFactor(-0.1f, BKPerks.Instance.CaravaneerDealer.Name);
                }
            }

            return result;
        }

        public override int GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
        {
            var result = new ExplainedNumber(base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost) * 1.4f);
            result.LimitMin(GetCharacterWage(troop.Tier) * 2f);

            if (buyerHero != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(buyerHero);
                if (troop.Occupation == Occupation.Mercenary && education.HasPerk(BKPerks.Instance.MercenaryLocalConnections))
                {
                    result.AddFactor(-0.1f, BKPerks.Instance.MercenaryLocalConnections.Name);
                }

                if (troop.IsMounted && education.HasPerk(BKPerks.Instance.RitterOathbound))
                {
                    result.AddFactor(-0.15f, BKPerks.Instance.RitterOathbound.Name);
                }

                if (Utils.Helpers.IsRetinueTroop(troop))
                {
                    result.AddFactor(0.20f);
                }

                if (troop.Culture == buyerHero.Culture)
                {
                    result.AddFactor(-0.05f, GameTexts.FindText("str_culture"));
                }

                if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Artisan))
                {
                    result.AddFactor(0.15f, DefaultLifestyles.Instance.Artisan.Name);
                }

                if (buyerHero.Clan != null)
                {

                    if (troop.Culture.StringId == "aserai" && BannerKingsConfig.Instance.ReligionsManager
                        .HasBlessing(buyerHero, DefaultDivinities.Instance.AseraSecondary2))
                    {
                        result.AddFactor(-0.1f);
                    }

                    if (buyerHero.CurrentSettlement is {OwnerClan: { }} 
                        && buyerHero.CurrentSettlement.OwnerClan == buyerHero.Clan)
                    {
                        result.AddFactor(-0.15f);
                    }

                    if (troop.IsInfantry)
                    {
                        result.AddFactor(-0.05f);
                    }

                    var buyerKingdom = buyerHero.Clan.Kingdom;
                    if (buyerKingdom != null && troop.Culture != buyerHero.Culture)
                    {
                        result.AddFactor(0.25f, GameTexts.FindText("str_kingdom"));
                    }
                    else
                    {
                        result.AddFactor(-0.1f, GameTexts.FindText("str_kingdom"));
                    }

                    switch (buyerHero.Clan.Tier)
                    {
                        case >= 4:
                            result.AddFactor((buyerHero.Clan.Tier - 3) * 0.05f);
                            break;
                        case <= 1:
                            result.AddFactor((buyerHero.Clan.Tier - 2) * 0.05f);
                            break;
                    }
                }
            }

            return (int) result.ResultNumber;
        }
    }
}