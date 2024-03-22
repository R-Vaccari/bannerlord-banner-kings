using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Utils.Models;
using Bannerlord.UIExtenderEx.Attributes;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.VanillaTabs.Kingdoms.Groups
{
    public class RadicalGroupVM : GroupItemVM
    {
        private bool isDemandEnabled, hasLeader, isInviteEnabled;
        private string demandName, createChance;
        private HintViewModel demandHint, inviteHint;
        private HintViewModel chanceHint;

        public RadicalGroup RadicalGroup => (RadicalGroup)Group;

        public RadicalGroupVM(RadicalGroup radical, KingdomGroupsVM groupsVM) : base(radical, groupsVM)
        {
            Members = new MBBindingList<GroupMemberVM>();
            Headers = new MBBindingList<StringPairItemVM>();
        }

        [DataSourceProperty] public string LeaderText => new TextObject("{=SrfYbg3x}Leader").ToString();
        [DataSourceProperty] public string GroupName => Group.Name.ToString();
        [DataSourceProperty] public string GroupText => Group.Description.ToString();
        [DataSourceProperty] public string InviteName => new TextObject("{=2xWSvbVc}Invite Members").ToString();
        [DataSourceProperty] public string ChanceHeader => new TextObject("{=Un7UY83V}Creation Chance").ToString();
        [DataSourceProperty] public HintViewModel Hint => new HintViewModel(Group.Description);

        public override void RefreshValues()
        {
            base.RefreshValues();
            Members.Clear();
            Headers.Clear();
            IsEmpty = Group.Members.Count == 0;
            HasLeader = Group.Leader != null;
            if (Group.Leader != null)
            {
                Leader = new GroupMemberVM(Group.Leader, true);
                if (Group.Leader.Clan != null)
                {
                    ClanBanner = new ImageIdentifierVM(BannerCode.CreateFrom(Group.Leader.Clan.Banner), true);
                }
            }

            foreach (var member in Group.GetSortedMembers(KingdomDiplomacy).Take(5))
            {
                if (member != Leader.Hero)
                {
                    Members.Add(new GroupMemberVM(member, true));
                }
            }

            if (Group.Members.IsEmpty())
            {
                List<Hero> heroes = new List<Hero>(30);
                BKExplainedNumber result = new BKExplainedNumber(0f, true);
                result.LimitMin(0f);
                result.LimitMax(1f);
                foreach (Hero hero in KingdomDiplomacy.Kingdom.Heroes)
                    if (BannerKingsConfig.Instance.InterestGroupsModel.CanHeroCreateAGroup(hero, KingdomDiplomacy))
                        heroes.Add(hero);

                float total = 0f;
                foreach (Hero hero in heroes)
                {
                    float r = BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroJoinChance(hero, Group, KingdomDiplomacy)
                        .ResultNumber / heroes.Count;
                    result.Add(r, hero.Name);
                    total += MathF.Max(0f, r);
                }

                ChanceText = FormatValue(total);
                ChanceHint = new HintViewModel(new TextObject("{=oVr1RVY0}{EXPLANATION}")
                    .SetTextVariable("EXPLANATION", result.GetFormattedPercentage()));

                EmptyGroupText = new TextObject("{=Bfkjk1o0}There is no {GROUP} currently active in the {REALM}. At any time, non-ruling clan leaders may start a radical group according to their interests, political leverage, relationships and support of the ruler.").ToString();

                ActionName = new TextObject("{=bLwFU6mw}Create Group").ToString();
                IsActionEnabled = BannerKingsConfig.Instance.InterestGroupsModel.CanHeroCreateAGroup(Hero.MainHero, KingdomDiplomacy);
            }
            else
            {
                if (Group.Members.Contains(Hero.MainHero))
                {
                    IsActionEnabled = Group.CanHeroLeave(Hero.MainHero, KingdomDiplomacy);
                    ActionName = new TextObject("Leave Group").ToString();
                    ActionHint = new HintViewModel(new TextObject("Leave Group"));
                }
                else
                {
                    IsActionEnabled = BannerKingsConfig.Instance.InterestGroupsModel.CanHeroJoinARadicalGroup(Hero.MainHero, KingdomDiplomacy);
                    ActionName = new TextObject("Join Group").ToString();
                }

                Headers.Add(new StringPairItemVM(new TextObject("{=znEakOmv}Radicalism").ToString(),
                new TextObject("{=8YCJrv0F}{NUMBER} / {CAPACITY}")
                .SetTextVariable("NUMBER", FormatValue(RadicalGroup.Radicalism))
                .SetTextVariable("CAPACITY", FormatValue(Group.CurrentDemand.MinimumGroupInfluence))
                .ToString(),
                new BasicTooltipViewModel(() => new TextObject("{=znEakOmv}Radicalism indicates the group's readiness. The minimum radicalism required to make an ultimatum is determined by the type of demand being made. Radicalism grows while the group represents 50% or more of the military force within the realm, and goes down otherwise. A group is dissolved once radicalism reaches 0%.").ToString())));

                Headers.Add(new StringPairItemVM(new TextObject("{=ZgRQ1v2d}Demand").ToString(),
                Group.CurrentDemand.Name.ToString(),
                new BasicTooltipViewModel(() => Group.CurrentDemand.Description.ToString())));

                Headers.Add(new StringPairItemVM(new TextObject("{=9G5uYwk6}Strength").ToString(),
                FormatValue(RadicalGroup.PowerProportion),
                new BasicTooltipViewModel(() => new TextObject("{=iaCoQ8Px}The military strength of the group's participants, in comparison to all other non-participants of the realm. 100% strength would mean that both sides have equal strength.").ToString())));
            }

            DemandName = new TextObject("{=30S3yEVo}Make Ultimatum").ToString();
            var canPush = Group.CanPushDemand(Group.CurrentDemand, RadicalGroup.Radicalism);
            IsDemandEnabled = canPush.Item1;
            DemandHint = new HintViewModel(
                new TextObject("{=8CtOagZE}Make an ultimatum to your ruler demanding they accept your terms. If rejected, you and your group peers will be denounced as enemies of the realm, and a civil war will begin."));

            IsInviteEnabled = Group.Leader == Hero.MainHero;
            InviteHint = new HintViewModel(new TextObject("{=vmbdT2Wf}Invite other members to your group. Only the group's leader can invite other members, at the expense of their influence. Members will be avaiable or not to be invited according to their willingness to participate in the group. Willing lords and ladies may also occasionally join the group on their own volition, without any costs to the leader."));
        }

        [DataSourceMethod]
        private void ExecuteInvite()
        {
            if (IsInviteEnabled)
            {
                List<InquiryElement> list = new List<InquiryElement>(10);
                foreach (Hero hero in KingdomDiplomacy.Kingdom.Heroes)
                {
                    if (BannerKingsConfig.Instance.InterestGroupsModel.CanHeroJoinARadicalGroup(hero, KingdomDiplomacy))
                    {
                        if (!Group.Members.Contains(hero))
                        {
                            BKExplainedNumber willing = BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroJoinChance(hero, Group, KingdomDiplomacy, true);
                            float influence = BannerKingsConfig.Instance.InterestGroupsModel.InviteToGroupInfluenceCost(Group, hero, KingdomDiplomacy).ResultNumber;
                            bool possible = true;
                            TextObject hint = new TextObject("{=GFAEtBRb}{HERO} leads the {CLAN}, a family of {PEERAGE}.{newline}Fiefs: {FIEFS}{newline}Estates: {ESTATES}{newline}{newline}{REASON}{newline}{newline}Willingness: {RESULT}{newline}-----{newline}{EXPLANATION}")
                                .SetTextVariable("HERO", hero.Name)
                                .SetTextVariable("CLAN", hero.Clan.Name)
                                .SetTextVariable("FIEFS", hero.Clan.Fiefs.Count)
                                .SetTextVariable("PEERAGE", BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan).Peerage.Name)
                                .SetTextVariable("ESTATES", BannerKingsConfig.Instance.PopulationManager.GetEstates(hero).Count)
                                .SetTextVariable("REASON", new TextObject("{=F2N7WBbz}This person is willing to back your radical group."))
                                .SetTextVariable("RESULT", FormatValue(willing.ResultNumber))
                                .SetTextVariable("EXPLANATION", willing.GetFormattedPercentage());
                            if (willing.ResultNumber < 0f)
                            {
                                possible = false;
                                hint = hint.SetTextVariable("REASON", new TextObject("{=RdWAc9p5}This person is not willing to back your radical group."));
                            }

                            if (Clan.PlayerClan.Influence < influence)
                            {
                                possible = false;
                                hint = hint.SetTextVariable("REASON", new TextObject("{=hVJNXynE}Not enough influence."));
                            }

                            list.Add(new InquiryElement(hero,
                                new TextObject("{=Hyfgj4Mw}{TYPE} - {INFLUENCE}{INFLUENCE_ICON}")
                                .SetTextVariable("TYPE", hero.Name)
                                .SetTextVariable("INFLUENCE", influence.ToString("0.0"))
                                .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                                .ToString(),
                                new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, true)),
                                possible,
                                hint.ToString()));
                        }
                    }
                }
                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=2xWSvbVc}Invite Members").ToString(),
                    new TextObject("{=vmbdT2Wf}Invite other members to your group. Only the group's leader can invite other members, at the expense of their influence. Members will be avaiable or not to be invited according to their willingness to participate in the group. Willing lords and ladies may also occasionally join the group on their own volition, without any costs to the leader.").ToString(),
                    list,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_done").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    (list) =>
                    {
                        Hero hero = (Hero)list.First().Identifier;
                        float influence = BannerKingsConfig.Instance.InterestGroupsModel.InviteToGroupInfluenceCost(Group, hero, KingdomDiplomacy).ResultNumber;
                        ChangeClanInfluenceAction.Apply(Clan.PlayerClan, -influence);
                        Group.AddMember(hero);
                        RefreshValues();
                    },
                    null));
            }
        }

        [DataSourceMethod]
        private void ExecuteAction()
        {
            if (IsEmpty)
            {
                InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=bLwFU6mw}Create Group").ToString(),
                    new TextObject("{=vRvVDXgC}Push for a demand as a radical {GROUP} group. Once created, you can not abandon the group without suffering consequences. Other members will join based on how they like you, the demand and their perception of the ruler.{newline}{newline}A radical group slowly gathers Radicalism, so long their combined forces are equal or greater to 50% of the loyalist forces. The group  can push an ultimatum to the ruler once Radicalism reaches the minimum defined threshold set by the demand type.")
                    .SetTextVariable("GROUP", GroupName)
                    .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    () =>
                    {
                        RadicalGroup.SetupRadicalGroup(Hero.MainHero, this);
                        RefreshValues();
                    },
                    null));
            }
            else
            {
                if (Group.Members.Contains(Hero.MainHero))
                {
                    InformationManager.ShowInquiry(new InquiryData(
                        new TextObject("{=ds1KP4Qc}Leave Group").ToString(),
                        new TextObject("{=pXkS1xV5}Leaving the group will harm the members' opinion of you, specially if you lead the group.").ToString(),
                        true,
                        true,
                        GameTexts.FindText("str_accept").ToString(),
                        GameTexts.FindText("str_cancel").ToString(),
                        () =>
                        {
                            Group.RemoveMember(Hero.MainHero);
                            RefreshValues();
                        },
                        null));
                }
                else
                {
                    InformationManager.ShowInquiry(new InquiryData(
                       new TextObject("{=9SnWS77u}Join Group").ToString(),
                       new TextObject("{=XWOjp2ZM}You may join the {GROUP} group, represented by {LEADER}. Once joined, other members will expect your participation.")
                       .SetTextVariable("GROUP", GroupName)
                       .SetTextVariable("LEADER", Group.Leader.Name)
                       .ToString(),
                       true,
                       true,
                       GameTexts.FindText("str_accept").ToString(),
                       GameTexts.FindText("str_cancel").ToString(),
                       () =>
                       {
                           Group.AddMember(Hero.MainHero);
                           RefreshValues();
                       },
                       null));
                }
            }
        }

        [DataSourceMethod]
        private void ExecuteDemand()
        {
            float rebelStrength = RadicalGroup.TotalStrength;
            Hero ruler = Group.KingdomDiplomacy.Kingdom.Leader;
            float accept = RadicalGroup.CurrentDemand.PositiveAnswer.CalculateAiLikelihood(ruler);
            float deny = RadicalGroup.CurrentDemand.NegativeAnswer.CalculateAiLikelihood(ruler);
            float total = 0f;
            if (accept > 0f) total += accept;
            if (deny > 0f) total += deny;
            float chance = accept / total;

            InformationManager.ShowInquiry(new InquiryData(
                    new TextObject("{=30S3yEVo}Make Ultimatum").ToString(),
                    new TextObject("{=8CtOagZE}Make an ultimatum to your ruler demanding they accept your terms. If rejected, you and your group peers will be denounced as enemies of the realm, and a civil war will begin.{newline}{newline}{RULER} is {CHANCE} likely to conceive to this demand.{newline}{newline}Loyalist strength: {LOYALIST_STRENGTH}{newline}Rebel strength: {REBEL_STRENGTH}")
                    .SetTextVariable("LOYALIST_STRENGTH", Group.KingdomDiplomacy.Kingdom.TotalStrength - rebelStrength)
                    .SetTextVariable("REBEL_STRENGTH", rebelStrength)
                    .SetTextVariable("RULER", ruler.Name)
                    .SetTextVariable("CHANCE", FormatValue(chance))
                    .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    () =>
                    {
                        Group.CurrentDemand.PushForDemand();
                    },
                    null));
        }

        [DataSourceProperty]
        public bool HasLeader
        {
            get => hasLeader;
            set
            {
                if (value != hasLeader)
                {
                    hasLeader = value;
                    OnPropertyChangedWithValue(value, "HasLeader");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ChanceHint
        {
            get => chanceHint;
            set
            {
                if (value != chanceHint)
                {
                    chanceHint = value;
                    OnPropertyChangedWithValue(value, "ChanceHint");
                }
            }
        }

        [DataSourceProperty]
        public bool IsDemandEnabled
        {
            get => isDemandEnabled;
            set
            {
                if (value != isDemandEnabled)
                {
                    isDemandEnabled = value;
                    OnPropertyChangedWithValue(value, "IsDemandEnabled");
                }
            }
        }

        [DataSourceProperty]
        public bool IsInviteEnabled
        {
            get => isInviteEnabled;
            set
            {
                if (value != isInviteEnabled)
                {
                    isInviteEnabled = value;
                    OnPropertyChangedWithValue(value, "IsInviteEnabled");
                }
            }
        }

        [DataSourceProperty]
        public string ChanceText
        {
            get => createChance;
            set
            {
                if (value != createChance)
                {
                    createChance = value;
                    OnPropertyChangedWithValue(value, "ChanceText");
                }
            }
        }

        [DataSourceProperty]
        public string DemandName
        {
            get => demandName;
            set
            {
                if (value != demandName)
                {
                    demandName = value;
                    OnPropertyChangedWithValue(value, "DemandName");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel DemandHint
        {
            get => demandHint;
            set
            {
                if (value != demandHint)
                {
                    demandHint = value;
                    OnPropertyChangedWithValue(value, "DemandHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel InviteHint
        {
            get => inviteHint;
            set
            {
                if (value != inviteHint)
                {
                    inviteHint = value;
                    OnPropertyChangedWithValue(value, "InviteHint");
                }
            }
        }
    }
}
