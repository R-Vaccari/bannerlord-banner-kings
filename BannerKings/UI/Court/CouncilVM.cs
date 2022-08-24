using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BannerKings.Managers.Court;
using BannerKings.Models.BKModels;
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

        public CouncilVM(Action<Hero> onDone, CouncilData council, CouncilPosition position, List<Hero> courtMembers) :
            base(null, onDone)
        {
            this.onDone = onDone;
            this.council = council;
            Position = position;
            this.courtMembers = courtMembers;
            RefreshValues();
        }

        public CouncilPosition Position { get; set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            var currentCouncil = council.GetMembers();
            var newList = new MBBindingList<SettlementGovernorSelectionItemVM> {AvailableGovernors[0]};
            var councilPosition = council.GetCouncilMember(Position);
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
                foreach (var vm in AvailableGovernors)
                {
                    ImageIdentifier image = null;
                    var name = "None";
                    var hint = "";
                    if (vm.Governor != null)
                    {
                        image = new ImageIdentifier(CampaignUIHelper.GetCharacterCode(vm.Governor.CharacterObject));
                        name = vm.Governor.Name.ToString();
                        var sb = new StringBuilder(GameTexts.FindText("str_tooltip_label_type") + ": " +
                                                   HeroHelper.GetCharacterTypeName(vm.Governor));
                        sb.AppendLine(new TextObject("{=RMUyXy4e}Competence:").ToString() +
                                      council.GetCompetence(vm.Governor, Position));
                        hint = sb.ToString();
                    }

                    options.Add(new InquiryElement(vm.Governor, name, image, true, hint));
                }

                var model = (BKCouncilModel) BannerKingsConfig.Instance.Models.First(x => x is BKCouncilModel);
                MBInformationManager.ShowMultiSelectionInquiry(
                    new MultiSelectionInquiryData(
                        new TextObject("{=91fEPp8b}Select Councillor").ToString(),
                        new TextObject("{=i8YeRbDt}Select who you would like to fill this position.").ToString(),
                        options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                        delegate(List<InquiryElement> x)
                        {
                            var requester = (Hero?) x[0].Identifier;
                            var position = council.AllPositions.FirstOrDefault(x => x.Position == Position);
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
            else
            {
                var target = council.AllPositions.FirstOrDefault(x => x.Position == Position);
                if (target == null)
                {
                    return;
                }

                CouncilAction action;
                var model = (BKCouncilModel) BannerKingsConfig.Instance.Models.First(x => x is BKCouncilModel);
                if (target.Member == Hero.MainHero)
                {
                    action = model.GetAction(CouncilActionType.RELINQUISH, council, Hero.MainHero, target);
                }
                else if (council.GetHeroPosition(Hero.MainHero) == null || target.Member == null)
                {
                    action = model.GetAction(CouncilActionType.REQUEST, council, Hero.MainHero, target);
                }
                else
                {
                    action = model.GetAction(CouncilActionType.SWAP, council, Hero.MainHero, target,
                        council.GetHeroPosition(Hero.MainHero));
                }

                UIHelper.ShowActionPopup(action, this);
            }
        }

        private void OnSelection(SettlementGovernorSelectionItemVM item)
        {
            onDone(item.Governor);
        }
    }
}