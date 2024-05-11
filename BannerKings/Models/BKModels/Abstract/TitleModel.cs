using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class TitleModel
    {
        public abstract TitleAction GetAction(ActionType type, FeudalTitle title, Hero taker, Hero receiver = null);
        public abstract TitleAction GetFoundEmpire(Kingdom faction, Hero founder);
        public abstract TitleAction GetFoundKingdom(Kingdom faction, Hero founder);
        public abstract float GetGoldUsurpCost(FeudalTitle title);
        public abstract int GetRelationImpact(FeudalTitle title);
        public abstract int GetSkillReward(TitleType title, ActionType type);
        public abstract ExplainedNumber GetGrantKnighthoodCost(Hero grantor);
        public abstract ExplainedNumber GetInheritanceHeirScore(Hero currentLeader, Hero candidate, FeudalContract contract, bool explanations = false);
        public abstract ExplainedNumber GetSuccessionHeirScore(Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations = false);

        public List<Hero> GetGrantCandidates(Hero grantor)
        {
            var heroes = new List<Hero>();
            var kingdom = grantor.Clan.Kingdom;
            if (kingdom != null)
                foreach (var clan in kingdom.Clans)
                    if (!clan.IsUnderMercenaryService && clan != grantor.Clan)
                        heroes.Add(clan.Leader);

            return heroes;
        }

        public IEnumerable<KeyValuePair<Hero, ExplainedNumber>> CalculateSuccessionLine(FeudalTitle title, Clan clan, Hero victim = null, int count = 6)
        {
            var leader = victim != null ? victim : clan.Leader;
            var candidates = GetSuccessionCandidates(leader, title);
            var explanations = new Dictionary<Hero, ExplainedNumber>();

            foreach (Hero hero in candidates)
            {
                var explanation = BannerKingsConfig.Instance.TitleModel.GetSuccessionHeirScore(leader,
                    hero, title, true);
                explanations.Add(hero, explanation);
            }

            var result = from x in explanations
                         orderby x.Value.ResultNumber descending
                         select x;

            if (count > 0) return result.Take(count);
            else return result;
        }

        public IEnumerable<KeyValuePair<Hero, ExplainedNumber>> CalculateInheritanceLine(Clan clan, Hero victim = null, int count = 6)
        {
            var leader = victim != null ? victim : clan.Leader;
            var candidates = GetInheritanceCandidates(leader);
            var explanations = new Dictionary<Hero, ExplainedNumber>();
            var clanTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);

            foreach (Hero hero in candidates)
            {
                var contract = clanTitle != null ? clanTitle.Contract : null;
                var explanation = GetInheritanceHeirScore(leader,
                    hero, contract, true);
                if (!explanations.ContainsKey(hero)) explanations.Add(hero, explanation);
            }

            return (from x in explanations
                    orderby x.Value.ResultNumber descending
                    select x).Take(count);
        }

        public HashSet<Hero> GetSuccessionCandidates(Hero currentLeader, FeudalTitle title)
        {
            Succession succession = title.Contract.Succession;
            return succession.GetSuccessionCandidates(currentLeader, title); ;
        }

        public List<Hero> GetInheritanceCandidates(Hero currentLeader)
        {
            var list = new List<Hero>();
            foreach (var x in currentLeader.Clan.Heroes)
            {
                if (!x.IsChild && x != currentLeader && x.IsAlive && x.Occupation == Occupation.Lord)
                {
                    list.Add(x);
                }
            }

            return list;
        }

        public Dictionary<Hero, TextObject> GetClaimants(FeudalTitle title)
        {
            var claimants = new Dictionary<Hero, TextObject>(4);
            var deFacto = title.DeFacto;
            if (deFacto != title.deJure)
            {
                if (title.Fief == null)
                {
                    if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(deFacto))
                        claimants[deFacto] = new TextObject("{=XRMMs6QY}De facto title holder");
                }
                else claimants[deFacto] = new TextObject("{=zp4c76pS}De facto unlanded title holder");
            }

            if (title.Sovereign != null &&
                title.Sovereign.deJure != null &&
                title.Sovereign.deJure != title.deJure &&
                !claimants.ContainsKey(title.Sovereign.deJure))
            {
                claimants[title.Sovereign.deJure] = new TextObject("{=pkZ0J4Fo}De jure sovereign of this title");
            }

            if (title.Vassals != null && title.Vassals.Count > 0)
                foreach (var vassal in title.Vassals)
                    if (vassal.deJure != null && vassal.deJure != title.deJure && !claimants.ContainsKey(vassal.deJure))
                        claimants[vassal.deJure] = new TextObject("{=J07mQQ6k}De jure vassal of this title");

            FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(title);
            if (suzerain != null && suzerain.deJure != null)
                claimants[suzerain.deJure] = new TextObject("{=ymbhLtjf}De jure suzerain of this title");

            return claimants;
        }
    }
}
