using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Behaviours
{
    public class BKClanBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, new Action<Clan>(DailyClanTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }


        private void DailyClanTick(Clan clan)
        {
            if (clan.IsEliminated || clan.IsBanditFaction || clan.Kingdom == null || clan == Clan.PlayerClan ||
                BannerKingsConfig.Instance.TitleManager == null) return;

            if (clan.WarPartyComponents.Count < clan.CommanderLimit && clan.Companions.Count < clan.CompanionLimit && 
                clan.Settlements.Count(x => x.IsVillage ) > 1 && clan.Influence >= 150)
            {
                Settlement village = clan.Settlements.FirstOrDefault(x => x.IsVillage);
                if (village == null) return;
                List<FeudalTitle> clanTitles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(village);
                if (clanTitles.Count == 0 || title == null || !clanTitles.Contains(title) || title.deJure != clan.Leader) return;

                GenderLaw genderLaw = title.contract.genderLaw;
                CharacterObject template = genderLaw == GenderLaw.Agnatic ? clan.Culture.NotableAndWandererTemplates.FirstOrDefault(x => x.Occupation == Occupation.Wanderer && !x.IsFemale) : 
                    clan.Culture.NotableAndWandererTemplates.FirstOrDefault(x => x.Occupation == Occupation.Wanderer);
                if (template == null) return;

                Settlement settlement = clan.Settlements.FirstOrDefault();
                if (settlement == null) settlement = Town.AllTowns.FirstOrDefault(x => x.Culture == clan.Culture).Settlement;

                IEnumerable<MBEquipmentRoster> source = from e in MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>()
                                                        where e.EquipmentCulture == clan.Culture
                                                        select e;
                if (source == null) return;
                MBEquipmentRoster roster = (from e in source where e.IsMediumNobleEquipmentTemplate
                                                select e into x orderby MBRandom.RandomInt()
                                                select x).FirstOrDefault<MBEquipmentRoster>();
                if (roster == null) return;

                float price = this.GetPrice(village.Village.MarketTown.Settlement, roster);
                if (clan.Leader.Gold >= price * 2f)
                {
                    Hero hero = HeroCreator.CreateSpecialHero(template, settlement, clan, null,
                    Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
                    EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, roster.AllEquipments.GetRandomElement());
                    GainKingdomInfluenceAction.ApplyForDefault(clan.Leader, -150f);
                    BannerKingsConfig.Instance.TitleManager.GrantLordship(title, title.deJure, hero);
                    bool mainParty = hero.PartyBelongedTo == MobileParty.MainParty;
                    MobilePartyHelper.CreateNewClanMobileParty(hero, clan, out mainParty);
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
                    EquipmentElement element = new EquipmentElement(equip[i].Item, equip[i].ItemModifier, null, false);
                    if (!element.IsEmpty && element.Item != null)
                        price += settlement.Town.MarketData.GetPrice(element.Item);
                }
            }
            return price * 0.1f;
        }
    }
}
