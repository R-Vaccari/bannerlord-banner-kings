using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Models.BKModels;
using HarmonyLib;
using SandBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKReligionsBehavior : CampaignBehaviorBase
    {
        private Divinity selectedDivinity;
        private Rite selectedRite;
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(DailyTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddDialogue(starter);
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
        {
            BannerKingsConfig.Instance.ReligionsManager.RemoveHero(victim);
        }

        private void DailyTick()
        {
            BKPietyModel model = ((BKPietyModel)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKPietyModel)));
            foreach (Religion religion in BannerKingsConfig.Instance.ReligionsManager.GetReligions())
                foreach (Hero hero in BannerKingsConfig.Instance.ReligionsManager.GetFaithfulHeroes(religion))
                    BannerKingsConfig.Instance.ReligionsManager.AddPiety(religion, hero, model.CalculateEffect(hero).ResultNumber);
        }

        private void DailySettlementTick(Settlement settlement)
        {

        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null ||
                BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target)) 
                return;

            ReligionData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).ReligionData;
            if (data == null || data.Clergyman == null) return;

            Utils.Helpers.AddSellerToKeep(data.Clergyman.Hero, target);
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
               "{=!}I would like to be inducted.",
               new ConversationSentence.OnConditionDelegate(IsPreacher), 
               null, 100, 
               new ConversationSentence.OnClickableConditionDelegate(InductionOnClickable), 
               null);

            starter.AddDialogLine("bk_answer_induction_1", "bk_preacher_asked_induction", "bk_preacher_asked_induction_last",
                "{=!}{CLERGYMAN_INDUCTION}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);

            starter.AddDialogLine("bk_answer_induction_2", "bk_preacher_asked_induction_last", "lord_talk_ask_something",
                "{=!}{CLERGYMAN_INDUCTION_LAST}",
                new ConversationSentence.OnConditionDelegate(this.IsPreacher), null, 100, null);



            starter.AddPlayerLine("bk_question_boon", "lord_talk_ask_something_2", "bk_preacher_asked_boon",
                "{=!}{CLERGYMAN_BLESSING_ACTION}",
                new ConversationSentence.OnConditionDelegate(IsPreacher),
                new ConversationSentence.OnConsequenceDelegate(BlessingOnConsequence), 
                100, 
                new ConversationSentence.OnClickableConditionDelegate(BlessingOnClickable), 
                null);

            starter.AddDialogLine("bk_answer_boon", "bk_preacher_asked_boon", "bk_preacher_asked_boon_answer",
                "{=!}{CLERGYMAN_BLESSING_QUESTION}",
                null, new ConversationSentence.OnConsequenceDelegate(BlessingPositiveAnswerOnConsequence), 100, null);

            starter.AddPlayerLine("bk_preacher_asked_boon_answer", "bk_preacher_asked_boon_answer", "bk_boon_confirm",
              "{=!}I have decided.", null, 
              null, 
              100, null, null);

            starter.AddPlayerLine("bk_preacher_asked_boon_answer", "bk_preacher_asked_boon_answer", "lord_talk_ask_something",
              "{=D33fIGQe}Never mind.", null, null, 100, null, null);

            starter.AddDialogLine("bk_boon_confirm", "bk_boon_confirm", "bk_boon_confirm",
                "{=!}{CLERGYMAN_BLESSING_CONFIRM}",
                null, null, 100, null);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "lord_talk_ask_something", "{=!}See it done.",
                () => selectedDivinity != null, new ConversationSentence.OnConsequenceDelegate(BlessingConfirmOnConsequence),
                100, null, null);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "lord_talk_ask_something", "{=D33fIGQe}Never mind.",
                null, null, 100, null, null);




            starter.AddPlayerLine("bk_question_rite", "lord_talk_ask_something_2", "bk_preacher_asked_rites",
                "{=!}I would like to perform a rite.",
                new ConversationSentence.OnConditionDelegate(IsPreacher),
                new ConversationSentence.OnConsequenceDelegate(RitesOnConsequence), 
                100, 
                new ConversationSentence.OnClickableConditionDelegate(RitesOnClickable), 
                null);

            starter.AddDialogLine("bk_answer_rite", "bk_preacher_asked_rites", "bk_preacher_asked_rites_answer",
                "{=!}{CLERGYMAN_RITE}",
                null, 
                new ConversationSentence.OnConsequenceDelegate(RitesPositiveAnswerOnConsequence), 
                100,
                null);

            starter.AddDialogLine("bk_answer_rite_impossible", "bk_preacher_asked_rites", "lord_talk_ask_something",
                "{=!}I am afraid that won't be possible.",
                new ConversationSentence.OnConditionDelegate(IsPreacher), 
                null, 
                100, 
                null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer", "bk_rite_confirm",
              "{=!}I have decided.", null,
              null,
              100, null, null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer", "lord_talk_ask_something",
              "{=D33fIGQe}Never mind.", null,
              null,
              100, null, null);

            starter.AddDialogLine("bk_rite_confirm", "bk_rite_confirm", "bk_rite_confirm",
                "{=!}{CLERGYMAN_RITE_CONFIRM}",
                null, null, 100, null);
            starter.AddPlayerLine("bk_rite_confirm", "bk_rite_confirm", "lord_talk_ask_something", "{=!}See it done.",
                null, () => { if (selectedRite != null) selectedRite.Complete(Hero.MainHero); },
                100, null, null);
            starter.AddPlayerLine("bk_rite_confirm", "bk_rite_confirm", "lord_talk_ask_something", "{=D33fIGQe}Never mind.",
                null, null, 100, null, null);

        }

        private bool InductionOnClickable(out TextObject hintText)
        {
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
            Religion playerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);

            if (playerReligion.Faith.GetId() == religion.Faith.GetId())
            {
                hintText = new TextObject("{=!}Already an adherent of this faith.");
                return false;
            }

            (bool, TextObject) result = religion.Faith.GetInductionAllowed(Hero.MainHero, clergyman.Rank);
            if (!result.Item1)
            {
                hintText = result.Item2;
                return false;
            }

            hintText = new TextObject("{=!}{POSSIBLE}. Changing faiths will significantly impact your clan's renown. Your piety in the new faith will be zero. Lords of {CURRENT_FAITH} faith may disapprove your change.")
                .SetTextVariable("POSSIBLE", result.Item2)
                .SetTextVariable("CURRENT_FAITH", playerReligion.Faith.GetFaithName());
            return true;
        }

        private void BlessingPositiveAnswerOnConsequence()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            float piety = BannerKingsConfig.Instance.ReligionsManager.GetPiety(religion, Hero.MainHero);

            foreach (Divinity div in religion.Faith.GetSecondaryDivinities())
                list.Add(new InquiryElement(div, 
                    div.Name.ToString(), 
                    null, 
                    piety >= div.BlessingCost, 
                    new TextObject("{=!}{DESCRIPTION}\n{EFFECTS}")
                    .SetTextVariable("DESCRIPTION", div.Description)
                    .SetTextVariable("EFFECTS", div.Effects)
                    .ToString()));

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    religion.Faith.GetSecondaryDivinitiesDescription().ToString(), 
                    new TextObject("{=!}Select which of the {SECONDARIES} you would like to {BLESSING_ACTION}.")
                    .SetTextVariable("SECONDARIES", religion.Faith.GetSecondaryDivinitiesDescription())
                    .SetTextVariable("BLESSING_ACTION", religion.Faith.GetBlessingActionName())
                    .ToString(), 
                    list, 
                    false, 1,
                    GameTexts.FindText("str_done", null).ToString(), string.Empty,
                    delegate (List<InquiryElement> x)
                    {
                        Divinity divinity = (Divinity?)x[0].Identifier;
                        selectedDivinity = divinity;
                    },
                    null, 
                    string.Empty), false);
        }

        private void BlessingConfirmOnConsequence()
        {
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
            BannerKingsConfig.Instance.ReligionsManager.AddBlessing(selectedDivinity, Hero.MainHero, religion);
        }

        private bool BlessingOnClickable(out TextObject hintText)
        {
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
            Religion playerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);

            if (religion.Faith.GetId() != playerReligion.Faith.GetId())
            {
                hintText = new TextObject("{=!}You do not adhere to the {FAITH} faith.")
                    .SetTextVariable("FAITH", religion.Faith.GetFaithName());
                return false;
            }

            float piety = BannerKingsConfig.Instance.ReligionsManager.GetPiety(playerReligion, Hero.MainHero);
            float minPiety = 500f;
            bool anyPossible = false;
            foreach (Divinity divinity in religion.Faith.GetSecondaryDivinities())
            {
                bool optionPossible = piety >= divinity.BlessingCost;
                if (optionPossible) anyPossible = true;
                if (divinity.BlessingCost < minPiety) minPiety = divinity.BlessingCost;
            }

            if (!anyPossible)
            {
                hintText = new TextObject("{=!}Not enough piety to receive any blessing (minimum {PIETY} piety).")
                    .SetTextVariable("PIETY", minPiety);
                return false;
            }

            hintText = new TextObject("{=!}Expend your piety in exchange of a blessing or inspiration.");
            return true;
        }

        private void BlessingOnConsequence()
        {
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);

            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_CONFIRM", religion.Faith.GetBlessingConfirmQuestion());
            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_QUESTION", religion.Faith.GetBlessingQuestion());

        }

        private bool RitesOnClickable(out TextObject hintText)
        {
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
            Religion playerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);

            if (religion.Faith.GetId() != playerReligion.Faith.GetId())
            {
                hintText = new TextObject("{=!}You do not adhere to the {FAITH} faith.")
                    .SetTextVariable("FAITH", religion.Faith.GetFaithName());
                return false;
            }

            bool anyPossible = false;
            foreach (Rite rite in religion.Rites)
                if (rite.MeetsCondition(Hero.MainHero))
                {
                    anyPossible = true;
                    break;
                }
 
            if (!anyPossible)
            {
                hintText = new TextObject("{=!}No rite is currently possible to perform.");
                return false;
            }

            hintText = new TextObject("{=!}Perform a rite such as an offering in exchange for piety.");
            return true;
        }

        private void RitesPositiveAnswerOnConsequence()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            float piety = BannerKingsConfig.Instance.ReligionsManager.GetPiety(religion, Hero.MainHero);

            foreach (Rite rite in religion.Rites)
                list.Add(new InquiryElement(rite, rite.GetName().ToString(), null, rite.MeetsCondition(Hero.MainHero), rite.GetDescription().ToString()));

            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    religion.Faith.GetSecondaryDivinitiesDescription().ToString(),
                    string.Empty, list,
                    false, 1,
                    GameTexts.FindText("str_done", null).ToString(), string.Empty,
                    delegate (List<InquiryElement> x)
                    {
                        Rite rite = (Rite?)x[0].Identifier;
                        selectedRite = rite;
                        rite.Execute(Hero.MainHero);
                    },
                    null,
                    string.Empty), false);
        }

        private void RitesOnConsequence()
        {
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
               
            TextObject faithText = new TextObject("{=!}{FAITH} teaches us that we may perform {RITES}.");
            TextObject riteText = new TextObject("{=!}Certainly, {HERO}. Remember that proving your devotion is a life-long process. Once a rite is done, some time is needed before it may be consummated again. {RITES}")
                .SetTextVariable("HERO", Hero.MainHero.Name);

            StringBuilder sb = new StringBuilder();
            foreach (Rite rite in religion.Rites)
                sb.Append(rite.GetName().ToString() + ", ");
            

            MBTextManager.SetTextVariable("CLERGYMAN_RITE", riteText.SetTextVariable("RITES", faithText
                .SetTextVariable("FAITH", religion.Faith.GetFaithName())
                .SetTextVariable("RITES", sb.ToString())), false);

        }

        private bool CanPerformRite(RiteType rite)
        {
            bool possible = false;
            Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            if (clergyman != null)
            {
                Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
                if (religion != null) possible = religion.Rites.Any(x => x.GetRiteType() == rite);
            }

            return possible;
        }

        private bool IsPreacher() => Hero.OneToOneConversationHero.IsPreacher &&
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
            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_ACTION", religion.Faith.GetBlessingAction());
        }
    }

    namespace Patches
    {
        /*[HarmonyPatch(typeof(Hero), "IsNotable", MethodType.Getter)]
        class IsNotablePatch
        {
            static void Postfix(Hero __instance, ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager != null && __instance.IsPreacher && BannerKingsConfig.Instance.ReligionsManager.IsPreacher(__instance))
                {
                    Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(__instance));
                    __result = religion.Doctrines.Contains("druidism");
                }
            }
        } */


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