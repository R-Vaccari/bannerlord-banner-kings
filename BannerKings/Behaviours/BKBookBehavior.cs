using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using BannerKings.Managers.Education.Books;
using TaleWorlds.CampaignSystem.SandBox;

namespace BannerKings.Behaviours
{
    public class BKBookBehavior : CampaignBehaviorBase
    {
        private Dictionary<Hero, ItemRoster> BookSellers { get; set; } = new Dictionary<Hero, ItemRoster>();

        public override void RegisterEvents()
        {
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnWeeklyTick()
        {

            if (BookSellers.Count < DesiredSellerCount())
                SpawnInitialSellers();

            
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
        {
            if (BookSellers.ContainsKey(victim)) BookSellers.Remove(victim);
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null) return;
            if (BookSellers.Any(x => x.Key.StayingInSettlement == target) && target.IsTown)
                Utils.Helpers.AddSellerToKeep(BookSellers.First(x => x.Key.StayingInSettlement == target).Key, target);
        }

        private void SpawnInitialSellers()
        {
            List<CharacterObject> templates = CharacterObject.All.ToList().FindAll(x => x.Occupation == Occupation.Special && x.StringId.Contains("bannerkings_bookseller_"));
            foreach (CharacterObject character in templates)
            { 
                List<Settlement> currentSettlements = new List<Settlement>();
                foreach (Hero seller in BookSellers.Keys) currentSettlements.Add(seller.StayingInSettlement);
     
                Town town = Town.AllTowns.GetRandomElementWithPredicate(x => !currentSettlements.Contains(x.Settlement)
                    && x.Culture == character.Culture);
                if (town == null || town.Settlement == null) continue;

                Settlement randomSettlement = town.Settlement;
                Hero hero = HeroCreator.CreateSpecialHero(character, randomSettlement, null, null,
                       Campaign.Current.Models.AgeModel.HeroComesOfAge + 10 + MBRandom.RandomInt(60));
                hero.StayingInSettlement = randomSettlement;
                BookSellers.Add(hero, GetStartingBooks(hero.Culture));

            }
        }

        private ItemRoster GetStartingBooks(CultureObject culture)
        {
            HashSet<ItemObject> results = new HashSet<ItemObject>();
            List<(BookType, float)> candidates = new List<(BookType, float)>();
            foreach (BookType book in DefaultBookTypes.Instance.All)
            {
                float weight = 1f;
                if (book.Language.Culture == culture) weight++;
                candidates.Add((book, weight));
            }

            while (results.Count < 4) results.Add(MBRandom.ChooseWeighted(candidates).Item);
            ItemRoster roster = new ItemRoster();
            foreach (ItemObject item in results) roster.AddToCounts(item, 1);
            return roster;
        }

        private int DesiredSellerCount() => (int)(Town.AllTowns.Count / 15f);

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            
            if (BookSellers.Count == 0) SpawnInitialSellers();
            AddDialogue(campaignGameStarter);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("bk_question_preaching", "lord_talk_ask_something_2", "lord_talk_ask_something_2",
                "{=!}I would like to buy a book.",
                new ConversationSentence.OnConditionDelegate(IsBookSeller), 
                new ConversationSentence.OnConsequenceDelegate(OnBuyBookConsequence), 
                100, null, null);
        }

        private void OnBuyBookConsequence()
        {
            MobileParty party = new MobileParty();
            foreach (ItemRosterElement element in BookSellers[Hero.OneToOneConversationHero]) party.ItemRoster.Add(element);
            InventoryManager.OpenTradeWithCaravanOrAlleyParty(party, InventoryManager.InventoryCategoryType.Goods);
        }

        private bool IsBookSeller() => Hero.OneToOneConversationHero.IsSpecial && 
            Hero.OneToOneConversationHero.CharacterObject.OriginalCharacter.StringId.Contains("bannerkings_bookseller");
    }
}
