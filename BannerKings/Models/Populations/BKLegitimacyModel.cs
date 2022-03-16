using TaleWorlds.CampaignSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class BKLegitimacyModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            bool foreigner = settlement.Culture != settlement.Owner.Culture;
            if (title.deJure == settlement.Owner)
                result.Add((float)(foreigner ? LegitimacyType.Lawful_Foreigner : LegitimacyType.Lawful));
            else result.Add((float)(foreigner ? LegitimacyType.Unlawful_Foreigner : LegitimacyType.Unlawful));

            return result;
        }

        public LegitimacyType GetRuleType(Settlement settlement, Hero hero)
        {
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
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
