using BannerKings.Behaviours.Diplomacy.Wars;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy
{
    public class KingdomDiplomacy
    {
        public Kingdom Kingdom { get; }
        public List<Kingdom> TradePacts { get; private set; }
        public Dictionary<Kingdom, CampaignTime> Truces { get; private set; }

        public KingdomDiplomacy(Kingdom kingdom)
        {
            Kingdom = kingdom;
            TradePacts = new List<Kingdom>();
            Truces = new Dictionary<Kingdom, CampaignTime>();
        }

        public bool HasValidTruce(Kingdom kingdom)
        {
            if (Truces.ContainsKey(kingdom))
            {
                return Truces[kingdom].ElapsedHoursUntilNow < 0f;
            }

            return false;
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

        public void Update()
        {
            var trucesToDelete = new List<Kingdom>();
            foreach (var truce in Truces)
            {
                if (truce.Value.ElapsedHoursUntilNow < 0f)
                {
                    trucesToDelete.Add(truce.Key);
                }
            }

            foreach (var kingdom in trucesToDelete)
            {
                DissolveTruce(kingdom, new TextObject("{=!}The agreed time has expired."));
            }
        }
    }
}
