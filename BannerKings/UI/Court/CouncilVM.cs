using System;
using System.Collections.Generic;
using BannerKings.Managers.Court;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Court
{
    public class CouncilVM : SettlementGovernorSelectionVM
    {
        private readonly CouncilData council;
        private readonly List<Hero> courtMembers;
        private readonly Action<Hero> onDone;

        public CouncilVM(Action<Hero> onDone, CouncilData council, CouncilMember position, List<Hero> courtMembers) :
            base(null, onDone)
        {
            this.onDone = onDone;
            this.council = council;
            Position = position;
            this.courtMembers = courtMembers;
            RefreshValues();
        }

        public CouncilMember Position { get; set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            var currentCouncil = council.GetMembers();
            var newList = new MBBindingList<SettlementGovernorSelectionItemVM> {AvailableGovernors[0]};
            var councilPosition = council.GetCouncilPosition(Position);
            foreach (var hero in courtMembers)
            {
                if (!currentCouncil.Contains(hero) && hero.IsAlive && !hero.IsChild &&
                    councilPosition.IsValidCandidate(hero))
                {
                    newList.Add(new CouncilCandidateVM(hero, OnSelection,
                        Position, council.GetCompetence(hero, Position)));
                }
            }

            AvailableGovernors = newList;
        }

        public void ShowOptions()
        {
            var options = new List<InquiryElement>();
            if (council.Owner == Hero.MainHero)
            {
                var councilPosition = council.GetCouncilPosition(Position);
                foreach (var vm in AvailableGovernors)
                {
                    ImageIdentifier image = null;
                    var name = new TextObject("{=koX9okuG}None");
                    var hint = "";
                    if (vm.Governor != null)
                    {
                        image = new ImageIdentifier(CampaignUIHelper.GetCharacterCode(vm.Governor.CharacterObject));
                        name = vm.Governor.Name;
                        TextObject textObject = new TextObject("{=!}{HERO} is a {TYPE} with {COMPETENCE}% competence for this position.")
                            .SetTextVariable("COMPETENCE", (Position.CalculateCandidateCompetence(vm.Governor).ResultNumber * 100f).ToString("0.00"))
                            .SetTextVariable("TYPE", HeroHelper.GetCharacterTypeName(vm.Governor))
                            .SetTextVariable("HERO", name);

                        hint = textObject.ToString();
                    }

                    options.Add(new InquiryElement(vm.Governor, 
                        name.ToString(), 
                        image, 
                        true, 
                        hint));
                }
                TextObject current = new TextObject();
                if (Position.Member != null)
                {
                    current = new TextObject("{=!}{HERO} current holds this position with a competence of {COMPETENCE}%.")
                        .SetTextVariable("HERO", Position.Member.Name)
                        .SetTextVariable("COMPETENCE", (Position.Competence.ResultNumber * 100f).ToString("0.00"));
                }

                var model = BannerKingsConfig.Instance.CouncilModel;
                MBInformationManager.ShowMultiSelectionInquiry(
                    new MultiSelectionInquiryData(
                        new TextObject("{=91fEPp8b}Select Councillor").ToString(),
                        new TextObject("{=!}Select who you would like to fill this position. The {POSITION} requires competency in {PRIMARY} and {SECONDARY} skills. {CURRENT}")
                        .SetTextVariable("SECONDARY", councilPosition.SecondarySkill.Name)
                        .SetTextVariable("PRIMARY", councilPosition.PrimarySkill.Name)
                        .SetTextVariable("POSITION", councilPosition.Name)
                        .SetTextVariable("CURRENT", current)
                        .ToString(),
                        options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                        delegate(List<InquiryElement> x)
                        {
                            var requester = (Hero?) x[0].Identifier;
                            var position = council.GetCouncilPosition(Position);
                            CouncilAction action = null;
                            if (requester != null)
                            {
                                action = model.GetAction(CouncilActionType.REQUEST, council, requester, position, null,
                                    true);
                            }
                            else if (position.Member != null)
                            {
                                action = model.GetAction(CouncilActionType.RELINQUISH, council, requester, position);
                            }

                            if (action != null)
                            {
                                action.TakeAction();
                                onDone(requester);
                            }
                        }, null, string.Empty));
            }
        }

        private void OnSelection(SettlementGovernorSelectionItemVM item)
        {
            onDone(item.Governor);
        }
    }
}