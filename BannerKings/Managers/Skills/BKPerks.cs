using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Managers.Skills
{
    public class BKPerks : DefaultTypeInitializer<BKPerks>
    {
        private PerkObject scholarshipLiterate, scholarshipPolyglot;

        public override void Initialize()
        {
            scholarshipLiterate = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipLiterate"));
            scholarshipLiterate.InitializeNew("{=!}Literate", BKSkills.Instance.Scholarship, GetTierCost(1), null,
                "{=!}Allows reading books.", SkillEffect.PerkRole.Personal, 0f,
                SkillEffect.EffectIncrementType.Invalid, string.Empty,
                SkillEffect.PerkRole.None, 0f,
                SkillEffect.EffectIncrementType.Invalid,
                TroopClassFlag.None, TroopClassFlag.None);

            scholarshipPolyglot = Game.Current.ObjectManager.RegisterPresumedObject(new PerkObject("ScholarshipPolyglot"));
            scholarshipPolyglot.InitializeNew("{=!}Polyglot", BKSkills.Instance.Scholarship, GetTierCost(2), null, 
                "{=!}Increase language learning rate by 20%.", SkillEffect.PerkRole.Personal, 20f, 
                SkillEffect.EffectIncrementType.AddFactor, "{=!}Gain more relation when greeting lords in their language.", 
                SkillEffect.PerkRole.Personal, 3f, 
                SkillEffect.EffectIncrementType.Add, 
                TroopClassFlag.None, TroopClassFlag.None);
        }

        private int GetTierCost(int tierIndex) => Requirements[tierIndex - 1];

        private static readonly int[] Requirements = new int[]
        {
            25,
            50,
            75,
            100,
            125,
            150,
            175,
            200,
            225,
            250,
            275,
            300
        };
    }
}
