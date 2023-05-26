using BannerKings.Managers.Kingdoms.Council;
using BannerKings.Managers.Titles.Laws;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court
{
    public class CouncilAction : BannerKingsAction
    {
        public CouncilAction(CouncilActionType type, Hero actionTaker, CouncilMember target, CouncilMember current,
            CouncilData council)
        {
            ActionTaker = actionTaker;
            TargetPosition = target;
            CurrentPosition = current;
            Type = type;
            Council = council;
        }

        public CouncilMember TargetPosition { get; }
        public CouncilMember CurrentPosition { get; }
        public CouncilActionType Type { get; }
        public CouncilData Council { get; }

        public override void TakeAction(Hero receiver = null)
        {
            switch (Type)
            {
                case CouncilActionType.REQUEST:
                    {
                        Kingdom kingdom = Council.Clan.Kingdom;
                        if (kingdom != null && Council.Clan == kingdom.RulingClan)
                        {
                            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                            if (title != null && title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.CouncilElected))
                            {
                                var decision = new BKCouncilPositionDecision(kingdom.RulingClan,
                                    Council,
                                    TargetPosition,
                                    ActionTaker);
                                if (!decision.IsAllowed())
                                {
                                    if (Council.Clan == Clan.PlayerClan)
                                    {
                                        InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}A council election is already in place.").ToString(),
                                            Color.UIntToColorString(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                                    }
                                }
                                else
                                {
                                    var candidates = decision.NarrowDownCandidates(
                                        decision.DetermineInitialCandidates().ToList(), 
                                        3);
                                    if (candidates.Count() >= 3)
                                    {
                                        kingdom.AddDecision(decision);
                                    }
                                    else 
                                    {
                                        if (candidates.Count() == 1)
                                        {
                                            BannerKingsConfig.Instance.CourtManager.AddHeroToCouncil(this);
                                        }
                                        else if (Council.Clan == Clan.PlayerClan)
                                        {
                                            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}Not enough candidates for this position.").ToString(),
                                                                                        Color.UIntToColorString(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                                        }  
                                    }
                                }
                               
                                break;
                            }
                        }
                        BannerKingsConfig.Instance.CourtManager.AddHeroToCouncil(this);
                        break;
                    }
                case CouncilActionType.SWAP:
                    BannerKingsConfig.Instance.CourtManager.SwapCouncilPositions(this);
                    break;
                default:
                    BannerKingsConfig.Instance.CourtManager.RelinquishCouncilPosition(this);
                    break;
            }
        }

        public void Reject(Hero rejector)
        {
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(ActionTaker, rejector, CourtManager.ON_REJECTED_RELATION);
        }
    }

    public enum CouncilActionType
    {
        REQUEST,
        SWAP,
        RELINQUISH
    }
}