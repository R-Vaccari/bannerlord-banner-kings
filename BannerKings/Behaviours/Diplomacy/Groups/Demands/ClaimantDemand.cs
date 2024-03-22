using BannerKings.Actions;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public class ClaimantDemand : RadicalDemand
    {
        public ClaimantDemand() : base("Claimant")
        {
            SetTexts();
        }

        [SaveableProperty(1)] public Hero Claimant { get; set; }

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=kyB8tkgY}Concede"),
                    new TextObject("{=69rAwxib}Accept the demand to cede your rulership to the {CLAIMANT}. You will keep your properties and titles, but be replaced as a ruler. They will be satisfied with this outcome.")
                    .SetTextVariable("CLAIMANT", Claimant.Name),
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
                        
                        return 2f + fulfiller.GetTraitLevel(BKTraits.Instance.Humble) 
                        - fulfiller.GetTraitLevel(BKTraits.Instance.Ambitious)
                        + fulfiller.GetTraitLevel(DefaultTraits.Generosity);
                    },
                    (Hero fulfiller) =>
                    {
                        if (fulfiller == Hero.MainHero || Group.Members.Contains(Hero.MainHero))
                        {
                            InformationManager.DisplayMessage(new InformationMessage(
                                new TextObject("{=53q6oUzO}The {GROUP} is satisfied! {CLAIMANT} will now rule the {REALM}.")
                                .SetTextVariable("GROUP", Group.Name)
                                .SetTextVariable("CLAIMANT", Claimant.Name)
                                .SetTextVariable("REALM", Group.KingdomDiplomacy.Kingdom.Name)
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }

                        ChangeRulingClanAction.Apply(Group.KingdomDiplomacy.Kingdom, Claimant.Clan);

                        foreach (Hero member1 in Group.Members)
                        {
                            foreach (Hero member2 in Group.Members)
                            {
                                if (member1 != member2) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(member1, member2, 5);
                            }

                            if (member1 != Claimant)
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(member1, Claimant, 20);
                        }

                        return true;
                    });

        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=PoAmUqGR}Reject"),
                   new TextObject("{=2MvkhB8h}Deny the demand to cede rulership to the {CLAIMANT}, lead by {HERO}. They will not like this outcome.")
                   .SetTextVariable("RELIGION", Claimant.Name),
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
                       float result = 5f;
                       if (FactionManager.GetEnemyKingdoms(fulfiller.MapFaction as Kingdom).Count() == 0) result += 2f;
                       int gold = fulfiller.Clan.Gold;
                       if (gold > 100000) result += 2f;
                       else if (gold > 50000) result += 1f;

                       float strength = (Group as RadicalGroup).PowerProportion;
                       return result + fulfiller.GetTraitLevel(BKTraits.Instance.Ambitious)
                       + fulfiller.GetTraitLevel(DefaultTraits.Honor)
                       - fulfiller.GetTraitLevel(DefaultTraits.Generosity)
                       - strength;
                   },
                   (Hero fulfiller) =>
                   {
                       LoseRelationsWithGroup(fulfiller, -20, 0.5f);
                       if (fulfiller == Hero.MainHero || Group.Members.Contains(Hero.MainHero))
                       {
                           InformationManager.ShowInquiry(new InquiryData(new TextObject("{=9Tdt2RiC}Civil War").ToString(),
                               new TextObject("{=s3dhcwG2}A civil war breaks out! {RULER} has rejected the demand of your {GROUP} group. You and your fellow group members are now in open war with the original realm.")
                               .SetTextVariable("GROUP", Group.Name)
                               .SetTextVariable("LEADER", Group.Leader.Name)
                               .SetTextVariable("RULER", Group.KingdomDiplomacy.Kingdom.Leader.Name)
                               .ToString(),
                               true,
                               false,
                               GameTexts.FindText("str_accept").ToString(),
                               string.Empty,
                               null,
                               null));
                       }

                       return false;
                   });

        public override float MinimumGroupInfluence => 0.75f;

        public override bool Active => Claimant != null && Claimant.IsAlive;

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return PositiveAnswer;
                yield return NegativeAnswer;
            }
        }

        public override void EndRebellion(Kingdom rebels, Kingdom original, bool success)
        {
            List<Clan> rebelClans = new List<Clan>(rebels.Clans);
            ReintegrateMembers(rebels, original);
            if (success)
            {
                KingdomActions.SetRulerWithTitle(Claimant, original);
            }
            else
            {

            }
        }

        public override void Finish()
        {
            Claimant = null;
            DueDate = CampaignTime.Never;

            FinishRadicalDemand();
        }

        public override Demand GetCopy(DiplomacyGroup group)
        {
            ClaimantDemand demand = new ClaimantDemand();
            demand.Group = group;
            demand.Claimant = Claimant;
            return demand;
        }

        public override (bool, TextObject) IsDemandCurrentlyAdequate()
        {
            (bool, TextObject) result = new(false, null);

            if (Active)
            {
                result = new(true, new TextObject("{=WvxUuqmj}This demand is possible."));
            }

            return result;
        }

        public override void SetTexts()
        {
            if (Claimant != null)
            {
                Initialize(new TextObject("{=Yjq7GL10}Install {CLAIMANT}").SetTextVariable("CLAIMANT", Claimant.Name),
                    new TextObject("{=zdQMSQf4}Demand that the ruler cede rulership to the claimant backed by the group."));
            }
            else
            {
                Initialize(new TextObject("Claimant"),
                    new TextObject("{=zdQMSQf4}Demand that the ruler cede rulership to the claimant backed by the group."));
            }
        }

        public override void SetUp()
        {
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Group.KingdomDiplomacy.Kingdom);
            if (title == null) return;
            
            if (Group.Leader == Hero.MainHero)
            {
                ShowPlayerDemandOptions();
            }
            else
            {
                IEnumerable<KeyValuePair<Hero, ExplainedNumber>> claimants = BannerKingsConfig.Instance.TitleModel
                    .CalculateSuccessionLine(title, Group.KingdomDiplomacy.Kingdom.RulingClan, null, -1);
                List<(Hero, float)> results = new List<(Hero, float)>();
                foreach (var tuple in claimants)
                {
                    Claimant = tuple.Key;
                    results.Add(new (tuple.Key,
                        BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroJoinChance(Group.Leader, Group, Group.KingdomDiplomacy)
                        .ResultNumber));
                }

                Claimant = null;
                Claimant = MBRandom.ChooseWeighted(results);
            }

            if (Claimant != null)
            {
                if (Group.FactionLeader == Hero.MainHero)
                {
                    ShowPlayerPrompt();
                }
            }
            else Finish();
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
                new TextObject("{=BKAdUzjo}The {GROUP} is pushing for you to cede rulership to {CLAIMANT}. The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {STRENGTH}% military strength relative to your loyalist forces.")
                .SetTextVariable("STRENGTH", (Group as RadicalGroup).PowerProportion)
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("CLAIMANT", Claimant.Name)
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
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Group.KingdomDiplomacy.Kingdom);
            IEnumerable<KeyValuePair<Hero, ExplainedNumber>> claimants = BannerKingsConfig.Instance.TitleModel
                    .CalculateSuccessionLine(title, Group.KingdomDiplomacy.Kingdom.RulingClan, null, -1);
            List<InquiryElement> list = new List<InquiryElement>();
            foreach (var tuple in claimants)
            {
                Hero hero = tuple.Key;
                TextObject hint = new TextObject("{=UoPv1RvG}{HERO} {ROLE}, a family of {PEERAGE}.{newline}{newline}{REASON}{newline}{newline}Claim strength: {RESULT}{newline}-----{newline}{EXPLANATION}")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("ROLE", GetHeroRoleText(hero))
                    .SetTextVariable("FIEFS", hero.Clan.Fiefs.Count)
                    .SetTextVariable("PEERAGE", BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan).Peerage.Name)
                    .SetTextVariable("REASON", new TextObject("{=F2N7WBbz}This person is willing to back your radical group."))
                    .SetTextVariable("RESULT", tuple.Value.ResultNumber)
                    .SetTextVariable("EXPLANATION", tuple.Value.GetExplanations());

                list.Add(new InquiryElement(hero,
                    new TextObject("{=bn3YBXvb}{NAME} - {SCORE}")
                    .SetTextVariable("NAME", hero.Name)
                    .SetTextVariable("SCORE", tuple.Value.ResultNumber.ToString("0"))
                    .ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, true)),
                    true,
                    hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString(),
                new TextObject("{=YjfTwnhj}As leader of a claimant group, you must choose the claimant that shall be backed by the group. This decision is final and will not change until the entire claimant group is unmade.{newline}{newline}Possible claimants are all the possible successors of the {TITLE} according to its {SUCCESSION} succession law.")
                .SetTextVariable("TITLE", title.FullName)
                .SetTextVariable("SUCCESSION", title.Contract.Succession.Name)
                .ToString(),
                list,
                true,
                1,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    Hero hero = (Hero)list[0].Identifier;
                    Claimant = hero;
                    SetTexts();
                    (Group as RadicalGroup).ViewModel.RefreshValues();
                },
                (List<InquiryElement> list) =>
                {
                    Finish();
                    (Group as RadicalGroup).ViewModel.RefreshValues();
                }));
        }

        public override void ShowPlayerPrompt()
        {
        }

        public override void Tick()
        {
            if (!Active) Finish();
        }
    }
}
