using System.Collections.Generic;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Managers.Helpers
{
    public static class InheritanceHelper
    {
        public static void ApplyInheritanceAllTitles(List<FeudalTitle> titles, Hero victim)
        {
            if (BannerKingsConfig.Instance.TitleManager == null)
            {
                return;
            }

            FeudalTitle highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(victim);
            var mainHeir = GetHeirInternal(victim, highest != null ? highest.contract : null);
            if (mainHeir != null)
            {
                foreach (var t in titles)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(victim, mainHeir, t);
                }

                if (victim.IsClanLeader())
                {
                    ChangeClanLeaderAction.ApplyWithSelectedNewLeader(victim.Clan, mainHeir);
                }
            }

            /*var inheritanceDic = GetClanHeirs(titles, victim);
            foreach (var pair in inheritanceDic)
            {
                var heir = pair.Key;
                

                if (heir != mainHeir && mainHeir != null && pair.Value.Any(x => x.fief != null))
                {
                    var newClan = ClanActions.CreateNewClan(pair.Key, pair.Value.First(x => x.fief != null).fief,
                        pair.Key.StringId + "_split_clan");

                    foreach (var t in inheritanceDic[mainHeir])
                        t.AddClaim(newClan.Leader, ClaimType.Clan_Split, true);

                    foreach (var t in pair.Value)
                    {
                        t.AddClaim(mainHeir, ClaimType.Clan_Split, true);
                        if (t.fief != null && t.DeFacto.Clan == victim.Clan)
                        {
                            ChangeOwnerOfSettlementAction.ApplyByGift(t.fief, newClan.Leader);
                        }
                    }

                    var heroesToJoin = new List<Hero>();
                    foreach (var h in victim.Clan.Heroes)
                    {
                        if (h != mainHeir && (h == heir.Spouse || h.Children.Contains(h))) 
                        {
                            heroesToJoin.Add(h);
                        }
                    }

                    foreach (var hero in heroesToJoin)
                    {
                        ClanActions.JoinClan(hero, newClan);
                    }
                        
                    if (victim.Clan.Kingdom != null)
                    {
                        ChangeKingdomAction.ApplyByJoinToKingdom(newClan, victim.Clan.Kingdom);
                    }

                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=iykxiy3G}The {NEW} has branched off from {ORIGINAL} due to inheritance laws.")
                            .SetTextVariable("NEW", newClan.Name)
                            .SetTextVariable("ORIGINAL", victim.Clan.Name)
                            .ToString()));
                }
            }*/
        }

        public static Dictionary<Hero, List<FeudalTitle>> GetClanHeirs(List<FeudalTitle> titles, Hero leader)
        {
            var inheritanceDic = new Dictionary<Hero, List<FeudalTitle>>();
            foreach (var title in titles)
            {
                var heir = GetHeirInternal(leader, title.contract);
                if (!inheritanceDic.ContainsKey(heir))
                {
                    inheritanceDic.Add(heir, new List<FeudalTitle>() { title });
                }
                else
                {
                    inheritanceDic[heir].Add(title);
                }
            }
            return inheritanceDic;
        }


        private static Hero GetHeirInternal(Hero victim, FeudalContract contract)
        {
            Hero heir = null;
            float score = 0f;
            foreach (Hero hero in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(victim))
            {
                float result = BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(victim, hero, contract).ResultNumber;
                if (result > score)
                {
                    heir = hero;
                    score = result;
                }
            }
            return heir;
        }
    }
}