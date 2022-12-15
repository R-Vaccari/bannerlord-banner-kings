using BannerKings.Behaviours.Marriage;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Feasts
{
    public class Feast
    {
        public Feast(Hero host, List<Clan> guests, Town town, CampaignTime endDate, bool autoManaged, MarriageContract marriageContract = null)
        {
            Host = host;
            Guests = guests;
            Town = town;
            EndDate = endDate;
            MarriageContract = marriageContract;
            AutoManaged = autoManaged;
        }

        public static float FoodConsumptionRatio => 0.5f;
        public static float AlcoholConsumptionRatio => 1f;
        public static float SpiceConsumptionRatio => 0.02f;

        [SaveableProperty(1)] public Hero Host { get; private set; }
        [SaveableProperty(2)] public List<Clan> Guests { get; private set; }
        [SaveableProperty(3)] public Town Town { get; private set; }
        [SaveableProperty(4)] public CampaignTime EndDate { get; private set; }

        [SaveableProperty(5)] private int Ticks { get; set; }

        [SaveableProperty(6)] private float FoodQuantity { get; set; }
        [SaveableProperty(7)] private float FoodQuality { get; set; }
        [SaveableProperty(8)] private float FoodVariety { get; set; }
        [SaveableProperty(9)] private float Alcohol { get; set; }
        [SaveableProperty(10)] private float Spices { get; set; }
        [SaveableProperty(11)] private float HostPresence { get; set; }

        [SaveableProperty(12)] public MarriageContract MarriageContract { get; private set; }
        [SaveableProperty(13)] public bool AutoManaged { get; private set; }

        public void Tick(bool hourly = true)
        {
            if (hourly)
            {
                if (Host.CurrentSettlement == Town.Settlement)
                {
                    HostPresence++;
                }

                Ticks++;
            }
            else
            {
                var stash = Town.Settlement.Stash;
                int heroes = Town.Settlement.HeroesWithoutParty.Count;
                foreach (var party in Town.Settlement.Parties)
                {
                    if (party.IsLordParty && party.LeaderHero != null &&
                        Guests.Contains(party.LeaderHero.Clan))
                    {
                        heroes++;
                    }
                }

                var varietyList = new List<ItemObject>();
                float desiredVariety = Items.AllTradeGoods.ToList().FindAll(x => x.IsFood).Count * 0.7f;
                float desiredFood = heroes * FoodConsumptionRatio;
                float desiredAlcohol = heroes * AlcoholConsumptionRatio;

                if (AutoManaged)
                {
                    Automanage(desiredFood, desiredVariety, desiredAlcohol);
                }

                float availableQuantity = 0f;
                float availableAlcohol = 0f;
                float availableVariety = 0;
                float quality = 0;

                foreach (var element in stash)
                {
                    var item = element.EquipmentElement.Item;
                    if (item.IsFood)
                    {
                        availableQuantity += element.Amount;
                        if (!varietyList.Contains(item))
                        {
                            availableVariety++;
                            varietyList.Add(item);
                        }
                       
                        if (element.EquipmentElement.ItemModifier != null)
                        {
                            quality += element.EquipmentElement.ItemModifier.PriceMultiplier;
                        }
                        else
                        {
                            quality++;
                        }
                    }
                    else if (item.StringId == "wine" || item.StringId == "beer")
                    {
                        availableAlcohol += element.Amount;
                    }
                }

                FoodQuantity += MathF.Clamp(availableQuantity / desiredFood, 0f, 1f);
                FoodVariety += MathF.Clamp(availableVariety / desiredVariety, 0f, 1f);
                FoodQuality += MathF.Clamp(quality, 0f, 2f);
                Alcohol += MathF.Clamp(availableAlcohol / desiredAlcohol, 0f, 1f);

                int foodToConsume = (int)MathF.Min(availableQuantity, desiredFood);
                while (foodToConsume > 0)
                {
                    ItemRosterElement random = stash.GetRandomElementWithPredicate(x => x.EquipmentElement.Item.IsFood);
                    int amount = MBRandom.RandomInt(1, random.Amount);
                    foodToConsume -= amount;
                    stash.AddToCounts(random.EquipmentElement, -amount);
                }

                int alcoholToConsume = (int)MathF.Min(availableAlcohol, desiredAlcohol);
                while (alcoholToConsume > 0)
                {
                    ItemRosterElement random = stash.GetRandomElementWithPredicate(x => x.EquipmentElement.Item.StringId == "wine" ||
                            x.EquipmentElement.Item.StringId == "beer");
                    int amount = MBRandom.RandomInt(1, random.Amount);
                    alcoholToConsume -= amount;
                    stash.AddToCounts(random.EquipmentElement, -amount);
                }
            }
        }

        private void Automanage(float food, float variety, float alcohol)
        {
            int boughtFood = 0;
            int boughAlcohol = 0;
            //HashSet<ItemObject> items = new HashSet<ItemObject>();
            foreach (var element in Town.Owner.ItemRoster)
            {
                var item = element.EquipmentElement.Item;
                bool isAlcohol = item.StringId == "wine" || item.StringId == "beer";
                if (item.IsFood || isAlcohol)
                {
                    int count = MBRandom.RandomInt(1, MathF.Min(element.Amount, (int)food));
                    int cost = (int)(Town.GetItemPrice(element.EquipmentElement) * (float)count);

                    if (!isAlcohol && boughtFood >= food) continue;
                    if (isAlcohol && boughAlcohol >= alcohol) continue;

                    if (isAlcohol) boughAlcohol += count;
                    else boughtFood += count;
                    Host.ChangeHeroGold(-cost);
                    Town.ChangeGold(cost);
                    Town.Settlement.Stash.AddToCounts(element.EquipmentElement, count);
                }
            }
        }

        public void Finish(TextObject reason)
        {
            List<TextObject> goodComments = GetCompliments();
            List<TextObject> badComments = GetComplaints();
            float satisfaction = ((FoodQuality / 7f) + (FoodQuantity / 7f) +
                (FoodVariety / 7f) + (Alcohol / 7f) + (HostPresence / Ticks)) / 5f;

            if (MarriageContract != null)
            {
                Campaign.Current.GetCampaignBehavior<BKMarriageBehavior>().ApplyMarriageContract();
            }

            foreach (var clan in Guests)
            {
                foreach (var hero in clan.Heroes)
                {
                    if (hero.CurrentSettlement == Town.Settlement)
                    {
                        if (hero.PartyBelongedTo != null)
                        {
                            hero.PartyBelongedTo.Ai.EnableAi();
                        }
                        else if (clan.Fiefs.Count > 0)
                        {
                            LeaveSettlementAction.ApplyForCharacterOnly(hero);
                            EnterSettlementAction.ApplyForCharacterOnly(hero, clan.Fiefs.GetRandomElement().Settlement);
                        }
                    }
                }

                int relation = (int)MathF.Clamp((MBRandom.RandomInt(3, 8) * satisfaction - 0.5f), -10f, 20f);
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Host, clan.Leader, relation);
            }

            GainRenownAction.Apply(Host, 15 * satisfaction);
            if (Host.MapFaction == Hero.MainHero.MapFaction)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=W7RqGAhs}The feast at {TOWN} has ended! {REASON}")
                    .SetTextVariable("TOWN", Town.Name)
                    .SetTextVariable("REASON", reason));

                TextObject text;
                if (satisfaction >= 0.8f)
                {
                    text = new TextObject("{=e83AjuGY}The feast was a success! The guests praised {COMPLIMENT}.")
                        .SetTextVariable("COMPLIMENT", goodComments.GetRandomElement());
                }
                else if (satisfaction >= 0.3f && satisfaction < 0.8f)
                {
                    text = new TextObject("{=9QbnYBn0}The feast was acceptable. The guests compliment that {COMPLIMENT}, but complain that {COMPLAINT}.")
                        .SetTextVariable("COMPLIMENT", goodComments.GetRandomElement())
                        .SetTextVariable("COMPLAINT", badComments.GetRandomElement());
                }
                else
                {
                    text = new TextObject("{=bAgf4mvT}The feast was a failure... The guests complain that {COMPLAINT}.")
                        .SetTextVariable("COMPLAINT", badComments.GetRandomElement());
                }

                InformationManager.DisplayMessage(new InformationMessage(text.ToString()));
            }
        }

        private List<TextObject> GetCompliments()
        {
            var list = new List<TextObject>();
            if (FoodQuantity >= 0.8f)
            {
                list.Add(new TextObject("{=amLhmtQV}the food quantity was plentiful"));
            }
            else if (FoodQuantity >= 0.5f)
            {
                list.Add(new TextObject("{=UJefPYWS}the food quantity was adequate"));
            }

            if (FoodQuality >= 0.8f)
            {
                list.Add(new TextObject("{=cdMTu4OV}the food quality was exceptional"));
            }
            else if (FoodQuality >= 0.5f)
            {
                list.Add(new TextObject("{=VbPGdCx3}the food was tasteful"));
            }

            if (FoodVariety >= 0.8f)
            {
                list.Add(new TextObject("{=Ojj8ijxR}the food selection was bountiful"));
            }
            else if (FoodVariety >= 0.5f)
            {
                list.Add(new TextObject("{=9FSPgGmD}the food selection was sufficient"));
            }

            if (Alcohol >= 0.8f)
            {
                list.Add(new TextObject("{=pOCbabjg}there was alcohol enough to drown in"));
            }
            else if (Alcohol >= 0.5f)
            {
                list.Add(new TextObject("{=XzMKdK1S}the alcohol was adequate"));
            }

            if (HostPresence >= 0.8f)
            {
                list.Add(new TextObject("{=VmRpyXt3}the host was present at all times"));
            }
            else if (HostPresence >= 0.5f)
            {
                list.Add(new TextObject("{=q5Ton9zO}the host was mostly present"));
            }

            return list;
        }

        private List<TextObject> GetComplaints()
        {
            var list = new List<TextObject>();

            if (FoodQuantity < 0.3f)
            {
                list.Add(new TextObject("{=0McQAWdx}the food was barely enough"));
            }
            else if (FoodQuantity < 0.5f)
            {
                list.Add(new TextObject("{=ADmtXnxz}there wasn't enough food for a peasants convention, much less a feast"));
            }

            if (FoodQuality < 0.3f)
            {
                list.Add(new TextObject("{=P935z6PR}the food quality was debatable"));
            }
            else if (FoodQuality < 0.5f)
            {
                list.Add(new TextObject("{=fJvfH54Y}the food was vomit-inducing"));
            }

            if (FoodVariety < 0.3f)
            {
                list.Add(new TextObject("{=A8USsmS9}the food selection was unsatisfactory"));
            }
            else if (FoodVariety < 0.5f)
            {
                list.Add(new TextObject("{=Jeouw9gC}the food selection was miserable"));
            }

            if (Alcohol < 0.3f)
            {
                list.Add(new TextObject("{=HeZnF3Y3}there was not enough alcohol to get drunk"));
            }
            else if (Alcohol < 0.5f)
            {
                list.Add(new TextObject("{=6Lv1k7pE}the alcohol was barely enough for a teenager maiden to get drunk"));
            }

            if (HostPresence < 0.3f)
            {
                list.Add(new TextObject("{=KOtYTfHn}the host was leaving regularly"));
            }
            else if (HostPresence < 0.5f)
            {
                list.Add(new TextObject("{=s3xVTfef}the host threw a feast and yet was nowhere to be seen"));
            }

            return list;
        }
    }
}
