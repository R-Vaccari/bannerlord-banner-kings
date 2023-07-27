using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Managers.Traits;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
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
        public CouncilMember CourtPhysician { get; set; } = new CouncilMember("CourtPhysician");
        public CouncilMember CourtSmith { get; set; } = new CouncilMember("CourtSmith");
        public CouncilMember CourtMusician { get; set; } = new CouncilMember("CourtMusician");
        public CouncilMember Antiquarian { get; set; } = new CouncilMember("Antiquarian");
        public CouncilMember Elder { get; set; } = new CouncilMember("Elder");
        public CouncilMember Spouse { get; set; } = new CouncilMember("Spouse");
        public CouncilMember LegionCommander1 { get; set; } = new CouncilMember("LegionCommander1");
        public CouncilMember LegionCommander2 { get; set; } = new CouncilMember("LegionCommander2");
        public CouncilMember LegionCommander3 { get; set; } = new CouncilMember("LegionCommander3");
        public CouncilMember LegionCommander4 { get; set; } = new CouncilMember("LegionCommander4");
        public CouncilMember LegionCommander5 { get; set; } = new CouncilMember("LegionCommander5");

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
                yield return Castellan;
                yield return Constable;
                yield return CourtPhysician;
                yield return CourtSmith;
                yield return CourtMusician;
                yield return Antiquarian;
                yield return LegionCommander1;
                yield return LegionCommander2;
                yield return LegionCommander3;
                yield return LegionCommander4;
                yield return LegionCommander5;
                foreach (CouncilMember item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            LegionCommander1.Initialize(
                DefaultSkills.Leadership,
                DefaultSkills.Tactics,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.GatherLegion.GetCopy()
                },
                new List<CouncilPrivileges>()
                {
                    CouncilPrivileges.ARMY_PRIVILEGE,
                    CouncilPrivileges.NOBLE_EXCLUSIVE
                },
                (CouncilData data) =>
                 {
                    var kingdom = data.Clan.Kingdom;
                    if (kingdom != null)
                    {
                        var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                        if (sovereign != null)
                        {
                            return data.IsRoyal && sovereign.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion);
                        }
                    }

                    return false;
                },
                (CouncilMember position, Hero hero) =>
                {
                    if (!hero.IsClanLeader()) return new(false, new TextObject("{=!}Hero must be a clan leader."));
                    return new(true, null);
                }, 
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (id == "empire") return new TextObject("{=!}Legatus");

                    return new TextObject("{=!}Legate");
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Valor, 0.05f },
                    { DefaultTraits.Calculating, 0.05f }
                },
                true);

            LegionCommander2.Initialize(
                DefaultSkills.Leadership,
                DefaultSkills.Tactics,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.GatherLegion.GetCopy()
                },
                new List<CouncilPrivileges>()
                {
                    CouncilPrivileges.ARMY_PRIVILEGE,
                    CouncilPrivileges.NOBLE_EXCLUSIVE
                },
                (CouncilData data) =>
                {
                    var kingdom = data.Clan.Kingdom;
                    if (kingdom != null)
                    {
                        var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                        if (sovereign != null)
                        {
                            return data.IsRoyal && sovereign.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion);
                        }
                    }

                    return false;
                },
                (CouncilMember position, Hero hero) =>
                {
                    if (!hero.IsClanLeader()) return new(false, new TextObject("{=!}Hero must be a clan leader."));
                    return new(true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (id == "empire") return new TextObject("{=!}Legatus");

                    return new TextObject("{=!}Legate");
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Valor, 0.05f },
                    { DefaultTraits.Calculating, 0.05f }
                },
                true);

            LegionCommander3.Initialize(
                DefaultSkills.Leadership,
                DefaultSkills.Tactics,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.GatherLegion.GetCopy()
                },
                new List<CouncilPrivileges>()
                {
                    CouncilPrivileges.ARMY_PRIVILEGE,
                    CouncilPrivileges.NOBLE_EXCLUSIVE
                },
                (CouncilData data) =>
                {
                    var kingdom = data.Clan.Kingdom;
                    if (kingdom != null)
                    {
                        var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                        if (sovereign != null)
                        {
                            return data.IsRoyal && sovereign.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion);
                        }
                    }

                    return false;
                },
                (CouncilMember position, Hero hero) =>
                {
                    if (!hero.IsClanLeader()) return new(false, new TextObject("{=!}Hero must be a clan leader."));
                    return new(true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (id == "empire") return new TextObject("{=!}Legatus");

                    return new TextObject("{=!}Legate");
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Valor, 0.05f },
                    { DefaultTraits.Calculating, 0.05f }
                },
                true);

            LegionCommander4.Initialize(
                DefaultSkills.Leadership,
                DefaultSkills.Tactics,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.GatherLegion.GetCopy()
                },
                new List<CouncilPrivileges>()
                {
                    CouncilPrivileges.ARMY_PRIVILEGE,
                    CouncilPrivileges.NOBLE_EXCLUSIVE
                },
                (CouncilData data) =>
                {
                    var kingdom = data.Clan.Kingdom;
                    if (kingdom != null)
                    {
                        var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                        if (sovereign != null)
                        {
                            return data.IsRoyal && sovereign.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion);
                        }
                    }

                    return false;
                },
                (CouncilMember position, Hero hero) =>
                {
                    if (!hero.IsClanLeader()) return new(false, new TextObject("{=!}Hero must be a clan leader."));
                    return new(true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (id == "empire") return new TextObject("{=!}Legatus");

                    return new TextObject("{=!}Legate");
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Valor, 0.05f },
                    { DefaultTraits.Calculating, 0.05f }
                },
                true);

            LegionCommander5.Initialize(
                DefaultSkills.Leadership,
                DefaultSkills.Tactics,
                new List<CouncilTask>()
                {
                    DefaultCouncilTasks.Instance.GatherLegion.GetCopy()
                },
                new List<CouncilPrivileges>() 
                { 
                    CouncilPrivileges.ARMY_PRIVILEGE,
                    CouncilPrivileges.NOBLE_EXCLUSIVE
                },
                (CouncilData data) =>
                {
                    var kingdom = data.Clan.Kingdom;
                    if (kingdom != null)
                    {
                        var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                        if (sovereign != null)
                        {
                            return data.IsRoyal && sovereign.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion);
                        }
                    }

                    return false;
                },
                (CouncilMember position, Hero hero) =>
                {
                    if (!hero.IsClanLeader()) return new(false, new TextObject("{=!}Hero must be a clan leader."));
                    return new(true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (id == "empire") return new TextObject("{=!}Legatus");

                    return new TextObject("{=!}Legate");
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Valor, 0.05f },
                    { DefaultTraits.Calculating, 0.05f }
                },
                true);

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
                    return new (true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=iTWqZLM4}Ard Marasgal");
                        if (id == "empire") return new TextObject("{=MqHWpT0K}Magister Domesticus");
                        if (id == "khuzait") return new TextObject("{=Qtt0vXAT}Tumetu-iin Noyan");

                        return new TextObject("{=7TxiJwdM}Grand Marshal");
                    }

                    if (id == "battania") return new TextObject("{=2SU2KRvB}Marasgal");
                    if (id == "khuzait") return new TextObject("{=hfqCCmZi}Jagutu-iin Darga");
                    if (id == "empire") return new TextObject("{=Qk2mgePL}Domesticus");

                    return new TextObject("{=SCsGXova}Marshal");
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
                    return new(true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=M6eW9798}Ard Sheumarlan");
                        if (id == "empire") return new TextObject("{=8sSPs8QV}Magister Sacrarum Largitionum");

                        return new TextObject("{=3OSi32pX}High Steward");
                    }

                    if (id == "battania") return new TextObject("{=DJkHjoo4}Sheumarlan");
                    if (id == "empire") return new TextObject("{=uP0GHCjS}Praefectus Largitionum");

                    return new TextObject("{=k4oyM9dT}Steward");
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Generosity, -0.05f },
                    { DefaultTraits.Calculating, 0.1f }
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
                    return new(true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=wWNKVNgU}Ard Seansalair");
                        if (id == "empire") return new TextObject("{=RHT0X2ZU}Magister Cancellarius");

                        return new TextObject("{=EYfcHKO1}High Chancellor");
                    }

                    if (id == "battania") return new TextObject("{=pA79P1LE}Seansalair");
                    if (id == "empire") return new TextObject("{=qRVOadig}Cancellarius");

                    return new TextObject("{=tgz9ut5s}Chancellor");
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
                    return new(true, null);
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {
                        if (id == "battania") return new TextObject("{=fTuydBMn}Ard Treòraiche");
                        if (id == "empire") return new TextObject("{=HWfVPgFa}Magister Officiorum");
                        if (id == "khuzait") return new TextObject("{=7PLFhL3m}Cherbi");

                        return new TextObject("{=08umUPH5}Grand Spymaster");
                    }

                    if (id == "battania") return new TextObject("{=FQe5GXkp}Treòraiche");
                    if (id == "khuzait") return new TextObject("{=FsFE8NSM}Khevtuul");
                    if (id == "empire") return new TextObject("{=bZCeizLU}Custodis");

                    return new TextObject("{=ZJ8eRkS2}Spymaster");
                },
                 new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Mercy, -0.1f },
                    { DefaultTraits.Calculating, 0.1f }
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

                    return new (BannerKingsConfig.Instance.ReligionsManager.IsPreacher(hero) && matchingFaith, 
                        new TextObject("{=1A5Q6wHM}The candidate must be a preacher of matching faith with the council leader."));
                },
                (CouncilMember member) =>
                {
                    var id = member.Culture.StringId;
                    if (member.IsRoyal)
                    {

                        if (id == "battania") return new TextObject("{=PkQ9BKTk}Ard Draoidh");
                        if (id == "sturgia") return new TextObject("{=ogAzFznn}Volkhvs");
                        if (id == "aserai") return new TextObject("{=!Murshid");

                        return new TextObject("{=rhL4NnWR}High Seneschal");
                    }

                    if (id == "battania") return new TextObject("{=ELf8YFXe}Draoidh");
                    if (id == "sturgia") return new TextObject("{=ogAzFznn}Volkhvs");
                    if (id == "aserai") return new TextObject("{=!}Murshid");

                    return new TextObject("{=ZNzX7SKR}Seneschal");
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
                    return new (hero.Spouse == position.Clan.Leader, new TextObject("{=ZQujL7sW}The candidate must be a/the spouse of the council leader."));
                },
                (CouncilMember member) =>
                {
                    return GameTexts.FindText("str_spouse");
                });

            CourtPhysician.Initialize(
               DefaultSkills.Medicine,
               BKSkills.Instance.Scholarship,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.FamilyCare.GetCopy()
               },
               new List<CouncilPrivileges>() { },
               (CouncilData data) =>
               {
                   return data.Clan.Fiefs.Count > 0;
               },
               (CouncilMember position, Hero hero) =>
               {
                   return new(true, null);
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=Gc1CyVPk}Court Physician");
               },
               new Dictionary<TraitObject, float>()
               {
               });

            CourtSmith.Initialize(
               DefaultSkills.Crafting,
               null,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.SmithWeapons.GetCopy(),
                    DefaultCouncilTasks.Instance.SmithArmors.GetCopy(),
                    DefaultCouncilTasks.Instance.SmithBardings.GetCopy()
               },
               new List<CouncilPrivileges>() { },
               (CouncilData data) =>
               {
                   return data.Clan.Fiefs.Count > 0;
               },
               (CouncilMember position, Hero hero) =>
               {
                   return new(true, null);
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=fWxtaYqn}Court Smith");
               });

            CourtMusician.Initialize(
               DefaultSkills.Medicine,
               BKSkills.Instance.Scholarship,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.EntertainFeastsMusician.GetCopy()
               },
               new List<CouncilPrivileges>() { },
               (CouncilData data) =>
               {
                   return data.Clan.Fiefs.Count > 0;
               },
               (CouncilMember position, Hero hero) =>
               {
                   return new(true, null);
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=O951oUMh}Court Musician");
               },
               new Dictionary<TraitObject, float>()
               {
                   { BKTraits.Instance.Musician, 0.8f }
               });

            Antiquarian.Initialize(
               DefaultSkills.Medicine,
               BKSkills.Instance.Scholarship,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.EducateFamilyAntiquarian.GetCopy()
               },
               new List<CouncilPrivileges>() { },
               (CouncilData data) =>
               {
                   return data.Clan.Fiefs.Count > 0;
               },
               (CouncilMember position, Hero hero) =>
               {
                   return new(true, null);
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=KfZ29QpZ}Antiquarian");
               },
               new Dictionary<TraitObject, float>()
               {
               });

            Castellan.Initialize(
               DefaultSkills.Steward,
               BKSkills.Instance.Lordship,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.OverseeBaronies.GetCopy()
               },
               new List<CouncilPrivileges>() { },
               (CouncilData data) =>
               {
                   var kingdom = data.Clan.Kingdom;
                   return data.IsRoyal && kingdom != null && kingdom.Culture == Utils.Helpers.GetCulture("vlandia");
               },
               (CouncilMember position, Hero hero) =>
               {
                   return new(true, null);
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=Y3yvvUct}Castellan");
               });

            Constable.Initialize(
               DefaultSkills.Steward,
               BKSkills.Instance.Lordship,
               new List<CouncilTask>()
               {
                    DefaultCouncilTasks.Instance.EnforceLaw.GetCopy()
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
                           return data.IsRoyal && (sovereign.Contract.Government == Titles.GovernmentType.Feudal || 
                           sovereign.Contract.Government == Titles.GovernmentType.Imperial);
                       }
                   }
                   
                   return false;
               },
               (CouncilMember position, Hero hero) =>
               {
                   return new(true, null);
               },
               (CouncilMember member) =>
               {
                   return new TextObject("{=65dCSoEB}Constable");
               });
        }
    }
}
