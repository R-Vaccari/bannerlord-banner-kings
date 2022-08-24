using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKEncourageMercantilism : BannerKingsDecision
    {
        public BKEncourageMercantilism(Settlement settlement, bool enabled) : base(settlement, enabled)
        {
        }

        public override string GetHint()
        {
            return new TextObject("{=HGtTMAzDt}Extend exemptions to artisans and guilds, improving their output efficiency while decreasing state revenue.").ToString();
        }

        public override string GetIdentifier()
        {
            return "decision_mercantilism";
        }

        public override string GetName()
        {
            return new TextObject("{=c5hugcMGu}Encourage mercantilism").ToString();
        }
    }
}