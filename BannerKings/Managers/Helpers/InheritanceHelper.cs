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
                List<Hero> candidates = new List<Hero>();
                foreach (Hero x in clan.Heroes)
                    if (!x.IsChild && x != victim && x.IsAlive && (x.IsNoble || x.IsMinorFactionHero))
                        candidates.Add(x);

                InheritanceType inheritance = title.contract.inheritance;
                if (inheritance == InheritanceType.Primogeniture)
                    ApplyPrimogeniture(title, victim, candidates);
                else if (inheritance == InheritanceType.Ultimogeniture)
                    ApplyUltimogeniture(title, victim, candidates);
                else ApplySeniority(title, victim, candidates);
            }
            else
                ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(clan);
        }

        private static void ApplySeniority(FeudalTitle title, Hero victim, List<Hero> candidates)
        {
            GenderLaw genderLaw = title.contract.genderLaw;
            Hero heir;
            List<Hero> orderedList = candidates.OrderBy((Hero hero) => hero.Age).ToList();
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = orderedList.FirstOrDefault(x => !x.IsFemale);
                if (heir == null) heir = orderedList.FirstOrDefault();
            }
            else
                heir = orderedList.FirstOrDefault();

            if (heir != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
                BannerKingsConfig.Instance.TitleManager.InheritTitles(victim, heir);
            }
        }

        private static void ApplyPrimogeniture(FeudalTitle title, Hero victim, List<Hero> candidates)
        {
            GenderLaw genderLaw = title.contract.genderLaw;
            Hero heir;
            candidates.Sort((Hero x, Hero y) => y.Age.CompareTo(x.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale && (x.Father == victim || x.Mother == victim || x.Spouse == victim || x.Siblings.Contains(victim)));
                if (heir == null) heir = candidates.FirstOrDefault(x => x.Father == victim || x.Mother == victim || x.Spouse == victim || x.Siblings.Contains(victim));
            }
            else
                heir = candidates.FirstOrDefault(x => x.Father == victim || x.Mother == victim || x.Spouse == victim || x.Siblings.Contains(victim));

            if (heir == null && candidates.Count > 0)
                heir = candidates[0];

            if (heir != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
                BannerKingsConfig.Instance.TitleManager.InheritTitles(victim, heir);
            }
        }

        private static void ApplyUltimogeniture(FeudalTitle title, Hero victim, List<Hero> candidates)
        {
            GenderLaw genderLaw = title.contract.genderLaw;
            Hero heir;
            candidates.Sort((Hero x, Hero y) => x.Age.CompareTo(y.Age));
            if (genderLaw == GenderLaw.Agnatic)
            {
                heir = candidates.FirstOrDefault(x => !x.IsFemale && (x.Father == victim || x.Mother == victim || x.Spouse == victim || x.Siblings.Contains(victim)));
                if (heir == null) heir = candidates.FirstOrDefault(x => x.Father == victim || x.Mother == victim || x.Spouse == victim || x.Siblings.Contains(victim));
            }
            else
                heir = candidates.FirstOrDefault(x => x.Father == victim || x.Mother == victim || x.Spouse == victim || x.Siblings.Contains(victim));

            if (heir == null && candidates.Count > 0)
                heir = candidates[0];

            if (heir != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
                BannerKingsConfig.Instance.TitleManager.InheritTitles(victim, heir);
            }
        }

        public static void ApplyImperialInheritance(Hero victim, Hero heir)
        {
            ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, heir);
            BannerKingsConfig.Instance.TitleManager.InheritTitles(victim, heir);
        }
    }
}
