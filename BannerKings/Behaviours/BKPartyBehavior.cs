using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Components;
using BannerKings.Managers.Populations;
using BannerKings.Settings;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Behaviours
{
    public class BKPartyBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
            CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener(this, OnMobilePartyDestroyed);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, OnSiegeStarted);
        }

        private void OnSiegeStarted(SiegeEvent siegeEvent) 
        {
            if (siegeEvent.BesiegedSettlement == null)
            {
                return;
            }

            var toRemove = new List<MobileParty>();
            foreach (var party in siegeEvent.BesiegedSettlement.Parties)
            {
                if (BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party))
                {
                    toRemove.Add(party);
                }
            }

            foreach (var party in toRemove)
            {
                DestroyPartyAction.Apply(null, party);
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
            }
        }

        private void HourlyTickParty(MobileParty party)
        {
            if (party == null || BannerKingsConfig.Instance.PopulationManager == null || party.GetTotalStrengthWithFollowers() < 25f)
            {
                return;
            }

            if (party.IsBandit || (party.MapFaction.IsKingdomFaction && party.IsLordParty && party == MobileParty.MainParty))
            {
                EvaluateSendGarrison(SettlementHelper.FindNearestSettlement(x =>
                    {
                        if (x.MapFaction == null || x.OwnerClan == null)
                        {
                            return false;
                        }
                        var stance = x.MapFaction.GetStanceWith(party.MapFaction);
                        return stance != null && x.Town != null && (stance.IsAtWar || stance.IsAtConstantWar);
                    },
                    party),
                    party);
            }

            AddCustomPartyBehaviors(party);
        }

        private void AddCustomPartyBehaviors(MobileParty party)
        {
            if (party.PartyComponent is not BannerKingsComponent)
            {
                return;
            }

            party.Ai.DisableAi();
            var bkComponent = (BannerKingsComponent)party.PartyComponent;
            if (bkComponent.HomeSettlement == null)
            {
                DestroyPartyAction.Apply(null, party);
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
                return;
            }

            bkComponent.TickHourly();
        }

        private void EvaluateSendGarrison(Settlement origin, MobileParty target)
        {
            if (origin == null || target == null)
            {
                return;
            }

            var distance = Campaign.Current.Models.MapDistanceModel.GetDistance(target, origin);
            if (distance > 10f)
            {
                return;
            }

            var garrison = origin.Town.GarrisonParty;
            if (origin.IsUnderSiege || garrison.MemberRoster.TotalHealthyCount < 100)
            {
                return;
            }

            MobileParty garrisonParty = GarrisonPartyComponent.CreateParty(origin, target);
            if (garrisonParty != null)
            {
                (garrisonParty.PartyComponent as GarrisonPartyComponent).TickHourly();
            }
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement == null || !settlement.IsTown)
            {
                return;
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsDecisionEnacted(settlement, "decision_slaves_export") &&
                DecideSendSlaveCaravan(settlement) && !settlement.IsUnderSiege)
            {
                var villages = settlement.BoundVillages;
                var villageTarget = villages.FirstOrDefault(village => village.Settlement != null && !BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(village.Settlement, PopType.Slaves));

                if (villageTarget != null)
                {
                    SendSlaveCaravan(villageTarget);
                }
            }

            DecideSendTraders(settlement);
            DecideSendTravellers(settlement);
        }

        private void DecideSendTraders(Settlement settlement)
        {
            var random = MBRandom.RandomInt(1, 100);
            if (random > 40)
            {
                return;
            }

            var target = GetTownToTravel(settlement);
            if (target == null)
            {
                return;
            }

            if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target) &&
               BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                CharacterObject civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()
                            .FirstOrDefault(x => x.StringId == "villager_" + settlement.Culture.StringId);
                int count = MBRandom.RandomInt(12, 25);
                var name = "{=!}Traders from {ORIGIN}";

                if (civilian != null)
                {
                    MobileParty tradersParty = PopulationPartyComponent.CreateTravellerParty("travellers_", settlement, target,
                        name, count, PopType.Serfs, civilian, true);

                    int budget = 500 + (int)(settlement.Prosperity / 1000f);

                    var localData = settlement.Town.MarketData;
                    var targetData = target.Town.MarketData;
                    var townStock = settlement.Town.Owner.ItemRoster;
                    foreach (var element in townStock)
                    {
                        if (budget <= 5)
                        {
                            break;
                        }

                        EquipmentElement equipment = element.EquipmentElement;
                        ItemCategory category = equipment.Item.ItemCategory;
                        if (localData.GetSupply(category) > localData.GetDemand(category) &&
                            targetData.GetSupply(category) < targetData.GetDemand(category))
                        {
                            var price = localData.GetPrice(equipment, null, true);
                            int totalCount = MBMath.ClampInt((int)(budget / (float)price), 0, element.Amount);
                            if (totalCount > 1f)
                            {
                                townStock.AddToCounts(equipment, -totalCount);
                                target.Town.ChangeGold((int)(price * (float)totalCount));
                                tradersParty.ItemRoster.AddToCounts(new EquipmentElement(equipment.Item, equipment.ItemModifier), 
                                    totalCount);
                            }
                        }
                    }
                }
            }  
        }

        private void DecideSendTravellers(Settlement settlement)
        {
            var random = MBRandom.RandomInt(1, 100);
            if (random > 5)
            {
                return;
            }

            var target = GetTownToTravel(settlement);
            if (target == null)
            {
                return;
            }

            if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target) &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                SendTravellerParty(settlement, target);
            }
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party == null)
            {
                return;
            }

            AddGarrisonParty(party, target);
            AddRealisticIncome(party, target);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target);
            if (data == null)
            {
                return;
            }

            AddCaravanFees(party, target, data);
            AddPopulationPartyBehavior(party, target, data);
        }

        private void AddGarrisonParty(MobileParty party, Settlement settlement)
        {
            if (party.PartyComponent is GarrisonPartyComponent)
            {
                var component = party.PartyComponent as GarrisonPartyComponent;
                if (settlement != component.HomeSettlement)
                {
                    return;
                }

                foreach (var element in party.MemberRoster.GetTroopRoster())
                {
                    settlement.Town.GarrisonParty.MemberRoster.AddToCounts(element.Character, 
                        element.Number, 
                        false, 
                        element.WoundedNumber);
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data == null)
                {
                    return;
                }

                foreach (var element in party.MemberRoster.GetTroopRoster())
                {
                    data.UpdatePopType(PopType.Slaves, element.Number, true);
                }

                DestroyPartyAction.Apply(null, party);
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
            }
        }

        private void AddCaravanFees(MobileParty party, Settlement target, PopulationData data)
        {
            if (!party.IsCaravan)
            {
                return;
            }

            int fee = data.EconomicData.CaravanFee(party);
            if (fee > 0)
            {
                party.PartyTradeGold -= fee;
                target.Town.TradeTaxAccumulated += fee;
            }
        }

        private void AddPopulationPartyBehavior(MobileParty party, Settlement target, PopulationData data)
        {
            if (party.PartyComponent is PopulationPartyComponent)
            {
                var component = (PopulationPartyComponent)party.PartyComponent;
                if (component.Trading)
                {
                    var localData = target.Town.MarketData;
                    foreach (var element in party.ItemRoster)
                    {
                        float price = localData.GetPrice(element.EquipmentElement);
                        target.Town.ChangeGold(-(int)(price * element.Amount));
                        target.Town.Owner.ItemRoster.AddToCounts(element.EquipmentElement, element.Amount);
                    }
                }

                if (component is MilitiaComponent && target.IsVillage)
                {
                    foreach (var element in party.MemberRoster.GetTroopRoster())
                    {
                        target.MilitiaPartyComponent.MobileParty.MemberRoster.AddToCounts(element.Character,
                            element.Number);
                    }

                    if (party.PrisonRoster.TotalRegulars > 0)
                    {
                        foreach (var element in party.PrisonRoster.GetTroopRoster().Where(element => !element.Character.IsHero))
                        {
                            data.UpdatePopType(PopType.Slaves, element.Number);
                        }
                    }
                }

                if (component.SlaveCaravan)
                {
                    var slaves = Utils.Helpers.GetRosterCount(party.PrisonRoster);
                    data.UpdatePopType(PopType.Slaves, slaves);
                }
                else if (component.PopulationType != PopType.None)
                {
                    var filter = component.PopulationType == PopType.Serfs ? "villager" :
                        component.PopulationType == PopType.Craftsmen ? "craftsman" : "noble";
                    var pops = Utils.Helpers.GetRosterCount(party.MemberRoster, filter);
                    data.UpdatePopType(component.PopulationType, pops);
                }

                DestroyPartyAction.Apply(null, party);
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(party);
            }
        }

        private void AddRealisticIncome(MobileParty party, Settlement target)
        {
            if (party.IsCaravan && BannerKingsSettings.Instance.RealisticCaravanIncome)
            {
                var caravanOwner = party.Owner;
                if (target.Owner == caravanOwner || target.HeroesWithoutParty.Contains(caravanOwner) ||
                    (caravanOwner.PartyBelongedTo != null && target.Parties.Contains(caravanOwner.PartyBelongedTo)))
                {
                    int income = MathF.Max(0, party.PartyTradeGold - 10000);
                    if (income > 0)
                    {
                        GiveGoldAction.ApplyForPartyToCharacter(party.Party, caravanOwner, income);
                        if (caravanOwner == Hero.MainHero)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(
                                new TextObject("{=6WM78pd8}The {CARAVAN} has deposited you {GOLD}{GOLD_ICON}")
                                .SetTextVariable("CARAVAN", party.Name)
                                .SetTextVariable("GOLD", income).ToString()));
                        }

                        if (party.LeaderHero != null)
                        {
                            SkillLevelingManager.OnTradeProfitMade(party.LeaderHero, income);
                        }

                        if (party.Owner.Clan != null && party.Party.Owner.Clan.Leader.GetPerkValue(DefaultPerks.Trade.GreatInvestor))
                        {
                            party.Party.Owner.Clan.AddRenown(DefaultPerks.Trade.GreatInvestor.PrimaryBonus, true);
                        }
                    }
                }
            }
        }

        private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
        {
            if (mobileParty != null && BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(mobileParty))
            {
                BannerKingsConfig.Instance.PopulationManager.RemoveCaravan(mobileParty);
            }
        }

        private bool DecideSendSlaveCaravan(Settlement settlement)
        {
            if (!settlement.IsTown || settlement.Town == null)
            {
                return false;
            }

            var villages = settlement.BoundVillages;
            return villages is {Count: > 0} && BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Slaves);
        }

        private Settlement GetTownToTravel(Settlement origin)
        {
            var kingdom = origin.OwnerClan?.Kingdom;
            if (kingdom?.Settlements == null || kingdom.Settlements.Count <= 1)
            {
                return null;
            }

            var list = (from settlement in kingdom.Settlements where settlement.IsTown && settlement != origin select new ValueTuple<Settlement, float>(settlement, 1f)).ToList();

            return MBRandom.ChooseWeighted(list);
        }

        private void SendTravellerParty(Settlement origin, Settlement target)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            var random = MBRandom.RandomInt(1, 100);
            CharacterObject civilian;
            PopType type;
            int count;
            switch (random)
            {
                case < 60:
                    civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()
                        .FirstOrDefault(x => x.StringId == "villager_" + origin.Culture.StringId);
                    count = MBRandom.RandomInt(30, 70);
                    type = PopType.Serfs;
                    break;
                case < 90:
                    civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()
                        .FirstOrDefault(x => x.StringId == "craftsman_" + origin.Culture.StringId);
                    count = MBRandom.RandomInt(15, 30);
                    type = PopType.Craftsmen;
                    break;
                default:
                    civilian = MBObjectManager.Instance.GetObjectTypeList<CharacterObject>()
                        .FirstOrDefault(x => x.StringId == "noble_" + origin.Culture.StringId);
                    count = MBRandom.RandomInt(10, 15);
                    type = PopType.Nobles;
                    break;
            }

            var name = "{=xEwX83aU}Travelling {CLASS} from {ORIGIN}";
            name = name.Replace("{CLASS}", Utils.Helpers.GetClassName(type, origin.Culture).ToString());

            if (civilian != null)
            {
                PopulationPartyComponent.CreateTravellerParty("travellers_", origin, target,
                    name, count, type, civilian, false);
            }
        }

        private void SendSlaveCaravan(Village target)
        {
            var origin = target.Bound;
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(origin);
            var slaves = (int) (data.GetTypeCount(PopType.Slaves) * 0.005d);
            data.UpdatePopType(PopType.Slaves, (int) (slaves * -1f));
            PopulationPartyComponent.CreateSlaveCaravan(origin, target.Settlement, slaves);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialog(campaignGameStarter);
        }

        private void AddDialog(CampaignGameStarter starter)
        {
            starter.AddDialogLine("traveller_serf_party_start", "start", "traveller_party_greeting",
                "M'lord! We are humble folk, travelling between towns, looking for work and trade.",
                traveller_serf_start_on_condition, null);

            starter.AddDialogLine("traveller_craftsman_party_start", "start", "traveller_party_greeting",
                "Good day to you. We are craftsmen travelling for business purposes.",
                traveller_craftsman_start_on_condition, null);

            starter.AddDialogLine("traveller_noble_party_start", "start", "traveller_party_greeting",
                "Yes? Please do not interfere with our caravan.",
                traveller_noble_start_on_condition, null);


            starter.AddPlayerLine("traveller_party_loot", "traveller_party_greeting", "close_window",
                new TextObject("{=dOcj05n6}Whatever you have, I'm taking it. Surrender or die!").ToString(),
                traveller_aggression_on_condition,
                delegate { PlayerEncounter.Current.IsEnemy = true; });

            starter.AddPlayerLine("traveller_party_leave", "traveller_party_greeting", "close_window",
                new TextObject("{=zhRJeYOY}Carry on, then. Farewell.").ToString(), null,
                delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddDialogLine("slavecaravan_friend_party_start", "start", "slavecaravan_party_greeting",
                "My lord, we are taking these rabble somewhere they can be put to good use.",
                slavecaravan_amicable_on_condition, null);

            starter.AddDialogLine("slavecaravan_neutral_party_start", "start", "slavecaravan_party_greeting",
                "If you're not planning to join those vermin back there, move away![rf:idle_angry][ib:aggressive]",
                slavecaravan_neutral_on_condition, null);

            starter.AddPlayerLine("slavecaravan_party_leave", "slavecaravan_party_greeting", "close_window",
                new TextObject("{=zhRJeYOY}Carry on, then. Farewell.").ToString(), null,
                delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddPlayerLine("slavecaravan_party_threat", "slavecaravan_party_greeting", "slavecaravan_threat",
                new TextObject("{=Nk8tSdcu}Give me your slaves and gear, or else!").ToString(),
                slavecaravan_neutral_on_condition,
                null);

            starter.AddDialogLine("slavecaravan_party_threat_response", "slavecaravan_threat", "close_window",
                "One more for the mines! Lads, get the whip![rf:idle_angry][ib:aggressive]",
                null, delegate { PlayerEncounter.Current.IsEnemy = true; });

            starter.AddDialogLine("raised_militia_party_start", "start", "raised_militia_greeting",
                "M'lord! We are ready to serve you.",
                raised_militia_start_on_condition, null);

            starter.AddPlayerLine("raised_militia_party_follow", "raised_militia_greeting", "raised_militia_order",
                new TextObject("{=Hvi96rXx}Follow my company.").ToString(),
                () => true,
                raised_militia_follow_on_consequence);

            starter.AddPlayerLine("raised_militia_party_retreat", "raised_militia_greeting", "raised_militia_order",
                new TextObject("{=xPvsVw4b}You may go home.").ToString(),
                () => true,
                raised_militia_retreat_on_consequence);

            starter.AddDialogLine("raised_militia_order_response", "raised_militia_order", "close_window",
                "Aye!",
                null, delegate { PlayerEncounter.LeaveEncounter = true; });
        }

        private bool IsTravellerParty(PartyBase party)
        {
            var value = false;
            if (party is not {MobileParty: { }})
            {
                return false;
            }

            if (BannerKingsConfig.Instance.PopulationManager.IsPopulationParty(party.MobileParty))
            {
                value = true;
            }

            return value;
        }

        private bool traveller_serf_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            if (component.PopulationType == PopType.Serfs)
            {
                value = true;
            }

            return value;
        }

        private void raised_militia_retreat_on_consequence()
        {
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return;
            }

            var component = (MilitiaComponent) party.MobileParty.PartyComponent;
            component.Behavior = AiBehavior.GoToSettlement;
        }

        private void raised_militia_follow_on_consequence()
        {
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return;
            }

            var component = (MilitiaComponent) party.MobileParty.PartyComponent;
            component.Behavior = AiBehavior.EscortParty;
        }

        private bool raised_militia_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return value;
            }

            if (party.MobileParty.PartyComponent is MilitiaComponent)
            {
                value = true;
            }

            return value;
        }

        private bool raised_militia_order_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            if (party.MobileParty.PartyComponent is MilitiaComponent && party.Owner == Hero.MainHero)
            {
                value = true;
            }

            return value;
        }

        private bool traveller_craftsman_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            if (component.PopulationType == PopType.Craftsmen)
            {
                value = true;
            }

            return value;
        }

        private bool traveller_noble_start_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            if (component.PopulationType == PopType.Nobles)
            {
                value = true;
            }

            return value;
        }

        private bool traveller_aggression_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            var partyKingdom = component.HomeSettlement.OwnerClan.Kingdom;
            if (partyKingdom != null)
            {
                if (Hero.MainHero.Clan.Kingdom == null ||
                    component.HomeSettlement.OwnerClan.Kingdom != Hero.MainHero.Clan.Kingdom)
                {
                    value = true;
                }
            }

            return value;
        }

        private bool slavecaravan_neutral_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            var partyKingdom = component.HomeSettlement.OwnerClan.Kingdom;
            if (partyKingdom == null || !component.SlaveCaravan)
            {
                return false;
            }

            if (Hero.MainHero.Clan.Kingdom == null ||
                component.HomeSettlement.OwnerClan.Kingdom != Hero.MainHero.Clan.Kingdom)
            {
                value = true;
            }

            return value;
        }

        private bool slavecaravan_amicable_on_condition()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return false;
            }

            var component = (PopulationPartyComponent) party.MobileParty.PartyComponent;
            var partyKingdom = component.HomeSettlement.OwnerClan.Kingdom;
            var heroKingdom = Hero.MainHero.Clan.Kingdom;
            if (component.SlaveCaravan && ((partyKingdom != null && heroKingdom != null && partyKingdom == heroKingdom) || component.HomeSettlement.OwnerClan == Hero.MainHero.Clan))
            {
                value = true;
            }

            return value;
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}