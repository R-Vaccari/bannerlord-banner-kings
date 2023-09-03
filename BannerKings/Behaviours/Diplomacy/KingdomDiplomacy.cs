using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Institutions.Religions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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
      
        public KingdomDiplomacy(Kingdom kingdom)
        {
            Kingdom = kingdom;
            TradePacts = new List<Kingdom>();
            Truces = new Dictionary<Kingdom, CampaignTime>();
            Groups = new List<InterestGroup>();
        }

        public void PostInitialize()
        {
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
            DissolveTruce(otherKingdom, new TextObject("{=!}War has broken out!"));
            DissolveTradePact(otherKingdom, new TextObject("{=!}War has broken out!"));
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
                list.AddRange(GetTargetKingdomCasusBelli(kingdom));
            }

            return list;
        }

        private List<CasusBelli> GetTargetKingdomCasusBelli(Kingdom targetKingdom)
        {
            var list = new List<CasusBelli>();
            foreach (var fief in targetKingdom.Fiefs)
            {
                CasusBelli liberation = DefaultCasusBelli.Instance.CulturalLiberation.GetCopy();
                liberation.SetInstanceData(Kingdom, targetKingdom, fief.Settlement);
                if (liberation.IsAdequate(Kingdom, liberation.Defender, liberation))
                {
                    list.Add(liberation);
                }
            }

            CasusBelli greatRaid = DefaultCasusBelli.Instance.GreatRaid.GetCopy();
            greatRaid.SetInstanceData(Kingdom, targetKingdom);
            if (greatRaid.IsAdequate(Kingdom, targetKingdom, greatRaid))
            {
                list.Add(greatRaid);
            }

            CasusBelli invasion = DefaultCasusBelli.Instance.Invasion.GetCopy();
            invasion.SetInstanceData(Kingdom, targetKingdom);
            if (invasion.IsAdequate(Kingdom, targetKingdom, invasion))
            {
                list.Add(invasion);
            }

            CasusBelli superiority = DefaultCasusBelli.Instance.ImperialSuperiority.GetCopy();
            superiority.SetInstanceData(Kingdom, targetKingdom);
            if (superiority.IsAdequate(Kingdom, targetKingdom, superiority))
            {
                list.Add(superiority);
            }

            CasusBelli reconquest = DefaultCasusBelli.Instance.ImperialReconquest.GetCopy();
            reconquest.SetInstanceData(Kingdom, targetKingdom);
            if (reconquest.IsAdequate(Kingdom, targetKingdom, reconquest))
            {
                list.Add(reconquest);
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
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}The trade pact with {KINGDOM} has ended. {REASON}")
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
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}The truce with {KINGDOM} has ended. {REASON}")
                        .SetTextVariable("REASON", reason),
                        0,
                        null,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public InterestGroup GetHeroGroup(Hero hero)
        {
            foreach (var group in Groups)
            {
                if (group.Members.Contains(hero))
                {
                    return group;
                }
            }

            return null;
        }

        public void Update()
        {
            var trucesToDelete = new List<Kingdom>();
            foreach (var truce in Truces)
            {
                if (truce.Value.RemainingDaysFromNow < 1f)
                {
                    trucesToDelete.Add(truce.Key);
                }
            }

            AddFatigue(-0.005f);

            foreach (var kingdom in trucesToDelete)
            {
                DissolveTruce(kingdom, new TextObject("{=!}The agreed time has expired."));
            }

            if (Religion == null)
            {
                Religion = BannerKingsConfig.Instance.ReligionModel.GetKingdomStateReligion(Kingdom);
            }

            foreach (var group in Groups)
            {
                group.Tick();
            }

            foreach (var group in DefaultInterestGroup.Instance.All)
            {
                bool adequate = BannerKingsConfig.Instance.InterestGroupsModel.IsGroupAdequateForKingdom(this, group);
                if (adequate && !Groups.Any(x => group.StringId == x.StringId))
                {
                    var copy = group.GetCopy(this);
                    if (copy.Equals(DefaultInterestGroup.Instance.Zealots))
                    {
                        copy.SetName(Religion.Faith.GetZealotsGroupName());
                    }

                    Groups.Add(copy);
                }

                if (!adequate && Groups.Contains(group))
                {
                    Groups.Remove(group);
                }
            }

            foreach (var clan in Kingdom.Clans)
            {
                if (clan.IsUnderMercenaryService)
                {
                    continue;
                }

                foreach (var member in clan.Lords)
                {
                    if (member == Hero.MainHero) continue;
                    EvaluateJoinAGroup(member);
                }
            }

            foreach (var settlement in Kingdom.Settlements)
            {
                if (settlement.Notables != null)
                {
                    foreach (var notable in settlement.Notables)
                    {
                        EvaluateJoinAGroup(notable);
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
        }
    }
}
