using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    public class CouncilVM : SettlementGovernorSelectionVM
    {
        private Action<Hero> onDone;
        private CouncilData council;
        private List<Hero> courtMembers;
        public CouncilPosition Position { get; set; }

        public CouncilVM(Action<Hero> onDone, CouncilData council, CouncilPosition position, List<Hero> courtMembers) : base(null, onDone)
        {
            this.onDone = onDone;
            this.council = council;
            Position = position;
            this.courtMembers = courtMembers;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            List<Hero> currentCouncil = council.GetMembers();
            MBBindingList<SettlementGovernorSelectionItemVM> newList = new MBBindingList<SettlementGovernorSelectionItemVM>();
            newList.Add(AvailableGovernors[0]);
            CouncilMember councilPosition = new CouncilMember(null, Position, null);
            foreach (Hero hero in courtMembers)
                if (!currentCouncil.Contains(hero) && hero.IsAlive && !hero.IsChild && councilPosition.IsValidCandidate(hero))
                    newList.Add(new CouncilMemberVM(hero, OnSelection,
                                    Position, council.GetCompetence(hero, Position)));

            AvailableGovernors = newList;
        }

        public void ShowOptions()
        {
            List<InquiryElement> options = new List<InquiryElement>();
            foreach (SettlementGovernorSelectionItemVM vm in AvailableGovernors)
            {
                ImageIdentifier image = null;
                string name = "None";
                if (vm.Governor != null) 
                {
                    image = new ImageIdentifier(CampaignUIHelper.GetCharacterCode(vm.Governor.CharacterObject));
                    name = vm.Governor.Name.ToString();
                } 
                options.Add(new InquiryElement(vm.Governor, name, image));
            }

            InformationManager.ShowMultiSelectionInquiry(
                    new MultiSelectionInquiryData(
                        new TextObject("{=!}Select Councillor").ToString(),
                        new TextObject("{=!}Select a lord who you would like to grant this title to.").ToString(),
                        options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                        delegate (List<InquiryElement> x)
                        {
                            onDone((Hero?)x[0].Identifier);
                        }, null, string.Empty));
        }

        private void OnSelection(SettlementGovernorSelectionItemVM item)
        {
            onDone(item.Governor);
        }
    }
}
