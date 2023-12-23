using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Actions;
using BannerKings.Behaviours;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Models.BKModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using ActionType = BannerKings.Managers.Titles.ActionType;

namespace BannerKings.Managers
{
    public class TitleManager
    {
        public TitleManager(Dictionary<FeudalTitle, Hero> titles, Dictionary<Kingdom, FeudalTitle> kingdoms)
        {
            Titles = titles;
            Kingdoms = kingdoms;
            Knighthood = true; 
        }

        [SaveableProperty(1)] private Dictionary<FeudalTitle, Hero> Titles { get; set; }

        [SaveableProperty(2)] public Dictionary<Kingdom, FeudalTitle> Kingdoms { get; private set; }

        [SaveableProperty(3)] public bool Knighthood { get; set; } = true;

        [SaveableProperty(4)] private Dictionary<Hero, float> Knights { get; set; } = new();

        private Dictionary<Hero, List<FeudalTitle>> DeJuresCache { get; set; }
        private Dictionary<Settlement, FeudalTitle> SettlementCache { get; set; }

        public void RefreshCaches()
        {
            SettlementCache ??= new Dictionary<Settlement, FeudalTitle>();

            if (DeJuresCache == null)
            {
                DeJuresCache = new Dictionary<Hero, List<FeudalTitle>>();
            }
            else
            {
                SettlementCache.Clear();
                DeJuresCache.Clear();
            }

            foreach (var title in Titles.Keys.ToList())
            {
                var hero = title.deJure;
                if (!DeJuresCache.ContainsKey(hero))
                {
                    DeJuresCache.Add(hero, new List<FeudalTitle> {title});
                }
                else
                {
                    DeJuresCache[hero].Add(title);
                }

                if (title.Fief != null)
                {
                    SettlementCache.Add(title.Fief, title);
                }
            }

            Knights ??= new Dictionary<Hero, float>();
        }

        public void PostInitialize()
        {
            RefreshCaches();

            List<Kingdom> eliminatedKingdoms = new List<Kingdom>();
            foreach (var pair in Kingdoms)
            {
                if (pair.Key.IsEliminated)
                {
                    eliminatedKingdoms.Add(pair.Key);
                }
            }

            foreach (var k in eliminatedKingdoms)
                Kingdoms.Remove(k);

            foreach (var title in Titles.Keys.ToList())
            {
                if (title.Contract.DemesneLaws == null || title.Contract.DemesneLaws.Count == 0)
                {
                    title.SetLaws(DefaultDemesneLaws.Instance.GetAdequateLaws(title));
                }

                title.PostInitialize();

                foreach (var law in DefaultDemesneLaws.Instance.GetAdequateLaws(title))
                {
                    if (!title.Contract.DemesneLaws.Any(x => x.LawType == law.LawType))
                    {
                        title.Contract.DemesneLaws.Add(law);
                    }
                }
            }
        }

        public bool IsHeroTitleHolder(Hero hero)
        {
            if (DeJuresCache.ContainsKey(hero))
            {
                return DeJuresCache[hero].Count > 0;
            }

            return false;
        }

        public bool IsKnight(Hero hero) => Knights.ContainsKey(hero);

        public FeudalTitle GetTitle(Settlement settlement)
        {
            try
            {
                if (SettlementCache != null && SettlementCache.ContainsKey(settlement))
                {
                    return SettlementCache[settlement];
                }

                return Titles.Keys.ToList().Find(x => x.Fief == settlement);
            }
            catch (Exception ex)
            {
                const string cause = "Exception in Banner Kings GetTitle method. ";
                var objInfo = settlement != null ? $"Name [{settlement.Name}], Id [{settlement.StringId}], Culture [{settlement.Culture}]." : "Null settlement.";

                throw new BannerKingsException(cause + objInfo, ex);
            }
        }

        public List<FeudalTitle> GetAllTitlesByType(TitleType type)
        {
            return Titles.Keys.ToList().FindAll(x => x.TitleType == type);
        }

        public FeudalTitle GetTitleByName(string name)
        {
            return Titles.FirstOrDefault(x => x.Key.FullName.ToString() == name).Key;
        }

        public FeudalTitle GetTitleByStringId(string stringId)
        {
            return Titles.FirstOrDefault(x => x.Key.StringId == stringId).Key;
        }

        public Government GetSettlementGovernment(Settlement settlement)
        {
            Government type = DefaultGovernments.Instance.Feudal;
            var title = GetTitle(settlement);
            if (title?.Contract != null)
            {
                type = title.Contract.Government;
            }

            return type;
        }

        public void GrantKnighthood(FeudalTitle title, Hero knight, Hero grantor)
        {
            var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, title, grantor);
            action.Influence = -BannerKingsConfig.Instance.TitleModel.GetGrantKnighthoodCost(grantor).ResultNumber;
            action.TakeAction(knight);

            if (grantor == Hero.MainHero)
            {
                GiveGoldAction.ApplyBetweenCharacters(grantor, knight, 5000);
            }

            ClanActions.JoinClan(knight, grantor.Clan);

            if (Clan.PlayerClan.Kingdom != null && grantor.Clan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=AyXDhK2V}The {CLAN} has knighted {KNIGHT}.")
                        .SetTextVariable("CLAN", grantor.Clan.EncyclopediaLinkWithName)
                        .SetTextVariable("KNIGHT", knight.EncyclopediaLinkWithName)
                        .ToString()));
            }

            grantor.AddSkillXp(BKSkills.Instance.Lordship, 300f);
            AddKnightInfluence(knight, 0f);
        }

        public void GrantKnighthood(Estate estate, Hero knight, Hero grantor)
        {
            var action = BannerKingsConfig.Instance.EstatesModel.GetGrant(estate, grantor, knight);
            action.Influence = -BannerKingsConfig.Instance.TitleModel.GetGrantKnighthoodCost(grantor).ResultNumber;
            action.TakeAction(knight);

            ClanActions.JoinClan(knight, grantor.Clan);

            if (Clan.PlayerClan.Kingdom != null && grantor.Clan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=AyXDhK2V}The {CLAN} has knighted {KNIGHT}.")
                        .SetTextVariable("CLAN", grantor.Clan.EncyclopediaLinkWithName)
                        .SetTextVariable("KNIGHT", knight.EncyclopediaLinkWithName)
                        .ToString()));
            }

            grantor.AddSkillXp(BKSkills.Instance.Lordship, 600f);
            AddKnightInfluence(knight, 0f);
        }

        public bool IsHeroKnighted(Hero hero)
        {
            return hero.IsLord && IsHeroTitleHolder(hero);
        }

        public FeudalTitle GetImmediateSuzerain(FeudalTitle target)
        {
            FeudalTitle result = null;
            foreach (var pair in Titles)
            {
                if (pair.Key.Vassals != null && pair.Key.Vassals.Contains(target))
                {
                    result = pair.Key;
                    break;
                }
            }

            return result;
        }

        private void ExecuteOwnershipChange(Hero oldOwner, Hero newOwner, FeudalTitle title, bool deJure)
        {
            if (Titles.ContainsKey(title))
            {
                if (deJure)
                {
                    title.deJure = newOwner;
                    Titles[title] = newOwner;
                    DeJuresCache[oldOwner].Remove(title);
                    if (DeJuresCache.ContainsKey(newOwner))
                    {
                        DeJuresCache[newOwner].Add(title);
                    }
                    else
                    {
                        DeJuresCache.Add(newOwner, new List<FeudalTitle> {title});
                    }


                    if (DeJuresCache[oldOwner].Count == 0)
                    {
                        DeJuresCache.Remove(oldOwner);
                        if (Knights.ContainsKey(oldOwner))
                        {
                            Knights.Remove(oldOwner);
                        }
                    }
                }
                else
                {
                    title.deFacto = newOwner;
                }
            }
        }

        internal void ExecuteAddTitle(FeudalTitle title)
        {
            var keys = Titles.Keys.ToList();
            if (!keys.Contains(title))
            {
                Titles.Add(title, title.deJure);
            }

            RefreshCaches();
        }

        public FeudalTitle CalculateHeroSuzerain(Hero hero)
        {
            var title = GetHighestTitle(hero);
            if (title == null)
            {
                return null;
            }

            var kingdom1 = GetTitleFaction(title);

            if (kingdom1 == null || hero.Clan.Kingdom == null)
            {
                return null;
            }

            var suzerain = GetImmediateSuzerain(title);
            if (suzerain != null)
            {
                var kingdom2 = GetTitleFaction(suzerain);
                if (kingdom2 == kingdom1)
                {
                    return suzerain;
                }

                var factionTitle = GetHighestTitleWithinFaction(hero, kingdom1);
                if (factionTitle != null)
                {
                    var suzerainFaction = GetImmediateSuzerain(factionTitle);
                    return suzerainFaction;
                }

                return GetHighestTitle(kingdom1.Leader);
            }

            return null;
        }

        public List<Hero> CalculateAllVassals(Clan clan)
        {
            var list = new List<Hero>();
            var behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKGentryBehavior>();
            foreach (var title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan))
            {
                if (title.Fief != null && title.Fief.IsVillage)
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(title.Fief);
                    if (data != null && data.EstateData != null)
                    {
                        foreach (var estate in data.EstateData.Estates)
                        {
                            if (estate.Owner != null && estate.Owner.IsLord && estate.Owner.MapFaction == clan.MapFaction)
                            {
                                (bool, Estate) isGentry = behavior.IsGentryClan(estate.Owner.Clan);
                                if (isGentry.Item1 && isGentry.Item2 == estate && estate.Owner.MapFaction == clan.MapFaction)
                                {
                                    list.Add(estate.Owner);
                                }
                            }
                        }
                    }
                }

                if (title.Vassals == null || title.Vassals.Count == 0)
                {
                    continue;
                }

                foreach (var vassal in title.Vassals)
                {
                    var deJure = vassal.deJure;
                    if (deJure != null && deJure != clan.Leader)
                    {
                        if (deJure.Clan == clan)
                        {
                            list.Add(deJure);
                        }
                        else
                        {
                            var suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(deJure);
                            if (suzerain != null && suzerain.deJure == clan.Leader && clan.MapFaction == vassal.deJure.MapFaction)
                            {
                                list.Add(deJure);
                            }
                        }
                    }
                }
            }

            return list;
        }

        public Dictionary<Clan, List<FeudalTitle>> CalculateVassals(Clan suzerainClan, Clan targetClan = null)
        {
            var clans = new Dictionary<Clan, List<FeudalTitle>>();
            var kingdom = suzerainClan?.Kingdom;
            if (kingdom == null || suzerainClan == null)
            {
                return clans;
            }

            var suzerainTitles = GetAllDeJure(suzerainClan);
            if (suzerainTitles.Count == 0)
            {
                return clans;
            }

            foreach (var title in suzerainTitles)
            {
                if (title.Vassals is not {Count: > 0})
                {
                    continue;
                }

                foreach (var vassal in title.Vassals)
                {
                    if (vassal.deJure.Clan == suzerainClan || (targetClan != null && vassal.deJure.Clan != targetClan))
                    {
                        continue;
                    }

                    var vassalSuzerain = CalculateHeroSuzerain(vassal.deJure);
                    if (vassalSuzerain == null)
                    {
                        continue;
                    }

                    var suzerainDeJureClan = vassalSuzerain.deJure.Clan;
                    if (suzerainDeJureClan != suzerainClan)
                    {
                        continue;
                    }

                    var vassalDeJureClan = vassal.deJure.Clan;
                    if (!clans.ContainsKey(vassalDeJureClan))
                    {
                        clans.Add(vassalDeJureClan, new List<FeudalTitle> {vassal});
                    }
                    else
                    {
                        clans[vassalDeJureClan].Add(title);
                    }
                }
            }


            return clans;
        }

        public bool HasSuzerain(FeudalTitle vassal)
        {
            var suzerain = GetImmediateSuzerain(vassal);
            return suzerain != null;
        }

        public void InheritAllTitles(Hero oldOwner, Hero heir)
        {
            if (IsHeroTitleHolder(oldOwner))
            {
                var set = GetAllDeJure(oldOwner);
                var titles = new List<FeudalTitle>(set);
                foreach (var title in titles)
                {
                    if (title.deJure == oldOwner)
                    {
                        ExecuteOwnershipChange(oldOwner, heir, title, true);
                    }

                    if (title.deFacto == oldOwner)
                    {
                        ExecuteOwnershipChange(oldOwner, heir, title, false);
                    }
                }
            }
        }

        public void InheritTitle(Hero oldOwner, Hero heir, FeudalTitle title)
        {
            if (IsHeroTitleHolder(oldOwner))
            {
                if (title.deJure == oldOwner)
                {
                    ExecuteOwnershipChange(oldOwner, heir, title, true);
                }

                if (title.deFacto == oldOwner)
                {
                    ExecuteOwnershipChange(oldOwner, heir, title, false);
                }
            }
        }

        public void AddOngoingClaim(TitleAction action)
        {
            var claimant = action.ActionTaker;

            var lordshipClaimant = BKPerks.Instance.LordshipClaimant;
            if (claimant.GetPerkValue(lordshipClaimant))
            {
                action.Gold -= action.Gold * 0.05f / 100;
                action.Renown -= action.Renown * 0.05f / 100;
            }

            action.Title.AddOngoingClaim(action.ActionTaker);
            GainKingdomInfluenceAction.ApplyForDefault(claimant, -action.Influence);
            claimant.ChangeHeroGold((int) -action.Gold);
            claimant.Clan.Renown -= action.Renown;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.ActionTaker, action.Title.deJure, (int) Math.Min(-5f, new BKTitleModel().GetRelationImpact(action.Title) * -0.1f));

            if (action.Title.deJure == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=xEOemRjF}{CLAIMANT} is building a claim on your title, {TITLE}.")
                        .SetTextVariable("CLAIMANT", claimant.EncyclopediaLinkWithName)
                        .SetTextVariable("TITLE", action.Title.FullName));
            }

            if (action.ActionTaker == Hero.MainHero)
            {

            }
        }

        public void RevokeTitle(TitleAction action)
        {
            var lordshipClaimant = BKPerks.Instance.LordshipClaimant;
            if (action.ActionTaker.GetPerkValue(lordshipClaimant))
            {
                action.Gold -= action.Gold * 0.05f / 100;
                action.Renown -= action.Renown * 0.05f / 100;
                action.Influence -= action.Influence * 0.05f / 100;
            }

            var currentOwner = action.Title.deJure;
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=D50E4DZk}{REVOKER} has revoked the {TITLE}.")
                    .SetTextVariable("REVOKER", action.ActionTaker.EncyclopediaLinkWithName)
                    .SetTextVariable("TITLE", action.Title.FullName)
                    .ToString()));
            var impact = new BKTitleModel().GetRelationImpact(action.Title);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.ActionTaker, currentOwner, impact);

            action.Title.RemoveClaim(action.ActionTaker);
            action.Title.AddClaim(currentOwner, ClaimType.Previous_Owner, true);
            ExecuteOwnershipChange(currentOwner, action.ActionTaker, action.Title, true);

            if (action.Gold > 0)
            {
                action.ActionTaker.ChangeHeroGold((int) -action.Gold);
            }

            if (action.Influence > 0)
            {
                action.ActionTaker.Clan.Influence -= action.Influence;
            }

            if (action.Renown > 0)
            {
                action.ActionTaker.Clan.Renown -= action.Renown;
            }
        }

        public void GrantEstate(EstateAction action)
        {
            var grantor = action.ActionTaker;

            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(grantor, action.ActionTarget, 15);
            GainKingdomInfluenceAction.ApplyForDefault(grantor, -action.Influence);
            grantor.AddSkillXp(BKSkills.Instance.Lordship, 25);

            action.Estate.SetOwner(action.ActionTarget);
        }

        public void GrantTitle(TitleAction action, Hero receiver)
        {
            var grantor = action.ActionTaker;

            ExecuteOwnershipChange(grantor, receiver, action.Title, true);
            var kingdom = grantor.Clan.Kingdom;
            if (receiver.Clan.Kingdom != null && receiver.Clan.Kingdom == kingdom)
            {
                ExecuteOwnershipChange(grantor, receiver, action.Title, false);
            }

            var relationChange = BannerKingsConfig.Instance.TitleModel.GetRelationImpact(action.Title);

            var lordshipPatron = BKPerks.Instance.LordshipPatron;
            if (action.ActionTaker.GetPerkValue(lordshipPatron))
            {
                action.Renown += 15;
                relationChange += (int)(relationChange * 0.1f / 100);
            }

            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(grantor, receiver, -relationChange);
            GainKingdomInfluenceAction.ApplyForDefault(grantor, action.Influence);
            grantor.AddSkillXp(BKSkills.Instance.Lordship, 
                BannerKingsConfig.Instance.TitleModel.GetSkillReward(action.Title.TitleType, action.Type));

            GainRenownAction.Apply(grantor, action.Renown);

            var fief = action.Title.Fief;
            if (receiver.Clan.Leader == receiver && fief != null && (fief.IsTown || fief.IsCastle))
            {
                ChangeOwnerOfSettlementAction.ApplyByGift(fief, receiver);
            }

            if (receiver.CompanionOf != null)
            {
                ClanActions.JoinClan(receiver, grantor.Clan);
            }
        }

        public void UsurpTitle(Hero oldOwner, TitleAction action)
        {
            var usurper = action.ActionTaker;

            var lordshipClaimant = BKPerks.Instance.LordshipClaimant;
            if (action.ActionTaker.GetPerkValue(lordshipClaimant))
            {
                action.Gold -= action.Gold * 0.05f / 100;
                action.Renown -= action.Renown * 0.05f / 100;
                action.Influence -= action.Influence * 0.05f / 100;
            }

            var title = action.Title;
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=c9RCCv20}{USURPER} has usurped the {TITLE}.")
                    .SetTextVariable("USURPER", usurper.EncyclopediaLinkWithName)
                    .SetTextVariable("TITLE", action.Title.FullName)
                    .ToString()));
            if (title.deJure == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=ZAjBRwSY}{USURPER} has usurped your title, {TITLE}.")
                    .SetTextVariable("USURPER", usurper.EncyclopediaLinkWithName)
                    .SetTextVariable("TITLE", action.Title.FullName));
            }

            var impact = BannerKingsConfig.Instance.TitleModel.GetRelationImpact(title);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(usurper, oldOwner, impact);
            var kingdom = oldOwner.Clan.Kingdom;
            if (kingdom != null)
            {
                foreach (var clan in kingdom.Clans)
                {
                    if (clan == oldOwner.Clan || clan == usurper.Clan || clan.IsUnderMercenaryService)
                    {
                        continue;
                    }

                    var random = MBRandom.RandomInt(1, 100);
                    if (random <= 10)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(usurper, oldOwner, (int) (impact * 0.3f));
                    }
                }
            }

            if (action.Gold > 0)
            {
                usurper.ChangeHeroGold((int) -action.Gold);
            }

            if (action.Influence > 0)
            {
                usurper.Clan.Influence -= action.Influence;
            }

            if (action.Renown > 0)
            {
                usurper.Clan.Renown -= action.Renown;
            }

            title.RemoveClaim(usurper);
            title.AddClaim(oldOwner, ClaimType.Previous_Owner, true);
            ExecuteOwnershipChange(oldOwner, usurper, title, true);

            action.ActionTaker.AddSkillXp(BKSkills.Instance.Lordship, 
                BannerKingsConfig.Instance.TitleModel.GetSkillReward(action.Title.TitleType, action.Type));
        }

        public void GiveLordshipOnKingdomJoin(Kingdom newKingdom, Clan clan, bool force = false)
        {
            var clanTitles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan.Leader);
            if (clanTitles.Count > 0)
            {
                return;
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(newKingdom);
            if (sovereign?.Contract == null)
            {
                return;
            }

            if (force)
            {
                goto GIVE;
            }

            if (!sovereign.Contract.ContractAspects.Contains(DefaultContractAspects.Instance.Enfoeffment))
            {
                return;
            }

        GIVE:
            Hero owner = newKingdom.Leader;
            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(newKingdom.Leader);
            if (titles.Count == 0 || titles.FindAll(x => x.TitleType == TitleType.Lordship).Count == 0)
            {
                foreach (Clan kingdomClan in newKingdom.Clans)
                {
                    if (kingdomClan != Clan.PlayerClan)
                    {
                        owner = kingdomClan.Leader;
                        titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(kingdomClan.Leader);
                        if (titles.Count > 0 && titles.FindAll(x => x.TitleType == TitleType.Lordship).Count > 0)
                        {
                            break;
                        }
                    }
                }
            }

            var lordships = titles.FindAll(x => x.TitleType == TitleType.Lordship);
            if (lordships.Count == 0)
            {
                return;
            }

            var lordship = (from l in lordships where l.Fief != null select l into x orderby x.Fief.Village.Hearth select x)
                .FirstOrDefault();
            if (lordship != null)
            {
                var action = BannerKingsConfig.Instance.TitleModel.GetAction(ActionType.Grant, lordship, owner);
                action.Influence = -BannerKingsConfig.Instance.TitleModel.GetGrantKnighthoodCost(owner)
                    .ResultNumber;
                action.TakeAction(clan.Leader);

                if (clan == Clan.PlayerClan)
                {
                    GameTexts.SetVariable("FIEF", lordship.FullName);
                    GameTexts.SetVariable("SOVEREIGN", sovereign.FullName);
                    InformationManager.ShowInquiry(new InquiryData("Enfoeffement Right",
                        new TextObject("{=pmmxMLmr}You have been generously granted the {FIEF} as part of your vassal rights to the {SOVEREIGN}.").ToString(),
                        true, false, GameTexts.FindText("str_done").ToString(), null, null, null));
                }
            }
        }

        public void AddKnightInfluence(Hero hero, float influence)
        {
            if (Knights.ContainsKey(hero))
            {
                Knights[hero] += influence;
            }
            else
            {
                Knights.Add(hero, influence);
            }
        }

        public void RemoveKnights(Hero hero)
        {
            if (Knights.ContainsKey(hero))
            {
                Knights.Remove(hero);
            }
        }

        public float GetKnightInfluence(Hero hero)
        {
            if (Knights.ContainsKey(hero))
            {
                return Knights[hero];
            }

            return 0f;
        }

        public List<FeudalTitle> GetAllDeJure(Hero hero)
        {
            if (DeJuresCache != null)
            {
                DeJuresCache.TryGetValue(hero, out var titleList);
                if (titleList == null)
                {
                    titleList = new List<FeudalTitle>();
                }

                return titleList;
            }


            var list = new List<FeudalTitle>();
            foreach (var title in Titles.Keys.ToList())
            {
                if (title.deJure == hero)
                {
                    list.Add(title);
                }
            }

            return list;
        }

        public List<FeudalTitle> GetAllDeJure(Clan clan)
        {
            var list = new List<FeudalTitle>();
            foreach (var hero in clan.Heroes)
            {
                list.AddRange(GetAllDeJure(hero));
            }

            return list;
        }

        public FeudalTitle GetHighestTitle(Hero hero)
        {
            if (hero != null)
            {
                FeudalTitle highestTitle = null;
                foreach (var title in GetAllDeJure(hero))
                {
                    if (highestTitle == null || title.TitleType < highestTitle.TitleType)
                    {
                        highestTitle = title;
                    }
                }

                return highestTitle;
            }

            return null;
        }

        public FeudalTitle GetHighestTitleWithinFaction(Hero hero, Kingdom faction)
        {
            if (hero != null && faction != null && IsHeroTitleHolder(hero))
            {
                FeudalTitle highestTitle = null;
                foreach (var title in GetAllDeJure(hero))
                {
                    if ((highestTitle == null || title.TitleType < highestTitle.TitleType) && GetTitleFaction(title) == faction)
                    {
                        highestTitle = title;
                    }
                }

                return highestTitle;
            }

            return null;
        }

        public FeudalTitle GetSovereignTitle(Kingdom faction)
        {
            try
            {
                if (faction != null && Kingdoms.ContainsKey(faction))
                {
                    return Kingdoms[faction];
                }

                return null;
            }
            catch (Exception ex)
            {
                var cause = "Exception in Banner Kings GetSovereignTitle method. ";
                string objInfo = null;
                if (faction != null)
                {
                    objInfo = $"Name [{faction.Name}], Id [{faction.StringId}], Culture [{faction.Culture}].";
                }
                else
                {
                    objInfo = "Null faction.";
                }

                throw new BannerKingsException(cause + objInfo, ex);
            }
        }

        public FeudalTitle GetSovereignFromSettlement(Settlement settlement)
        {
            var title = GetTitle(settlement);
            return title?.Sovereign;
        }

        public List<FeudalTitle> GetVassals(TitleType threshold, Hero lord)
        {
            var allTitles = GetAllDeJure(lord);
            var vassals = new List<FeudalTitle>();
            foreach (var title in allTitles)
            {
                if (title.deFacto.MapFaction == lord.MapFaction && (title.deFacto == title.deJure ||
                                                                    title.deJure.MapFaction == lord.MapFaction)
                                                                && (int) title.TitleType <= (int) threshold)
                {
                    vassals.Add(title);
                }
            }

            return vassals;
        }

        public List<FeudalTitle> GetVassals(Hero lord)
        {
            var vassals = new List<FeudalTitle>();
            var highest = GetHighestTitle(lord);
            if (highest != null)
            {
                var threshold = GetHighestTitle(lord).TitleType + 1;
                var allTitles = GetAllDeJure(lord);

                foreach (var title in allTitles)
                {
                    if (title.deFacto.MapFaction == lord.MapFaction && (title.deFacto == title.deJure ||
                                                                        title.deJure.MapFaction == lord.MapFaction)
                                                                    && (int) title.TitleType >= (int) threshold)
                    {
                        vassals.Add(title);
                    }
                }
            }

            return vassals;
        }

        public Kingdom GetTitleFaction(FeudalTitle title)
        {
            Kingdom faction = null;
            var sovereign = title.Sovereign;
            if (sovereign != null)
            {
                faction = Kingdoms.FirstOrDefault(x => x.Value == sovereign).Key;
            }
            else if (Kingdoms.ContainsValue(title))
            {
                faction = Kingdoms.FirstOrDefault(x => x.Value == title).Key;
            }

            return faction;
        }

        public void ApplyOwnerChange(Settlement settlement, Hero newOwner)
        {
            var title = GetTitle(settlement);
            if (title == null)
            {
                return;
            }

            ExecuteOwnershipChange(settlement.Owner, newOwner, title, false);
            if (!settlement.IsVillage && settlement.BoundVillages is {Count: > 0} &&
                title.Vassals is {Count: > 0})
            {
                foreach (var lordship in title.Vassals.Where(y => y.TitleType == TitleType.Lordship))
                {
                    ExecuteOwnershipChange(settlement.Owner, newOwner, title, false);
                }
            }
        }

        public void DeactivateTitle(FeudalTitle title)
        {
            ExecuteOwnershipChange(title.deJure, null, title, true);
            ExecuteOwnershipChange(title.deFacto, null, title, false);
        }

        public void DeactivateDeJure(FeudalTitle title)
        {
            ExecuteOwnershipChange(title.deJure, null, title, true);
        }

        public void ShowContract(Hero lord, string buttonString)
        {
            var kingdom = lord.Clan.Kingdom;
            if (kingdom == null)
            {
                return;
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (sovereign?.Contract == null)
            {
                return;
            }

            var description = BannerKingsConfig.Instance.TitleManager.GetContractText(sovereign);
            InformationManager.ShowInquiry(new InquiryData(
                $"Enfoeffement Contract for {sovereign.FullName}",
                description, true, false, buttonString, "", null, null));
        }

        public FeudalTitle GetDuchy(FeudalTitle title)
        {
            var duchies = Titles.Keys.Where(x => x.TitleType == TitleType.Dukedom && x.Sovereign != null && x.Sovereign == title.Sovereign);

            var suzerain1 = GetImmediateSuzerain(title);
            if (suzerain1 == null)
            {
                return null;
            }

            if (suzerain1.TitleType == TitleType.Dukedom)
            {
                return suzerain1;
            }

            var suzerain2 = GetImmediateSuzerain(suzerain1);
            if (suzerain2 == null)
            {
                return null;
            }

            if (suzerain2.TitleType == TitleType.Dukedom)
            {
                return suzerain2;
            }

            var suzerain3 = GetImmediateSuzerain(suzerain2);
            return suzerain3 is {TitleType: TitleType.Dukedom} 
                ? suzerain3 
                : null;
        }

        public string GetContractText(FeudalTitle title)
        {
            TextObject text = new TextObject("{=AkTU4Qwg}You, {NAME}, formally accept to be henceforth bound to the {TITLE}, fulfill your duties as well as uphold your rights, what can not be undone by means other than abdication of all rights and lands associated with the contract, treachery, or death." +
                "\n\nDuties\n{DUTY1}\n{DUTY2}\n\nRights\n{RIGHT1}\n{RIGHT2}")
                .SetTextVariable("NAME", Hero.MainHero.Name)
                .SetTextVariable("TITLE", title.FullName)
                .SetTextVariable("RIGHT1", GetRightString(title.Contract.Rights.ElementAt(0)))
                .SetTextVariable("RIGHT2", GetRightString(title.Contract.Rights.ElementAt(1)))
                .SetTextVariable("DUTY1", GetDutyString(title.Contract.Duties.ElementAt(0)))
                .SetTextVariable("DUTY2", GetDutyString(title.Contract.Duties.ElementAt(1)));

            return text.ToString();
        }

        private TextObject GetDutyString(KeyValuePair<FeudalDuties, float> pair)
        {
            FeudalDuties duty = pair.Key;
            float factor = pair.Value;
            GameTexts.SetVariable("DUTY_FACTOR", (factor * 100f).ToString("0") + '%');
            var text = duty switch
            {
                FeudalDuties.Taxation => new TextObject("{=wWpgZ1QE}You are due {DUTY_FACTOR} of your fiefs' income to your suzerain."),
                FeudalDuties.Auxilium => new TextObject("{=kk4HK4wg}You are obliged to militarily participate in armies, for {DUTY_FACTOR} of their durations."),
                _ => new TextObject("{=bcVxdc0x}You are obliged to contribute to {DUTY_FACTOR} of your suzerain's ransom.")
            };

            return text;
        }

        private TextObject GetRightString(FeudalRights right)
        {
            return right switch
            {
                FeudalRights.Absolute_Land_Rights => new TextObject("{=pmw8kEKb}You are entitled to ownership of any conquered lands whose title you own."),
                FeudalRights.Enfoeffement_Rights => new TextObject("{=kEvL0vNU}You are entitled to be granted land in case you have none, whenever possible."),
                FeudalRights.Conquest_Rights => new TextObject("{=7TCkYXav}You are entitled to the ownership of any lands you conquered by yourself."),
                _ => new TextObject("{=!}")
            };
        }
    }
}