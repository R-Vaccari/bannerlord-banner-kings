using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Patches
{
    internal class ReligionPatches
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
