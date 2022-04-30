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
            if (clan.IsEliminated || clan.IsBanditFaction || clan.Kingdom == null || clan == Clan.PlayerClan ||
                BannerKingsConfig.Instance.TitleManager == null) return;

            foreach (WarPartyComponent component in clan.WarPartyComponents)
            {
                Hero leader = component.Leader;
                if (leader != null && leader != clan.Leader && leader.IsWanderer)
                {
                    leader.SetNewOccupation(Occupation.Lord);
                    leader.Clan = null;
                    leader.Clan = clan;
                }
            }

            if (clan.WarPartyComponents.Count < clan.CommanderLimit && clan.Companions.Count < clan.CompanionLimit && 
                clan.Settlements.Count(x => x.IsVillage ) > 1 && clan.Influence >= 150)
            {
                Settlement village = clan.Settlements.FirstOrDefault(x => x.IsVillage);
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
                MBEquipmentRoster roster = (from e in source where e.IsMediumNobleEquipmentTemplate
                                                select e into x orderby MBRandom.RandomInt()
                                                select x).FirstOrDefault();
                if (roster == null) return;

                float price = GetPrice(village.Village.MarketTown.Settlement, roster);
                if (clan.Leader.Gold >= price * 2f)
                {
                    Hero hero = HeroCreator.CreateSpecialHero(template, settlement, clan, null,
                    Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
                    hero.SetNewOccupation(Occupation.Lord);
                    EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, roster.AllEquipments.GetRandomElement());
                    GainKingdomInfluenceAction.ApplyForDefault(clan.Leader, -150f);
                    BannerKingsConfig.Instance.TitleManager.GrantLordship(title, title.deJure, hero);
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
            return price * 0.1f;
        }
    }
}
