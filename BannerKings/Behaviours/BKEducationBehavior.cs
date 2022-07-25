using System.Linq;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using BannerKings.Managers.Education.Books;
using TaleWorlds.Localization;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Education;
using HarmonyLib;
using BannerKings.Managers.Skills;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using System.Reflection;

namespace BannerKings.Behaviours
{
    public class BKEducationBehavior : CampaignBehaviorBase
    {
        [SaveableField(1)]
        private Dictionary<Hero, ItemRoster> bookSellers  = new Dictionary<Hero, ItemRoster>();

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnHeroComesOfAge);
            CampaignEvents.PerkOpenedEvent.AddNonSerializedListener(this, OnPerkOpened);
        }

        public Hero GetBookSeller(Settlement settlement)
        {
            Hero hero = null;
            foreach (Hero key in bookSellers.Keys)
                if (key.CurrentSettlement == settlement)
                    hero = key;

            return hero;
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData)
            {
                bookSellers = null;
            }

            dataStore.SyncData("bannerkings-booksellers", ref bookSellers);

            if (dataStore.IsLoading)
                if (bookSellers == null) bookSellers = new Dictionary<Hero, ItemRoster>();
        }

        private void OnPerkOpened(Hero hero, PerkObject perk)
        {
            if (perk.AlternativePerk != null && !hero.GetPerkValue(perk.AlternativePerk))
            {

                if (perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic ||
                    perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic)
                {
                    InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Double Perks").ToString(), 
                        new TextObject("{=!}From now on, double perks will be yielded for the {SKILL} skill. The perks will be rewarded after closing the Character tab with 'Done', not immediatly after selecting them.")
                        .SetTextVariable("SKILL", perk.Skill.Name)
                        .ToString(),
                        true, false, 
                        GameTexts.FindText("str_selection_widget_accept").ToString(), 
                        string.Empty, 
                        null, null, string.Empty));
                } else
                {
                    SkillObject skill = perk.Skill;
                    if ((skill == DefaultSkills.Engineering && hero.GetPerkValue(BKPerks.Instance.ScholarshipMechanic)) || (skill == DefaultSkills.Steward && hero.GetPerkValue(BKPerks.Instance.ScholarshipAccountant))
                        || (skill == DefaultSkills.Medicine && hero.GetPerkValue(BKPerks.Instance.ScholarshipNaturalScientist)) || (skill == DefaultSkills.Trade && hero.GetPerkValue(BKPerks.Instance.ScholarshipTreasurer)))
                    {
                        hero.HeroDeveloper.SetPropertyValue(perk.AlternativePerk, 1);
                        InformationManager.AddQuickInformation(new TextObject("{=!}You have received the {PERK} as a double perk yield reward.")
                            .SetTextVariable("PERK", perk.AlternativePerk.Name));
                    }
                }
            }
        }

        private void OnDailyTick(Hero hero)
        {
            BannerKingsConfig.Instance.EducationManager.UpdateHeroData(hero);
            if (hero.IsSpecial && hero.Template.StringId.Contains("bannerkings_bookseller_"))
                if (!bookSellers.ContainsKey(hero))
                    bookSellers.Add(hero, GetStartingBooks(hero.Culture));
        }

        private void OnWeeklyTick()
        {
            if (bookSellers.Count < DesiredSellerCount())
                SpawnInitialSellers();
        }

        private void OnHeroComesOfAge(Hero hero) 
        {
            BannerKingsConfig.Instance.EducationManager.InitHeroEducation(hero);
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
        {
            if (bookSellers.ContainsKey(victim)) bookSellers.Remove(victim);
            BannerKingsConfig.Instance.EducationManager.RemoveHero(victim);
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null) return;
            if (bookSellers.Any(x => x.Key.StayingInSettlement == target) && target.IsTown)
                Utils.Helpers.AddSellerToKeep(bookSellers.First(x => x.Key.StayingInSettlement == target).Key, target);
        }

        private void SpawnInitialSellers()
        {
            List<CharacterObject> templates = CharacterObject.All.ToList().FindAll(x => x.Occupation == Occupation.Special && x.StringId.Contains("bannerkings_bookseller_"));
            foreach (CharacterObject character in templates)
            {
                if (bookSellers.Keys.Any(x => x.Culture == character.Culture)) continue;

                List<Settlement> currentSettlements = new List<Settlement>();
                foreach (Hero seller in bookSellers.Keys) currentSettlements.Add(seller.StayingInSettlement);

                Town town = Town.AllTowns.GetRandomElementWithPredicate(x => !currentSettlements.Contains(x.Settlement)
                    && x.Culture == character.Culture);
                if (town == null || town.Settlement == null) continue;

                Settlement randomSettlement = town.Settlement;
                Hero hero = HeroCreator.CreateSpecialHero(character, randomSettlement, null, null,
                       Campaign.Current.Models.AgeModel.HeroComesOfAge + 10 + MBRandom.RandomInt(60));
                TextObject firstName = hero.IsFemale ? hero.Culture.FemaleNameList.GetRandomElement() : hero.Culture.MaleNameList.GetRandomElement();
                hero.SetName(character.Name.SetTextVariable("FIRSTNAME", firstName), firstName);
                hero.StayingInSettlement = randomSettlement;
                bookSellers.Add(hero, GetStartingBooks(hero.Culture));

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

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            BannerKingsConfig.Instance.EducationManager.PostInitialize();
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogue(campaignGameStarter);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("bk_question_preaching", "lord_talk_ask_something_2", "lord_talk_ask_something",
                "{=!}I would like to buy a book.",
                new ConversationSentence.OnConditionDelegate(IsBookSeller), 
                new ConversationSentence.OnConsequenceDelegate(OnBuyBookConsequence), 
                100, null, null);



            starter.AddPlayerLine("lord_meet_player_response3", "lord_meet_player_response", "lord_introduction",
                "{=NmGJs7yB}My name is {PLAYER.NAME}, {?CONVERSATION_NPC.GENDER}madam{?}sir{\\?}. May I ask your name?." + " (You ask in {NPC_LANGUAGE})", 
                new ConversationSentence.OnConditionDelegate(OnMeetLanguageCondition), 
                new ConversationSentence.OnConsequenceDelegate(OnMeetLanguageConsequence), 
                100, null, null);
        }

        private bool OnMeetLanguageCondition()
        {
            bool speakslLanguage = false;
            if (BannerKingsConfig.Instance.EducationManager != null)
            {
                Language playerLanguage = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(Hero.MainHero);
                Language npcLanguage = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(Hero.OneToOneConversationHero);
                if (npcLanguage != null && playerLanguage != null && playerLanguage != npcLanguage)
                {
                    EducationData data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
                    if (data.GetLanguageFluency(npcLanguage) >= 0.5f)
                        speakslLanguage = true;

                    MBTextManager.SetTextVariable("NPC_LANGUAGE", npcLanguage.Name);
                }
            }
            

            return Campaign.Current.ConversationManager.CurrentConversationIsFirst && speakslLanguage;
        }

        private void OnMeetLanguageConsequence()
        {
            Language npcLanguage = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(Hero.OneToOneConversationHero);
            float relation = 6 * BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero).GetLanguageFluency(npcLanguage);
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, (int)relation, false, true);
        }

        private void OnBuyBookConsequence()
        {
            MobileParty party = new MobileParty();
            foreach (ItemRosterElement element in bookSellers[Hero.OneToOneConversationHero]) party.ItemRoster.Add(element);
            InventoryManager.OpenTradeWithCaravanOrAlleyParty(party, InventoryManager.InventoryCategoryType.Goods);
        }

        private bool IsBookSeller() => Hero.OneToOneConversationHero.IsSpecial && 
            Hero.OneToOneConversationHero.CharacterObject.OriginalCharacter.StringId.Contains("bannerkings_bookseller");

        
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(Attributes), "All", MethodType.Getter)]
        class AttributesPatch
        {
            static bool Prefix(ref MBReadOnlyList<CharacterAttribute> __result)
            {

                List<CharacterAttribute> list = new List<CharacterAttribute>(BKAttributes.AllAttributes);
                list.Remove(BKAttributes.Instance.Wisdom);
                __result = list.GetReadOnlyList();
                return false;
            }
        }
    }
}
