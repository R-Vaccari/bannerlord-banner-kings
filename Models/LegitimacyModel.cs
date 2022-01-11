using TaleWorlds.CampaignSystem;
using static Populations.Managers.TitleManager;

namespace Populations.Models
{
    class LegitimacyModel
    {

        public LegitimacyType GetRuleType(Settlement settlement, Hero hero)
        {
            FeudalTitle title = PopulationConfig.Instance.TitleManager.GetTitle(settlement);
            bool foreigner = settlement.Culture != hero.Culture;
            if (title.deJure == hero)
                return foreigner ? LegitimacyType.Lawful_Foreigner : LegitimacyType.Lawful;
            else return foreigner ? LegitimacyType.Unlawful_Foreigner : LegitimacyType.Unlawful;
        }

        public LegitimacyType GetRuleType(FeudalTitle title)
        {
            Hero hero = title.deFacto;
            bool foreigner;
            if (title.fief != null)
                foreigner = title.fief.Culture != hero.Culture;
            else if (title.deJure.MapFaction != null) foreigner = title.deJure.MapFaction.Culture != hero.Culture;
            else foreigner = title.deJure.Culture != hero.Culture;

            if (title.deJure == hero)
                return foreigner ? LegitimacyType.Lawful_Foreigner : LegitimacyType.Lawful;
            else return foreigner ? LegitimacyType.Unlawful_Foreigner : LegitimacyType.Unlawful;
        }
    }
}
