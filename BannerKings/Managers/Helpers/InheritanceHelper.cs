using System.Collections.Generic;
using System.Linq;
using BannerKings.Actions;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Helpers
{
    public static class InheritanceHelper
    {
        public static void ApplyInheritanceAllTitles(List<FeudalTitle> titles, Hero victim)
        {
            var inheritanceDic = new Dictionary<InheritanceType, List<FeudalTitle>>();
            var noContracts = new List<FeudalTitle>();
            FeudalTitle highest = null;

            foreach (var title in titles)
            {
                if (highest != null)
                {
                    if (title.type < highest.type)
                    {
                        highest = title;
                    }
                }
                else
                {
                    highest = title;
                }

                if (title.contract != null)
                {
                    var inheritance = title.contract.Inheritance;
                    if (!inheritanceDic.ContainsKey(inheritance))
                    {
                        inheritanceDic.Add(inheritance, new List<FeudalTitle> {title});
                    }
                    else
                    {
                        inheritanceDic[inheritance].Add(title);
                    }
                }
                else
                {
                    noContracts.Add(title);
                }
            }

            var candidates = GetCandidates(victim.Clan, victim);
            var mainHeir = GetHeirInternal(highest.contract.Inheritance, highest.contract.GenderLaw, victim, candidates);
            var mainHeirTitles = new List<FeudalTitle>();
            var secondaryHeirs = new Dictionary<Hero, List<FeudalTitle>>();
            foreach (var pair in inheritanceDic)
            {
                var heir = GetHeirInternal(pair.Key, highest.contract.GenderLaw, victim, candidates);
                foreach (var t in pair.Value)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(victim, heir, t);
                    if (heir != mainHeir)
                    {
                        t.AddClaim(mainHeir, ClaimType.Clan_Split, true);
                    }
                }

                if (heir != mainHeir)
                {
                    secondaryHeirs.Add(heir, pair.Value);
                }
            }

            if (mainHeir != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, mainHeir);
            }

            if (secondaryHeirs.Count > 0)
            {
                foreach (var pair in secondaryHeirs)
                {
                    if (pair.Value.Any(x => x.fief is {IsVillage: false} && x.DeFacto.Clan == victim.Clan))
                    {
                        var landed = pair.Value.FindAll(x => x.fief is {IsVillage: false});
                        var newClan = ClanActions.CreateNewClan(pair.Key, landed[0].fief,
                            pair.Key.StringId + "_split_clan");

                        foreach (var t in mainHeirTitles)
                            t.AddClaim(newClan.Leader, ClaimType.Clan_Split, true);

                        foreach (var t in pair.Value)
                            t.AddClaim(mainHeir, ClaimType.Clan_Split, true);

                        foreach (var t in landed)
                            if (t.DeFacto.Clan == victim.Clan)
                            {
                                ChangeOwnerOfSettlementAction.ApplyByGift(t.fief, newClan.Leader);
                            }

                        if (victim.Clan.Kingdom != null)
                        {
                            ChangeKingdomAction.ApplyByJoinToKingdom(newClan, victim.Clan.Kingdom);
                        }

                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=6fgqLH7uo}The {NEW} has branched off from {ORIGINAL} due to inheritance laws.")
                                .SetTextVariable("NEW", newClan.Name)
                                .SetTextVariable("ORIGINAL", victim.Clan.Name)
                                .ToString()));
                    }
                }
            }
        }

        /* public static void ApplyInheritance(FeudalTitle title, Hero victim)
         {
             Clan clan = victim.Clan;
             if (title != null)
             {
                 List<Hero> candidates = GetCandidates(victim.Clan, victim);
                 Hero heir = null;
                 SuccessionType succession = title.contract.Succession;
                 InheritanceType inheritance = title.contract.Inheritance;
    
                 Kingdom kingdom = clan.Kingdom;
                 if (kingdom != null)
                 {
                     if (succession == SuccessionType.Imperial && clan == kingdom.RulingClan)
                         return;
                 }
                 
                 if (inheritance == InheritanceType.Primogeniture)
                     heir = ApplyPrimogeniture(title.contract.GenderLaw, victim, candidates);
                 else if (inheritance == InheritanceType.Ultimogeniture)
                     heir = ApplyUltimogeniture(title, victim, candidates);
                 else heir = ApplySeniority(title, victim, candidates);
    
                 if (heir != null)
                 {
                     ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
                     BannerKingsConfig.Instance.TitleManager.InheritTitle(victim, heir, title);
                     if (Clan.PlayerClan == heir.Clan)
                         InformationManager.AddQuickInformation(new TextObject("{=6zhbUP1hw}{HEIR} has rightfully inherited the {TITLE}")
                             .SetTextVariable("HEIR", heir.Name)
                             .SetTextVariable("TITLE", title.FullName));
    
                     return;
                 }
             }
             
             ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(clan);
         } */

        private static Hero GetHeirInternal(InheritanceType type, GenderLaw genderLaw, Hero victim,
            List<Hero> candidates)
        {
            return type switch
            {
                InheritanceType.Primogeniture => ApplyPrimogeniture(genderLaw, victim, candidates),
                InheritanceType.Ultimogeniture => ApplyUltimogeniture(genderLaw, victim, candidates),
                _ => ApplySeniority(genderLaw, victim, candidates)
            };
        }

        private static List<Hero> GetCandidates(Clan clan, Hero victim)
        {
            var candidates = new List<Hero>();
            foreach (var x in clan.Heroes)
                if (!x.IsChild && x != victim && x.IsAlive && (x.Occupation == Occupation.Lord || x.IsMinorFactionHero))
                {
                    candidates.Add(x);
                }

            return candidates;
        }

        private static Hero ApplySeniority(GenderLaw genderLaw, Hero victim, List<Hero> candidates)
        {
            Hero heir;
            candidates.Sort((Hero x, Hero y) => y.Age.CompareTo(x.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale);
                if (heir == null)
                {
                    heir = candidates.FirstOrDefault();
                }
            }
            else
            {
                heir = candidates.FirstOrDefault();
            }

            return heir;
        }

        private static Hero ApplyPrimogeniture(GenderLaw genderLaw, Hero victim, List<Hero> candidates)
        {
            Hero heir;
            candidates.Sort((Hero x, Hero y) => y.Age.CompareTo(x.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale && IsFamily(victim, x));
                if (heir == null)
                {
                    heir = candidates.FirstOrDefault(x => IsFamily(victim, x));
                }
            }
            else
            {
                heir = candidates.FirstOrDefault(x => IsFamily(victim, x));
            }

            if (heir == null && candidates.Count > 0)
            {
                heir = candidates[0];
            }

            return heir;
        }

        private static Hero ApplyUltimogeniture(GenderLaw genderLaw, Hero victim, List<Hero> candidates)
        {
            Hero heir;
            candidates.Sort((Hero x, Hero y) => x.Age.CompareTo(y.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale && IsFamily(victim, x));
                if (heir == null)
                {
                    heir = candidates.FirstOrDefault(x => IsFamily(victim, x));
                }
            }
            else
            {
                heir = candidates.FirstOrDefault(x => IsFamily(victim, x));
            }

            if (heir == null && candidates.Count > 0)
            {
                heir = candidates[0];
            }

            return heir;
        }

        private static bool IsFamily(Hero victim, Hero x) => x.Father == victim || x.Mother == victim ||
                                                             x.Spouse == victim || x.Siblings.Contains(victim);
    }
}