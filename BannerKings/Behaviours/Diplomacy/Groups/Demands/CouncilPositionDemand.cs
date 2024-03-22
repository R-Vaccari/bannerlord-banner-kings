using BannerKings.Managers.Court;
using BannerKings.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class CouncilPositionDemand : Demand
    {
        [SaveableProperty(1)] private CouncilMember Position { get; set; }
        [SaveableProperty(2)] private Hero Benefactor { get; set; }

        public CouncilPositionDemand() : base("CouncilPosition")
        {
            SetTexts();
        }

        public override void SetTexts()
        {
            Initialize(new TextObject("{=kyB8tkgY}Council Position"),
                new TextObject());
        }

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=kyB8tkgY}Concede"),
                    new TextObject("{=dp7pGQxg}Accept the demand to put {BENEFACTOR} in charge of the {POSITION} position. They will be satisfied with this outcome.")
                    .SetTextVariable("BENEFACTOR", Benefactor.Name)
                    .SetTextVariable("POSITION", Position.Name),
                    new TextObject("{=Pr6r49e8}On {DATE}, the {GROUP} were conceded their {DEMAND} demand.")
                    .SetTextVariable("GROUP", Group.Name)
                    .SetTextVariable("DEMAND", Name),
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
                        if (fulfiller == Hero.MainHero)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(
                                new TextObject("{=qp2ApfcA}The {GROUP} is satisfied! {BENEFACTOR} is now in charge of the {POSITION} position.")
                                .SetTextVariable("GROUP", Group.Name)
                                .SetTextVariable("BENEFACTOR", Benefactor.Name)
                                .SetTextVariable("POSITION", Position.Name)
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }
                        Position.SetMember(Benefactor);
                        return true;
                    });
        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=PoAmUqGR}Reject"),
                   new TextObject("{=RYmV2PEY}Deny the demand to put {BENEFACTOR} in charge of the {POSITION} position. They will not like this outcome.")
                   .SetTextVariable("BENEFACTOR", Benefactor.Name)
                   .SetTextVariable("POSITION", Position.Name),
                   new TextObject("{=icR6DbJR}On {DATE}, the {GROUP} were rejected their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
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
                       LoseRelationsWithGroup(fulfiller, -10, 0.5f);
                       if (fulfiller == Hero.MainHero)
                       {
                           InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=Wi3oUWpJ}The {GROUP} is not satisfied...")
                               .SetTextVariable("GROUP", Group.Name)
                               .SetTextVariable("LEADER", Group.Leader.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                       }
                       return false;
                   });

        public override bool Active => Position != null && Benefactor != null;

        public override float MinimumGroupInfluence => 0.2f;

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return PositiveAnswer;
                yield return NegativeAnswer;
                yield return FinancialCompromise;
                yield return LeverageInfluence;
                yield return AppeaseCharm;
                yield return DisputeLordship;
            }
        }

        public override Demand GetCopy(DiplomacyGroup group)
        {
            CouncilPositionDemand demand = new CouncilPositionDemand();
            demand.Group = group;
            return demand;
        }

        public override void SetUp()
        {
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Group.FactionLeader.Clan);
            if ((Group as InterestGroup).FavoredPosition == null)
            {
                Position = council.Positions.FindAll(x => x.IsCorePosition(x.StringId)
                          && x.IsValidCandidate(Group.Leader).Item1).GetRandomElement();
            }
            else
            {
                Position = council.Positions.FindAll(x => x.IsCorePosition(x.StringId)
                          && x.IsValidCandidate(Group.Leader).Item1 && x.StringId == (Group as InterestGroup).FavoredPosition.StringId).FirstOrDefault();
            }

            if (Position != null)
            {
                Benefactor = Group.Leader;
                if (Group.Members.Contains(Hero.MainHero))
                {
                    ShowPlayerDemandOptions();
                }
                else
                {
                    if (Group.Members.Count > 5)
                    {
                        ChooseBenefactor();
                    }
                    else
                    {
                        Benefactor = Group.Leader;
                    }
                }

                if (Group.FactionLeader == Hero.MainHero)
                {
                    ShowPlayerPrompt();
                }
            }
            else
            {
                Finish();
            }
        }

        public override (bool, TextObject) IsDemandCurrentlyAdequate()
        {
            (bool, TextObject) result = new(false, null);
            ExceptionUtils.TryCatch(() =>
            {
                CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Group.FactionLeader.Clan);
                CouncilMember currentPosition = council.Positions.FirstOrDefault(x => x.IsCorePosition(x.StringId) && Group.Leader == x.Member);
                if (currentPosition != null && currentPosition.Member != null)
                {
                    result = new(false, new TextObject("{=Wjk0m1Ww}{HERO} already occupies the {POSITION} position.")
                        .SetTextVariable("HERO", currentPosition.Member.Name)
                        .SetTextVariable("POSITION", currentPosition.Name));
                }

                if (council.Positions.FindAll(x => x.IsCorePosition(x.StringId)
                && x.IsValidCandidate(Group.Leader).Item1).Count == 0)
                {
                    result = new(false, new TextObject("{=Qehv3JAb}No adequate positions were found."));
                }

                if (Active)
                {
                    result = new(false, new TextObject("{=RnN79qMx}This demand is already under revision by the ruler."));
                }

                result = new(true, new TextObject("{=WvxUuqmj}This demand is possible."));
            },
            GetType().Name,
            false);

            return result;
        }

        public override void ShowPlayerPrompt()
        {
            SetTexts();
            InformationManager.ShowInquiry(new InquiryData(Name.ToString(),
                new TextObject("{=pQgwavHm}The {GROUP} group is demanding the grant of the {POSITION} council position to {HERO}{ROLE}. You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name)
                .SetTextVariable("POSITION", Position.Name)
                .SetTextVariable("HERO", Benefactor.Name)
                .SetTextVariable("ROLE", GetHeroRoleText(Benefactor))
                .ToString(),
                true,
                true,
                new TextObject("{=j90Aa0xG}Resolve").ToString(),
                new TextObject("{=sbwMaTwx}Postpone").ToString(),
                () =>
                {
                    ShowPlayerDemandAnswers();
                },
                () =>
                {
                    DueDate = CampaignTime.DaysFromNow(7f);
                },
                Utils.Helpers.GetKingdomDecisionSound()),
                true,
                true);
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
                new TextObject("{=2EYEFGAZ}The {GROUP} is pushing for {HERO}{ROLE} to be appointed for the {POSITION} position. The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {INFLUENCE}% influence in the realm and {SUPPORT}% support towards you.")
                .SetTextVariable("SUPPORT", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupSupport((Group as InterestGroup)).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("INFLUENCE", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupInfluence((Group as InterestGroup)).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("POSITION", Position.Name)
                .SetTextVariable("ROLE", GetHeroRoleText(Benefactor))
                .SetTextVariable("HERO", Benefactor.Name)
                .SetTextVariable("GROUP", Group.Name)
                .ToString(),
                options,
                false,
                1,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    DemandResponse response = (DemandResponse)list[0].Identifier;
                    Fulfill(response, Hero.MainHero);
                },
                null,
                Utils.Helpers.GetKingdomDecisionSound()),
                true);
        }

        public override void ShowPlayerDemandOptions()
        {
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Group.FactionLeader.Clan);
            List<InquiryElement> positions = new List<InquiryElement>();
            foreach (var position in council.Positions)
            {
                if (position.IsCorePosition(position.StringId))
                {
                    positions.Add(new InquiryElement(position,
                        position.Name.ToString(),
                        null,
                        true,
                        position.Description.ToString()));
                }
            }

            List<InquiryElement> candidates = new List<InquiryElement>();
            foreach (var member in Group.Members)
            {
                var competence = Position.CalculateCandidateCompetence(member, true);
                TextObject hint;
                if (member == Hero.MainHero)
                {
                    hint = new TextObject("{=fcSgp74y}Vote on yourself as beneficiary to this demand.");
                }
                else
                {
                    hint = new TextObject("{=SFuL7rHv}Vote on {HERO}{HERO_TEXT} as beneficiary for the {POSITION} position. Their opinion of you is ({RELATION}).")
                        .SetTextVariable("RELATION", member.GetRelationWithPlayer())
                        .SetTextVariable("HERO", member.Name)
                        .SetTextVariable("HERO_TEXT", GetHeroRoleText(member))
                        .SetTextVariable("POSITION", Position.Name);
                }

                candidates.Add(new InquiryElement(member,
                    new TextObject("{=PP7GiXEk}{HERO} - {COMPETENCE}% competence")
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
                description = new TextObject("{=kyB8tkgY}Choose the benefactor for the {POSITION} position.");
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString() + " (1/2)",
                    new TextObject("{=dJQa0dws}As a leader of your group you can decide what council position to demand. Only positions from the Privy Council are suitable.").ToString(),
                    positions,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_accept").ToString(),
                    String.Empty,
                    (List<InquiryElement> list) =>
                    {
                        CouncilMember position = (CouncilMember)list[0].Identifier;
                        this.Position = position;

                        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString() + " (2/2)",
                            description.ToString(),
                            candidates,
                            true,
                            1,
                            1,
                            GameTexts.FindText("str_accept").ToString(),
                            String.Empty,
                            (List<InquiryElement> list) =>
                            {
                                Hero hero = (Hero)list[0].Identifier;
                                Benefactor = hero;
                            },
                            null));
                    },
                    null), 
                    true);
            }
            else
            {
                description = new TextObject("{=kyB8tkgY}Cast your vote on who should be the benefactor for the {POSITION} position.")
                    .SetTextVariable("POSITION", Position.Name);
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString(),
                       description.ToString(),
                       candidates,
                       true,
                       1,
                       1,
                       GameTexts.FindText("str_accept").ToString(),
                       String.Empty,
                       (List<InquiryElement> list) =>
                       {
                            ChooseBenefactor(Group.Leader, playerLead);
                       },
                       null), 
                       true);
            }
        }

        private void ChooseBenefactor(Hero playerChoice = null, bool leader = false)
        {
            Dictionary<Hero, int> votes = new Dictionary<Hero, int>();
            if (playerChoice != null)
            {
                votes.Add(playerChoice, leader ? 5 : 1);
            }

            foreach (Hero member in Group.Members)
            {
                if (member == Hero.MainHero) continue;
                List<ValueTuple<Hero, float>> options = new List<(Hero, float)>();
                foreach (Hero option in Group.Members)
                {
                    if (option == member) continue;
                    float value = member.GetRelation(option) / 100f;
                    value += BannerKingsConfig.Instance.CouncilModel.CalculateHeroCompetence(option, Position, false)
                        .ResultNumber / 100f;
                    options.Add(new(option, value));
                }

                Hero result = MBRandom.ChooseWeighted(options);
                if (result == null)
                {
                    result = member;
                }
                if (votes.ContainsKey(result))
                {
                    votes[result] += 1;
                }
                else
                {
                    votes.Add(result, member == Group.Leader ? 5 : 1);
                }
            }

            Benefactor = votes.FirstOrDefault(x => x.Value == votes.Values.Max()).Key;
        }

        public override void Finish()
        {
            Position = null;
            Benefactor = null;
            DueDate = CampaignTime.Never;
        }

        public override void Tick()
        {
            if (IsDueDate)
            {
                if (Position.Member == Benefactor)
                {
                    PositiveAnswer.Fulfill(Group.FactionLeader);
                }
                else 
                {
                    PushForDemand();
                }
            }
        }
    }
}
