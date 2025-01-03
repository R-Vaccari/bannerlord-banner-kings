using BannerKings.Actions;
using BannerKings.CampaignContent.Traits;
using BannerKings.Managers.Titles;
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
        public override bool Equals(object obj)
        {
            bool same = false;
            if (obj is ClaimantDemand) same = (obj as ClaimantDemand).Claimant == Claimant;

            return base.Equals(obj) && same;
        }
        public ClaimantDemand() : base("Claimant")
        {
            SetTexts();
        }

        protected override bool IsFulfilled() => Claimant.Clan == Group.KingdomDiplomacy.Kingdom.RulingClan;

        [SaveableProperty(10)] public Hero Claimant { get; set; }

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=kyB8tkgY}Concede"),
                    new TextObject("{=69rAwxib}Accept the demand to cede your rulership to the {CLAIMANT}. You will keep your properties and titles, but be replaced as a ruler. They will be satisfied with this outcome.")
                    .SetTextVariable("CLAIMANT", Claimant.Name),
                    new TextObject("{=Pr6r49e8}On {DATE}, the {GROUP} were conceded their {DEMAND} demand.")
                    .SetTextVariable("DATE", CampaignTime.Now.ToString())
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
                   new TextObject("{=!}Deny the demand to cede rulership to the {CLAIMANT}, led by {HERO}. They will not like this outcome.")
                   .SetTextVariable("CLAIMANT", Claimant.Name)
                   .SetTextVariable("HERO", Group.Leader.Name),
                   new TextObject("{=icR6DbJR}On {DATE}, the {GROUP} were rejected their {DEMAND} demand.")
                   .SetTextVariable("DATE", CampaignTime.Now.ToString())
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

        protected override TextObject PlayerPromptText => new TextObject("{=!}The {GROUP} group is pushing for you to cede rulership to {CLAIMANT}. You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name)
                .SetTextVariable("CLAIMANT", Claimant.Name);

        protected override TextObject PlayerAnswersText => new TextObject("{=BKAdUzjo}The {GROUP} is pushing for you to cede rulership to {CLAIMANT}. The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {STRENGTH}% military strength relative to your loyalist forces.")
                .SetTextVariable("STRENGTH", (Group as RadicalGroup).PowerProportion)
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("CLAIMANT", Claimant.Name)
                .SetTextVariable("GROUP", Group.Name);

        public override void EndRebellion(Kingdom rebels, Kingdom original, bool success)
        {
            TextObject text = success ? new TextObject("{=!}The rebels of the {REBELS}, led by {LEADER} have been successful in their rebellion! {CLAIMANT} will now rule over the {KINGDOM}...") 
                .SetTextVariable("KINGDOM", original.Name)
                .SetTextVariable("CLAIMANT", Claimant.Name)
                .SetTextVariable("LEADER", rebels.RulingClan.Leader.Name)
                .SetTextVariable("REBELS", rebels.Name)
                : 
                new TextObject("{=!}The rebels of the {REBELS}, led by {LEADER} have failed their rebellion... Their judgement falls now to {RULER}.")
                .SetTextVariable("RULER", original.RulingClan.Leader.Name)
                .SetTextVariable("REBELS", rebels.Name)
                .SetTextVariable("LEADER", rebels.RulingClan.Leader.Name);
            InformationManager.DisplayMessage(new InformationMessage(text.ToString(),
                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));

            ReintegrateMembers(rebels, original);
            DestroyKingdomAction.Apply(rebels);
            if (success) KingdomActions.SetRulerWithTitle(Claimant, original);
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
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Group.KingdomDiplomacy.Kingdom);
                Initialize(new TextObject("{=Yjq7GL10}Install {CLAIMANT}").SetTextVariable("CLAIMANT", Claimant.Name),
                    new TextObject("{=!}Members of the group demand that {CLAIMANT} be made sovereign ruler in stead of {RULER}. They defend {CLAIMANT} is a more legitimate ruler on the grounds of {SUCCESSION} succession laws as per the traditions of the {TITLE}, and are willing to impose their views.")
                    .SetTextVariable("RULER", Group.KingdomDiplomacy.Kingdom.RulingClan.Leader.Name) 
                    .SetTextVariable("SUCCESSION", title.Contract.Succession.Name)
                    .SetTextVariable("TITLE", title.FullName)
                    .SetTextVariable("CLAIMANT", Claimant.Name));
            }
            else
            {
                Initialize(new TextObject("{=!}Claimant"),
                    new TextObject("{=0XhSiqsR}A claimant group supports replacing the realm's current ruler with a claimant. The claimant must be a valid candidate under the realm's Succession law. The stronger a candidate, the more likely others will join the Claimant group. Current ruler's legitimacy and personal relationship with individual lords are also very important factors. In case the claimant is of the same clan as the current ruler, this means that they would become the family head themselves."));
            }
        }

        protected override void SetUpInternally()
        {
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Group.KingdomDiplomacy.Kingdom);
            if (title == null) return;

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
            SetTexts();
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
    }
}
