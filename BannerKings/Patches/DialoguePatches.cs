using HarmonyLib;
using SandBox.CampaignBehaviors;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Patches
{
    internal class DialoguePatches
    {
        //AI companion dialogue fixes
        [HarmonyPatch(typeof(LordConversationsCampaignBehavior))]
        internal class LordDialoguePatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("conversation_player_ask_prisoners_on_consequence")]
            private static bool LordPrisonersPrefix()
            {
                List<Hero> list = new List<Hero>();
                foreach (TroopRosterElement troopRosterElement in Hero.OneToOneConversationHero.PartyBelongedTo.PrisonRoster.GetTroopRoster())
                {
                    if (troopRosterElement.Character.IsHero && troopRosterElement.Character.HeroObject.IsLord)
                    {
                        list.Add(troopRosterElement.Character.HeroObject);
                    }
                }
                ConversationSentence.SetObjectsToRepeatOver(list, 5);

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("conversation_wanderer_meet_player_on_condition")]
            private static bool MeetCompanionPrefix(ref bool __result)
            {
                __result = CharacterObject.OneToOneConversationCharacter != null &&
                    CharacterObject.OneToOneConversationCharacter.IsHero &&
                    CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer &&
                    CharacterObject.OneToOneConversationCharacter.HeroObject.HeroState != Hero.CharacterStates.Prisoner &&
                    Hero.OneToOneConversationHero.Clan == null;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("conversation_wanderer_preintroduction_on_condition")]
            private static void PreIntroductionPostfix(ref bool __result)
            {
                if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan)
                {
                    __result = false;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("conversation_companion_hire_gold_on_condition")]
            private static void HirePostfix(ref bool __result)
            {
                if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan)
                {
                    __result = false;
                }
            }
        }
    }
}
