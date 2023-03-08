using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class DefaultCouncilPositions : DefaultTypeInitializer<DefaultCouncilPositions, CouncilMember>
    {
        public CouncilMember Marshal { get; set; } = new CouncilMember("Marshall");
        public CouncilMember Steward { get; set; } = new CouncilMember("Steward");
        public CouncilMember Chancellor { get; set; } = new CouncilMember("Chancellor");
        public CouncilMember Spiritual { get; set; } = new CouncilMember("Spiritual");
        public CouncilMember Spymaster { get; set; } = new CouncilMember("Spymaster");
        public CouncilMember Philosopher { get; set; } = new CouncilMember("Philosopher");
        public CouncilMember Castellan { get; set; } = new CouncilMember("Castellan");
        public CouncilMember Constable { get; set; } = new CouncilMember("Constable");
        public CouncilMember CourtPhysician { get; set; } = new CouncilMember("Constable");
        public CouncilMember CourtSmith { get; set; } = new CouncilMember("Constable");
        public CouncilMember Elder { get; set; } = new CouncilMember("Elder");
        public CouncilMember Spouse { get; set; } = new CouncilMember("Spouse");

        public override IEnumerable<CouncilMember> All
        {
            get
            {
                yield return Marshal;
                yield return Steward;
                yield return Chancellor;
                yield return Spymaster;
                yield return Spiritual;
                yield return Spouse;
            }
        }

        public override void Initialize()
        {
            Marshal.Initialize(
                DefaultSkills.Leadership,
                DefaultSkills.Tactics,
                new List<CouncilTask>() 
                { 
                    DefaultCouncilTasks.Instance.OrganizeMiltia.GetCopy(),
                    DefaultCouncilTasks.Instance.EncourageMilitarism.GetCopy()
                },
                new List<CouncilPrivileges>() { },
                (CouncilData data) =>
                {
                    return true;
                },
                (CouncilMember position, Hero hero) =>
                {
                    return true;
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=!}Ard Marasgal");
                        if (id == "empire") return new TextObject("{=!}Magister Domesticus");
                        if (id == "khuzait") return new TextObject("{=!}Tumetu-iin Noyan");

                        return new TextObject("{=!}Grand Marshal");
                    }

                    if (id == "battania") return new TextObject("{=!}Marasgal");
                    if (id == "khuzait") return new TextObject("{=!}Jagutu-iin Darga");
                    if (id == "empire") return new TextObject("{=!}Domesticus");

                    return new TextObject("{=!}Marshal");
                });

            Steward.Initialize(
                DefaultSkills.Steward,
                DefaultSkills.Trade,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.DevelopEconomy.GetCopy(),
                    DefaultCouncilTasks.Instance.OverseeProduction.GetCopy(),
                    DefaultCouncilTasks.Instance.PromoteCulture.GetCopy()
                },
                new List<CouncilPrivileges>() { },
                (CouncilData data) =>
                {
                    return true;
                },
                (CouncilMember position, Hero hero) =>
                {
                    return true;
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=!}Ard Sheumarlan");
                        if (id == "empire") return new TextObject("{=!}Magister Sacrarum Largitionum");

                        return new TextObject("{=!}High Steward");
                    }

                    if (id == "battania") return new TextObject("{=!}Sheumarlan");
                    if (id == "empire") return new TextObject("{=!}Praefectus Largitionum");

                    return new TextObject("{=!}Steward");
                });

            Chancellor.Initialize(
                DefaultSkills.Charm,
                BKSkills.Instance.Lordship,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.OverseeDignataries.GetCopy(),
                    DefaultCouncilTasks.Instance.ManageVassals.GetCopy(),
                    DefaultCouncilTasks.Instance.ArbitrateRelations.GetCopy()
                },
                new List<CouncilPrivileges>() { },
                (CouncilData data) =>
                {
                    return true;
                },
                (CouncilMember position, Hero hero) =>
                {
                    return true;
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=!}Ard Seansalair");
                        if (id == "empire") return new TextObject("{=!}Magister Cancellarius");

                        return new TextObject("{=!}High Chancellor");
                    }

                    if (id == "battania") return new TextObject("{=!}Seansalair");
                    if (id == "empire") return new TextObject("{=!}Cancellarius");

                    return new TextObject("{=!}Chancellor");
                });

            Spymaster.Initialize(
                DefaultSkills.Roguery,
                BKSkills.Instance.Lordship,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.OverseeSecurity.GetCopy(),
                    DefaultCouncilTasks.Instance.RepressCriminality.GetCopy()
                },
                new List<CouncilPrivileges>() { },
                (CouncilData data) =>
                {
                    return true;
                },
                (CouncilMember position, Hero hero) =>
                {
                    return true;
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=!}Ard Treòraiche");
                        if (id == "empire") return new TextObject("{=!}Magister Officiorum");
                        if (id == "khuzait") return new TextObject("{=!}Cherbi");

                        return new TextObject("{=!}Grand Spymaster");
                    }

                    if (id == "battania") return new TextObject("{=!}Treòraiche");
                    if (id == "khuzait") return new TextObject("{=!}Khevtuul");
                    if (id == "empire") return new TextObject("{=!}Custodis");

                    return new TextObject("{=!}Spymaster");
                });

            Spiritual.Initialize(
                BKSkills.Instance.Theology,
                BKSkills.Instance.Scholarship,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.PromoteFaith.GetCopy(),
                    DefaultCouncilTasks.Instance.CultivatePiety.GetCopy()
                },
                new List<CouncilPrivileges>() 
                {
                    CouncilPrivileges.CLERGYMEN_EXCLUSIVE
                },
                (CouncilData data) =>
                {
                    var clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(data.Clan.Leader);
                    return clanReligion != null;
                },
                (CouncilMember position, Hero hero) =>
                {
                    bool matchingFaith = false;
                    var clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(position.Clan.Leader);
                    if (clanReligion != null)
                    {
                        var heroReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
                        matchingFaith = heroReligion != null && heroReligion.Equals(clanReligion);
                    }

                    return BannerKingsConfig.Instance.ReligionsManager.IsPreacher(hero) && matchingFaith;
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {

                        if (id == "battania") return new TextObject("{=!}Ard Draoidh");
                        if (id == "sturgia") return new TextObject("{=!}Volkhvs");
                        if (id == "aserai") return new TextObject("{=!}Murshid");

                        return new TextObject("{=!}High Seneschal");
                    }

                    if (id == "battania") return new TextObject("{=!}Draoidh");
                    if (id == "sturgia") return new TextObject("{=!}Volkhvs");
                    if (id == "aserai") return new TextObject("{=!}Murshid");

                    return new TextObject("{=!}Seneschal");
                });

            Spouse.Initialize(
                BKSkills.Instance.Lordship,
                DefaultSkills.Steward,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.ManageDemesne.GetCopy()
                },
                new List<CouncilPrivileges>() { },
                (CouncilData data) =>
                {
                    return true;
                },
                (CouncilMember position, Hero hero) =>
                {
                    return hero.Spouse == position.Clan.Leader;
                },
                (CouncilMember member) =>
                {
                    return GameTexts.FindText("str_spouse");
                });

            Castellan.Initialize(
               DefaultSkills.Steward,
               BKSkills.Instance.Lordship,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.ManageDemesne.GetCopy()
               },
               new List<CouncilPrivileges>() { },
               (CouncilData data) =>
               {
                   var kingdom = data.Clan.Kingdom;
                   return kingdom != null && kingdom.Culture == Utils.Helpers.GetCulture("vlandia");
               },
               (CouncilMember position, Hero hero) =>
               {
                   return true;
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=!}Castellan");
               });

            Constable.Initialize(
               DefaultSkills.Steward,
               BKSkills.Instance.Lordship,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.ManageDemesne.GetCopy()
               },
               new List<CouncilPrivileges>() { },
               (CouncilData data) =>
               {
                   var kingdom = data.Clan.Kingdom;
                   if (kingdom != null)
                   {
                       var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                       if (sovereign != null)
                       {
                           return sovereign.contract.Government == Titles.GovernmentType.Feudal || 
                           sovereign.contract.Government == Titles.GovernmentType.Imperial;
                       }
                   }
                   
                   return false;
               },
               (CouncilMember position, Hero hero) =>
               {
                   return true;
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=!}Constable");
               });
        }
    }
}
