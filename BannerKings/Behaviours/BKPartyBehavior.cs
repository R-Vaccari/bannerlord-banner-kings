using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Components;
using BannerKings.Managers.Items;
using BannerKings.Managers.Populations;
using BannerKings.Settings;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
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
        private ItemObject chicken;
        private ItemObject goose;

        private ItemObject Chicken
        {
            get
            {
                if (chicken == null) chicken = TaleWorlds.CampaignSystem.Campaign.Current.ObjectManager.GetObject<ItemObject>("chicken");
                return chicken;
            }
        }

        private ItemObject Goose
        {
            get
            {
                if (goose == null) goose = TaleWorlds.CampaignSystem.Campaign.Current.ObjectManager.GetObject<ItemObject>("goose");
                return goose;
            }
        }

        private Dictionary<Settlement, List<Settlement>> travelCache;

        public override void RegisterEvents()
        {
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, HourlyTickParty);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener(this, OnSiegeStarted);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        private void OnSettlementLeft(MobileParty mobileParty, Settlement settlement)
        {
            if (!mobileParty.IsCaravan) return;

            if (settlement.Town == null) return;

            float space = mobileParty.InventoryCapacity * 0.8f - mobileParty.TotalWeightCarried;
            if (space > 5f)
            {
                foreach (var element in settlement.Party.ItemRoster)
                {
                    float price = settlement.Town.GetItemPrice(element.EquipmentElement, mobileParty);
                    if (price < element.EquipmentElement.ItemValue * 0.33f)
                    {
                        int count = (int)MathF.Min(space / element.EquipmentElement.Item.Weight, (float)element.Amount);
                        if (count > 0)
                        {
                            int cost = (int)(count * price);
                            if (mobileParty.PartyTradeGold >= cost)
                            {
                                mobileParty.ItemRoster.AddToCounts(element.EquipmentElement, count);
                                mobileParty.PartyTradeGold -= cost;
                                settlement.Town.Owner.ItemRoster.AddToCounts(element.EquipmentElement, -count);
                                settlement.Town.ChangeGold(cost);
                            }
                        }
                    }
                }
            }
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            foreach (var party in MobileParty.All)
            {
                var memberElements = party.MemberRoster.GetTroopRoster();
                var memberList = new MBList<TroopRosterElement>(memberElements.Count);
                memberList.AddRange(memberElements);
                foreach (var element in memberList)
                {
                    if (element.Character == null || element.Character.Culture == null)
                    {
                        memberElements.Remove(element);
                    }
                }

                var prisonElements = party.PrisonRoster.GetTroopRoster();
                var prisonList = new MBList<TroopRosterElement>(prisonElements.Count);
                prisonList.AddRange(prisonElements);
                foreach (var element in prisonList)
                {
                    if (element.Character == null || element.Character.Culture == null)
                    {
                        prisonElements.Remove(element);
                    }
                }

                if (party.ActualClan != null && party.LeaderHero != null && party.LeaderHero.Clan == null)
                {
                    party.RemovePartyLeader();
                }
            }
        }

        private void OnDailyTick(MobileParty party)
        {
            if (!party.IsActive) return;

            float flock = party.ItemRoster.GetItemNumber(Chicken) +
                party.ItemRoster.GetItemNumber(Goose);

            if (flock > 0)
            {
                int eggs = 0;
                for (int i = 0; i < flock; i++)
                    if (MBRandom.RandomFloat < 0.05f) eggs++;             

                if (eggs > 0) party.ItemRoster.AddToCounts(BKItems.Instance.Egg, eggs);
            }
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
                if (party.PartyComponent != null && party.PartyComponent is PopulationPartyComponent)
                {
                    toRemove.Add(party);
                }
            }

            foreach (var party in toRemove)
            {
                DestroyPartyAction.Apply(null, party);
            }
        }

        private void HourlyTickParty(MobileParty party)
        {
            if (party.LeaderHero != null && party.LeaderHero.Clan == null)
            {
                KillCharacterAction.ApplyByMurder(party.LeaderHero);
            }

            VillagersCastleTick(party);
            AddCustomPartyBehaviors(party);
            GoBuyFood(party);
        }

        private void GoBuyFood(MobileParty lordParty)
        {
            if (!lordParty.IsLordParty || lordParty == MobileParty.MainParty) return;

            if (lordParty.Army != null || lordParty.MapEvent != null) return;

            if (lordParty.Ai.DefaultBehavior != AiBehavior.PatrolAroundPoint) return;

            if (TaleWorlds.CampaignSystem.Campaign.Current.Models.MobilePartyFoodConsumptionModel.DoesPartyConsumeFood(lordParty) &&
                lordParty.TotalFoodAtInventory < (int)(MathF.Abs(lordParty.FoodChange * 5f)))
            {
                Settlement settlement = SettlementHelper.FindNearestSettlement((Settlement settlement) =>
                {
                    return (settlement.Town != null || settlement.IsVillage) && settlement.MapFaction != null &&
                    !settlement.MapFaction.IsAtWarWith(lordParty.MapFaction) && settlement.ItemRoster.TotalFood > 0;
                },
                lordParty);
                if (settlement != null) lordParty.Ai.SetMoveGoToSettlement(settlement);
            }
        }

        private void TryBuyingFood(MobileParty mobileParty, Settlement settlement)
        {
            if (TaleWorlds.CampaignSystem.Campaign.Current.GameStarted && mobileParty.LeaderHero != null && settlement.IsCastle && 
                TaleWorlds.CampaignSystem.Campaign.Current.Models.MobilePartyFoodConsumptionModel.DoesPartyConsumeFood(mobileParty) && 
                (mobileParty.Army == null || mobileParty.Army.LeaderParty == mobileParty) && 
                (settlement.IsVillage || (mobileParty.MapFaction != null && !mobileParty.MapFaction.IsAtWarWith(settlement.MapFaction))) && 
                settlement.ItemRoster.TotalFood > 0)
            {
                PartyFoodBuyingModel partyFoodBuyingModel = TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyFoodBuyingModel;
                float minimumDaysToLast = settlement.IsVillage ? partyFoodBuyingModel.MinimumDaysFoodToLastWhileBuyingFoodFromVillage : partyFoodBuyingModel.MinimumDaysFoodToLastWhileBuyingFoodFromTown;
                if (mobileParty.Army == null)
                {
                    this.BuyFoodInternal(mobileParty, settlement, this.CalculateFoodCountToBuy(mobileParty, minimumDaysToLast));
                    return;
                }
            }
        }

        private void BuyFoodInternal(MobileParty mobileParty, Settlement settlement, int numberOfFoodItemsNeededToBuy)
        {
            if (!mobileParty.IsMainParty)
            {
                for (int i = 0; i < numberOfFoodItemsNeededToBuy; i++)
                {
                    ItemRosterElement subject;
                    float num;
                    TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyFoodBuyingModel.FindItemToBuy(mobileParty, settlement, out subject, out num);
                    if (subject.EquipmentElement.Item == null)
                    {
                        break;
                    }
                    if (num <= (float)mobileParty.LeaderHero.Gold)
                    {
                        SellItemsAction.Apply(settlement.Party, mobileParty.Party, subject, 1, null);
                    }
                    if (subject.EquipmentElement.Item.HasHorseComponent && subject.EquipmentElement.Item.HorseComponent.IsLiveStock)
                    {
                        i += subject.EquipmentElement.Item.HorseComponent.MeatCount - 1;
                    }
                }
            }
        }

        private int CalculateFoodCountToBuy(MobileParty mobileParty, float minimumDaysToLast)
        {
            float num = (float)mobileParty.TotalFoodAtInventory / -mobileParty.FoodChange;
            float num2 = minimumDaysToLast - num;
            if (num2 > 0f)
            {
                return (int)(-mobileParty.FoodChange * num2);
            }
            return 0;
        }

        private void VillagersCastleTick(MobileParty villagerParty)
        {
            if (!villagerParty.IsVillager || villagerParty.MapEvent != null)
            {
                return;
            }
           
            if (villagerParty.CurrentSettlement != null && villagerParty.CurrentSettlement.IsCastle)
            {
                villagerParty.Ai.SetMoveGoToSettlement(villagerParty.HomeSettlement);
            }
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
                return;
            }

            bkComponent.TickHourly();
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
            CharacterObject civilian = settlement.Culture.Villager;
            if (civilian == null) return;

            var target = GetTownsToTravel(settlement);
            if (target.IsEmpty()) return;
            
            foreach (var town in target)
            {
                if (settlement.MapFaction.IsAtWarWith(town.MapFaction)) continue;

                var random = MBRandom.RandomInt(1, 100);
                if (random > 20) continue;
     
                int count = MBRandom.RandomInt(12, 25);
                var name = "{=ds9BcMxr}Traders from {ORIGIN}";

                if (civilian != null)
                {
                    MobileParty tradersParty = PopulationPartyComponent.CreateTravellerParty("travellers_", 
                        settlement,
                        town,
                        name, 
                        count,
                        PopType.Tenants, 
                        civilian, 
                        true);

                    int budget = 1000 + (int)(settlement.Town.Prosperity / 10f);
                    var localData = settlement.Town.MarketData;
                    var targetData = town.Town.MarketData;
                    var townStock = settlement.Town.Owner.ItemRoster;
                    float traderCapacity = tradersParty.InventoryCapacity * 0.95f;

                    foreach (var element in townStock)
                    {
                        if (budget <= 5) break;

                        if (tradersParty.TotalWeightCarried >= traderCapacity) break;

                        EquipmentElement equipment = element.EquipmentElement;
                        ItemCategory category = equipment.Item.ItemCategory;
                        if (settlement.Town.GetItemCategoryPriceIndex(category) < 0.15f)
                        {
                            var price = localData.GetPrice(equipment, null, true);
                            int totalCount = MBMath.ClampInt((int)(budget / (float)price), 0, element.Amount);
                            if (totalCount > 1f)
                            {
                                townStock.AddToCounts(equipment, -totalCount);
                                town.Town.ChangeGold((int)(price * (float)totalCount));
                                tradersParty.ItemRoster.AddToCounts(new EquipmentElement(equipment.Item, equipment.ItemModifier),
                                    totalCount);
                            }
                        }
                    }

                    foreach (var element in townStock)
                    {
                        if (budget <= 5) break;

                        if (tradersParty.TotalWeightCarried >= traderCapacity) break;

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
                                town.Town.ChangeGold((int)(price * (float)totalCount));
                                tradersParty.ItemRoster.AddToCounts(new EquipmentElement(equipment.Item, equipment.ItemModifier),
                                    totalCount);
                            }
                        }
                    }

                    break;
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

            var target = GetTownsToTravel(settlement).FirstOrDefault();
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
            if (party == null || party == MobileParty.MainParty || Utils.Helpers.IsNonBaseGameSettlement(target))
            {
                return;
            }

            if (party.IsLordParty) TryBuyingFood(party, target);

            AddRealisticIncome(party, target);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target);
            if (data == null)
            {
                return;
            }

            AddGarrisonParty(party, target, data);
            AddCaravanFees(party, target, data);
            AddPopulationPartyBehavior(party, target, data);
        }

        private void AddGarrisonParty(MobileParty party, Settlement settlement, PopulationData data)
        {
            if (party.PartyComponent is GarrisonPartyComponent)
            {
                var component = party.PartyComponent as GarrisonPartyComponent;
                if (settlement != component.HomeSettlement)
                {
                    return;
                }

                if (settlement.Town.GarrisonParty == null)
                {
                    settlement.AddGarrisonParty();
                }

                foreach (var element in party.MemberRoster.GetTroopRoster())
                {
                    settlement.Town.GarrisonParty.MemberRoster.AddToCounts(element.Character, 
                        element.Number, 
                        false, 
                        element.WoundedNumber);
                }

                foreach (var element in party.PrisonRoster.GetTroopRoster())
                {
                    bool hero = element.Character.IsHero;
                    if (!hero)
                    {
                        data.UpdatePopType(PopType.Slaves, element.Number, true);
                    }
                    else
                    {
                        TakePrisonerAction.Apply(settlement.Party, element.Character.HeroObject);
                    }
                }

                DestroyPartyAction.Apply(null, party);
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

        private bool DecideSendSlaveCaravan(Settlement settlement)
        {
            if (!settlement.IsTown || settlement.Town == null)
            {
                return false;
            }

            var villages = settlement.BoundVillages;
            return villages is {Count: > 0} && BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement) != null &&
                BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Slaves);
        }

        private List<Settlement> GetTownsToTravel(Settlement origin)
        {
            List<Settlement> list;  
            if (travelCache == null)
            {
                travelCache = new Dictionary<Settlement, List<Settlement>>(Town.AllFiefs.Count());
            }

            if (!travelCache.TryGetValue(origin, out list)) 
            {
                list = new List<Settlement>();
                foreach (var fortification in Town.AllFiefs)
                {
                    if (fortification.Settlement == origin) continue;

                    if (!origin.MapFaction.IsAtWarWith(fortification.MapFaction))
                    {
                        if (TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(fortification.Settlement, origin) < 100f)
                            list.Add(fortification.Settlement);
                    }
                }

                travelCache[origin] = list;
            }
            

            return list;
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

            var name = new TextObject("{=xEwX83aU}Travelling {CLASS} from {ORIGIN}")
                .SetTextVariable("CLASS", Utils.Helpers.GetClassName(type, origin.Culture))
                .SetTextVariable("ORIGIN", origin.Name)
                .ToString();

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
            //WipeTraders();
        }

        private void WipeTraders()
        {
            var list = new List<MobileParty>();
            foreach (var party in MobileParty.All)
            {
                if (party.PartyComponent is PopulationPartyComponent)
                {
                    var component = party.PartyComponent as PopulationPartyComponent;
                    if (component.Trading && (component.TargetSettlement == null || component.HomeSettlement == null || 
                        component.Name.ToString().IsEmpty()))
                    {
                        list.Add(party);
                    }
                }
            }

            foreach (var party in list)
            {
                DestroyPartyAction.Apply(null, party);
            }
        }

        private void AddDialog(CampaignGameStarter starter)
        {
            starter.AddDialogLine("traveller_serf_party_start", "start", "traveller_party_greeting",
                "{=q8yhpkxZ}{?PLAYER.GENDER}M'lady!{?}M'lord!{\\?} We are humble folk, travelling between towns, looking for work and trade.",
                traveller_serf_start_on_condition, null);

            starter.AddDialogLine("traveller_craftsman_party_start", "start", "traveller_party_greeting",
                "{=yJ91Ooas}Good day to you. We are craftsmen travelling for business purposes.",
                traveller_craftsman_start_on_condition, null);

            starter.AddDialogLine("traveller_noble_party_start", "start", "traveller_party_greeting",
                "{=eetBXdvK}Yes? Please do not interfere with our caravan.",
                traveller_noble_start_on_condition, null);


            starter.AddPlayerLine("traveller_party_loot", "traveller_party_greeting", "close_window",
                new TextObject("{=dOcj05n6}Whatever you have, I'm taking it. Surrender or die!").ToString(),
                traveller_aggression_on_condition,
                delegate { PlayerEncounter.Current.IsEnemy = true; });

            starter.AddPlayerLine("traveller_party_leave", "traveller_party_greeting", "close_window",
                new TextObject("{=zhRJeYOY}Carry on, then. Farewell.").ToString(), null,
                delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddDialogLine("slavecaravan_friend_party_start", "start", "slavecaravan_party_greeting",
                "{=jqSXkN5h}{?PLAYER.GENDER}My lady{?}My lord{\\?}, we are taking these rabble somewhere they can be put to good use.",
                slavecaravan_amicable_on_condition, null);

            starter.AddDialogLine("slavecaravan_neutral_party_start", "start", "slavecaravan_party_greeting",
                "{=WTVAdsFN}If you're not planning to join those vermin back there, move away![rf:idle_angry][ib:aggressive]",
                slavecaravan_neutral_on_condition, null);

            starter.AddPlayerLine("slavecaravan_party_leave", "slavecaravan_party_greeting", "close_window",
                new TextObject("{=zhRJeYOY}Carry on, then. Farewell.").ToString(), null,
                delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddPlayerLine("slavecaravan_party_threat", "slavecaravan_party_greeting", "slavecaravan_threat",
                new TextObject("{=Nk8tSdcu}Give me your slaves and gear, or else!").ToString(),
                slavecaravan_neutral_on_condition,
                null);

            starter.AddDialogLine("slavecaravan_party_threat_response", "slavecaravan_threat", "close_window",
                "{=18j2nO70}One more for the mines! Lads, get the whip![rf:idle_angry][ib:aggressive]",
                null, delegate { PlayerEncounter.Current.IsEnemy = true; });

            starter.AddDialogLine("raised_militia_party_start", "start", "raised_militia_greeting",
                "{=SRg8cwUN}{?PLAYER.GENDER}M'lady!{?}M'lord!{\\?} We are ready to serve you.",
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
                "{=Oo88Sazm}Aye!",
                null, delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddDialogLine("raised_retinue_party_start", "start", "raised_retinue_greeting",
               "{=SRg8cwUN}{?PLAYER.GENDER}M'lady!{?}M'lord!{\\?} We are ready to serve you.",
               IsPlayerEstateRetinue, null);

            starter.AddPlayerLine("retinue_party_follow", "raised_retinue_greeting", "retinue_order",
               new TextObject("{=Hvi96rXx}Follow my company.").ToString(),
               () => true,
               () =>
               {
                   var party = PlayerEncounter.EncounteredParty;
                   var component = (EstateComponent)party.MobileParty.PartyComponent;
                   component.Behavior = AiBehavior.EscortParty;
                   component.Escort = MobileParty.MainParty;
               });

            starter.AddPlayerLine("retinue_party_retreat", "raised_retinue_greeting", "retinue_order",
                new TextObject("{=xPvsVw4b}You may go home.").ToString(),
                () => true,
                () =>
                {
                    var party = PlayerEncounter.EncounteredParty;
                    var component = (EstateComponent)party.MobileParty.PartyComponent;
                    component.Behavior = AiBehavior.GoToSettlement;
                });

            starter.AddDialogLine("retinue_order_response", "retinue_order", "close_window",
                "{=Oo88Sazm}Aye!",
                null, delegate { PlayerEncounter.LeaveEncounter = true; });

            starter.AddDialogLine("retinue_continue", "retinue_continue", "raised_retinue_greeting",
                "{=sFJ0pObc}Anything else?",
                null,
                null);

            starter.AddPlayerLine("retinue_party_retreat", "raised_retinue_greeting", "retinue_continue",
                new TextObject("{=UKybEESB}Let me check your ranks.").ToString(),
                () => true,
                () =>
                {
                    PartyScreenManager.OpenScreenAsManageTroopsAndPrisoners(PlayerEncounter.EncounteredParty.MobileParty);
                });

            starter.AddPlayerLine("retinue_party_leave", "raised_retinue_greeting", "close_window",
                "{=G4ALCxaA}Never mind.",
                () => true,
                delegate { PlayerEncounter.LeaveEncounter = true; });
        }

        private bool IsTravellerParty(PartyBase party)
        {
            var value = false;
            if (party is not { MobileParty: { } })
            {
                return false;
            }

            try
            {
                if (party.MobileParty.PartyComponent != null && party.MobileParty.PartyComponent is PopulationPartyComponent)
                {
                    value = true;
                }

                if (party.MobileParty.PartyComponent is not PopulationPartyComponent)
                {
                    value = false;
                }
            } catch (Exception ex) { }

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

        private bool IsPlayerEstateRetinue()
        {
            var value = false;
            var party = PlayerEncounter.EncounteredParty;
            if (!IsTravellerParty(party))
            {
                return value;
            }

            if (party.MobileParty.PartyComponent is EstateComponent)
            {
                value = (party.MobileParty.PartyComponent as EstateComponent).Estate.Owner == Hero.MainHero;
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