using BannerKings.Managers.Policies;
using BannerKings.Managers.Skills;
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
using System.Linq;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Recruits;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles;

namespace BannerKings.Models.Vanilla
{
    public class BKVolunteerModel : VolunteerModel
    {
        public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
          => (int)Math.Floor(CalculateMaximumRecruitmentIndex(buyerHero, sellerHero, useValueAsRelation, false).ResultNumber);

        public ExplainedNumber CalculateMaximumRecruitmentIndex(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101, bool explanations = false)
        {
            var result = new ExplainedNumber(1f, explanations);
            result.LimitMin(0f);
            result.LimitMax(sellerHero.VolunteerTypes.Length);

            if (buyerHero != null)
            {
                useValueAsRelation = sellerHero.GetRelation(buyerHero);
            }

            result.Add(GetRelationImpact(useValueAsRelation), 
                new TextObject("{=!}Relationship with {HERO}")
                .SetTextVariable("HERO", sellerHero.Name));

            var settlement = sellerHero.CurrentSettlement;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title == null)
            {
                return new ExplainedNumber(base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation));
            }

            var contract = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement).Contract;
            if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingVassalage))
            {
                AddVassalage(ref result, buyerHero, sellerHero, title);
            }
            else if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingHidage))
            {
                AddHidage(ref result, buyerHero, sellerHero, title);
            }
            else if (result.ResultNumber >= 2f)
            {
                result.AddFactor(-0.5f, new TextObject("{=!}Drafting Demesne Law ({LAW}) in {TITLE}")
                    .SetTextVariable("LAW", DefaultDemesneLaws.Instance.DraftingFreeContracts.Name)
                    .SetTextVariable("TITLE", title.FullName));
            }

            AddPerks(ref result, buyerHero, sellerHero, useValueAsRelation);
            return result;
        }

        private void AddHidage(ref ExplainedNumber result, Hero buyerHero, Hero sellerHero, FeudalTitle title)
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

                factor = -hides * 0.15f;
            }
            else if (settlement.IsVillage)
            {
                factor = -1f;
            }

            result.Add(factor, new TextObject("{=!}Drafting Demesne Law ({LAW}) in {TITLE}")
                    .SetTextVariable("LAW", DefaultDemesneLaws.Instance.DraftingHidage.Name)
                    .SetTextVariable("TITLE", title.FullName));
        }

        private void AddVassalage(ref ExplainedNumber result, Hero buyerHero, Hero sellerHero, FeudalTitle title)
        {
            float factor = 0f;

            Settlement settlement = title.Fief;
            if (buyerHero.MapFaction == sellerHero.MapFaction)
            {
                if (title.deJure == buyerHero)
                {
                    factor = 0.8f;
                }
                else if (settlement.IsVillage ? settlement.Village.GetActualOwner() == buyerHero :
                    settlement.OwnerClan != null && settlement.Owner == buyerHero)
                {
                    factor = 0.6f;
                }
                else if (buyerHero.PartyBelongedTo != null && buyerHero.PartyBelongedTo.Army != null)
                {
                    factor = 0.4f;
                }

                if (buyerHero.MapFaction.IsKingdomFaction && FactionManager.GetEnemyKingdoms(buyerHero.MapFaction as Kingdom).Count() > 0)
                {
                    factor += 0.15f;
                }

                result.Add(BannerKingsSettings.Instance.VolunteersLimit * factor, new TextObject("{=!}Drafting Demesne Law ({LAW}) in {TITLE}")
                    .SetTextVariable("LAW", DefaultDemesneLaws.Instance.DraftingVassalage.Name)
                    .SetTextVariable("TITLE", title.FullName));
            }
            else 
            {
                result.Add(-1f, new TextObject("{=!}Drafting Demesne Law ({LAW}) in {TITLE}")
                    .SetTextVariable("LAW", DefaultDemesneLaws.Instance.DraftingVassalage.Name)
                    .SetTextVariable("TITLE", title.FullName));
            }  
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
            int result;
            float divided = relation / BannerKingsSettings.Instance.VolunteersLimit;
            result = (int)divided;

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
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data == null) return base.GetBasicVolunteer(sellerHero);

            if (sellerHero.IsPreacher && data.ReligionData != null)
            {
                var religion = data.ReligionData.DominantReligion;
                if (religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Druidism) &&
                    data.MilitaryData.NobleManpower > 0)
                {
                    return sellerHero.Culture.EliteBasicTroop;
                }
            }

            List<(PopType, float)> options = new List<(PopType, float)>(4);
            foreach ((PopType, float) militaryClass in GetMilitaryClasses(data.Settlement))
            {
                float chance = GetPopTypeSpawnChance(data, militaryClass.Item1);
                if (chance > 0f) options.Add(new(militaryClass.Item1, chance));
            }

            return GetPopTypeRecruit(sellerHero, MBRandom.ChooseWeighted(options), settlement);
        }

        public CharacterObject GetPopTypeRecruit(Hero sellerHero, PopType popType, Settlement settlement)
        {
            List<RecruitSpawn> options = DefaultRecruitSpawns.Instance.GetPossibleSpawns(sellerHero.Culture, popType, settlement);
            if (options.Count > 0)
            {
                if (options.Count == 1) return options.First().Troop;
                while(true) 
                { 
                    foreach (RecruitSpawn spawn in options)
                    {
                        if (MBRandom.RandomFloat <= spawn.GetChance(popType))
                        {
                            return spawn.Troop;
                        }
                    }
                }
            }
            else return base.GetBasicVolunteer(sellerHero);
        }

        public float GetPopTypeSpawnChance(PopulationData data, PopType popType)
        {
            float popFactor = data.MilitaryData.GetManpower(popType);
            if (popType == PopType.Nobles)
            {
                var ownerEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(data.Settlement.OwnerClan.Leader);
                if (ownerEducation.HasPerk(BKPerks.Instance.RitterPettySuzerain))
                {
                    popFactor *= 1.2f;
                }
            }

            return MathF.Max(popFactor, 0f) / (float)data.MilitaryData.Manpower;
        }


        public override float GetDailyVolunteerProductionProbability(Hero hero, int index, Settlement settlement)
        {
            return GetDraftEfficiency(hero, settlement).ResultNumber;
        }

        public override ExplainedNumber GetDraftEfficiency(Hero hero, Settlement settlement)
        {
            if (hero == null)
            {
                return new ExplainedNumber(0f);
            }

            ExplainedNumber explainedNumber = new ExplainedNumber(0.5f);
            explainedNumber.AddFactor(hero.Power / 150f, new TextObject("{=fwAxicYv}Power"));

            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data != null)
            {
                explainedNumber.AddFactor(data.Stability - 0.5f, new TextObject("{=dEuappyT}Settlement Stability"));
                var religionData = data.ReligionData;
                if (religionData != null)
                {
                    var religion = religionData.DominantReligion;
                    if (religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Pastoralism))
                    {
                        explainedNumber.Add(-0.2f, DefaultDoctrines.Instance.Pastoralism.Name);
                    }
                }
            }

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Marshal,
                DefaultCouncilTasks.Instance.EncourageMilitarism,
                0.25f, true);
            
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
            if (government == DefaultGovernments.Instance.Tribal)
            {
                explainedNumber.AddFactor(0.2f, new TextObject("{=PSrEtF5L}Government"));
            }

            var lordshipMilitaryAdministration = BKPerks.Instance.LordshipMilitaryAdministration;
            if (settlement.Owner.GetPerkValue(lordshipMilitaryAdministration))
            {
                explainedNumber.AddFactor(0.2f, lordshipMilitaryAdministration.Name);
            }

            return explainedNumber;
        }

        public ExplainedNumber GetMilitarism(Settlement settlement)
        {
            var explainedNumber = new ExplainedNumber(0.1f, true);

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, settlement.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Marshal,
                DefaultCouncilTasks.Instance.EncourageMilitarism,
                0.03f, false);

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
            var list = new List<ValueTuple<PopType, float>>(4);
            float militarism = GetMilitarism(settlement).ResultNumber;
            float serfFactor = militarism;
            float tenantsFactor = militarism * 0.9f;
            float craftsmenFactor = militarism * 0.4f;
            float nobleFactor = militarism * 1.2f;

            if (settlement.OwnerClan != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(settlement.OwnerClan.Kingdom);
                if (title != null)
                {
                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlaveryAserai))
                    {
                        list.Add(new(PopType.Slaves, 0.06f));
                    }

                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsMilitaryServiceDuties))
                    {
                        serfFactor += 0.03f;
                    }
                    else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SerfsLaxDuties))
                    {
                        serfFactor -= 0.015f;
                    }

                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.CraftsmenMilitaryServiceDuties))
                    {
                        craftsmenFactor += 0.03f;
                    }
                    else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.CraftsmenLaxDuties))
                    {
                        serfFactor -= 0.015f;
                    }

                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.NoblesMilitaryServiceDuties))
                    {
                        nobleFactor += 0.03f;
                    }
                    else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.NoblesLaxDuties))
                    {
                        serfFactor -= 0.015f;
                    }
                }
            }

            list.Add(new(PopType.Serfs, serfFactor));
            list.Add(new(PopType.Craftsmen, craftsmenFactor));
            list.Add(new(PopType.Nobles, nobleFactor));
            list.Add(new(PopType.Tenants, tenantsFactor));

            return list;
        }
    }
}