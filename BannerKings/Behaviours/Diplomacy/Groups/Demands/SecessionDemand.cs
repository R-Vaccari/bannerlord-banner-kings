using BannerKings.Actions;
using BannerKings.CampaignContent.Traits;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Models;
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
    public class SecessionDemand : RadicalDemand
    {
        public SecessionDemand() : base("Secession")
        {
            SetTexts();
        }

        [SaveableProperty(1)] public Clan Rebel { get; private set; }

        public override DemandResponse PositiveAnswer => new DemandResponse(new TextObject("{=kyB8tkgY}Concede"),
                    new TextObject("{=!}Accept the demand to cede independence to the members of the Secession group led by {HERO}. They will be satisfied with this outcome.")
                    .SetTextVariable("HERO", Group.Leader.Name),
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
                                new TextObject("{=53q6oUzO}The {GROUP} is satisfied! The members of the group will now organize themselves in a newly made realm.")
                                .SetTextVariable("GROUP", Group.Name)
                                .ToString(),
                                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        }

                        foreach (Hero member1 in Group.Members)
                            foreach (Hero member2 in Group.Members)
                                if (member1 != member2) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(member1, member2, 5);

                        RebellionActions.CreateRebelKingdom(Group.Leader.Clan, 
                            Group.Members.ConvertAll(x => x.Clan), 
                            Group.KingdomDiplomacy.Kingdom);

                        return true;
                    });

        public override DemandResponse NegativeAnswer => new DemandResponse(new TextObject("{=PoAmUqGR}Reject"),
                   new TextObject("{=!}Deny the demand to cede independence to the rebels led by {HERO}. They will not like this outcome.")
                   .SetTextVariable("HERO", Rebel.Leader.Name),
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

        public override bool Active => Rebel != null && !Rebel.IsEliminated;
        protected override bool IsFulfilled() => Rebel.Kingdom != Group.KingdomDiplomacy.Kingdom;

        public override IEnumerable<DemandResponse> DemandResponses
        {
            get
            {
                yield return PositiveAnswer;
                yield return NegativeAnswer;
            }
        }

        protected override TextObject PlayerPromptText => new TextObject("{=!}The {GROUP} group is pushing for the independence of its members. You may choose to resolve it now or postpone the decision. If so, the group will demand a definitive answer 7 days from now.")
                .SetTextVariable("GROUP", Group.Name);

        protected override TextObject PlayerAnswersText => new TextObject("{=BKAdUzjo}The {GROUP} is pushing for the independenc of its members. The group is currently lead by {LEADER}{LEADER_ROLE}. The group currently has {STRENGTH}% military strength relative to your loyalist forces.")
                .SetTextVariable("STRENGTH", (Group as RadicalGroup).PowerProportion)
                .SetTextVariable("LEADER_ROLE", GetHeroRoleText(Group.Leader))
                .SetTextVariable("LEADER", Group.Leader.Name)
                .SetTextVariable("GROUP", Group.Name);

        public override void EndRebellion(Kingdom rebels, Kingdom original, bool success)
        {
            TextObject text = success ? new TextObject("{=!}The rebels of the {REBELS}, led by {LEADER} have been successful in their rebellion! {LEADER} has newfound legitimacy as the sovereign ruler of the {REBELS}...")
                .SetTextVariable("LEADER", rebels.RulingClan.Leader.Name)
                .SetTextVariable("REBELS", rebels.Name)
                :
                new TextObject("{=!}The rebels of the {REBELS}, led by {LEADER} have failed their rebellion... Their judgement falls now to {RULER}.")
                .SetTextVariable("RULER", original.RulingClan.Leader.Name)
                .SetTextVariable("REBELS", rebels.Name)
                .SetTextVariable("LEADER", rebels.RulingClan.Leader.Name);
            InformationManager.DisplayMessage(new InformationMessage(text.ToString(),
                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));

            KingdomDiplomacy diplomacy = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(rebels);
            if (diplomacy != null)
            {
                diplomacy.AddLegitimacy(0.2f);
            }

            if (!success)
            {
                ReintegrateMembers(rebels, original);
                DestroyKingdomAction.Apply(rebels);
            }
        }

        public override void Finish()
        {
            Rebel = null;
            DueDate = CampaignTime.Never;

            FinishRadicalDemand();
        }

        public override Demand GetCopy(DiplomacyGroup group)
        {
            SecessionDemand demand = new SecessionDemand();
            demand.Group = group;
            demand.Rebel = Rebel;
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
            if (Rebel == null) Initialize(new TextObject("{=!}Secession"),
                    new TextObject("{=!}Demand secession from the current realm. Group members are reorganized in a newly founded realm lead by a ruler of their choosing. Secession apologists do not pursue any changes in their existing realm - only to leave it for a realm of their own making. Unlike independence apologists, secession group members will be bound together in a single new realm. Members will be persuaded to join according to their opinions of their current ruler and the ruler-to-be supported by the group."));
            else Initialize(new TextObject("{=!}Secession for {REBEL}").SetTextVariable("REBEL", Rebel.Name),
                    new TextObject("{=!}Members of the group demand that they be let go of the {KINGDOM}, such that they may organize themselves in a new realm, led by {REBEL}.")
                    .SetTextVariable("KINGDOM", Group.KingdomDiplomacy.Kingdom.Name)
                    .SetTextVariable("REBEL", Rebel.Name));
        }

        protected override void SetUpInternally()
        {
            List<(Clan, float)> results = new List<(Clan, float)>();
            foreach (Clan clan in Group.MemberClans)
            {
                results.Add(new (clan,
                    BannerKingsConfig.Instance.InterestGroupsModel.CalculateClanInfluence(clan, Group.KingdomDiplomacy)
                    .ResultNumber));
            }

            Rebel = MBRandom.ChooseWeighted(results);
        }

        public override void ShowPlayerDemandOptions()
        {
            List<InquiryElement> list = new List<InquiryElement>();
            var playerClan = Clan.PlayerClan;
            list.Add(new InquiryElement(playerClan,
                playerClan.Name.ToString(),
                new ImageIdentifier(playerClan.Banner)));

            foreach (Clan clan in Group.KingdomDiplomacy.Kingdom.Clans)
            {
                if (clan == Group.KingdomDiplomacy.Kingdom.RulingClan || clan == playerClan) continue;

                Hero hero = clan.Leader;
                BKExplainedNumber join = BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroJoinChance(hero,
                    Group,
                    Group.KingdomDiplomacy);
                
                TextObject hint = new TextObject("{=UoPv1RvG}{HERO} {ROLE}, a family of {PEERAGE} and holder of {FIEFS} fiefs.{newline}Willingness to join group: {RESULT}{newline}-----{newline}{EXPLANATION}")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("ROLE", GetHeroRoleText(hero))
                    .SetTextVariable("FIEFS", hero.Clan.Fiefs.Count)
                    .SetTextVariable("PEERAGE", BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan).Peerage.Name)
                    .SetTextVariable("REASON", new TextObject("{=F2N7WBbz}This person is willing to back your radical group."))
                    .SetTextVariable("RESULT", join.ResultNumber)
                    .SetTextVariable("EXPLANATION", join.GetExplanations());

                list.Add(new InquiryElement(clan,
                    clan.Name.ToString(),
                    new ImageIdentifier(clan.Banner),
                    join.ResultNumber > 0f,
                    hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(Name.ToString(),
                new TextObject("{=!}As leader of the secession group, you must choose who would rule the newly founded realm that secedes from the {KINGDOM}. This decision is final and will not change until the entire group is unmade.{newline}{newline}You may chose yourself or any other peers that would be willing to join the group.")
                .SetTextVariable("KINGDOM", Group.KingdomDiplomacy.Kingdom.Name)
                .ToString(),
                list,
                true,
                1,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                (List<InquiryElement> list) =>
                {
                    Clan clan = (Clan)list[0].Identifier;
                    Rebel = clan;
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
