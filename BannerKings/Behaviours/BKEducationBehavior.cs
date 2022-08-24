using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Education.Books;
using BannerKings.Managers.Skills;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours
{
    public class BKEducationBehavior : CampaignBehaviorBase
    {
        [SaveableField(1)] private Dictionary<Hero, ItemRoster> bookSellers = new();

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
            foreach (var key in bookSellers.Keys)
            {
                if (key.CurrentSettlement == settlement)
                {
                    hero = key;
                }
            }

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
            {
                bookSellers ??= new Dictionary<Hero, ItemRoster>();
            }
        }

        private void OnPerkOpened(Hero hero, PerkObject perk)
        {
            if (perk.AlternativePerk == null || hero.GetPerkValue(perk.AlternativePerk))
            {
                return;
            }

            if (perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic || perk == BKPerks.Instance.ScholarshipMechanic)
            {
                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=4f2Qdr8vx}Double Perks").ToString(),
                    new TextObject("{=M7KF5X9jT}From now on, double perks will be yielded for the {SKILL} skill. The perks will be rewarded after closing the Character tab with 'Done', not immediatly after selecting them.")
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
                MBInformationManager.AddQuickInformation(new TextObject("{=XEAcLkgaB}You have received the {PERK} as a double perk yield reward.")
                    .SetTextVariable("PERK", perk.AlternativePerk.Name));
            }
        }

        private void OnDailyTick(Hero hero)
        {
            BannerKingsConfig.Instance.EducationManager.UpdateHeroData(hero);
            ApplyScholarshipBedTimeStoryEffect(hero);

            if (!hero.IsSpecial || !hero.Template.StringId.Contains("bannerkings_bookseller_"))
            {
                return;
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

        private void OnHeroComesOfAge(Hero hero)
        {
            BannerKingsConfig.Instance.EducationManager.InitHeroEducation(hero);

            ApplyScholarshipTutorEffect(hero);
            ApplyScholarshipTeacherEffect(hero);
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
                Utils.Helpers.AddSellerToKeep(bookSellers.First(x => x.Key.StayingInSettlement == target).Key, target);
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
            BannerKingsConfig.Instance.EducationManager.PostInitialize();
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogue(campaignGameStarter);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("bk_question_preaching", "lord_talk_ask_something_2", "lord_talk_ask_something",
                "{=j1tMvaVj6}I would like to buy a book.",
                IsBookSeller,
                OnBuyBookConsequence);


            starter.AddPlayerLine("lord_meet_player_response3", "lord_meet_player_response", "lord_introduction",
                "{=NmGJs7yB}My name is {PLAYER.NAME}, {?CONVERSATION_NPC.GENDER}madam{?}sir{\\?}. May I ask your name?." +
                " (You ask in {NPC_LANGUAGE})",
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
            var party = new MobileParty();
            foreach (var element in bookSellers[Hero.OneToOneConversationHero])
            {
                party.ItemRoster.Add(element);
            }

            InventoryManager.OpenTradeWithCaravanOrAlleyParty(party, InventoryManager.InventoryCategoryType.Goods);
        }

        private bool IsBookSeller()
        {
            return Hero.OneToOneConversationHero.IsSpecial &&
                   Hero.OneToOneConversationHero.CharacterObject.OriginalCharacter.StringId.Contains(
                       "bannerkings_bookseller");
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
            if (hero.Father.GetPerkValue(BKPerks.Instance.ScholarshipTeacher))
            {
                additionalFocusPoints += 1;
            }

            if (hero.Mother.GetPerkValue(BKPerks.Instance.ScholarshipTeacher))
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