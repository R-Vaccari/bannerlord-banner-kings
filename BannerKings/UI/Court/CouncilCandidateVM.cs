using System;
using BannerKings.Managers.Court;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.Items;

internal class CouncilCandidateVM : SettlementGovernorSelectionItemVM
{
    private BasicTooltipViewModel courtHint;

    public CouncilCandidateVM(Hero member, Action<SettlementGovernorSelectionItemVM> onSelection,
        CouncilPosition position, float competence) : base(member, onSelection)
    {
        GovernorHint =
            new BasicTooltipViewModel(() => UIHelper.GetHeroGovernorEffectsTooltip(member, position, competence));
        CourtHint = new BasicTooltipViewModel(() => UIHelper.GetHeroCourtTooltip(member));
    }

    [DataSourceProperty]
    public BasicTooltipViewModel CourtHint
    {
        get => courtHint;
        set
        {
            if (value != courtHint)
            {
                courtHint = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}