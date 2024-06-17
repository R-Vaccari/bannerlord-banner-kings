using BannerKings.Managers;
using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class DefaultMercenaryPrivileges : DefaultTypeInitializer<DefaultMercenaryPrivileges, MercenaryPrivilege>
    {
        public override IEnumerable<MercenaryPrivilege> All
        {
            get
            {
                yield return IncreasedPay;
                yield return EstateGrant;
                yield return WorkshopGrant;
                yield return CustomTroop3;
                yield return CustomTroop5;
                yield return BaronyGrant;
                yield return FullPeerage;
            }
        }

        public MercenaryPrivilege IncreasedPay { get; set; } = new MercenaryPrivilege("privilege_increased_pay");
        public MercenaryPrivilege EstateGrant { get; set; } = new MercenaryPrivilege("privilege_estate_grant");
        public MercenaryPrivilege WorkshopGrant { get; set; } = new MercenaryPrivilege("privilege_workshop_grant");
        public MercenaryPrivilege CustomTroop3 { get; set; } = new MercenaryPrivilege("privilege_levy_troop");
        public MercenaryPrivilege CustomTroop5 { get; set; } = new MercenaryPrivilege("privilege_professional_troop");
        public MercenaryPrivilege BaronyGrant { get; set; } = new MercenaryPrivilege("privilege_barony_grant");
        public MercenaryPrivilege FullPeerage { get; set; } = new MercenaryPrivilege("privilege_full_peerage");

        public override void Initialize()
        {
            IncreasedPay.Initialize(new TextObject("{=Qf9prhMy}Increased Pay"),
                new TextObject("{=6a12xd7A}Increase the revenue of your mercenary contract by 5% per level."),
                new TextObject("{=NWcM56ZP}You fail to meet the requirements or privilege is already maxed out.\n\nPoints: {POINTS}\nMax level: {LEVEL}")
                .SetTextVariable("LEVEL", 5)
                .SetTextVariable("POINTS", 100),
                100f,
                5,
                delegate (MercenaryCareer career)
                {
                    return true;
                },
                (MercenaryCareer career) => true);

            WorkshopGrant.Initialize(new TextObject("Workshop Grant"),
                new TextObject("{=ViDeTjkQ}Acquire a workshop property. The property will be situated in one of the towns of your contractor."),
                new TextObject("{=9PCO8Wkn}Your contractor is not currently capable of granting a workshop, due to lacking themselves or funds to subsidize the acquisition of one. Alternatively, you've reached your workshop limit.\nPoints: {POINTS}\nMax level: {LEVEL}")
                .SetTextVariable("LEVEL", 2)
                .SetTextVariable("POINTS", 500),
                500f,
                2,
                delegate (MercenaryCareer career)
                {

                    return MercenaryCareer.GetWorkshopPrivilege(career) != null;
                },
                (MercenaryCareer career) =>
                {
                    var workshop = MercenaryCareer.GetWorkshopPrivilege(career);
                    if (workshop != null)
                    {
                        ChangeOwnerOfWorkshopAction.ApplyByDeath(workshop, career.Clan.Leader);
                        MBInformationManager.AddQuickInformation(new TextObject("{=7OaTLJKt}You are now the owner of {WORKSHOP} at {TOWN}!")
                            .SetTextVariable("WORKSHOP", workshop.Name)
                            .SetTextVariable("TOWN", workshop.Settlement.Name),
                            0,
                            null,
                            Utils.Helpers.GetRelationDecisionSound());
                        return true;
                    }

                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=kQDtk9bU}Your contractor was not able to secure a Workshop.")
                        .ToString()));
                    return false;
                });

            EstateGrant.Initialize(new TextObject("{=GvTJdsfX}Estate Grant"),
                new TextObject("{=zk3hbtC1}Acquire an estate property. The property will be situated in one of the villages of your contractor."),
                new TextObject("{=v0PrfFDN}Your contractor is currently unable to provide a vacant estate.\nPoints: {POINTS}\nMax level: {LEVEL}")
                .SetTextVariable("LEVEL", 2)
                .SetTextVariable("POINTS", 650),
                650f,
                2,
                delegate (MercenaryCareer career)
                {
                    return MercenaryCareer.GetEstatePrivilege(career) != null;
                },
                (MercenaryCareer career) =>
                {
                    var estate = MercenaryCareer.GetEstatePrivilege(career);
                    if (estate != null)
                    {
                        var action = BannerKingsConfig.Instance.EstatesModel.GetGrant(estate, estate.Owner, career.Clan.Leader);
                        action.TakeAction();
                        return true;
                    }

                    InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=YV3Df2q9}Your contractor was not able to secure an Estate.")
                        .ToString()));
                    return false;
                });

            CustomTroop3.Initialize(new TextObject("{=ytHNCcJQ}Mercenary Levy"),
                new TextObject("{=tDejPnS2}Stablish a mercenary levy (tier III) troop for your company. A custrom troop is designable and will only be available for your clan, in towns of the kingdom associated with the career they were designed."),
                new TextObject("{=NWcM56ZP}You fail to meet the requirements or privilege is already maxed out.\n\nPoints: {POINTS}\nMax level: {LEVEL}")
                .SetTextVariable("POINTS", 200)
                .SetTextVariable("LEVEL", 1),
                200f,
                1,
                delegate (MercenaryCareer career)
                {
                    return true;
                },
                (MercenaryCareer career) => true);

            CustomTroop5.Initialize(new TextObject("{=ON7vvuzA}Mercenary Professional"),
                new TextObject("{=rFuJyb17}Stablish a mercenary professional (tier V) troop for your company. A custrom troop is designable and will only be available for your clan, in towns of the kingdom associated with the career they were designed."),
                new TextObject("{=NWcM56ZP}You fail to meet the requirements or privilege is already maxed out.\n\nPoints: {POINTS}\nMax level: {LEVEL}")
                .SetTextVariable("LEVEL", 1)
                .SetTextVariable("POINTS", 300),
                300f,
                1,
                delegate (MercenaryCareer career)
                {
                    return true;
                },
                (MercenaryCareer career) => true);

            BaronyGrant.Initialize(new TextObject("Barony Grant"),
                new TextObject("{=53uskzyT}Become landed in the fashion of a lord. Request a castle alongside it's barony-level title. The settlement ownership will not undo your mercenary contract."),
                new TextObject("{=r88bme7w}You must not own any settlements or titles. Your contractor may not have an extra castle and it's title available.\nPoints: {POINTS}\nMax level: {LEVEL}")
                .SetTextVariable("LEVEL", 1)
                .SetTextVariable("POINTS", 1000f),
                1000f,
                1,
                delegate (MercenaryCareer career)
                {
                    return MercenaryCareer.GetBaronyPrivilege(career) != null;
                },
                (MercenaryCareer career) =>
                {
                    var barony = MercenaryCareer.GetBaronyPrivilege(career);
                    if (barony != null)
                    {
                        var title = BannerKingsConfig.Instance.TitleManager.GetTitle(barony);
                        var action = BannerKingsConfig.Instance.TitleModel.GetAction(Managers.Titles.ActionType.Grant,
                            title, title.deJure, career.Clan.Leader);
                        action.TakeAction(career.Clan.Leader);
                        return true;
                    }

                    return false;
                });

            FullPeerage.Initialize(new TextObject("Full Peerage"),
                new TextObject("{=CBzT54PD}For your extraordinary service, have your clan considered a full Peer. While the Peerage won't be effective until you leave your mercenary life behind, you will be able to join kingdoms as the elite of the nobility, rather than a lesser Peer."),
                new TextObject("{=3Hg6tAbR}Your clan must not have a Peerage that allows all Peer privileges.\n\nPoints: {POINTS}\nMax level: {LEVEL}")
                .SetTextVariable("POINTS", 1000)
                .SetTextVariable("LEVEL", 1),
                1000f,
                1,
                delegate (MercenaryCareer career)
                {
                    var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(career.Clan);
                    Peerage peerage = council.Peerage;
                    return peerage == null || !peerage.IsFullPeerage;
                },
                (MercenaryCareer career) =>
                {
                    var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
                    if (council.Peerage == null || !council.Peerage.IsFullPeerage)
                    {
                        council.SetPeerage(new Peerage(new TextObject("{=9OhMK2Wk}Full Peerage"), true,
                                        true, true, true, true, false));
                        if (career.Clan == Clan.PlayerClan)
                        {
                            var peerage = council.Peerage;
                            InformationManager.ShowInquiry(new InquiryData(
                                peerage.Name.ToString(),
                                new TextObject("{=NgoW3jRZ}A a privilege for your loyal service, the {CLAN} is now considered to have {PEERAGE}. {TEXT}")
                                .SetTextVariable("CLAN", Clan.PlayerClan.Name)
                                .SetTextVariable("PEERAGE", peerage.Name)
                                .SetTextVariable("TEXT", peerage.PeerageGrantedText())
                                .ToString(),
                                true,
                                false,
                                GameTexts.FindText("str_ok").ToString(),
                                String.Empty,
                                null,
                                null));
                        }

                        return true;
                    }
                    return false;
                });
        }
    }
}