using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Models;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy
{
    public class KingdomDiplomacy
    {
        [SaveableProperty(1)] public Kingdom Kingdom { get; private set; }
        [SaveableProperty(2)] public Religion Religion { get; private set; }
        [SaveableProperty(3)] public List<InterestGroup> Groups { get; private set; }
        [SaveableProperty(5)] public List<Kingdom> TradePacts { get; private set; }
        [SaveableProperty(4)] public Dictionary<Kingdom, CampaignTime> Truces { get; private set; }
        [SaveableProperty(6)] public float Fatigue { get; private set; }
        [SaveableProperty(7)] public float Legitimacy { get; private set; }
        [SaveableProperty(8)] public List<RadicalGroup> RadicalGroups { get; private set; }
        public float LegitimacyChange
        {
            get
            {
                var target = LegitimacyTarget.ResultNumber;
                float change = target * 0.01f;
                float diff = target - Legitimacy;
                if (Legitimacy < target) return MathF.Clamp(change, 0f, diff);
                else if (Legitimacy > target) return MathF.Clamp(-change, diff, 0f);
                return 0f;
            }
        }

        public void AddLegitimacy(float legitimacy)
        {
            Legitimacy = MathF.Min(1f, Legitimacy + legitimacy);
        }

        public BKExplainedNumber LegitimacyTarget => BannerKingsConfig.Instance.LegitimacyModel.CalculateKingdomLegitimacy(this, false);
        public BKExplainedNumber LegitimacyTargetExplained => BannerKingsConfig.Instance.LegitimacyModel.CalculateKingdomLegitimacy(this, true);

        public KingdomDiplomacy(Kingdom kingdom)
        {
            Kingdom = kingdom;
            TradePacts = new List<Kingdom>();
            Truces = new Dictionary<Kingdom, CampaignTime>();
            Groups = new List<InterestGroup>(4);
            RadicalGroups = new List<RadicalGroup>();
        }

        public void PostInitialize()
        {
            if (Religion != null) Religion.PostInitialize();

            foreach (var group in Groups)
            {
                group.PostInitialize();
            }

            if (TradePacts == null)
            {
                TradePacts = new List<Kingdom>();
            }

            if (Truces == null)
            {
                Truces = new Dictionary<Kingdom, CampaignTime>();
            }

            if (RadicalGroups == null)
            {
                RadicalGroups = new List<RadicalGroup>();
            }

            foreach (var group in RadicalGroups)
            {
                group.PostInitialize();
            }
        }

        public void AddFatigue(float fatigue)
        {
            Fatigue += fatigue;
            if (Fatigue > 1f) Fatigue = 1f;
            else if (Fatigue < 0f) Fatigue = 0f;
        }

        public bool HasValidTruce(Kingdom kingdom)
        {
            if (Truces.ContainsKey(kingdom))
            {
                return Truces[kingdom].RemainingHoursFromNow > 0f;
            }

            return false;
        }

        public void AddTruce(Kingdom otherKingdom, float years)
        {
            if (Truces.ContainsKey(otherKingdom))
            {
                Truces.Remove(otherKingdom);
            }

            Truces.Add(otherKingdom, CampaignTime.YearsFromNow(years));
        }

        public void AddPact(Kingdom otherKingdom)
        {
            if (!TradePacts.Contains(otherKingdom))
            {
                TradePacts.Add(otherKingdom);
            }
        }

        public void OnWar(Kingdom otherKingdom)
        {
            DissolveTruce(otherKingdom, new TextObject("{=yrTObrmg}War has broken out!"));
            DissolveTradePact(otherKingdom, new TextObject("{=yrTObrmg}War has broken out!"));
        }

        public List<CasusBelli> GetAvailableCasusBelli(Kingdom targetKingdom = null)
        {
            if (targetKingdom != null)
            {
                return GetTargetKingdomCasusBelli(targetKingdom);
            }

            var list = new List<CasusBelli>();
            foreach (var kingdom in Kingdom.All)
            {
                if (kingdom == Kingdom || !Kingdom.GetStanceWith(kingdom).IsNeutral || HasValidTruce(kingdom)) continue;
                list.AddRange(GetTargetKingdomCasusBelli(kingdom));
            }

            return list;
        }

        private List<CasusBelli> GetTargetKingdomCasusBelli(Kingdom targetKingdom)
        {
            var list = new List<CasusBelli>();

            foreach (CasusBelli cb in DefaultCasusBelli.Instance.All)
            {
                CasusBelli justification = cb.GetCopy();
                if (justification.RequiresFief && !justification.RequiresClaimant)
                {
                    foreach (var fief in targetKingdom.Fiefs)
                    {
                        CasusBelli c = justification.GetCopy();
                        c.SetInstanceData(Kingdom, targetKingdom, fief.Settlement);
                        if (c.IsAdequate(Kingdom, c.Defender, c))
                            list.Add(c);
                    }

                    continue;
                }

                if (justification.RequiresClaimant)
                {
                    foreach (FeudalTitle title in BannerKingsConfig.Instance.TitleManager.AllTitles)
                    {
                        if (title.deJure != null && title.deJure.MapFaction != null && title.deJure.MapFaction == targetKingdom)
                        {
                            foreach (Clan clan in Kingdom.Clans)
                            {
                                if (clan.IsUnderMercenaryService) continue;

                                CasusBelli c = justification.GetCopy();
                                c.SetInstanceData(Kingdom,
                                    targetKingdom,
                                    title,
                                    clan.Leader);

                                if (c.IsAdequate(Kingdom, c.Defender, c))
                                    list.Add(c);
                            }
                        }
                    }
                }

                justification.SetInstanceData(Kingdom, targetKingdom);
                if (justification.IsAdequate(Kingdom, targetKingdom, justification))
                    list.Add(justification);          
            }

            return list;
        }

        public bool HasTradePact(Kingdom kingdom) => TradePacts.Contains(kingdom);

        public void DissolveTradePact(Kingdom kingdom, TextObject reason)
        {
            if (HasTradePact(kingdom))
            {
                TradePacts.Remove(kingdom);
                if (kingdom.MapFaction == Hero.MainHero.MapFaction || Kingdom.MapFaction == Hero.MainHero.MapFaction)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=S4Owp9cp}The trade pact with {KINGDOM} has ended. {REASON}")
                        .SetTextVariable("REASON", reason),
                        0,
                        null,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public void DissolveTruce(Kingdom kingdom, TextObject reason)
        {
            if (HasValidTruce(kingdom))
            {
                Truces.Remove(kingdom);
                if (kingdom.MapFaction == Hero.MainHero.MapFaction || Kingdom.MapFaction == Hero.MainHero.MapFaction)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=95csqL0K}The truce with {KINGDOM} has ended. {REASON}")
                        .SetTextVariable("REASON", reason),
                        0,
                        null,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public void CreateGroup(DiplomacyGroup group, Hero leader)
        {
            group.SetLeader(leader);
            if (!group.IsInterestGroup)
            {
                RadicalGroups.Add((RadicalGroup)group);
            }
            else Groups.Add((InterestGroup)group);

            if (Kingdom == Clan.PlayerClan.Kingdom)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=xqGUkJZH}The group {GROUP} has formed under the leadership of {LEADER}.")
                    .SetTextVariable("GROUP", group.Name)
                    .SetTextVariable("LEADER", group.Leader.Name)
                    .ToString(),
                    Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
            }
        }

        public InterestGroup GetHeroGroup(Hero hero)
        {
            foreach (var group in Groups)
                if (group.Members.Contains(hero))
                    return group;

            return null;
        }

        public RadicalGroup GetHeroRadicalGroup(Hero hero)
        {
            foreach (var group in RadicalGroups)
                if (group.Members.Contains(hero))
                    return group;

            return null;
        }

        public void Update()
        {
            var trucesToDelete = new List<Kingdom>();
            foreach (var truce in Truces) if (truce.Value.RemainingDaysFromNow < 1f)
                    trucesToDelete.Add(truce.Key);

            AddFatigue(-0.005f);
            foreach (var kingdom in trucesToDelete) DissolveTruce(kingdom, new TextObject("{=zW5K0UcD}The agreed time has expired."));


            if (Religion == null)
            {
                Religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Kingdom.RulingClan.Leader);
                //Religion = BannerKingsConfig.Instance.ReligionModel.GetKingdomStateReligion(Kingdom);
            }

            AddLegitimacy(LegitimacyChange);

            foreach (var group in Groups)
            {
                if (group.IsGroupActive) group.Tick();
                else group.SetNewLeader(this);
            }

            foreach (var group in RadicalGroups)
            {
                group.Tick();
                if (!group.IsGroupActive) group.SetNewLeader(this);
                if (!group.IsGroupActive) group.CurrentDemand.Finish();
            } 

            foreach (var group in DefaultInterestGroup.Instance.All)
            {
                bool adequate = BannerKingsConfig.Instance.InterestGroupsModel.IsGroupAdequateForKingdom(this, group);
                if (adequate && !Groups.Any(x => group.StringId == x.StringId))
                {
                    InterestGroup copy = (InterestGroup)group.GetCopy(this);
                    if (copy.Equals(DefaultInterestGroup.Instance.Zealots))
                    {
                        copy.SetName(Religion.Faith.GetZealotsGroupName());
                    }

                    Groups.Add(copy);
                }

                if (!adequate && Groups.Contains(group)) Groups.Remove(group);
            }

            foreach (var group in DefaultRadicalGroups.Instance.All)
            {
                if (!RadicalGroups.Any(x => group.StringId == x.StringId))
                {
                    RadicalGroups.Add((RadicalGroup)group.GetCopy(this));
                }
            }

            foreach (var clan in Kingdom.Clans)
            {
                if (clan.IsUnderMercenaryService) continue;

                foreach (var member in clan.Lords)
                {
                    if (member == Hero.MainHero) continue;
                    EvaluateJoinAGroup(member);
                }
            }

            foreach (var settlement in Kingdom.Settlements)
                if (settlement.Notables != null)
                    foreach (var notable in settlement.Notables)
                        EvaluateJoinAGroup(notable);

            foreach (var group in RadicalGroups)
            {
                foreach (Clan clan in Kingdom.Clans)
                {
                    Hero hero = clan.Leader;
                    if (BannerKingsConfig.Instance.InterestGroupsModel.WillHeroCreateGroup(group, hero, this))
                    {
                        group.SetupRadicalGroup(hero, null);
                    }
                }
            }
        }

        private void EvaluateJoinAGroup(Hero hero)
        {
            InterestGroup currentGroup = GetHeroGroup(hero);
            if (currentGroup == null) // && MBRandom.RandomFloat < 0.05f
            {
                foreach (var group in Groups)
                {
                    float chance = BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroJoinChance(hero, group, this)
                        .ResultNumber;
                    if (MBRandom.RandomFloat < chance)
                    {
                        group.AddMember(hero);
                        group.SetNewLeader(this);
                        break;
                    }
                }
            }

            RadicalGroup radicalGroup = GetHeroRadicalGroup(hero);

        }
    }
}
