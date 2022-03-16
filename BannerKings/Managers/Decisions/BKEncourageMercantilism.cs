using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Decisions
{
    public class BKEncourageMercantilism : BannerKingsDecision
    {
        public BKEncourageMercantilism(Settlement settlement, bool enabled) : base(settlement, enabled)
        {

        }

        public override string GetHint() => new TextObject("{=!}Extend exemptions to artisans and guilds, improving their output efficiency while decreasing state revenue.").ToString();

        public override string GetIdentifier() => "decision_mercantilism";

        public override string GetName() => new TextObject("{=!}Encourage mercantilism").ToString();
    }
}
