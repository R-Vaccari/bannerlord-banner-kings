using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Helpers
{
    public static class InheritanceHelper
    {

        public static void ApplyInheritance(FeudalTitle title, Hero victim)
        {
            Clan clan = victim.Clan;
            if (title != null)
            {
                List<Hero> candidates = GetCandidates(victim.Clan, victim);
                SuccessionType succession = title.contract.Succession;
                InheritanceType inheritance = title.contract.Inheritance;

                Kingdom kingdom = clan.Kingdom;
                if (kingdom != null)
                {
                    if (succession == SuccessionType.Imperial && clan == kingdom.RulingClan)
                        return;
                }
                
                if (inheritance == InheritanceType.Primogeniture)
                    ApplyPrimogeniture(title, victim, candidates);
                else if (inheritance == InheritanceType.Ultimogeniture)
                    ApplyUltimogeniture(title, victim, candidates);
                else ApplySeniority(title, victim, candidates);
            }
            else
                ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(clan);
        }

        private static List<Hero> GetCandidates(Clan clan, Hero victim)
        {
            List<Hero> candidates = new List<Hero>();
            foreach (Hero x in clan.Heroes)
                if (!x.IsChild && x != victim && x.IsAlive && (x.IsNoble || x.IsMinorFactionHero))
                    candidates.Add(x);
            return candidates;
        }

        private static void ApplySeniority(FeudalTitle title, Hero victim, List<Hero> candidates)
        {
            GenderLaw genderLaw = title.contract.GenderLaw;
            Hero heir;
            candidates.Sort((Hero x, Hero y) => y.Age.CompareTo(x.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale);
                if (heir == null) heir = candidates.FirstOrDefault();
            }
            else
                heir = candidates.FirstOrDefault();

            if (heir != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
                BannerKingsConfig.Instance.TitleManager.InheritAllTitles(victim, heir);
            }
        }

        private static void ApplyPrimogeniture(FeudalTitle title, Hero victim, List<Hero> candidates)
        {
            GenderLaw genderLaw = title.contract.GenderLaw;
            Hero heir;
            candidates.Sort((Hero x, Hero y) => y.Age.CompareTo(x.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale && IsFamily(victim, x));
                if (heir == null) heir = candidates.FirstOrDefault(x => IsFamily(victim, x));
            }
            else
                heir = candidates.FirstOrDefault(x => IsFamily(victim, x));

            if (heir == null && candidates.Count > 0)
                heir = candidates[0];

            if (heir != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
                BannerKingsConfig.Instance.TitleManager.InheritAllTitles(victim, heir);
            }
        }

        private static void ApplyUltimogeniture(FeudalTitle title, Hero victim, List<Hero> candidates)
        {
            GenderLaw genderLaw = title.contract.GenderLaw;
            Hero heir;
            candidates.Sort((Hero x, Hero y) => x.Age.CompareTo(y.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale && IsFamily(victim, x));
                if (heir == null) heir = candidates.FirstOrDefault(x => IsFamily(victim, x));
            }
            else
                heir = candidates.FirstOrDefault(x => IsFamily(victim, x));

            if (heir == null && candidates.Count > 0)
                heir = candidates[0];

            if (heir != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
                BannerKingsConfig.Instance.TitleManager.InheritAllTitles(victim, heir);
            }
        }

        private static bool IsFamily(Hero victim, Hero x) => x.Father == victim || x.Mother == victim || x.Spouse == victim || x.Siblings.Contains(victim);

        public static void ApplyImperialInheritance(Hero victim, Hero heir)
        {
            if (victim.Clan == heir.Clan)
            {
                BannerKingsConfig.Instance.TitleManager.InheritAllTitles(victim, heir);
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
            }      
            else
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitleWithinFaction(victim, victim.Clan.Kingdom);
                if (title.sovereign == null) BannerKingsConfig.Instance.TitleManager.InheritTitle(victim, heir, title);
                List<Hero> candidates = GetCandidates(victim.Clan, victim);
                InheritanceType inheritance = title.contract.Inheritance;
                FeudalTitle remainingTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(victim);
                if (remainingTitle != null)
                    if (inheritance == InheritanceType.Primogeniture)
                            ApplyPrimogeniture(remainingTitle, victim, candidates);
                        else if (inheritance == InheritanceType.Ultimogeniture)
                            ApplyUltimogeniture(remainingTitle, victim, candidates);
                        else ApplySeniority(remainingTitle, victim, candidates);
            }
        }
    }
}
