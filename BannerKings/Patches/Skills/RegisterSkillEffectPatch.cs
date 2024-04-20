using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using BannerKings.Utils;
using TaleWorlds.Library;
using BannerKings.Settings;

namespace BannerKings.Patches
{
    internal partial class PerksAndSkillsPatches
    {
        [HarmonyPatch(typeof(DefaultSkillEffects), "RegisterAll")]
        class RegisterSkillEffectPatch
        {
            public static MBReadOnlyList<SkillEffect> AllSkillEffects => Game.Current.ObjectManager.GetObjectTypeList<SkillEffect>();
            static void Postfix()
            {
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardSkills && BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
                {
                    var stewardPartySizeBonus = AllSkillEffects.FirstOrDefault(d => d.StringId == "StewardPartySizeBonus");
                    if (stewardPartySizeBonus != null)
                    {
                        stewardPartySizeBonus.SetPrivatePropertyValue("SecondaryBonus", 0.1f);
                        stewardPartySizeBonus.SetPrivatePropertyValue("SecondaryRole", SkillEffect.PerkRole.PartyMember);
                    }
                }
            }
        }
    }
}
