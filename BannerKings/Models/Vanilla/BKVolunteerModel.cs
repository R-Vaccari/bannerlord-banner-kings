using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Policies.BKDraftPolicy;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Extensions;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Settings;
using System;
using System.Collections.Generic;

namespace BannerKings.Models.Vanilla
{
    public class BKVolunteerModel : DefaultVolunteerModel
    {

        public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
          => (int)Math.Floor(CalculateMaximumRecruitmentIndex(buyerHero, sellerHero, useValueAsRelation, false).ResultNumber);

        public ExplainedNumber CalculateMaximumRecruitmentIndex(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(BannerKingsSettings.Instance.VolunteersLimit);

            if (buyerHero != null)
            {
                useValueAsRelation = sellerHero.GetRelation(buyerHero);
            }
         

            result.Add(GetRelationImpact(useValueAsRelation), GameTexts.FindText("str_notable_relations"));

            var settlement = sellerHero.CurrentSettlement;
            var contract = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement).contract;
            if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingVassalage))
            {
                AddVassalage(ref result, buyerHero, sellerHero);
            }
            else if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingHidage))
            {
                AddHidage(ref result, buyerHero, sellerHero);
            }
            else
            {
                int baseResult = base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);
                result.Add(baseResult * (BannerKingsSettings.Instance.VolunteersLimit / 6f) * 0.5f, DefaultDemesneLaws.Instance.DraftingFreeContracts.Name);
            }

            AddPerks(ref result, buyerHero, sellerHero, useValueAsRelation);
            return result;
        }

        private void AddHidage(ref ExplainedNumber result, Hero buyerHero, Hero sellerHero)
        {
            Settlement settlement = sellerHero.CurrentSettlement;
            float factor = 0f;

            if (buyerHero.MapFaction == sellerHero.MapFaction)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                float hides;

                if (data.EstateData != null && data.EstateData.HeroHasEstate(sellerHero))
                {
                    var estate = data.EstateData.GetHeroEstate(sellerHero);
                    hides = estate.Acreage / BannerKingsConfig.Instance.EstatesModel.MinimumEstateAcreage;
                }
                else
                {
                    hides = sellerHero.Power / 150f;
                }

                factor = hides * 0.15f;
            }
            else if (settlement.IsVillage)
            {
                factor = -1f;
            }


            result.Add(BannerKingsSettings.Instance.VolunteersLimit * factor, DefaultDemesneLaws.Instance.DraftingVassalage.Name);
        }

        private void AddVassalage(ref ExplainedNumber result, Hero buyerHero, Hero sellerHero)
        {
            Settlement settlement = sellerHero.CurrentSettlement;
            float factor = 0f;

            if (buyerHero.MapFaction == sellerHero.MapFaction)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                if (title.deJure == buyerHero)
                {
                    factor = 0.8f;
                }
                else if (settlement.IsVillage ? settlement.Village.GetActualOwner() == buyerHero : settlement.Owner == buyerHero)
                {
                    factor = 0.5f;
                }
                else if (buyerHero.PartyBelongedTo != null && buyerHero.PartyBelongedTo.Army != null)
                {
                    factor = 0.4f;
                }
            }
            else if (settlement.IsVillage)
            {
                factor = -1f;
            }


            result.Add(BannerKingsSettings.Instance.VolunteersLimit * factor, DefaultDemesneLaws.Instance.DraftingVassalage.Name);
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
        public override bool CanHaveRecruits(Hero hero)
        {
            var occupation = hero.Occupation;
            var valid = occupation == Occupation.Mercenary || occupation - Occupation.Artisan <= 5;
            if (valid)
            {
                var settlement = hero.CurrentSettlement;
                if (settlement != null && BannerKingsConfig.Instance.PopulationManager != null &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    return data.MilitaryData.Manpower > 0;
                }

                return true;
            }

            return false;
        }

        public override CharacterObject GetBasicVolunteer(Hero sellerHero)
        {
            var settlement = sellerHero.CurrentSettlement;
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                var power = sellerHero.Power;
                var chance = settlement.IsTown ? power * 0.03f : power * 0.05f;
                var random = MBRandom.RandomFloatRanged(1f, 100f);

                var ownerEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.OwnerClan.Leader);
                if (ownerEducation.HasPerk(BKPerks.Instance.RitterPettySuzerain))
                {
                    chance *= 1.2f;
                }

                if (data.MilitaryData.NobleManpower > 0)
                {

                    if (sellerHero.IsPreacher && data.ReligionData != null)
                    {
                        var religion = data.ReligionData.DominantReligion;
                        if (religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Druidism))
                        {
                            return sellerHero.Culture.EliteBasicTroop;
                        }
                    }

                    if (chance >= random)
                    {
                        return sellerHero.Culture.EliteBasicTroop;
                    }
                }

                return sellerHero.Culture.BasicTroop;
            }

            return base.GetBasicVolunteer(sellerHero);
        }

        public ExplainedNumber GetDraftEfficiency(Hero hero, int index, Settlement settlement)
        {
            if (hero == null)
            {
                return new ExplainedNumber(0f);
            }

            var num = 0.7f;
            var num2 = 0;
            foreach (var town in hero.CurrentSettlement.MapFaction.Fiefs)
            {
                num2 += town.IsTown
                    ? (town.Settlement.Prosperity < 3000f ? 1 : town.Settlement.Prosperity < 6000f ? 2 : 3) +
                      town.Villages.Count
                    : town.Villages.Count;
            }

            var num3 = num2 < 46 ? num2 / 46f * (num2 / 46f) : 1f;
            num += hero.CurrentSettlement != null && num3 < 1f ? (1f - num3) * 0.2f : 0f;
            var baseNumber = 0.75f * MathF.Clamp(MathF.Pow(num, index + 1), 0f, 1f);
            var explainedNumber = new ExplainedNumber(baseNumber, true);
            var clan = hero.Clan;
            if (clan?.Kingdom != null && hero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Cantons))
            {
                explainedNumber.AddFactor(0.2f, new TextObject("{=zGx6c77M}Cantons kingdom policy"));
            }

            if (hero.VolunteerTypes?[index] != null && hero.VolunteerTypes[index].IsMounted && PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.CavalryTactics, settlement.IsVillage ? settlement.Village.Bound.Town : settlement.Town))
            {
                explainedNumber.AddFactor(DefaultPerks.Riding.CavalryTactics.PrimaryBonus * 0.01f, DefaultPerks.Riding.CavalryTactics.PrimaryDescription);
            }

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, settlement.OwnerClan.Leader, CouncilPosition.Marshall, 0.25f, true);
            
            var draftPolicy = ((BKDraftPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "draft")).Policy;
            switch (draftPolicy)
            {
                case DraftPolicy.Conscription:
                    explainedNumber.Add(0.15f, new TextObject("{=2z4YQTuu}Draft policy"));
                    break;
                case DraftPolicy.Demobilization:
                    explainedNumber.Add(-0.15f, new TextObject("{=2z4YQTuu}Draft policy"));
                    break;
            }

            var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
            if (government == GovernmentType.Tribal)
            {
                explainedNumber.AddFactor(0.2f, new TextObject("{=PSrEtF5L}Government"));
            }

            var lordshipMilitaryAdministration = BKPerks.Instance.LordshipMilitaryAdministration;
            if (settlement.Owner.GetPerkValue(lordshipMilitaryAdministration))
            {
                explainedNumber.AddFactor(0.2f, lordshipMilitaryAdministration.Name);
            }

            var religionData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).ReligionData;
            if (religionData != null)
            {
                var religion = religionData.DominantReligion;
                if (religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Pastoralism))
                {
                    explainedNumber.Add(-0.2f, DefaultDoctrines.Instance.Pastoralism.Name);
                }
            }

            return explainedNumber;

        }

        public ExplainedNumber GetMilitarism(Settlement settlement)
        {
            var explainedNumber = new ExplainedNumber(0f, true);
            var serfMilitarism = GetClassMilitarism(PopType.Serfs);
            var craftsmenMilitarism = GetClassMilitarism(PopType.Craftsmen);
            var nobleMilitarism = GetClassMilitarism(PopType.Nobles);

            explainedNumber.Add(serfMilitarism, new TextObject("{=pop_class_serfs}Serfs"));

            explainedNumber.Add(serfMilitarism, new TextObject("{=pop_class_craftsmen}Craftsmen"));

            explainedNumber.Add(serfMilitarism, new TextObject("{=pop_class_nobles}Nobles"));

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, settlement.OwnerClan.Leader, CouncilPosition.Marshall, 0.03f, false);

            if (settlement.Culture == settlement.Owner.Culture)
            {
                var lordshipTraditionalistPerk = BKPerks.Instance.LordshipTraditionalist;
                if (settlement.Owner.GetPerkValue(BKPerks.Instance.LordshipTraditionalist))
                {
                    explainedNumber.AddFactor(0.01f, lordshipTraditionalistPerk.Name);
                }

                var lordshipMilitaryAdministration = BKPerks.Instance.LordshipMilitaryAdministration;
                if (settlement.Owner.GetPerkValue(lordshipMilitaryAdministration))
                {
                    explainedNumber.AddFactor(0.02f, lordshipMilitaryAdministration.Name);
                }
            }

            return explainedNumber;
        }

        public List<ValueTuple<PopType, float>> GetMilitaryClasses(Settlement settlement)
        {
            var list = new List<ValueTuple<PopType, float>>();
            float serfFactor = 0.1f;
            float craftsmenFactor = 0.04f;
            float nobleFactor = 0.12f;

            if (settlement.OwnerClan != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(settlement.OwnerClan.Kingdom);
                if (title != null)
                {
                    if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsMilitaryServiceDuties))
                    {
                        serfFactor += 0.03f;

                    }
                }
            }



            list.Add(new(PopType.Serfs, serfFactor));
            list.Add(new(PopType.Craftsmen, craftsmenFactor));
            list.Add(new(PopType.Nobles, nobleFactor));


            return list;
        }

        public float GetClassMilitarism(PopType type)
        {
            return type switch
            {
                PopType.Serfs => 0.1f,
                PopType.Craftsmen => 0.03f,
                PopType.Nobles => 0.12f,
                _ => 0
            };
        }
    }
}