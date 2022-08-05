using BannerKings.Actions;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Behaviours
{
    public class BKClanBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, DailyClanTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void DailyClanTick(Clan clan)
        {
            if (clan.IsEliminated || clan.IsBanditFaction || clan.Kingdom == null ||
                BannerKingsConfig.Instance.TitleManager == null) return;

            BannerKingsConfig.Instance.CourtManager.UpdateCouncil(clan);
            int councillours = BannerKingsConfig.Instance.CourtManager.GetCouncilloursCount(clan);
            if (councillours != 0) clan.Leader.AddSkillXp(BKSkills.Instance.Lordship, councillours * 2f);

            if (clan == Clan.PlayerClan) return;

            EvaluateRecruitKnight(clan);
            EvaluateRecruitCompanion(clan);
        }

        private void EvaluateRecruitCompanion(Clan clan)
        {
            if (clan.Leader.PartyBelongedTo == null || clan.Leader.IsPrisoner) return;

            WarPartyComponent warParty = clan.WarPartyComponents.FirstOrDefault(x => x.Leader == clan.Leader);
            if (warParty == null || warParty.MobileParty == null) return;

            MobileParty mobileParty = warParty.MobileParty;
            if (!mobileParty.IsActive || !mobileParty.IsReady) return;

            List<(SkillEffect.PerkRole, float)> candidates = new List<(SkillEffect.PerkRole, float)>();

            if (mobileParty.Scout == null) candidates.Add(new(SkillEffect.PerkRole.Scout, 1f));
            if (mobileParty.Surgeon == null) candidates.Add(new(SkillEffect.PerkRole.Surgeon, 1f));
            if (mobileParty.Engineer == null) candidates.Add(new(SkillEffect.PerkRole.Engineer, 1f));
            if (mobileParty.Quartermaster == null) candidates.Add(new(SkillEffect.PerkRole.Quartermaster, 1f));

            if (candidates.Count == 0) return;

            SkillEffect.PerkRole result = MBRandom.ChooseWeighted(candidates);
            Dictionary<SkillEffect.PerkRole, List<TraitObject>> traits = new Dictionary<SkillEffect.PerkRole, List<TraitObject>>() 
            { 
                { SkillEffect.PerkRole.Scout, new List<TraitObject>() { DefaultTraits.WoodsScoutSkills, DefaultTraits.SteppeScoutSkills, DefaultTraits.HillScoutSkills, DefaultTraits.DesertScoutSkills } },
                { SkillEffect.PerkRole.Surgeon, new List<TraitObject>() { DefaultTraits.Surgery } },
                { SkillEffect.PerkRole.Engineer, new List<TraitObject>() { DefaultTraits.Siegecraft } },
                { SkillEffect.PerkRole.Quartermaster, new List<TraitObject>() { DefaultTraits.Manager } },
            };


            CharacterObject template = GetAdequateTemplate(traits[result], clan.Culture);
            if (template == null) return;

            Equipment equipment = GetEquipmentIfPossible(clan, false);
            if (equipment == null) return;

            Hero hero = HeroCreator.CreateSpecialHero(template, null, null, null,
                    Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
            hero.CompanionOf = clan;

            AddHeroToPartyAction.Apply(hero, mobileParty, false);
            if (result == SkillEffect.PerkRole.Scout) mobileParty.SetPartyScout(hero);
            else if (result == SkillEffect.PerkRole.Surgeon) mobileParty.SetPartySurgeon(hero);
            else if (result == SkillEffect.PerkRole.Quartermaster) mobileParty.SetPartyQuartermaster(hero);
            else mobileParty.SetPartyEngineer(hero);
        }

        private CharacterObject GetAdequateTemplate(List<TraitObject> traits, CultureObject culture)
        {
            CharacterObject template = null;
            foreach (TraitObject trait in traits)
                if (template == null) 
                    template = (from x in culture.NotableAndWandererTemplates
                                where x.Occupation == Occupation.Wanderer && x.GetTraitLevel(trait) >= 2
                                select x).GetRandomElementInefficiently();
            return template;
        }

        private Equipment GetEquipmentIfPossible(Clan clan, bool noble, Town town = null)
        {
            IEnumerable<MBEquipmentRoster> source = from e in MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>()
                                                    where e.EquipmentCulture == clan.Culture
                                                    select e;
            if (source == null) return null;
            MBEquipmentRoster roster = (from e in source where e.EquipmentCulture == clan.Culture select e).ToList()
                .GetRandomElementWithPredicate(x => noble ? x.IsMediumNobleEquipmentTemplate : x.StringId.Contains("bk_companion"));

            if (roster == null) return null;

            if (town == null) town = Town.AllTowns.FirstOrDefault(x => x.Culture == clan.Culture);
            if (town != null)
            {
                float price = GetPrice(town.Settlement, roster);
                if (clan.Leader.Gold >= price * 2f) return roster.AllEquipments.GetRandomElement();
            }

            return null;
        }
        

        private void EvaluateRecruitKnight(Clan clan)
        {
            if (clan.WarPartyComponents.Count < clan.CommanderLimit && clan.Companions.Count < clan.CompanionLimit &&
               clan.Settlements.Count(x => x.IsVillage) > 1 && clan.Influence >= BannerKingsConfig.Instance.TitleModel
               .GetGrantKnighthoodCost(clan.Leader).ResultNumber)
            {
                Settlement village = clan.Settlements.GetRandomElementWithPredicate(x => x.IsVillage);
                if (village == null) return;
                List<FeudalTitle> clanTitles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(village);
                if (clanTitles.Count == 0 || title == null || !clanTitles.Contains(title) || title.deJure != clan.Leader) return;

                CharacterObject template;
                GenderLaw genderLaw = title.contract.GenderLaw;
                if (genderLaw == GenderLaw.Agnatic)
                    template = (from e in clan.Culture.NotableAndWandererTemplates
                                where e.Occupation == Occupation.Wanderer && !e.IsFemale
                                select e).GetRandomElementInefficiently();

                else template = (from e in clan.Culture.NotableAndWandererTemplates
                                 where e.Occupation == Occupation.Wanderer
                                 select e).GetRandomElementInefficiently();

                if (template == null) return;

                Settlement settlement = clan.Settlements.FirstOrDefault();
                if (settlement == null) settlement = Town.AllTowns.FirstOrDefault(x => x.Culture == clan.Culture).Settlement;

                IEnumerable<MBEquipmentRoster> source = from e in MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>()
                                                        where e.EquipmentCulture == clan.Culture
                                                        select e;
                if (source == null) return;
                MBEquipmentRoster roster = (from e in source
                                            where e.IsMediumNobleEquipmentTemplate
                                            select e into x
                                            orderby MBRandom.RandomInt()
                                            select x).FirstOrDefault();
                if (roster == null) return;

                float price = GetPrice(village.Village.MarketTown.Settlement, roster);
                if (clan.Leader.Gold >= price * 2f)
                {
                    Hero hero = HeroCreator.CreateSpecialHero(template, settlement, clan, null,
                    Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));

                    BannerKingsConfig.Instance.TitleManager.GrantKnighthood(title, hero, title.deJure);

                    EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, roster.AllEquipments.GetRandomElement());
                    bool mainParty = hero.PartyBelongedTo == MobileParty.MainParty;
                    MobilePartyHelper.CreateNewClanMobileParty(hero, clan, out mainParty);
                    WarPartyComponent component = clan.WarPartyComponents.FirstOrDefault(x => x.Leader == hero);

                    if (component != null)
                        EnterSettlementAction.ApplyForParty(component.MobileParty, settlement);
                }
            }
        }

        private float GetPrice(Settlement settlement, MBEquipmentRoster roster)
        {
            float price = 0;
            if (settlement != null)
            {
                Equipment equip = roster.AllEquipments.GetRandomElement<Equipment>();
                for (int i = 0; i < 12; i++)
                {
                    EquipmentElement element = new EquipmentElement(equip[i].Item, equip[i].ItemModifier);
                    if (!element.IsEmpty && element.Item != null)
                        price += settlement.Town.MarketData.GetPrice(element.Item);
                }
            }
            return price * 0.5f;
        }
    }
}
