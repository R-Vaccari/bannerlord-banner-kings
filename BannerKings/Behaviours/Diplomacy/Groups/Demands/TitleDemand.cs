using BannerKings.Managers.Court;
using BannerKings.Managers.Titles;
using BannerKings.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class TitleDemand : Demand
    {
        [SaveableProperty(1)] private FeudalTitle Title { get; set; }
        [SaveableProperty(2)] private Hero benefactor { get; set; }

        public TitleDemand() : base("DemandTitle")
        {
            SetTexts();
        }

        public override void SetTexts()
        {
            Initialize(new TextObject("{=!}Council Position"),
                new TextObject());
        }

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=!}Concede"),
                    new TextObject("{=!}Accept the demand grant {BENEFACTOR} the {TITLE} title. They will be satisfied with this outcome.")
                    .SetTextVariable("BENEFACTOR", benefactor.Name)
                    .SetTextVariable("TITLE", Title.FullName),
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
                                new TextObject("{=!}The {GROUP} is satisfied! {BENEFACTOR} is now the legal holder of {TITLE}.")
                                .SetTextVariable("GROUP", Group.Name)
                                .SetTextVariable("BENEFACTOR", benefactor.Name)
                                .SetTextVariable("TITLE", Title.FullName)
                                .ToString(),
                                Color.FromUint(TextHelper.COLOR_LIGHT_BLUE)));
                        }
                        BannerKingsConfig.Instance.TitleManager.InheritTitle(fulfiller, benefactor, Title);
                        return true;
                    });
        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=!}Reject"),
                   new TextObject("{=!}Deny the demand to grant {BENEFACTOR} the {TITLE} title. They will not like this outcome.")
                   .SetTextVariable("BENEFACTOR", benefactor.Name)
                   .SetTextVariable("TITLE", Title.FullName),
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
                               Color.FromUint(TextHelper.COLOR_LIGHT_RED)));
                       }
                       return false;
                   });

        public override bool Active => Title != null && benefactor != null;

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

        public override Demand GetCopy(InterestGroup group)
        {
            CouncilPositionDemand demand = new CouncilPositionDemand();
            demand.Group = group;
            return demand;
        }

        public override void SetUp()
        {
            Hero leader = Group.FactionLeader;
            List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(leader);
            FeudalTitle highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);

            if (titles.Count > 0)
            {
                if (BannerKingsConfig.Instance.StabilityModel.IsHeroOverDemesneLimit(leader))
                {
                    Title = titles.GetRandomElementWithPredicate(x => x != highest && x.Fief != null);
                }
                else if (BannerKingsConfig.Instance.StabilityModel.IsHeroOverUnlandedDemesneLimit(leader))
                {
                    Title = titles.GetRandomElementWithPredicate(x => x != highest && x.Fief == null);
                }
            }

            if (Title != null)
            {
                benefactor = Group.Leader;
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
                        benefactor = Group.Leader;
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
                Hero leader = Group.FactionLeader;
                List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(leader);
                FeudalTitle highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);

                if (titles.Count == 0)
                {
                    result = new(false, new TextObject("{=!}The ruler does not hold any titles legally."));
                }

                if (titles.Count == 1)
                {
                    result = new(false, new TextObject("{=!}The ruler's only title cannot be demanded."));
                }

                if (!BannerKingsConfig.Instance.StabilityModel.IsHeroOverDemesneLimit(leader) &&
                    !BannerKingsConfig.Instance.StabilityModel.IsHeroOverUnlandedDemesneLimit(leader))
                {
                    result = new(false, new TextObject("{=!}The ruler is not over their landed or unlanded demesne limit."));
                }

                if (Active)
                {
                    result = new(false, new TextObject("{=!}This demand is already under revision by the ruler."));
                }

                result = new(true, new TextObject("{=!}This demand is possible."));
            },
            GetType().Name,
            false);

            return result;
        }

        public override void ShowPlayerPrompt()
        {
            SetTexts();
            InformationManager.ShowInquiry(new InquiryData(Name.ToString(),
                new TextObject("{=!}The {GROUP} group is demanding the grant of the {TITLE} title to {HERO}{ROLE}. You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name)
                .SetTextVariable("TITLE", Title.FullName)
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
                new TextObject("{=!}The {GROUP} is pushing for {HERO}{ROLE} to be granted the {TITLE} title. The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {INFLUENCE}% influence in the realm and {SUPPORT}% support towards you.")
                .SetTextVariable("SUPPORT", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupSupport(Group).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("INFLUENCE", (BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupInfluence(Group).ResultNumber * 100f).ToString("0.00"))
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("TITLE", Title.FullName)
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
                Utils.Helpers.GetKingdomDecisionSound()),
                true);
        }

        public override void ShowPlayerDemandOptions()
        {
            /*CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Group.FactionLeader.Clan);
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
                var competence = position.CalculateCandidateCompetence(member, true);
                TextObject hint;
                if (member == Hero.MainHero)
                {
                    hint = new TextObject("{=!}Vote on yourself as beneficiary to this demand.");
                }
                else
                {
                    hint = new TextObject("{=!}Vote on {HERO}{HERO_TEXT} as beneficiary for the {TITLE} position. Their opinion of you is ({RELATION}).")
                        .SetTextVariable("RELATION", member.GetRelationWithPlayer())
                        .SetTextVariable("HERO", member.Name)
                        .SetTextVariable("HERO_TEXT", GetHeroRoleText(member))
                        .SetTextVariable("TITLE", position.Name);
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
                description = new TextObject("{=!}Choose the benefactor for the {TITLE} position.");
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
                description = new TextObject("{=!}Cast your vote on who should be granted the {TITLE} title.")
                    .SetTextVariable("TITLE", Title.FullName);
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
            }*/
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
                    value -= BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(member.Clan).ResultNumber 
                        / BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(member, false).ResultNumber;
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

            benefactor = votes.FirstOrDefault(x => x.Value == votes.Values.Max()).Key;
        }

        public override void Finish()
        {
            Title = null;
            benefactor = null;
            DueDate = CampaignTime.Never;
        }

        public override void Tick()
        {
            if (IsDueDate)
            {
                if (Title.deJure == benefactor)
                {
                    PositiveAnswer.Fulfill(Group.FactionLeader);
                }
                else if (Group.Leader == Hero.MainHero)
                {
                    ShowPlayerDemandAnswers();
                }
                else
                {
                    DoAiChoice();
                }
            }
        }
    }
}
