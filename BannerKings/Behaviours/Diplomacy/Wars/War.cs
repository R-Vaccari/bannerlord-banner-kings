using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class War
    {
        public War(IFaction attacker, IFaction defender, CasusBelli casusBelli, Kingdom sovereign = null)
        {
            Attacker = attacker;
            Defender = defender;
            CasusBelli = casusBelli;
            Sovereign = sovereign;

            SetBorders();
        }

        private void SetBorders()
        {
            Dictionary<Town, int> attackerDic = new Dictionary<Town, int>();
            foreach (var fief in Attacker.Fiefs)
            {
                Settlement settlement = SettlementHelper.FindNearestFortification(x => x.Town != null && x.MapFaction == Defender);
                if (settlement != null)
                {
                    Town town = settlement.Town;
                    if (attackerDic.ContainsKey(town))
                    {
                        attackerDic[town]++;
                    }
                    else
                    {
                        attackerDic.Add(town, 1);
                    }
                }
            }

            DefenderOriginalFront = attackerDic.FirstOrDefault(x => x.Value == attackerDic.Values.Max()).Key;

            Dictionary<Town, int> defenderDic = new Dictionary<Town, int>();
            foreach (var fief in Attacker.Fiefs)
            {
                Settlement settlement = SettlementHelper.FindNearestFortification(x => x.Town != null && x.MapFaction == Attacker);
                if (settlement != null)
                {
                    Town town = settlement.Town;
                    if (defenderDic.ContainsKey(town))
                    {
                        defenderDic[town]++;
                    }
                    else
                    {
                        defenderDic.Add(town, 1);
                    }
                }
            }

            AttackerOriginalFront = defenderDic.FirstOrDefault(x => x.Value == defenderDic.Values.Max()).Key;
        }

        public IFaction Attacker { get; }
        public IFaction Defender { get; }
        public CasusBelli CasusBelli { get; }
        public Kingdom Sovereign { get; }
        public Town AttackerOriginalFront { get; private set; }
        public Town DefenderOriginalFront { get; private set; }

        public bool IsOriginalFront(Town town) => town == AttackerOriginalFront || town == DefenderOriginalFront;

        public ExplainedNumber TotalWarScore => BannerKingsConfig.Instance.WarModel.CalculateTotalWarScore(this, false);

        public ExplainedNumber CalculateWarScore(IFaction faction, bool explanations)
        {
            if (faction == Attacker)
            {
                return BannerKingsConfig.Instance.WarModel.CalculateWarScore(this, Attacker, Defender, false, explanations);
            }

            if (faction == Defender)
            {
                return BannerKingsConfig.Instance.WarModel.CalculateWarScore(this, Attacker, Defender, true, explanations);
            }

            return new ExplainedNumber();
        }

        public bool IsInternalWar() => Attacker.IsClan && Defender.IsClan && Sovereign != null;
        public bool IsMatchingWar(IFaction faction1, IFaction faction2) => faction1 == Attacker && faction2 == Defender ||
            faction2 == Attacker && faction1 == Defender;

        public void EndWar()
        {

        }
    }
}
