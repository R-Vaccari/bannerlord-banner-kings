using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
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
            }
        }

        public override void Initialize()
        {
            Imperial.Initialize(new TextObject("{=!}Imperial"),
               new TextObject(),
               false,
               -0.4f,
               1f,
               0.2f,
               new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone capable of leading an army (marshal, legates)"),
               new TextObject("{=!}+ Leadership\n+ Tactics\n+ Lordship\n+ Charm\n+ Inheritance score (for clan inheritors)\n+ Title claim"),
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
                       if (highestTitle.TitleType <= TitleType.County)
                       {
                           result.Add(clan.Leader);
                       }
                   }

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
                           explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                   }

                   result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                   return result;
               });

            Hereditary.Initialize(new TextObject("{=!}Hereditary"),
                new TextObject(),
                false,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
                new TextObject("{=!}+++ Inheritance score (for clan inheritors)\n++ Title claim\n+ Clan tier"),
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
                        if (highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

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
                            explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                    }

                    result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            Republic.Initialize(new TextObject("{=!}Republican"),
               new TextObject(),
               true,
               -0.4f,
               1f,
               0.2f,
               new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
               new TextObject("{=!}++ Clan tier\n++ Inheritance score (for clan inheritors)\n++ Title claim"),
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
                       if (highestTitle.TitleType <= TitleType.County)
                       {
                           result.Add(clan.Leader);
                       }
                   }

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
                           explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                   }

                   result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                   return result;
               });

            TheocraticElective.Initialize(new TextObject("{=!}Theocratic Elective"),
                new TextObject(),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
                new TextObject("{=!}++ Clan tier\n++ Inheritance score (for clan inheritors)\n++ Title claim"),
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
                        if (highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

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
                            explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                    }

                    result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            BattanianElective.Initialize(new TextObject("{=!}Battanian Elective"),
                new TextObject(),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
                new TextObject("{=!}++ Clan tier\n++ Inheritance score (for clan inheritors)\n++ Title claim"),
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
                        if (highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

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
                            explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                    }

                    result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            FeudalElective.Initialize(new TextObject("{=!}Feudal Elective"),
                new TextObject(),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
                new TextObject("{=!}++ Inheritance score (for clan inheritors)\n++ Title claim\n+ Clan tier"),
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
                        if (highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

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
                            explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                    }

                    result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            TribalElective.Initialize(new TextObject("{=!}Tribal Elective"),
                new TextObject(),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of count or higher and clan tier 5 or higher"),
                new TextObject("{=!}++ Clan tier\n+ Inheritance score (for clan inheritors)\n"),
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
                        if (highestTitle.TitleType <= TitleType.County)
                        {
                            result.Add(clan.Leader);
                        }
                    }

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
                            explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                    }

                    result.Add(candidate.Clan.Tier * 75f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });

            WilundingElective.Initialize(new TextObject("{=!}Wilunding Elective"),
                new TextObject(),
                true,
                -0.4f,
                1f,
                0.2f,
                new TextObject("{=!}-Inheritance candidates of current ruler\n- Title claimants\n- Anyone with title of duke or higher (if there aren't at least 3 candidates)"),
                new TextObject("{=!}++ Inheritance score (for clan inheritors)\n+ Clan tier\n+ Title claim"),
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
                            if (highestTitle.TitleType <= TitleType.Dukedom)
                            {
                                result.Add(clan.Leader);
                            }
                        }
                    }

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
                            explanations).ResultNumber, new TextObject("{=!}Claim as inheritor of previous ruler"));
                    }

                    if (title.HeroHasValidClaim(candidate))
                    {
                        result.Add(200f, new TextObject("{=ipGDmaBZ}Claimant"));
                    }

                    result.Add(candidate.Clan.Tier * 50f, GameTexts.FindText("str_clan_tier_bonus"));
                    return result;
                });
        }
    }
}
