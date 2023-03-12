using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class CouncilPositionDemand : Demand
    {
        private CouncilMember position;
        private Hero benefactor;

        public CouncilPositionDemand() : base("CouncilPosition")
        {
            Initialize(new TextObject("{=!}Council Position"),
                new TextObject());
        }

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return new DemandResponse(new TextObject("{=!}Accept Demand"),
                    new TextObject("{=!}Accept the demand to put {BENEFACTOR} in charge of the {POSITION} position.")
                    .SetTextVariable("BENEFACTOR", benefactor.Name)
                    .SetTextVariable("POSITION", position.Name),
                    6,
                    250,
                    1000,
                    (Hero fulfiller) =>
                    {
                        return true;
                    },
                    (Hero fulfiller) =>
                    {
                        return 1f;
                    },
                    (Hero fulfiller) =>
                    {
                        position.SetMember(benefactor);
                    });

                yield return new DemandResponse(new TextObject("{=!}Deny Demand"),
                   new TextObject("{=!}Deny the demand to put {BENEFACTOR} in charge of the {POSITION} position.")
                   .SetTextVariable("BENEFACTOR", benefactor.Name)
                   .SetTextVariable("POSITION", position.Name),
                   6,
                   250,
                   1000,
                   (Hero fulfiller) =>
                   {
                       return true;
                   },
                   (Hero fulfiller) =>
                   {
                       return 1f;
                   },
                   (Hero fulfiller) =>
                   {
                       
                   });
            }
        }

        public override Demand GetCopy(InterestGroup group)
        {
            CouncilPositionDemand demand = new CouncilPositionDemand();
            demand.Group = group;
            return demand;
        }

        public override void SetUp()
        {
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Group.FactionLeader.Clan);
            position = council.Positions.FindAll(x => x.IsCorePosition(StringId)
            && x.IsValidCandidate(Group.Leader)).GetRandomElement();

            if (Group.Leader == Hero.MainHero)
            {
                ShowPlayerDemandOptions();
            }
            else
            {
                ShowPlayerDemandOptions();
                benefactor = Group.Leader;
            }
        }

        public override (bool, TextObject) IsDemandCurrentlyAdequate()
        {
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Group.FactionLeader.Clan);
            CouncilMember currentPosition = council.Positions.FirstOrDefault(x => x.IsCorePosition(x.StringId) && Group.Members.Contains(x.Member));
            if (currentPosition != null)
            {
                return new(false, new TextObject("{=!}{HERO} already occupies the {POSITION} position.")
                    .SetTextVariable("HERO", currentPosition.Member.Name)
                    .SetTextVariable("POSITION", currentPosition.Name));
            }

            if (council.Positions.FindAll(x => x.IsCorePosition(StringId)
            && x.IsValidCandidate(Group.Leader)).Count == 0)
            {
                return new(false, new TextObject("{=!}No adequate positions were found."));
            }

            return new(false, new TextObject("{=!}This demand is possible."));
        }

        public override void ShowPlayerDemandAnswers()
        {
            List<InquiryElement> options = new List<InquiryElement>();
            foreach (var answer in DemandResponses)
            {
                options.Add(new InquiryElement(answer,
                    answer.Name.ToString(),
                    null,
                    answer.IsAdequate(Hero.MainHero),
                    answer.Description.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString(),
                Description.ToString(),
                options,
                false,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    DemandResponse response = (DemandResponse)list[0].Identifier;
                    Fulfill(response, Hero.MainHero);
                },
                null,
                Utils.Helpers.GetKingdomDecisionSound()));
        }

        public override void ShowPlayerDemandOptions()
        {
            List<InquiryElement> options = new List<InquiryElement>();
            foreach (var member in Group.Members)
            {
                var competence = BannerKingsConfig.Instance.CouncilModel.CalculateHeroCompetence(member, position, true);
                options.Add(new InquiryElement(member,
                    new TextObject("{=!}{HERO} - {COMPETENCE}% competence")
                    .SetTextVariable("HERO", member.Name)
                    .SetTextVariable("COMPETENCE", (competence.ResultNumber * 100).ToString("0.00"))
                    .ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(member.CharacterObject, true)),
                    true,
                    String.Empty));
            }

            bool playerLead = Group.Leader == Hero.MainHero;
            TextObject description;
            if (playerLead)
            {
                description = new TextObject("{=!}Choose the benefactor for the {POSITION} position.");
            }
            else
            {
                description = new TextObject("{=!}Cast your vote on who should be the benefactor for the {POSITION} position.");
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString(),
                description.ToString(),
                options,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    Hero hero = (Hero)list[0].Identifier;
                    if (playerLead)
                    {
                        benefactor = hero;
                    }
                    else
                    {
                        benefactor = Group.Leader;
                    }
                },
                null));
        }

        public override void Finish()
        {
            position = null;
            benefactor = null;
        }
    }
}
