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
            if (hero != Hero.MainHero && target.StringId != "town_A1") return;
            
            ReligionData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).ReligionData;
            if (data == null) return;

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
            starter.AddDialogLine("minor_faction_preacher_introduction", "lord_introduction", "lord_start", 
                "{=!}{CLERGYMAN_GREETING}", 
                new ConversationSentence.OnConditionDelegate(this.OnConditionClergymanGreeting), null, 100, null);
        }

        private bool OnConditionClergymanGreeting()
        {
            if (Campaign.Current.ConversationManager.CurrentConversationIsFirst && Hero.OneToOneConversationHero.IsPreacher && 
                BannerKingsConfig.Instance.ReligionsManager != null)
            {
                Clergyman clergyman = BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
                Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(clergyman);
                MBTextManager.SetTextVariable("CLERGYMAN_GREETING", religion.Faith.GetClergyGreeting(clergyman.Rank), false);
                return true;
            }
            return false;
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
