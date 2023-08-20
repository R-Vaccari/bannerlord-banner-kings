using BannerKings.Behaviours.Diplomacy.Wars;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Barterable
{
    public class BKWarBarterable : DeclareWarBarterable
    {
        public BKWarBarterable(CasusBelli justification, IFaction declaringFaction, IFaction otherFaction) : base(declaringFaction, otherFaction)
        {
            CasusBelli = justification;
        }

        public CasusBelli CasusBelli { get; private set; }

        public override int GetUnitValueForFaction(IFaction faction)
        {
            int result = 0;
            Clan evaluatingFaction = ((faction is Clan) ? ((Clan)faction) : ((Kingdom)faction).RulingClan);
            TextObject reason;
            if (faction.MapFaction == base.OriginalOwner.MapFaction)
            {
                result = (int)BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfDeclaringWar(base.OriginalOwner.MapFaction, 
                    OtherFaction.MapFaction, evaluatingFaction, out reason, CasusBelli).ResultNumber;
            }
            else if (faction.MapFaction == OtherFaction.MapFaction)
            {
                result = (int)BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfDeclaringWar(OtherFaction.MapFaction, 
                    base.OriginalOwner.MapFaction, evaluatingFaction, out reason, CasusBelli).ResultNumber;
            }

            return result;
        }
    }
}
