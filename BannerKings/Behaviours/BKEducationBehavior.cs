using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Skills;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours
{
    public class BKEducationBehavior : CampaignBehaviorBase
    {
        [SaveableField(1)] private Dictionary<Hero, ItemRoster> bookSellers = new();

        public override void RegisterEvents()
        {
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
            CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnHeroComesOfAge);
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.WeeklyTickEvent.AddNonSerializedListener(this, OnWeeklyTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.PerkOpenedEvent.AddNonSerializedListener(this, OnPerkOpened);
        }

        public List<Hero> GetAllBookSellers()
        {
            if (bookSellers == null)
            {
                bookSellers = new Dictionary<Hero, ItemRoster>();
            }

            return bookSellers.Keys.ToList();
        }

        public Hero GetBookSeller(Settlement settlement)
        {
            Hero hero = null;
            foreach (var key in bookSellers.Keys)
            {
                if (key.CurrentSettlement == settlement)
                {
                    hero = key;
                }
            }

            return hero;
        }

        public ItemRoster GetBookRoster(Settlement settlement)
        {
            var seller = GetBookSeller(settlement);
            if (seller != null)
            {
                return bookSellers[seller];
            }

            return null;
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData)
            {
                bookSellers = null;
            }

            dataStore.SyncData("bannerkings-booksellers", ref bookSellers);

            if (dataStore.IsLoading)
            {
                bookSellers ??= new Dictionary<Hero, ItemRoster>();
            }
        }

        private void OnPerkOpened(Hero hero, PerkObject perk)
        {
            if (hero != Hero.MainHero || perk.AlternativePerk == null || hero.GetPerkValue(perk.AlternativePerk))
            {
                return;
            }

            if (perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=Vjg2DuT1}Double Perks").ToString(),
                    new TextObject("{=eodABOkZ}From now on, double perks will be yielded for the {SKILL} skill. The perks will be rewarded after closing the Character tab with 'Done', not immediatly after selecting them.")
                        .SetTextVariable("SKILL", perk.Skill.Name)
                        .ToString(),
                    true, false,
                    GameTexts.FindText("str_selection_widget_accept").ToString(),
                    string.Empty,
                    null, null, string.Empty));
            }
            else
            {
                var skill = perk.Skill;
                if ((skill != DefaultSkills.Engineering || !hero.GetPerkValue(BKPerks.Instance.ScholarshipMechanic)) && (skill != DefaultSkills.Steward || !hero.GetPerkValue(BKPerks.Instance.ScholarshipAccountant)) && (skill != DefaultSkills.Medicine || !hero.GetPerkValue(BKPerks.Instance.ScholarshipNaturalScientist)) && (skill != DefaultSkills.Trade || !hero.GetPerkValue(BKPerks.Instance.ScholarshipTreasurer)))
                {
                    return;
                }

                hero.HeroDeveloper.AddPerk(perk.AlternativePerk);
                MBInformationManager.AddQuickInformation(new TextObject("{=nk8mBkVd}You have received the {PERK} as a double perk yield reward.")
                    .SetTextVariable("PERK", perk.AlternativePerk.Name));
            }
        }

        private void OnDailyTick(Hero hero)
        {

            if (hero == null || hero.Culture == null)
            {
                return;
            }

            BannerKingsConfig.Instance.EducationManager.UpdateHeroData(hero);
            ApplyScholarshipBedTimeStoryEffect(hero);


            if (hero.IsNotable || hero.IsLord)
            {
                if (hero.GetSkillValue(BKSkills.Instance.Scholarship) < 30 && MBRandom.RandomFloat < 0.25f)
                {
                    hero.AddSkillXp(BKSkills.Instance.Scholarship, 1);
                }

                if (hero.Clan != null)
                {
                    var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan);
                    if (council != null)
                    {
                        var councilMember = council.GetMemberFromPosition(Managers.Court.CouncilPosition.Philosopher);
                        if (councilMember != null && councilMember.Member != null)
                        {
                            var skill = 5 * councilMember.Competence;
                            hero.AddSkillXp(BKSkills.Instance.Scholarship, (int)skill);
                        }
                    }
                }
            }

            if (!hero.IsSpecial || (hero.Template != null && !hero.Template.StringId.Contains("bannerkings_bookseller_")))
            {
                return;
            }

            if (bookSellers == null)
            {
                bookSellers = new Dictionary<Hero, ItemRoster>();
            }

            if (!bookSellers.ContainsKey(hero))
            {
                bookSellers.Add(hero, GetStartingBooks(hero.Culture));
            }
        }

        private void OnWeeklyTick()
        {
            if (bookSellers.Count < DesiredSellerCount())
            {
                SpawnInitialSellers();
            }
        }

        private void OnHeroCreated(Hero hero, bool bornNaturally)
        {
            InitializeEducation(hero);
        }

        private void OnHeroComesOfAge(Hero hero)
        {
            InitializeEducation(hero, true);

            ApplyScholarshipTutorEffect(hero);
            ApplyScholarshipTeacherEffect(hero);
        }

        private void InitializeEducation(Hero hero, bool addExtraLanguages = false)
        {
            Dictionary<Language, float> startingLanguages = null;
            if (hero.Clan != null && hero != hero.Clan.Leader && hero.Clan.Leader != null)
            {
                hero.Culture = hero.Clan.Leader.Culture;
                startingLanguages = new Dictionary<Language, float>();

                var leaderEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero.Clan.Leader);
                var native = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(hero.Culture);
                startingLanguages.Add(native, 1f);

                if (addExtraLanguages)
                {
                    foreach (var tuple in leaderEducation.Languages)
                    {
                        if (tuple.Key == native)
                        {
                            continue;
                        }

                        if (tuple.Value > 0.5f && MBRandom.RandomFloat < 0.15f && !startingLanguages.ContainsKey(tuple.Key))
                        {
                            startingLanguages.Add(tuple.Key, MBRandom.RandomFloatRanged(0.5f, tuple.Value));
                        }
                    }
                }
            }

            var currentEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
            if (currentEducation != null)
            {
                if (startingLanguages != null && startingLanguages.Count > 1)
                {
                    var languages = currentEducation.Languages;
                    foreach (var tuple in startingLanguages)
                    {
                        if (languages.ContainsKey(tuple.Key))
                        {
                            continue;
                        }

                        currentEducation.AddLanguageWithProgress(tuple.Key, tuple.Value);
                    }
                   
                }
                return;
            }

            BannerKingsConfig.Instance.EducationManager.InitHeroEducation(hero, startingLanguages);
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
        {
            if (bookSellers.ContainsKey(victim))
            {
                bookSellers.Remove(victim);
            }

            BannerKingsConfig.Instance.EducationManager.RemoveHero(victim);
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null)
            {
                return;
            }

            if (bookSellers.Any(x => x.Key.StayingInSettlement == target) && target.IsTown)
            {
                Utils.Helpers.AddCharacterToKeep(bookSellers.First(x => x.Key.StayingInSettlement == target).Key, target);
            }
        }

        private void SpawnInitialSellers()
        {
            var templates = CharacterObject.All.ToList().FindAll(x =>
                x.Occupation == Occupation.Special && x.StringId.Contains("bannerkings_bookseller_"));
            foreach (var character in templates)
            {
                if (bookSellers.Keys.Any(x => x.Culture == character.Culture))
                {
                    continue;
                }

                var currentSettlements = new List<Settlement>();
                foreach (var seller in bookSellers.Keys)
                {
                    currentSettlements.Add(seller.StayingInSettlement);
                }

                var town = Town.AllTowns.GetRandomElementWithPredicate(x => !currentSettlements.Contains(x.Settlement)
                                                                            && x.Culture == character.Culture);
                if (town?.Settlement == null)
                {
                    continue;
                }

                var randomSettlement = town.Settlement;
                var hero = HeroCreator.CreateSpecialHero(character, randomSettlement, null, null,
                    Campaign.Current.Models.AgeModel.HeroComesOfAge + 10 + MBRandom.RandomInt(60));
                var firstName = hero.IsFemale
                    ? hero.Culture.FemaleNameList.GetRandomElement()
                    : hero.Culture.MaleNameList.GetRandomElement();
                hero.SetName(character.Name.SetTextVariable("FIRSTNAME", firstName), firstName);
                hero.StayingInSettlement = randomSettlement;
                bookSellers.Add(hero, GetStartingBooks(hero.Culture));
            }
        }

        private ItemRoster GetStartingBooks(CultureObject culture)
        {
            var results = new HashSet<ItemObject>();
            var candidates = new List<(BookType, float)>();
            foreach (var book in DefaultBookTypes.Instance.All)
            {
                var weight = 1f;
                if (book.Language.Culture == culture)
                {
                    weight++;
                }

                candidates.Add((book, weight));
            }

            while (results.Count < 4)
            {
                results.Add(MBRandom.ChooseWeighted(candidates).Item);
            }

            var roster = new ItemRoster();
            foreach (var item in results)
            {
                roster.AddToCounts(item, 1);
            }

            return roster;
        }

        private int DesiredSellerCount()
        {
            return (int) (Town.AllTowns.Count / 15f);
        }

        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
           
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogue(campaignGameStarter);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("bk_question_books", "hero_main_options", "book_seller_buy",
                "{=yKxDFTQ3}I would like to buy a book.",
                IsBookSeller,
                OnBuyBookConsequence);

            starter.AddDialogLine("book_seller_buy",
                "book_seller_buy",
                "hero_main_options",
                "{=!}This is my literature collection, {PLAYER.NAME}. Be sure to not damage them.",
                null,
                null);

            starter.AddPlayerLine("bk_question_preaching", "hero_main_options", "book_seller_topics",
                "{=!}I would like to ask you scholarly questions.",
                IsBookSeller,
                null);

            starter.AddDialogLine("book_seller_topics",
                "book_seller_topics", 
                "book_seller_scholarly_questions",
                "{=!}Surely, {PLAYER.NAME}. What would you like to learn about?",
                IsBookSeller,
                null);

            starter.AddPlayerLine("book_seller_scholarly_questions", 
                "book_seller_scholarly_questions", 
                "book_seller_topic_books",
                "{=!}How can make use of books?",
                null,
                null);

            starter.AddPlayerLine("book_seller_scholarly_questions", 
                "book_seller_scholarly_questions", 
                "book_seller_topic_languages",
                "{=!}How can I make use of languages?",
                null,
                null);

            starter.AddPlayerLine("book_seller_scholarly_questions",
                "book_seller_scholarly_questions",
                "hero_main_options",
                "{=G4ALCxaA}Never mind.",
                null,
                null);

            starter.AddDialogLine("book_seller_topic_books",
               "book_seller_topic_books",
               "book_seller_topics",
               "{=!}Most books will serve to increase your learning in a certain Skill set. They allow you insight into the many martial arts and competences a noble requires, and may be used with all your family. Spouses our spouse candidates may also be interested in you reciting them poems. All cultures have their own books, so you must usually know the book's language. Alternatively you may use the Dictionarium Calradium, to read anything, although slowly. Of course, you must be Literate first.",
               null,
               null);

            starter.AddDialogLine("book_seller_topic_languages",
               "book_seller_topic_languages",
               "book_seller_topics",
               "{=!}{PLAYER.NAME}, languages are key for your scholarly progress. Languages will allow you to read books from foreign cultures. As a lord, they also allow you to better connect with foreign populations. Folks will always mistrust a foreigner, but more so one that takes no interest in their customs. Have a family member or servant that knows a language well and trusts you, and they will be able to instruct you.",
               null,
               null);

            starter.AddPlayerLine("lord_meet_player_response3", "lord_meet_player_response", "lord_introduction",
                "{=!}My name is {PLAYER.NAME}, {?CONVERSATION_NPC.GENDER}madam{?}sir{\\?}. May I ask your name?. (You ask in {NPC_LANGUAGE})",
                OnMeetLanguageCondition,
                OnMeetLanguageConsequence);
        }

        private bool OnMeetLanguageCondition()
        {
            var speakslLanguage = false;
            if (BannerKingsConfig.Instance.EducationManager != null)
            {
                var playerLanguage = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(Hero.MainHero);
                var npcLanguage =
                    BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(Hero.OneToOneConversationHero);
                if (npcLanguage != null && playerLanguage != null && playerLanguage != npcLanguage)
                {
                    var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
                    if (data.GetLanguageFluency(npcLanguage) >= 0.5f)
                    {
                        speakslLanguage = true;
                    }

                    MBTextManager.SetTextVariable("NPC_LANGUAGE", npcLanguage.Name);
                }
            }


            return Campaign.Current.ConversationManager.CurrentConversationIsFirst && speakslLanguage;
        }

        private void OnMeetLanguageConsequence()
        {
            var npcLanguage =
                BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(Hero.OneToOneConversationHero);
            var relation = 6 * BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero)
                .GetLanguageFluency(npcLanguage);
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, (int) relation, false);
        }

        private void OnBuyBookConsequence()
        {
            var elements = new List<InquiryElement>();
            var allBooks = DefaultBookTypes.Instance.All;
            foreach (var element in GetBookRoster(Hero.MainHero.CurrentSettlement))
            {
                var item = element.EquipmentElement.Item;
                var book = allBooks.FirstOrDefault(x => x.Item == element.EquipmentElement.Item);
                var price = book.Item.Value * 1000;

                var hint = $"{book.Description}";

                if (book.Skill != null)
                {
                    hint += Environment.NewLine + book.Skill.Name.ToString();
                }

                hint += Environment.NewLine + new TextObject("{=1c9TOPzH}{GOLD_AMOUNT}{GOLD_ICON}")
                    .SetTextVariable("GOLD_AMOUNT", price)
                .ToString();


                elements.Add(new InquiryElement(book, new TextObject("{=e8KTkKtX}{BOOK} ({LANGUAGE})")
                    .SetTextVariable("BOOK", item.Name)
                    .SetTextVariable("LANGUAGE", book.Language.Name).ToString(),
                    null, Hero.MainHero.Gold >= price,
                    hint));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=DNAVAvqp}Acquire Book").ToString(),
                new TextObject("{=2sftq1sF}Books can be read by those with the Literate perk. Skill books add xp to a specific skill while Focus books add both xp and a focus point, if possible. Dictionaries are used to help reading other books faster.")
                .ToString(),
                elements,
                true,
                1,
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    var book = (BookType)selectedOptions.First().Identifier;
                    Hero.MainHero.ChangeHeroGold(-book.Item.Value * 1000);
                    Hero.MainHero.PartyBelongedTo.ItemRoster.AddToCounts(book.Item, 1);
                },
                null,
                string.Empty));
        }

        private bool IsBookSeller()
        {
            return Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.IsSpecial &&
                Hero.OneToOneConversationHero.CharacterObject != null &&
                Hero.OneToOneConversationHero.CharacterObject.OriginalCharacter != null && 
                Hero.OneToOneConversationHero.CharacterObject.OriginalCharacter.StringId.Contains("bannerkings_bookseller");
        }

        private static void ApplyScholarshipBedTimeStoryEffect(Hero hero)
        {
            if (!hero.GetPerkValue(BKPerks.Instance.ScholarshipBedTimeStory))
            {
                return;
            }

            var skillObjects = Game.Current.ObjectManager.GetObjectTypeList<SkillObject>();

            var companions = hero.CompanionsInParty;
            foreach (var companion in companions)
            {
                var randomSkill = skillObjects.GetRandomElement();

                companion.AddSkillXp(randomSkill, MBRandom.RandomFloatRanged(1, 4));
            }
        }

        private static void ApplyScholarshipTutorEffect(Hero hero)
        {
            hero.HeroDeveloper.UnspentAttributePoints += hero.Clan.Heroes.Count(_ => hero.GetPerkValue(BKPerks.Instance.ScholarshipTutor));
        }

        private static void ApplyScholarshipTeacherEffect(Hero hero)
        {
            var additionalFocusPoints = 0;
            if (hero.Father?.GetPerkValue(BKPerks.Instance.ScholarshipTeacher) == true)
            {
                additionalFocusPoints += 1;
            }

            if (hero.Mother?.GetPerkValue(BKPerks.Instance.ScholarshipTeacher) == true)
            {
                additionalFocusPoints += 1;
            }

            hero.HeroDeveloper.UnspentFocusPoints += additionalFocusPoints;
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(Attributes), "All", MethodType.Getter)]
        internal class AttributesPatch
        {
            private static bool Prefix(ref MBReadOnlyList<CharacterAttribute> __result)
            {
                var list = new List<CharacterAttribute>(BKAttributes.AllAttributes);
                list.Remove(BKAttributes.Instance.Wisdom);
                __result = list.GetReadOnlyList();
                return false;
            }
        }
    }
}