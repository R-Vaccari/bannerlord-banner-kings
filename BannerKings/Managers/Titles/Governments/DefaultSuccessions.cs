using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Traits;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultSuccessions : DefaultTypeInitializer<DefaultSuccessions, Succession>
    {
        public Succession FeudalElective { get; } = new Succession("FeudalElective");
        public Succession TribalElective { get; } = new Succession("TribalElective");
        public Succession WilundingElective { get; } = new Succession("WilundingElective");
        public Succession BattanianElective { get; } = new Succession("BattanianElective");
        public Succession Hereditary { get; } = new Succession("Hereditary");
        public Succession TheocraticElective { get; } = new Succession("TheocraticElective");
        public Succession Imperial { get; } = new Succession("Imperial");
        public Succession Republic { get; } = new Succession("Republic");
        public Succession Dictatorship { get; } = new Succession("Dictatorship");
        public Succession AseraiElective { get; } = new Succession("AseraiElective");

        public override IEnumerable<Succession> All
        {
            get
            {
                yield return FeudalElective;
                yield return TribalElective;
                yield return WilundingElective;
                yield return BattanianElective;
                yield return Hereditary;
                yield return TheocraticElective;
                yield return Imperial;
                yield return Republic;
                yield return Dictatorship;
                yield return AseraiElective;
            }
        }

        public Succession GetKingdomIdealSuccession(Kingdom kingdom, Government government)
        {
            string id = kingdom.StringId;

            if (id == "vlandia")
            {
                return WilundingElective;
            }

            if (id == "battania")
            {
                return BattanianElective;
            }

            if (id == "empire_w")
            {
                return Dictatorship;
            }

            if (id == "aserai")
            {
                return AseraiElective;
            }

            if (government.Equals(DefaultGovernments.Instance.Feudal))
            {
                return FeudalElective;
            }
            else if (government.Equals(DefaultGovernments.Instance.Republic))
            {
                return Republic;
            }
            else if (government.Equals(DefaultGovernments.Instance.Imperial))
            {
                return Imperial;
            }
            else return TribalElective;
        }

        public override void Initialize()
        {
            AseraiElective.Initialize(new TextObject("{=lCsViV2X}Nahawasi Elective"),
               new TextObject("{=1RfENhgT}The Nahawasi succession is most interested in the economic prosperity of the realm, for trade is the blood that keeps the Nahasa clans alive. A strong leader, the Aserai say, is one both brave, so they may defeat their enemies, and generous, so they may be loved by those under their protection. Moreover, in keeping with the traditions of Banu Asera, the Nahawasi place value in the religious integrity and scholarly tendencies of their leader, or in other words, that they understand and seek Truth. A fool leader is no worse than a craven one."),
               true,
               -0.7f,
               1f,
               -0.2f,
               new TextObject("{=NHMBtMum}- Heir of current ruler\n- Any Full Peerage clan leader"),
               new TextObject("{=wRR6VJ5G}++ Skills\n++ Military power\n+ Clan tier\n+ Inheritance score (for clan inheritors)"),
               (Hero currentLeader, FeudalTitle title) =>
               {
                   HashSet<Hero> result = new HashSet<Hero>(3);
                   foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Take(1))
                   {
                       result.Add(hero);
                   }

                   foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                   {
                       if (clan.IsUnderMercenaryService) continue;

                       CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
                       if (council.Peerage != null && council.Peerage.IsFullPeerage)
                       {
                           result.Add(clan.Leader);
                       }
                   }

                   if (result.Contains(currentLeader))
                       result.Remove(currentLeader);

                   return result;
               },
               (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
               {
                   ExplainedNumber result = new ExplainedNumber(0f, explanations);

                   result.Add(candidate.Clan.Gold / 200f, new TextObject("{=Rj0SQi0p}Gold"));

                   result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 2f, DefaultSkills.Leadership.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 2f, DefaultSkills.Tactics.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 2f, DefaultSkills.Charm.Name);

                   result.Add(candidate.Clan.Tier * 25f, GameTexts.FindText("str_clan_tier_bonus"));
                   return result;
               });

            Dictatorship.Initialize(new TextObject("{=j6uUHNTQ}Dictatorship"),
               new TextObject("{=ag50C9hT}Imperial successions are completely dictated by the emperor/empress. They will choose from most competent members in their family, as well as other family leaders. Imperial succession values age, family prestigy, military and administration skills. No election takes place."),
               false,
               1f,
               -0.2f,
               -0.7f,
               new TextObject("{=JX2cMIle}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone capable of leading an army (marshal, legates)"),
               new TextObject("{=XDfXpHBd}++ Inheritance score (for clan inheritors)\n+ Title claim\n+ Skills"),
               (Hero currentLeader, FeudalTitle title) =>
               {
                   HashSet<Hero> result = new HashSet<Hero>(3);
                   foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader))
                   {
                       result.Add(hero);
                   }

                   foreach (var claimant in title.Claims)
                   {
                       Hero hero = claimant.Key;
                       if (claimant.Value != ClaimType.Ongoing && claimant.Value != ClaimType.None)
                       {
                           if (hero.IsClanLeader())
                           {
                               result.Add(hero);
                           }
                       }
                   }

                   foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                   {
                       if (clan.IsUnderMercenaryService) continue;

                       if (BannerKingsConfig.Instance.ArmyManagementModel.CanCreateArmy(clan.Leader))
                       {
                           result.Add(clan.Leader);
                       }
                   }

                   if (result.Contains(currentLeader))
                       result.Remove(currentLeader);

                   return result;
               },
               (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
               {
                   ExplainedNumber result = new ExplainedNumber(0f, explanations);

                   if (BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Contains(candidate))
                   {
                       result.Add(BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(currentLeader,
                           candidate,
                           title.Contract,
                           explanations).ResultNumber * 1.5f, new TextObject("{=Gr0NXV4A}{CLAN} inheritor")
                            .SetTextVariable("CLAN", currentLeader.Clan.Name));
                   }

                   if (title.HeroHasValidClaim(candidate))
                   {
                       result.Add(150f, new TextObject("{=ipGDmaBZ}Claimant"));
                   }

                   result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 3f, DefaultSkills.Leadership.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 3f, DefaultSkills.Tactics.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 3f, DefaultSkills.Charm.Name);

                   result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                   return result;
               });

            Imperial.Initialize(new TextObject("{=SW29YLBZ}Imperial"),
               new TextObject("{=Q0OcBy8D}Imperial successions, despite not being elective, can be quite uncertain."),
               false,
               1f,
               -0.2f,
               -0.7f,
               new TextObject("{=v4YDvIhA}- Inheritance candidates of current ruler\n- Title claimants\n- Any clan leader capable of leading an army (marshal, legates)"),
               new TextObject("{=vGNznX3x}++ Inheritance score (for clan inheritors)\n++ Influence cap\n+ Title claim\n+ Skills"),
               (Hero currentLeader, FeudalTitle title) =>
               {
                   HashSet<Hero> result = new HashSet<Hero>(3);
                   foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader))
                   {
                       result.Add(hero);
                   }

                   foreach (var claimant in title.Claims)
                   {
                       Hero hero = claimant.Key;
                       if (claimant.Value != ClaimType.Ongoing && claimant.Value != ClaimType.None)
                       {
                           if (hero.IsClanLeader())
                           {
                               result.Add(hero);
                           }
                       }
                   }

                   foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                   {
                       if (clan.IsUnderMercenaryService) continue;

                       if (BannerKingsConfig.Instance.ArmyManagementModel.CanCreateArmy(clan.Leader))
                       {
                           result.Add(clan.Leader);
                       }
                   }

                   if (result.Contains(currentLeader))
                       result.Remove(currentLeader);

                   return result;
               },
               (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
               {
                   ExplainedNumber result = new ExplainedNumber(0f, explanations);

                   if (BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Contains(candidate))
                   {
                       result.Add(BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(currentLeader,
                           candidate,
                           title.Contract,
                           explanations).ResultNumber * 1.5f, new TextObject("{=Gr0NXV4A}{CLAN} inheritor")
                            .SetTextVariable("CLAN", currentLeader.Clan.Name));
                   }

                   if (title.HeroHasValidClaim(candidate))
                   {
                       result.Add(150f, new TextObject("{=ipGDmaBZ}Claimant"));
                   }

                   result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 3f, DefaultSkills.Leadership.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 3f, DefaultSkills.Tactics.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 3f, DefaultSkills.Charm.Name);

                   result.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(candidate.Clan).ResultNumber / 3f,
                       new TextObject("{=3smpyIa3}Influence cap"));

                   return result;
               });

            Hereditary.Initialize(new TextObject("{=iYzZgP3y}Hereditary Monarchy"),
                new TextObject("{=9EjsMFJx}In hereditary monarchies, the monarch is always the ruling dynasty's leader. No election takes place, and the realm does not change leadership without extraordinary measures."),
                false,
                0.8f,
                -0.2f,
                -0.6f,
                new TextObject("{=rOaXGeO4}- Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of duke or higher (if there aren't at least 3 candidates)"),
                new TextObject("{=HvrxHJuT}+++ Inheritance score (for clan inheritors)\n++ Title claim\n+ Clan tier"),
                (Hero currentLeader, FeudalTitle title) =>
                {
                    HashSet<Hero> result = new HashSet<Hero>(3);
                    foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader))
                    {
                        result.Add(hero);
                    }

                    foreach (var claimant in title.Claims)
                    {
                        Hero hero = claimant.Key;
                        if (claimant.Value != ClaimType.Ongoing && claimant.Value != ClaimType.None)
                        {
                            if (hero.IsClanLeader())
                            {
                                result.Add(hero);
                            }
                        }
                    }

                    if (result.Count < 3)
                    {
                        foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                        {
                            if (clan.IsUnderMercenaryService) continue;

                            FeudalTitle highestTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                            if (highestTitle != null && highestTitle.TitleType <= TitleType.Dukedom)
                            {
                                result.Add(clan.Leader);
                            }
                        }
                    }

                    if (result.Contains(currentLeader))
                        result.Remove(currentLeader);

                    return result;
                },
                (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
                {
                    ExplainedNumber result = new ExplainedNumber(0f, explanations);

                    if (BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Contains(candidate))
                    {
                        result.Add(BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(currentLeader,
                            candidate,
                            title.Contract,
                            explanations).ResultNumber * 2f, new TextObject("{=Gr0NXV4A}{CLAN} inheritor")
                            .SetTextVariable("CLAN", currentLeader.Clan.Name));
                    }

                    if (title.HeroHasValidClaim(candidate))
                    {
                        result.Add(300f, new TextObject("{=ipGDmaBZ}Claimant"));
                    }

                    result.Add(candidate.Clan.Tier * 25f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            Republic.Initialize(new TextObject("{=vFXFxkM9}Republican"),
               new TextObject("{=ATmtkA1S}Republican successions ensure the power is never concentrated. Each year, a new ruler is chosen from the realm's dynasties. The previous ruler is strickly forbidden to participate. Age, family prestige and administration skills are sought after in candidates."),
               true,
               -1f,
               0.4f,
               0.9f,
               new TextObject("{=1GIYpIQD}- Any Full Peerage clan leader"),
               new TextObject("{=yctadrEk}+++ Political traits\n++ Clan tier\n++ Skills\n+ Age"),
               (Hero currentLeader, FeudalTitle title) =>
               {
                   HashSet<Hero> result = new HashSet<Hero>(3);

                   foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                   {
                       if (clan.IsUnderMercenaryService) continue;

                       CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
                       if (council.Peerage != null && council.Peerage.IsFullPeerage)
                       {
                           result.Add(clan.Leader);
                       }
                   }

                   if (result.Contains(currentLeader))
                       result.Remove(currentLeader);

                   return result;
               },
               (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
               {
                   ExplainedNumber result = new ExplainedNumber(0f, explanations);

                   result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 3f, DefaultSkills.Leadership.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 3f, DefaultSkills.Tactics.Name);
                   result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 3f, DefaultSkills.Charm.Name);
                   result.Add(candidate.GetSkillValue(BKSkills.Instance.Lordship) / 3f, DefaultSkills.Charm.Name);
                   result.Add(candidate.Age * 3f, new TextObject("{=uUmEcuV8}Age"));

                   result.Add(candidate.Clan.Tier * 50f, GameTexts.FindText("str_clan_tier_bonus"));

                   //foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                   //{

                   //}
                   
                   return result;
               });

            TheocraticElective.Initialize(new TextObject("{=Znk1920e}Theocratic Elective"),
                new TextObject("{=IEm4hhYy}"),
                true,
                -0.2f,
                -0.5f,
                0.7f,
                new TextObject("{=1GOw5UWI}-Every clan leader of same faith as current ruler"),
                new TextObject("{=PN5CiazU}++ Piety\n++ Virtues\n++ Zealotry\n+ Skills\n+ Clan tier"),
                (Hero currentLeader, FeudalTitle title) =>
                {
                    HashSet<Hero> result = new HashSet<Hero>(3);

                    Religion leaderReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(currentLeader);
                    foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                    {
                        if (clan.IsUnderMercenaryService) continue;

                        Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                        if (religion != null && religion.Equals(leaderReligion))
                        {
                            result.Add(clan.Leader);
                        }
                    }

                    if (result.Contains(currentLeader))
                        result.Remove(currentLeader);

                    return result;
                },
                (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
                {
                    ExplainedNumber result = new ExplainedNumber(0f, explanations);

                    Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(candidate);
                    result.Add(BannerKingsConfig.Instance.ReligionsManager.GetPiety(religion, candidate) / 2f, 
                        new TextObject("{=CS_piety}Piety"));

                    foreach (var tuple in religion.Faith.Traits)
                    {
                        TraitObject trait = tuple.Key;
                        int traitLevel = candidate.GetTraitLevel(trait);
                        if (traitLevel != 0)
                        {
                            result.Add(traitLevel * 100f, trait.Name);
                        }
                    }

                    result.Add(candidate.GetTraitLevel(BKTraits.Instance.Zealous) * 100f, BKTraits.Instance.Zealous.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 3f, DefaultSkills.Leadership.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 3f, DefaultSkills.Tactics.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 3f, DefaultSkills.Charm.Name);

                    result.Add(candidate.Clan.Tier * 25f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            BattanianElective.Initialize(new TextObject("{=BfLrZfDL}Battanian Elective"),
                new TextObject("{=oGEzkwny}The Battanian succession is traditionally reliant upon renown. Caladog fen Gruffendoc is a notorious exception, who managed to leverage his close relationship to the previous High King, Aeril fen Uthelhain, to rally the traditional houses behind him. Yet, the tradition of renowned, strong leaders remains - Battanians care little for 'legal arguments' such as claims, and instead respect force and name. Family members of the ruler are generally preferred, but will quickly be cast aside once a stronger option presents itself."),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=uPrlzI8g}- Inheritance candidates of current ruler\n- Any clan leader of clan tier 5 or higher"),
                new TextObject("{=gVWeE4Gx}+++ Clan tier\n++ Skills\n+ Inheritance score (for clan inheritors)\n+ Military power"),
                (Hero currentLeader, FeudalTitle title) =>
                {
                    HashSet<Hero> result = new HashSet<Hero>(3);
                    foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader))
                    {
                        result.Add(hero);
                    }

                    foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                    {
                        if (clan.IsUnderMercenaryService || clan.Tier < 5) continue;

                        FeudalTitle highestTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                        if (highestTitle != null && highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

                    if (result.Contains(currentLeader))
                        result.Remove(currentLeader);

                    return result;
                },
                (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
                {
                    ExplainedNumber result = new ExplainedNumber(0f, explanations);

                    if (BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Contains(candidate))
                    {
                        result.Add(BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(currentLeader,
                            candidate,
                            title.Contract,
                            explanations).ResultNumber, new TextObject("{=Gr0NXV4A}{CLAN} inheritor")
                            .SetTextVariable("CLAN", currentLeader.Clan.Name));
                    }

                    result.Add(Campaign.Current.Models.DiplomacyModel.GetClanStrength(candidate.Clan) / 600f,
                        new TextObject("{=Vl8tHkz6}Military power"));

                    result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 2f, DefaultSkills.Leadership.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 2f, DefaultSkills.Tactics.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 2f, DefaultSkills.Charm.Name);

                    result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));

                    return result;
                });

            FeudalElective.Initialize(new TextObject("{=HZRnRmF8}Feudal Elective"),
                new TextObject("{=P3EzcNY0}A feudal elective succession is a compromise between a hereditary monarchy and a fully elective monarchy. Votes will be cast by the Peers, however the main candidates are the former ruler's family. However, title claimants will also be considered. If there is a scarcity of candidates available, strong families in the realm will also be considered candidates. If the chosen heir is from the former ruler's family, they will inherit the clan, regardless of Inheritance laws."),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=U8gNnC5X}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
                new TextObject("{=hqpy1FAO}+++ Inheritance score (for clan inheritors)\n++ Title claim\nInfluence cap\n+ Clan tier\n+ Skills"),
                (Hero currentLeader, FeudalTitle title) =>
                {
                    HashSet<Hero> result = new HashSet<Hero>(3);
                    foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader))
                    {
                        result.Add(hero);
                    }

                    foreach (var claimant in title.Claims)
                    {
                        Hero hero = claimant.Key;
                        if (claimant.Value != ClaimType.Ongoing && claimant.Value != ClaimType.None)
                        {
                            if (hero.IsClanLeader())
                            {
                                result.Add(hero);
                            }
                        }
                    }

                    foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                    {
                        if (clan.IsUnderMercenaryService) continue;

                        FeudalTitle highestTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                        if (highestTitle != null && highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

                    if (result.Contains(currentLeader))
                        result.Remove(currentLeader);

                    return result;
                },
                (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
                {
                    ExplainedNumber result = new ExplainedNumber(0f, explanations);

                    if (BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Contains(candidate))
                    {
                        result.Add(BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(currentLeader,
                            candidate,
                            title.Contract,
                            explanations).ResultNumber * 2f, new TextObject("{=Gr0NXV4A}{CLAN} inheritor")
                            .SetTextVariable("CLAN", currentLeader.Clan.Name));
                    }

                    if (title.HeroHasValidClaim(candidate))
                    {
                        result.Add(300f, new TextObject("{=ipGDmaBZ}Claimant"));
                    }

                    result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 3f, DefaultSkills.Leadership.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 3f, DefaultSkills.Tactics.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 3f, DefaultSkills.Charm.Name);

                    result.Add(candidate.Clan.Tier * 25f, GameTexts.FindText("str_clan_tier_bonus"));

                    result.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(candidate.Clan).ResultNumber / 4f,
                       new TextObject("=3smpyIa3}Influence cap"));
                    return result;
                });

            TribalElective.Initialize(new TextObject("{=4MJVs8hh}Tribal Elective"),
                new TextObject("{=MZehamAY}Tribal Elective succession describes a generalized form of succession for the non-feudal and non-imperial cultures of Calradia. Such practices often care little for dynastic inheritance and none for legal trivialities such as claims. Instead, strong competent leaders are respected, specially those with renowned names, for these are the types of rulers that can rally together rivaling tribes that would otherwise be destroying each other."),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=nbZB7pPJ}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
                new TextObject("{=VYtNcGcs}++ Skills\n++ Military power\n+ Clan tier\n+ Inheritance score (for clan inheritors)"),
                (Hero currentLeader, FeudalTitle title) =>
                {
                    HashSet<Hero> result = new HashSet<Hero>(3);
                    foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader))
                    {
                        result.Add(hero);
                    }

                    foreach (var claimant in title.Claims)
                    {
                        Hero hero = claimant.Key;
                        if (claimant.Value != ClaimType.Ongoing && claimant.Value != ClaimType.None)
                        {
                            if (hero.IsClanLeader())
                            {
                                result.Add(hero);
                            }
                        }
                    }

                    foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                    {
                        if (clan.IsUnderMercenaryService) continue;

                        FeudalTitle highestTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                        if (highestTitle != null && highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

                    if (result.Contains(currentLeader))
                        result.Remove(currentLeader);

                    return result;
                },
                (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
                {
                    ExplainedNumber result = new ExplainedNumber(0f, explanations);

                    if (BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Contains(candidate))
                    {
                        result.Add(BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(currentLeader,
                            candidate,
                            title.Contract,
                            explanations).ResultNumber, new TextObject("{=Gr0NXV4A}{CLAN} inheritor")
                            .SetTextVariable("CLAN", currentLeader.Clan.Name));
                    }

                    result.Add(Campaign.Current.Models.DiplomacyModel.GetClanStrength(candidate.Clan) / 400f,
                        new TextObject("{=Vl8tHkz6}Military power"));

                    result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 2f, DefaultSkills.Leadership.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) / 2f, DefaultSkills.Tactics.Name);
                    result.Add(candidate.GetSkillValue(DefaultSkills.Charm) / 2f, DefaultSkills.Charm.Name);

                    result.Add(candidate.Clan.Tier * 25f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            WilundingElective.Initialize(new TextObject("{=60ydQ1dG}Wilunding Elective"),
                new TextObject("{=cECnWTbO}The Wilunding succession is the traditional practice brought the Wilunding peoples, also known as Vlandians, from their ancestral home. This practice gives precedence to the ruling dynasty's inheritors, but in the absence of a good match, legitimizes powerful claimants and lords."),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=uMe4EGQD}- Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of duke or higher (if there aren't at least 3 candidates)"),
                new TextObject("{=7Q6IAw5O}++ Inheritance score (for clan inheritors)\n++ Military power\n+ Clan tier\n+ Title claim\n+ Leadership"),
                (Hero currentLeader, FeudalTitle title) =>
                {
                    HashSet<Hero> result = new HashSet<Hero>(3);
                    foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader))
                    {
                        result.Add(hero);
                    }

                    foreach (var claimant in title.Claims)
                    {
                        Hero hero = claimant.Key;
                        if (claimant.Value != ClaimType.Ongoing && claimant.Value != ClaimType.None)
                        {
                            if (hero.IsClanLeader())
                            {
                                result.Add(hero);
                            }
                        }
                    }

                    if (result.Count < 3)
                    {
                        foreach (Clan clan in currentLeader.Clan.Kingdom.Clans)
                        {
                            if (clan.IsUnderMercenaryService) continue;

                            FeudalTitle highestTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                            if (highestTitle != null && highestTitle.TitleType <= TitleType.Dukedom)
                            {
                                result.Add(clan.Leader);
                            }
                        }
                    }

                    if (result.Contains(currentLeader))
                        result.Remove(currentLeader);

                    return result;
                },
                (Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations) =>
                {
                    ExplainedNumber result = new ExplainedNumber(0f, explanations);
       
                    if (BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(currentLeader).Contains(candidate))
                    {
                        result.Add(BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(currentLeader,
                            candidate,
                            title.Contract,
                            explanations).ResultNumber * 1.5f, new TextObject("{=Gr0NXV4A}{CLAN} inheritor")
                            .SetTextVariable("CLAN", currentLeader.Clan.Name));
                    }

                    if (title.HeroHasValidClaim(candidate))
                    {
                        result.Add(150f, new TextObject("{=ipGDmaBZ}Claimant"));
                    }

                    result.Add(Campaign.Current.Models.DiplomacyModel.GetClanStrength(candidate.Clan) / 400f, 
                        new TextObject("{=Vl8tHkz6}Military power"));

                    result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) / 3f, DefaultSkills.Leadership.Name);

                    result.Add(candidate.Clan.Tier * 25f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });
        }
    }
}
