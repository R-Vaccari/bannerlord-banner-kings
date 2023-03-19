using BannerKings.Managers.Court;
using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
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

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=!}Concede"),
                    new TextObject("{=!}Accept the demand to put {BENEFACTOR} in charge of the {POSITION} position. They will be satisfied with this outcome.")
                    .SetTextVariable("BENEFACTOR", benefactor.Name)
                    .SetTextVariable("POSITION", position.Name),
                    new TextObject("{=!}On {DATE}, the {GROUP} were conceded their {DEMAND} demand.")
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
                                new TextObject("{=!}The {GROUP} is satisfied! {BENEFACTOR} is now in charge of the {POSITION} position.")
                                .SetTextVariable("GROUP", Group.Name)
                                .SetTextVariable("BENEFACTOR", benefactor.Name)
                                .SetTextVariable("POSITION", position.Name)
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }
                        position.SetMember(benefactor);
                        return true;
                    });
        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=!}Reject"),
                   new TextObject("{=!}Deny the demand to put {BENEFACTOR} in charge of the {POSITION} position. They will not like this outcome.")
                   .SetTextVariable("BENEFACTOR", benefactor.Name)
                   .SetTextVariable("POSITION", position.Name),
                   new TextObject("{=!}On {DATE}, the {GROUP} were rejected their {DEMAND} demand.")
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
                               new TextObject("{=!}The {GROUP} is not satisfied...")
                               .SetTextVariable("GROUP", Group.Name)
                               .SetTextVariable("LEADER", Group.Leader.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                       }
                       return false;
                   });

        public override bool Active => position != null && benefactor != null;

        public override float MinimumGroupInfluence => 0.2f;

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return PositiveAnswer;
                yield return NegativeAnswer;
                yield return new DemandResponse(new TextObject("{=!}Financial Compromise"),
                   new TextObject("{=!}Negotiate with {LEADER} a financial compromise to appease the group's demand. A sum of denars based on your income will be paied out to the group, mostly to the leader. They will be satisfied with this outcome.")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("DENARS", MBRandom.RoundRandomized(BannerKingsConfig.Instance.InterestGroupsModel
                   .CalculateFinancialCompromiseCost(Hero.MainHero, 10000, 1f).ResultNumber)),
                   new TextObject("{=!}On {DATE}, a financial compromise was made with {GROUP} due to their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   5,
                   300,
                   300,
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
                       int denars = MBRandom.RoundRandomized(BannerKingsConfig.Instance.InterestGroupsModel
                       .CalculateFinancialCompromiseCost(fulfiller, 10000, 1f).ResultNumber);
                       fulfiller.ChangeHeroGold(-denars);

                       LoseRelationsWithGroup(fulfiller, -6, 0.2f);
                       if (fulfiller == Hero.MainHero)
                       {
                           InformationManager.DisplayMessage(new InformationMessage(
                               new TextObject("{=!}The {GROUP} accepts the compromise... However, some criticize it as bribery.")
                               .SetTextVariable("GROUP", Group.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                       }

                       return true;
                   });

                yield return new DemandResponse(new TextObject("{=!}Leverage Influence"),
                   new TextObject("{=!}Use your political influence to deny this demand. {LEADER} will not be pleased with this option, but the group will accept it. They will be satisfied with this outcome.")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("INFLUENCE", MBRandom.RoundRandomized(BannerKingsConfig.Instance.InterestGroupsModel
                   .CalculateLeverageInfluenceCost(Hero.MainHero, 100, 1f).ResultNumber)),
                   new TextObject("{=!}On {DATE}, the {GROUP} were politically influenced to abandon their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   -8,
                   500,
                   200,
                   (Hero fulfiller) =>
                   {
                       return fulfiller.Clan.Influence >=
                       BannerKingsConfig.Instance.InterestGroupsModel.CalculateLeverageInfluenceCost(fulfiller, 100, 1f).ResultNumber;
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
                               new TextObject("{=!}The {GROUP} back down on their demand! {LEADER} was politically compelled to give up.")
                               .SetTextVariable("GROUP", Group.Name)
                               .SetTextVariable("LEADER", Group.Leader.Name)
                               .ToString(),
                               Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                       }
                       
                       ChangeClanInfluenceAction.Apply(fulfiller.Clan, -BannerKingsConfig.Instance.InterestGroupsModel
                           .CalculateLeverageInfluenceCost(fulfiller, 100, 1f).ResultNumber);
                       return true;
                   });

                yield return new DemandResponse(new TextObject("{=!}Appease (Charm)"),
                   new TextObject("{=!}Use your diplomatic and charm skills to convince {LEADER} out of this idea. This option may or may not work. The chance of working depends on your Charm and how much {LEADER} likes you - its unlikely you will charm an enemy. Whether the group is satisfied or not depends on the leader being convinced.\nMinimum Charm: {CHARM}")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("CHARM", 150),
                   new TextObject("{=!}On {DATE}, the {GROUP} were appeased to forfeit their {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   0,
                   500,
                   100,
                   (Hero fulfiller) =>
                   {
                       return fulfiller.GetSkillValue(DefaultSkills.Charm) > 150;
                   },
                   (Hero fulfiller) =>
                   {
                       return 1f;
                   },
                   (Hero fulfiller) =>
                   {
                       int relation = Group.Leader.GetRelation(fulfiller);
                       int skill = (int)(fulfiller.GetSkillValue(DefaultSkills.Charm) / 3f);
                       float chance = (relation + skill) / 200f;
                       if (MBRandom.RandomFloat <= chance)
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was appeased! The {GROUP} back down on their demand.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                           }

                           ChangeRelationAction.ApplyRelationChangeBetweenHeroes(fulfiller, Group.Leader, 6);
                           return true;
                       }
                       else
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was not convinced... The {GROUP} is not satisfied with this outcome.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_RED)));
                           }

                           ChangeRelationAction.ApplyRelationChangeBetweenHeroes(fulfiller, Group.Leader, -9);
                           return false;
                       }
                   });

                yield return new DemandResponse(new TextObject("{=!}Dispute (Lordship)"),
                   new TextObject("{=!}Dispute on legal terms the efficacy of this demand. {LEADER} may or not be compelled to conceded, based on their Lordship ({LEADER_LORDSHIP}) against yours ({PLAYER_LORDSHIP}). Whether the group is satisfied or not depends on the leader being convinced.\nMinimum Lordship: {LORDSHIP}")
                   .SetTextVariable("LEADER", Group.Leader.Name)
                   .SetTextVariable("LEADER_LORDSHIP", Group.Leader.GetSkillValue(BKSkills.Instance.Lordship))
                   .SetTextVariable("PLAYER_LORDSHIP", Hero.MainHero.GetSkillValue(BKSkills.Instance.Lordship))
                   .SetTextVariable("LORDSHIP", 100),
                   new TextObject("{=!}On {DATE}, the {GROUP} were defeated on legal grounds over the {DEMAND} demand.")
                   .SetTextVariable("GROUP", Group.Name)
                   .SetTextVariable("DEMAND", Name),
                   -5,
                   1000,
                   100,
                   (Hero fulfiller) =>
                   {
                       return fulfiller.GetSkillValue(BKSkills.Instance.Lordship) > 100;
                   },
                   (Hero fulfiller) =>
                   {
                       return 1f;
                   },
                   (Hero fulfiller) =>
                   {
                       float factor = fulfiller.GetSkillValue(BKSkills.Instance.Lordship) / 
                       Group.Leader.GetSkillValue(BKSkills.Instance.Lordship);

                       if (MBRandom.RandomFloat <= factor * 0.5f)
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was appeased! The {GROUP} back down on their demand.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                           }

                           return true;
                       }
                       else
                       {
                           if (fulfiller == Hero.MainHero)
                           {
                               InformationManager.DisplayMessage(new InformationMessage(
                                   new TextObject("{=!}{LEADER} was appeased! The {GROUP} back down on their demand.")
                                   .SetTextVariable("GROUP", Group.Name)
                                   .SetTextVariable("LEADER", Group.Leader.Name)
                                   .ToString(),
                                   Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                           }

                           return false;
                       }
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
            if (Group.FavoredPosition == null)
            {
                position = council.Positions.FindAll(x => x.IsCorePosition(x.StringId)
                          && x.IsValidCandidate(Group.Leader)).GetRandomElement();
            }
            else
            {
                council.GetCouncilPosition(Group.FavoredPosition);
            }

            if (position != null)
            {
                benefactor = Group.Leader;
                if (Group.Members.Contains(Hero.MainHero))
                {
                    ShowPlayerDemandOptions();
                }
                else
                {
                    ChooseBenefactor();
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
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Group.FactionLeader.Clan);
            CouncilMember currentPosition = council.Positions.FirstOrDefault(x => x.IsCorePosition(x.StringId) && Group.Leader == x.Member);
            if (currentPosition != null)
            {
                return new(false, new TextObject("{=!}{HERO} already occupies the {POSITION} position.")
                    .SetTextVariable("HERO", currentPosition.Member.Name)
                    .SetTextVariable("POSITION", currentPosition.Name));
            }

            if (council.Positions.FindAll(x => x.IsCorePosition(x.StringId)
            && x.IsValidCandidate(Group.Leader)).Count == 0)
            {
                return new(false, new TextObject("{=!}No adequate positions were found."));
            }

            if (Active)
            {
                return new(false, new TextObject("{=!}This demand is already under revision by the ruler."));
            }

            return new(true, new TextObject("{=!}This demand is possible."));
        }

        public override void ShowPlayerPrompt()
        {
            InformationManager.ShowInquiry(new InquiryData(Name.ToString(),
                new TextObject("{=!}The {GROUP} group is demanding the grant of the {POSITION} council position to {HERO}{ROLE}. You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name)
                .SetTextVariable("POSITION", position.Name)
                .SetTextVariable("HERO", benefactor.Name)
                .SetTextVariable("ROLE", GetHeroRoleText(benefactor))
                .ToString(),
                true,
                true,
                new TextObject("{=!}Resolve").ToString(),
                new TextObject("{=!}Postpone").ToString(),
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
                new TextObject("{=!}The {GROUP} is pushing for {HERO}{ROLE} to be appointed for the {POSITION} position. The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {INFLUENCE}% influence in the realm and {SUPPORT}% support towards you.")
                .SetTextVariable("SUPPORT", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupSupport(Group).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("INFLUENCE", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupInfluence(Group).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("POSITION", position.Name)
                .SetTextVariable("ROLE", GetHeroRoleText(benefactor))
                .SetTextVariable("HERO", benefactor.Name)
                .SetTextVariable("GROUP", Group.Name)
                .ToString(),
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
                var competence = position.ProjectedCompetence;
                TextObject hint;
                if (member == Hero.MainHero)
                {
                    hint = new TextObject("{=!}Vote on yourself as beneficiary to this demand.");
                }
                else
                {
                    hint = new TextObject("{=!}Vote on {HERO}{HERO_TEXT} as beneficiary for the {POSITION} position. Their opinion of you is ({RELATION}).")
                        .SetTextVariable("RELATION", member.GetRelationWithPlayer())
                        .SetTextVariable("HERO", member.Name)
                        .SetTextVariable("HERO_TEXT", GetHeroRoleText(member))
                        .SetTextVariable("POSITION", position.Name);
                }

                candidates.Add(new InquiryElement(member,
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
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString() + " (1/2)",
                    new TextObject("{=!}As a leader of your group you can decide what council position to demand. Only positions from the Privy Council are suitable.").ToString(),
                    positions,
                    true,
                    1,
                    GameTexts.FindText("str_accept").ToString(),
                    String.Empty,
                    (List<InquiryElement> list) =>
                    {
                        CouncilMember position = (CouncilMember)list[0].Identifier;
                        this.position = position;

                        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString() + " (2/2)",
                            description.ToString(),
                            candidates,
                            true,
                            1,
                            GameTexts.FindText("str_accept").ToString(),
                            String.Empty,
                            (List<InquiryElement> list) =>
                            {
                                Hero hero = (Hero)list[0].Identifier;
                                benefactor = hero;
                            },
                            null));
                    },
                    null), 
                    true);
            }
            else
            {
                description = new TextObject("{=!}Cast your vote on who should be the benefactor for the {POSITION} position.");
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString(),
                       description.ToString(),
                       candidates,
                       true,
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
                    value += BannerKingsConfig.Instance.CouncilModel.CalculateHeroCompetence(option, position, false)
                        .ResultNumber / 100f;
                    options.Add(new(option, value));
                }

                Hero result = MBRandom.ChooseWeighted(options);
                if (votes.ContainsKey(result))
                {
                    votes[result] += 1;
                }
                else
                {
                    votes.Add(result, member == Group.Leader ? 5 : 1);
                }
            }

            benefactor = votes.FirstOrDefault(x => x.Value == votes.Values.Max()).Key;
        }

        public override void Finish()
        {
            position = null;
            benefactor = null;
            DueDate = CampaignTime.Never;
        }

        public override void Tick()
        {
            if (IsDueDate)
            {
                if (position.Member == benefactor)
                {
                    PositiveAnswer.Fulfill(Group.FactionLeader);
                }
                else
                {
                    ShowPlayerDemandAnswers();
                }
            }
        }
    }
}
