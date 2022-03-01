using BannerKings.Managers.Court;
using BannerKings.Utils;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core.ViewModelCollection;

namespace BannerKings.UI.Items
{
    class CouncilMemberVM : SettlementGovernorSelectionItemVM
    {

        public CouncilMemberVM(Hero member, Action<SettlementGovernorSelectionItemVM> onSelection, CouncilPosition position, float competence) : base(member, onSelection)
        { 
            this.GovernorHint = new BasicTooltipViewModel(() => UIHelper.GetHeroGovernorEffectsTooltip(member, position, competence));
        }
    }
}
