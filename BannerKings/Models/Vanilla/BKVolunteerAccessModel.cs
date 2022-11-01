using BannerKings.Extensions;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKVolunteerAccessModel : DefaultVolunteerModel
    {

        public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
        {
            var result = 0;

            if (sellerHero != null && buyerHero != null)
            {
                useValueAsRelation = sellerHero.GetRelation(buyerHero);
                //var explainedResult =

                if (sellerHero.IsPreacher && IsPreacherBlocked(sellerHero, buyerHero))
                {
                    return 0;
                }

                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(buyerHero);
                if (sellerHero.Culture.StringId == "khuzait" && data.HasPerk(BKPerks.Instance.KheshigHonorGuard))
                {
                    return MBMath.ClampInt(result + 1, 0, 6);
                }
            }
            else
            {
                result = base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);
            }

            return result;
        }

        public ExplainedNumber CalculateMaximumRecruitmentIndex(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(BannerKingsSettings.Instance.VolunteersLimit);

            result.Add(GetRelationImpact(useValueAsRelation), GameTexts.FindText("str_notable_relations"));

            var settlement = sellerHero.CurrentSettlement;
            var contract = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement).contract;
            if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingVassalage))
            {
                float factor = 0f;
                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                if (title.deJure == buyerHero)
                {
                    factor = 0.8f;
                }
                else if (settlement.IsVillage ? settlement.Village.GetActualOwner() == buyerHero : settlement.Owner == buyerHero)
                {
                    factor = 0.4f;
                }

                result.Add(BannerKingsSettings.Instance.VolunteersLimit * factor, DefaultDemesneLaws.Instance.DraftingVassalage.Name);
            }
            else if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingHidage))
            {

            } 
            else
            {
               // result.
            }

            AddPerks(ref result, buyerHero, sellerHero, useValueAsRelation);
            return result;
        }

        private void AddPerks(ref ExplainedNumber result, Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
        {

            Settlement currentSettlement = sellerHero.CurrentSettlement;
            if (sellerHero.IsGangLeader && currentSettlement != null && currentSettlement.OwnerClan == buyerHero.Clan)
            {
                if (currentSettlement.IsTown)
                {
                    Hero governor = currentSettlement.Town.Governor;
                    if (governor != null && governor.GetPerkValue(DefaultPerks.Roguery.OneOfTheFamily))
                    {
                        goto IL_138;
                    }
                }
                if (!currentSettlement.IsVillage)
                {
                    goto IL_148;
                }
                Hero governor2 = currentSettlement.Village.Bound.Town.Governor;
                if (governor2 == null || !governor2.GetPerkValue(DefaultPerks.Roguery.OneOfTheFamily))
                {
                    goto IL_148;
                }
            IL_138:
                result.Add(DefaultPerks.Roguery.OneOfTheFamily.SecondaryBonus, DefaultPerks.Roguery.OneOfTheFamily.Name);
            }

            IL_148:
            if (sellerHero.IsMerchant && buyerHero.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity))
            {
                result.Add(DefaultPerks.Trade.ArtisanCommunity.SecondaryBonus, DefaultPerks.Trade.ArtisanCommunity.Name);
            }

            if (sellerHero.Culture == buyerHero.Culture && buyerHero.GetPerkValue(DefaultPerks.Leadership.CombatTips))
            {
                result.Add(DefaultPerks.Leadership.CombatTips.SecondaryBonus, DefaultPerks.Leadership.CombatTips.Name);
            }

            if (sellerHero.IsRuralNotable && buyerHero.GetPerkValue(DefaultPerks.Charm.Firebrand))
            {
                result.Add(DefaultPerks.Charm.Firebrand.SecondaryBonus, DefaultPerks.Charm.Firebrand.Name);
            }

            if (sellerHero.IsUrbanNotable && buyerHero.GetPerkValue(DefaultPerks.Charm.FlexibleEthics))
            {
                result.Add(DefaultPerks.Charm.FlexibleEthics.SecondaryBonus, DefaultPerks.Charm.FlexibleEthics.Name);
            }

            if (sellerHero.IsArtisan && buyerHero.PartyBelongedTo != null && buyerHero.PartyBelongedTo.EffectiveEngineer != null && buyerHero.PartyBelongedTo.EffectiveEngineer.GetPerkValue(DefaultPerks.Engineering.EngineeringGuilds))
            {
                result.Add(DefaultPerks.Engineering.EngineeringGuilds.PrimaryBonus, DefaultPerks.Engineering.EngineeringGuilds.Name);
            }
        }

        private int GetRelationImpact(int relation)
        {
            int result = 0;
            float divided = relation / 50f;
            result = (int)(BannerKingsSettings.Instance.VolunteersLimit * (divided * 0.15f));

            return result;
        }

        private bool IsPreacherBlocked(Hero sellerHero, Hero buyerHero)
        {
            var clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(sellerHero);
            if (clergyman != null)
            {
                var clergyReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(sellerHero);
                var heroReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(buyerHero);
                return heroReligion != clergyReligion || !buyerHero.GetPerkValue(BKPerks.Instance.TheologyFaithful);
            }

            return false;
        }
    }
}
