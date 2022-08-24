using System.Collections.Generic;
using System.Linq;
using System.Text;
using BannerKings.Managers;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Models.BKModels;
using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKReligionsBehavior : CampaignBehaviorBase
    {
        private static ReligionsManager ReligionsManager => BannerKingsConfig.Instance.ReligionsManager;

        private Divinity selectedDivinity;
        private Rite selectedRite;

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
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

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            ReligionsManager.PostInitialize();
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
        {
            ReligionsManager.RemoveHero(victim);
        }

        private void DailyTick()
        {
            foreach (var religion in ReligionsManager.GetReligions())
            {
                religion.UpdateFervor();
                foreach (var hero in ReligionsManager.GetFaithfulHeroes(religion))
                {
                    ReligionsManager.AddPiety(religion, hero, BannerKingsConfig.Instance.PietyModel.CalculateEffect(hero).ResultNumber);
                }
            }
            
        }

        private void DailySettlementTick(Settlement settlement)
        {
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null || BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target))
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).ReligionData;
            if (data?.Clergyman == null)
            {
                return;
            }

            Utils.Helpers.AddSellerToKeep(data.Clergyman.Hero, target);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddDialogLine("bk_preacher_introduction", "lord_introduction", "lord_start",
                "{=xaHChKa8P}{CLERGYMAN_GREETING}",
                OnConditionClergymanGreeting, null);

            starter.AddPlayerLine("bk_question_preaching", "hero_main_options", "bk_preacher_asked_preaching",
                "{=iye5QaO8A}What are you preaching?",
                OnConditionClergymanGreeting, null);

            starter.AddDialogLine("bk_answer_preaching_1", "bk_preacher_asked_preaching",
                "bk_preacher_asked_preaching_last",
                "{=dvkTEycwm}{CLERGYMAN_PREACHING}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_preaching_2", "bk_preacher_asked_preaching_last",
                "hero_main_options",
                "{=vcsKOEJwb}{CLERGYMAN_PREACHING_LAST}",
                IsPreacher, null);


            starter.AddPlayerLine("bk_question_faith", "hero_main_options", "bk_preacher_asked_faith",
                "{=WGkmFBPKE}How do I prove my faith?",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_1", "bk_preacher_asked_faith", "bk_preacher_asked_faith_last",
                "{=QAdRVqbAJ}{CLERGYMAN_FAITH}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_2", "bk_preacher_asked_faith_last", "hero_main_options",
                "{=rA2NMg8BT}{CLERGYMAN_FAITH_LAST}",
                IsPreacher, null);


            starter.AddPlayerLine("bk_question_faith_forbidden", "hero_main_options",
                "bk_preacher_asked_faith_forbidden",
                "{=s49hNEVBO}What is forbidden to the faith?",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_1", "bk_preacher_asked_faith_forbidden",
                "bk_preacher_asked_faith_forbidden_last",
                "{=jr5ZkoFJ9}{CLERGYMAN_FAITH_FORBIDDEN}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_2", "bk_preacher_asked_faith_forbidden_last",
                "hero_main_options",
                "{=GUAxyjhxP}{CLERGYMAN_FAITH__FORBIDDEN_LAST}",
                IsPreacher, null);

            starter.AddPlayerLine("bk_question_induction", "hero_main_options", "bk_preacher_asked_induction",
                "{=Xz3pu5Chh}I would like to be inducted.",
                IsPreacher,
                null, 100,
                InductionOnClickable);

            starter.AddDialogLine("bk_answer_induction_1", "bk_preacher_asked_induction",
                "bk_preacher_asked_induction_last",
                "{=Xu6Wf7s3N}{CLERGYMAN_INDUCTION}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_induction_2", "bk_preacher_asked_induction_last",
                "hero_main_options",
                "{=JkSsCyXSX}{CLERGYMAN_INDUCTION_LAST}",
                IsPreacher, null);


            starter.AddPlayerLine("bk_question_boon", "hero_main_options", "bk_preacher_asked_boon",
                "{=SMQviDkTv}{CLERGYMAN_BLESSING_ACTION}",
                IsPreacher,
                BlessingOnConsequence,
                100,
                BlessingOnClickable);

            starter.AddDialogLine("bk_answer_boon", "bk_preacher_asked_boon", "bk_preacher_asked_boon_answer",
                "{=CXBcR5xmx}{CLERGYMAN_BLESSING_QUESTION}",
                null, BlessingPositiveAnswerOnConsequence);

            starter.AddPlayerLine("bk_preacher_asked_boon_answer", "bk_preacher_asked_boon_answer", "bk_boon_confirm",
                "{=LifQbUMya}I have decided.", null,
                null);

            starter.AddPlayerLine("bk_preacher_asked_boon_answer", "bk_preacher_asked_boon_answer",
                "hero_main_options",
                "{=D33fIGQe}Never mind.", null, null);

            starter.AddDialogLine("bk_boon_confirm", "bk_boon_confirm", "bk_boon_confirm",
                "{=ztOiL44Kp}{CLERGYMAN_BLESSING_CONFIRM}",
                null, null);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "hero_main_options", "{=7DmQFxW3X}See it done.",
                () => selectedDivinity != null, BlessingConfirmOnConsequence);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "hero_main_options",
                "{=D33fIGQe}Never mind.",
                null, null);


            starter.AddPlayerLine("bk_question_rite", "hero_main_options", "bk_preacher_asked_rites",
                "{=qVy2pJ36L}I would like to perform a rite.",
                IsPreacher,
                RitesOnConsequence,
                100,
                RitesOnClickable);

            starter.AddDialogLine("bk_answer_rite", "bk_preacher_asked_rites", "bk_preacher_asked_rites_answer",
                "{=BebXpPuot}{CLERGYMAN_RITE}",
                null,
                RitesPositiveAnswerOnConsequence);

            starter.AddDialogLine("bk_answer_rite_impossible", "bk_preacher_asked_rites", "hero_main_options",
                "{=u28B6J9DZ}I am afraid that won't be possible.",
                IsPreacher,
                null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer", "bk_rite_confirm",
                "{=LifQbUMya}I have decided.", null,
                null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer",
                "hero_main_options",
                "{=D33fIGQe}Never mind.", null,
                null);

            starter.AddDialogLine("bk_rite_confirm", "bk_rite_confirm", "bk_rite_confirm",
                "{=xFxKTLyGd}{CLERGYMAN_RITE_CONFIRM}",
                null, null);
            starter.AddPlayerLine("bk_rite_confirm", "bk_rite_confirm", "hero_main_options", "{=7DmQFxW3X}See it done.",
                null, () =>
                {
                    selectedRite?.Complete(Hero.MainHero);
                });
            starter.AddPlayerLine("bk_rite_confirm", "bk_rite_confirm", "hero_main_options",
                "{=D33fIGQe}Never mind.",
                null, null);
        }

        private bool InductionOnClickable(out TextObject hintText)
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            var playerReligion = ReligionsManager.GetHeroReligion(Hero.MainHero);

            if (playerReligion.Faith.GetId() == religion.Faith.GetId())
            {
                hintText = new TextObject("{=4PT2Su54j}Already an adherent of this faith.");
                return false;
            }

            var result = religion.Faith.GetInductionAllowed(Hero.MainHero, clergyman.Rank);
            if (!result.Item1)
            {
                hintText = result.Item2;
                return false;
            }

            hintText = new TextObject(
                    "{=ExGaGTcKb}{POSSIBLE}. Changing faiths will significantly impact your clan's renown. Your piety in the new faith will be zero. Lords of {CURRENT_FAITH} faith may disapprove your change.")
                .SetTextVariable("POSSIBLE", result.Item2)
                .SetTextVariable("CURRENT_FAITH", playerReligion.Faith.GetFaithName());
            return true;
        }

        private void BlessingPositiveAnswerOnConsequence()
        {
            var religion = ReligionsManager.GetHeroReligion(Hero.MainHero);
            var piety = ReligionsManager.GetPiety(religion, Hero.MainHero);

            var list = religion.Faith.GetSecondaryDivinities()
                .Select(div => new InquiryElement(div, div.Name.ToString(), null, piety >= div.BlessingCost, new TextObject("{=BdOKFFj8S}{DESCRIPTION}\n{EFFECTS}").SetTextVariable("DESCRIPTION", div.Description)
                    .SetTextVariable("EFFECTS", div.Effects)
                    .ToString()))
                .ToList();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                religion.Faith.GetSecondaryDivinitiesDescription().ToString(),
                new TextObject("{=ApGAeJkkJ}Select which of the {SECONDARIES} you would like to {BLESSING_ACTION}.")
                    .SetTextVariable("SECONDARIES", religion.Faith.GetSecondaryDivinitiesDescription())
                    .SetTextVariable("BLESSING_ACTION", religion.Faith.GetBlessingActionName())
                    .ToString(),
                list,
                false, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate(List<InquiryElement> x)
                {
                    var divinity = (Divinity?) x[0].Identifier;
                    selectedDivinity = divinity;
                },
                null,
                string.Empty));
        }

        private void BlessingConfirmOnConsequence()
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            ReligionsManager.AddBlessing(selectedDivinity, Hero.MainHero, religion);
        }

        private bool BlessingOnClickable(out TextObject hintText)
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            var playerReligion = ReligionsManager.GetHeroReligion(Hero.MainHero);

            if (religion.Faith.GetId() != playerReligion.Faith.GetId())
            {
                hintText = new TextObject("{=82aTF8pFN}You do not adhere to the {FAITH} faith.")
                    .SetTextVariable("FAITH", religion.Faith.GetFaithName());
                return false;
            }

            var piety = ReligionsManager.GetPiety(playerReligion, Hero.MainHero);
            var minPiety = 500f;
            var anyPossible = false;
            foreach (var divinity in religion.Faith.GetSecondaryDivinities())
            {
                var optionPossible = piety >= divinity.BlessingCost;
                if (optionPossible)
                {
                    anyPossible = true;
                }

                if (divinity.BlessingCost < minPiety)
                {
                    minPiety = divinity.BlessingCost;
                }
            }

            if (!anyPossible)
            {
                hintText = new TextObject("{=LMBYqZ0dd}Not enough piety to receive any blessing (minimum {PIETY} piety).")
                    .SetTextVariable("PIETY", minPiety);
                return false;
            }

            hintText = new TextObject("{=55g4RjXwx}Expend your piety in exchange of a blessing or inspiration.");
            return true;
        }

        private void BlessingOnConsequence()
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);

            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_CONFIRM", religion.Faith.GetBlessingConfirmQuestion());
            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_QUESTION", religion.Faith.GetBlessingQuestion());
        }

        private bool RitesOnClickable(out TextObject hintText)
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            var playerReligion = ReligionsManager.GetHeroReligion(Hero.MainHero);

            if (religion.Faith.GetId() != playerReligion.Faith.GetId())
            {
                hintText = new TextObject("{=82aTF8pFN}You do not adhere to the {FAITH} faith.")
                    .SetTextVariable("FAITH", religion.Faith.GetFaithName());
                return false;
            }

            var anyPossible = religion.Rites.Any(rite => rite.MeetsCondition(Hero.MainHero));
            if (!anyPossible)
            {
                hintText = new TextObject("{=0RGqdhQDn}No rite is currently possible to perform.");
                return false;
            }

            hintText = new TextObject("{=eiz6RSUiF}Perform a rite such as an offering in exchange for piety.");
            return true;
        }

        private void RitesPositiveAnswerOnConsequence()
        {
            var religion = ReligionsManager.GetHeroReligion(Hero.MainHero);
            var piety = ReligionsManager.GetPiety(religion, Hero.MainHero);

            var list = religion.Rites.Select(rite => new InquiryElement(rite, rite.GetName().ToString(), null, rite.MeetsCondition(Hero.MainHero), rite.GetDescription().ToString())).ToList();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                religion.Faith.GetSecondaryDivinitiesDescription().ToString(),
                string.Empty, list,
                false, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate(List<InquiryElement> x)
                {
                    var rite = (Rite?) x[0].Identifier;
                    selectedRite = rite;
                    rite.Execute(Hero.MainHero);
                },
                null,
                string.Empty));
        }

        private void RitesOnConsequence()
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);

            var faithText = new TextObject("{=oYUXOLfjc}{FAITH} teaches us that we may perform {RITES}.");
            var riteText =
                new TextObject(
                        "{=HkmHFtrxN}Certainly, {HERO}. Remember that proving your devotion is a life-long process. Once a rite is done, some time is needed before it may be consummated again. {RITES}")
                    .SetTextVariable("HERO", Hero.MainHero.Name);

            var sb = new StringBuilder();
            foreach (var rite in religion.Rites)
            {
                sb.Append(rite.GetName() + ", ");
            }


            MBTextManager.SetTextVariable("CLERGYMAN_RITE", riteText.SetTextVariable("RITES", faithText
                .SetTextVariable("FAITH", religion.Faith.GetFaithName())
                .SetTextVariable("RITES", sb.ToString())));
        }

        private bool CanPerformRite(RiteType rite)
        {
            var possible = false;
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            if (clergyman == null)
            {
                return false;
            }

            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            if (religion != null)
            {
                possible = religion.Rites.Any(x => x.GetRiteType() == rite);
            }

            return possible;
        }

        private static bool IsPreacher()
        {
            return Hero.OneToOneConversationHero.IsPreacher &&
                   BannerKingsConfig.Instance.ReligionsManager != null &&
                   ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
        }

        private bool OnConditionClergymanGreeting()
        {
            if (!IsPreacher())
            {
                return false;
            }

            InitializePreacherTexts();
            return true;

        }

        private void InitializePreacherTexts()
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            var greeting = ReligionsManager.IsReligionMember(Hero.MainHero, religion)
                ? religion.Faith.GetClergyGreetingInducted(clergyman.Rank)
                : religion.Faith.GetClergyGreeting(clergyman.Rank);

            MBTextManager.SetTextVariable("CLERGYMAN_GREETING", greeting);
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING",
                religion.Faith.GetClergyPreachingAnswer(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING_LAST",
                religion.Faith.GetClergyPreachingAnswerLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH", religion.Faith.GetClergyProveFaith(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_LAST",
                religion.Faith.GetClergyProveFaithLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_FORBIDDEN",
                religion.Faith.GetClergyForbiddenAnswer(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH__FORBIDDEN_LAST",
                religion.Faith.GetClergyForbiddenAnswerLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION", religion.Faith.GetClergyInduction(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION_LAST",
                religion.Faith.GetClergyInductionLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_ACTION", religion.Faith.GetBlessingAction());
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_puritan_preacher_introduction_on_condition")]
        internal class PuritanPreacherPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_minor_faction_preacher_introduction_on_condition")]
        internal class MinorFactionPreacherPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_mystic_preacher_introduction_on_condition")]
        internal class MysticPreacherPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_messianic_preacher_introduction_on_condition")]
        internal class MessianicPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }
    }
}