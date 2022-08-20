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
            int result;
            switch (tier)
            {
                case 0:
                    result = 3;
                    break;
                case 1:
                    result = 6;
                    break;
                case 2:
                    result = 10;
                    break;
                case 3:
                    result = 15;
                    break;
                case 4:
                    result = 26;
                    break;
                case 5:
                    result = 38;
                    break;
                case 6:
                    result = 50;
                    break;
                default:
                    result = 60;
                    break;
            }

            return result;
        }

        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
            var result = base.GetTotalWage(mobileParty, includeDescriptions);

            var leader = mobileParty.LeaderHero != null ? mobileParty.LeaderHero : mobileParty.Owner;
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
            var result = new ExplainedNumber(base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost));

            if (buyerHero != null)
            {
                if (Utils.Helpers.IsRetinueTroop(troop))
                {
                    result.AddFactor(0.20f);
                }

                if (troop.Culture == buyerHero.Culture)
                {
                    result.AddFactor(-0.05f, GameTexts.FindText("str_culture"));
                }

                if (buyerHero.Clan != null)
                {
                    if (buyerHero.CurrentSettlement != null && buyerHero.CurrentSettlement.OwnerClan != null
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

                    if (buyerHero.Clan.Tier >= 4)
                    {
                        result.AddFactor((buyerHero.Clan.Tier - 3) * 0.05f);
                    }
                    else if (buyerHero.Clan.Tier <= 1)
                    {
                        result.AddFactor((buyerHero.Clan.Tier - 2) * 0.05f);
                    }
                }
            }

            return (int) result.ResultNumber;
        }
    }
}