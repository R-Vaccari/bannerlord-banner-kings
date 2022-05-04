using BannerKings.Managers.Institutions.Religions;
using HarmonyLib;
using SandBox;
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
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }


        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddDialogue(starter);
        }
        private void DailySettlementTick(Settlement settlement)
        {

        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero && target.StringId != "town_A1" || target.Town == null) return;
            
            ReligionData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).ReligionData;
            if (data == null || data.Clergyman == null) return;

             AddClergymanToKeep(data, target);
        }

        private void AddClergymanToKeep(ReligionData data, Settlement settlement)
        {
            AgentData agent = new AgentData(new SimpleAgentOrigin(data.Clergyman.Hero.CharacterObject, 0));
            LocationCharacter locCharacter = new LocationCharacter(agent, 
                SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors, 
                null, true, LocationCharacter.CharacterRelations.Neutral, null, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture,
                LocationCharacter.CharacterRelations.Neutral, 1);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddDialogLine("bk_preacher_introduction", "lord_introduction", "lord_start", 
                "{=!}{CLERGYMAN_GREETING}", 
                OnConditionClergymanGreeting, null);

            
            starter.AddPlayerLine("bk_question_preaching", "lord_talk_ask_something_2", "bk_preacher_asked_preaching",
                "{=!}What are you preaching?", 
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_preaching_1", "bk_preacher_asked_preaching", "bk_preacher_asked_preaching_last",
                "{=!}{CLERGYMAN_PREACHING}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_preaching_2", "bk_preacher_asked_preaching_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_PREACHING_LAST}",
                IsPreacher, null);


            starter.AddPlayerLine("bk_question_faith", "lord_talk_ask_something_2", "bk_preacher_asked_faith",
                "{=!}How do I prove my faith?",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_1", "bk_preacher_asked_faith", "bk_preacher_asked_faith_last",
                "{=!}{CLERGYMAN_FAITH}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_2", "bk_preacher_asked_faith_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_FAITH_LAST}",
                IsPreacher, null);


            starter.AddPlayerLine("bk_question_faith_forbidden", "lord_talk_ask_something_2", "bk_preacher_asked_faith_forbidden",
                "{=!}What is forbidden to the faith?",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_1", "bk_preacher_asked_faith_forbidden", "bk_preacher_asked_faith_forbidden_last",
                "{=!}{CLERGYMAN_FAITH_FORBIDDEN}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_2", "bk_preacher_asked_faith_forbidden_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_FAITH__FORBIDDEN_LAST}",
                IsPreacher, null);

            starter.AddPlayerLine("bk_question_induction", "lord_talk_ask_something_2", "bk_preacher_asked_induction",
               "{=!}What is forbidden to the faith?",
               IsPreacher, null);

            starter.AddDialogLine("bk_answer_induction_1", "bk_preacher_asked_induction", "bk_preacher_asked_induction_last",
                "{=!}{CLERGYMAN_INDUCTION}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_induction_2", "bk_preacher_asked_induction_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_INDUCTION_LAST}",
                IsPreacher, null);
        }
        private bool IsPreacher() => Campaign.Current.ConversationManager.CurrentConversationIsFirst && Hero.OneToOneConversationHero.IsPreacher &&
                BannerKingsConfig.Instance.ReligionsManager != null && BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
        private bool OnConditionClergymanGreeting()
        {
            if (IsPreacher())
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
            MBTextManager.SetTextVariable("CLERGYMAN_GREETING", greeting);
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING", religion.Faith.GetClergyPreachingAnswer(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING_LAST", religion.Faith.GetClergyPreachingAnswerLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH", religion.Faith.GetClergyProveFaith(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_LAST", religion.Faith.GetClergyProveFaithLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_FORBIDDEN", religion.Faith.GetClergyForbiddenAnswer(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH__FORBIDDEN_LAST", religion.Faith.GetClergyForbiddenAnswerLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION", religion.Faith.GetClergyInduction(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION_LAST", religion.Faith.GetClergyInductionLast(clergyman.Rank));
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
