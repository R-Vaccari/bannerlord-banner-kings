using BannerKings.Managers.Institutions.Religions;
using HarmonyLib;
using SandBox;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKReligionsBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }


        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            this.AddDialogue(starter);
        }
        private void DailySettlementTick(Settlement settlement)
        {

        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero && target.StringId != "town_A1" || target.Town == null ||
                BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target)) return;

            ReligionData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).ReligionData;
            if (data == null || data.Clergyman == null) return;

            this.AddClergymanToKeep(data, target);
        }

        private void AddClergymanToKeep(ReligionData data, Settlement settlement)
        {
            AgentData agent = new AgentData(new SimpleAgentOrigin(data.Clergyman.Hero.CharacterObject, 0, null, default(UniqueTroopDescriptor)));
            LocationCharacter locCharacter = new LocationCharacter(agent,
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors),
                null, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture,
                LocationCharacter.CharacterRelations.Neutral, 1);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddDialogLine("bk_preacher_introduction", "lord_introduction", "lord_start",
                "{=!}{CLERGYMAN_GREETING}",
                new ConversationSentence.OnConditionDelegate(this.OnConditionClergymanGreeting), null, 100, null);


            starter.AddPlayerLine("bk_question_preaching", "lord_talk_ask_something_2", "bk_preacher_asked_preaching",
                "{=!}What are you preaching?",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null, null);

            starter.AddDialogLine("bk_answer_preaching_1", "bk_preacher_asked_preaching", "bk_preacher_asked_preaching_last",
                "{=!}{CLERGYMAN_PREACHING}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);

            starter.AddDialogLine("bk_answer_preaching_2", "bk_preacher_asked_preaching_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_PREACHING_LAST}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);


            starter.AddPlayerLine("bk_question_faith", "lord_talk_ask_something_2", "bk_preacher_asked_faith",
                "{=!}How do I prove my faith?",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null, null);

            starter.AddDialogLine("bk_answer_faith_1", "bk_preacher_asked_faith", "bk_preacher_asked_faith_last",
                "{=!}{CLERGYMAN_FAITH}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);

            starter.AddDialogLine("bk_answer_faith_2", "bk_preacher_asked_faith_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_FAITH_LAST}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);


            starter.AddPlayerLine("bk_question_faith_forbidden", "lord_talk_ask_something_2", "bk_preacher_asked_faith_forbidden",
                "{=!}What is forbidden to the faith?",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_1", "bk_preacher_asked_faith_forbidden", "bk_preacher_asked_faith_forbidden_last",
                "{=!}{CLERGYMAN_FAITH_FORBIDDEN}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_2", "bk_preacher_asked_faith_forbidden_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_FAITH__FORBIDDEN_LAST}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);

            starter.AddPlayerLine("bk_question_induction", "lord_talk_ask_something_2", "bk_preacher_asked_induction",
               "{=!}What is forbidden to the faith?",
               new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null, null);

            starter.AddDialogLine("bk_answer_induction_1", "bk_preacher_asked_induction", "bk_preacher_asked_induction_last",
                "{=!}{CLERGYMAN_INDUCTION}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);

            starter.AddDialogLine("bk_answer_induction_2", "bk_preacher_asked_induction_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_INDUCTION_LAST}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);
        }
        private bool IsPreacher() => Campaign.Current.ConversationManager.CurrentConversationIsFirst && Hero.OneToOneConversationHero.IsPreacher &&
                BannerKingsConfig.Instance.ReligionsManager != null && BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
        private bool OnConditionClergymanGreeting()
        {
            if (this.IsPreacher())
            {
                InitializePreacherTexts();
                return true;
            }
            return false;
        }
        private void InitializePreacherTexts()
        {
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
            TextObject greeting = BannerKingsConfig.Instance.ReligionsManager.IsReligionMember(Hero.MainHero, religion) ? religion.Faith.GetClergyGreetingInducted(clergyman.Rank) : religion.Faith.GetClergyGreeting(clergyman.Rank);
            MBTextManager.SetTextVariable("CLERGYMAN_GREETING", greeting, false);
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING", religion.Faith.GetClergyPreachingAnswer(clergyman.Rank), false);
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING_LAST", religion.Faith.GetClergyPreachingAnswerLast(clergyman.Rank), false);
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH", religion.Faith.GetClergyProveFaith(clergyman.Rank), false);
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_LAST", religion.Faith.GetClergyProveFaithLast(clergyman.Rank), false);
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_FORBIDDEN", religion.Faith.GetClergyForbiddenAnswer(clergyman.Rank), false);
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH__FORBIDDEN_LAST", religion.Faith.GetClergyForbiddenAnswerLast(clergyman.Rank), false);
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION", religion.Faith.GetClergyInduction(clergyman.Rank), false);
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION_LAST", religion.Faith.GetClergyInductionLast(clergyman.Rank), false);
        }
    }

    namespace Patches
    {

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_puritan_preacher_introduction_on_condition")]
        class PuritanPreacherPatch
        {
            static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager != null)
                {
                    if (Hero.OneToOneConversationHero.IsPreacher)
                    {
                        bool bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                        __result = !bannerKings;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_minor_faction_preacher_introduction_on_condition")]
        class MinorFactionPreacherPatch
        {
            static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager != null)
                {
                    if (Hero.OneToOneConversationHero.IsPreacher)
                    {
                        bool bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                        __result = !bannerKings;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_mystic_preacher_introduction_on_condition")]
        class MysticPreacherPatch
        {
            static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager != null)
                {
                    if (Hero.OneToOneConversationHero.IsPreacher)
                    {
                        bool bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                        __result = !bannerKings;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_messianic_preacher_introduction_on_condition")]
        class MessianicPatch
        {
            static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager != null)
                {
                    if (Hero.OneToOneConversationHero.IsPreacher)
                    {
                        bool bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                        __result = !bannerKings;
                    }
                }
            }
        }
    }
}