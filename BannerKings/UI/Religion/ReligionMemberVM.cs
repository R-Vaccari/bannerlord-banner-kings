using System;
using BannerKings.Managers.Institutions.Religions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.Items;

public class ReligionMemberVM : SettlementGovernorSelectionItemVM
{
    private BasicTooltipViewModel religionHint;

    public ReligionMemberVM(Clergyman clergyman, Action<SettlementGovernorSelectionItemVM> onSelection) : base(
        clergyman.Hero, onSelection)
    {
    }

    public ReligionMemberVM(Hero hero, Action<SettlementGovernorSelectionItemVM> onSelection) : base(hero, onSelection)
    {
    }


    [DataSourceProperty]
    public BasicTooltipViewModel ReligionHint
    {
        get => religionHint;
        set
        {
            if (value != religionHint)
            {
                religionHint = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}