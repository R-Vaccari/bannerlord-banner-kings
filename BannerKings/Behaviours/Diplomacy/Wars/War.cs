using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Utils.Models;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class War
    {
        public War(IFaction attacker, IFaction defender, CasusBelli casusBelli, Kingdom sovereign = null, RadicalDemand demand = null)
        {
            Attacker = attacker;
            Defender = defender;
            CasusBelli = casusBelli;
            Sovereign = sovereign;
            DefenderAllies = new List<IFaction>();
            Demand = demand;

            RecalculateFronts();
        }

        public void RecalculateFronts()
        {
            Dictionary<Town, int> attackerDic = new Dictionary<Town, int>();
            foreach (var fief in Attacker.Fiefs)
            {
                Settlement settlement = SettlementHelper.FindNearestFortification(x => x.Town != null && x.MapFaction == Defender, fief.Settlement);
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

            DefenderFront = attackerDic.FirstOrDefault(x => x.Value == attackerDic.Values.Max()).Key;

            Dictionary<Town, int> defenderDic = new Dictionary<Town, int>();
            foreach (var fief in Defender.Fiefs)
            {
                Settlement settlement = SettlementHelper.FindNearestFortification(x => 
                {
                    bool nullTown = x.Town != null;
                    bool attacker = x.MapFaction == Attacker;
                    return nullTown && attacker;
                }, fief.Settlement);
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

            AttackerFront = defenderDic.FirstOrDefault(x => x.Value == defenderDic.Values.Max()).Key;
        }

        public void PostInitialize()
        {
            CasusBelli.PostInitialize();
        }

        [SaveableProperty(1)] public IFaction Attacker { get; private set; }
        [SaveableProperty(2)] public IFaction Defender { get; private set; }
        [SaveableProperty(3)] public CasusBelli CasusBelli { get; private set; }
        [SaveableProperty(4)] public Kingdom Sovereign { get; private set; }
        [SaveableProperty(5)] public Town AttackerFront { get; private set; }
        [SaveableProperty(6)] public Town DefenderFront { get; private set; }
        [SaveableProperty(7)] public int DaysAttackerHeldObjective { get; private set; }
        [SaveableProperty(8)] public int DaysDefenderHeldObjective { get; private set; }
        [SaveableProperty(9)] public CampaignTime StartDate { get; private set; }
        [SaveableProperty(10)] public List<IFaction> DefenderAllies { get; private set; }
        [SaveableProperty(11)] public RadicalDemand Demand { get; private set; }

        public void AddAlly(IFaction enemy, IFaction faction, bool defender = true)
        {
            if (defender)
            {
                if (DefenderAllies == null) DefenderAllies = new List<IFaction>();
                DefenderAllies.Add(faction);
                DeclareWarAction.ApplyByDefault(enemy, faction);
                if (faction == Hero.MainHero.MapFaction || Defender == Hero.MainHero.MapFaction)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=1NHS00eA}The {ALLY} has joined the {DEFENDER} in their war effort!")
                        .SetTextVariable("ALLY", faction.Name)
                        .SetTextVariable("DEFENDER", Defender.Name)
                        .ToString(),
                        Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                }
            }
        }

        public bool IsOriginalFront(Town town) => town == AttackerFront || town == DefenderFront;

        public Town GetFront(IFaction faction)
        {
            if (faction == Attacker)
            {
                return AttackerFront;
            }

            return DefenderFront;
        }

        public IFaction GetPlayerEnemyFaction()
        {
            if (Attacker == Hero.MainHero.MapFaction)
            {
                return Defender;
            }

            return Attacker;
        }

        public int GetDaysHeldObjective(IFaction faction)
        {
            if (faction == Attacker)
            {
                return DaysAttackerHeldObjective;
            }

            return DaysDefenderHeldObjective;
        }

        public ExplainedNumber TotalWarScore => BannerKingsConfig.Instance.WarModel.CalculateTotalWarScore(this, false);

        public BKExplainedNumber CalculateWarScore(IFaction faction, bool explanations)
        {
            if (faction == Attacker)
            {
                return BannerKingsConfig.Instance.WarModel.CalculateWarScore(this, Attacker, Defender, false, explanations);
            }

            if (faction == Defender)
            {
                return BannerKingsConfig.Instance.WarModel.CalculateWarScore(this, Attacker, Defender, true, explanations);
            }

            return new BKExplainedNumber();
        }

        public bool IsInternalWar() => Attacker.IsClan && Defender.IsClan && Sovereign != null;
        public bool IsMatchingWar(IFaction faction1, IFaction faction2) => faction1 == Attacker && faction2 == Defender ||
            faction2 == Attacker && faction1 == Defender;

        public void Update()
        {
            if (CasusBelli.IsFulfilled(this))
            {
                DaysAttackerHeldObjective++;
            }
            else
            {
                DaysDefenderHeldObjective++;
            }

            if (StartDate == default(CampaignTime)) StartDate = CampaignTime.Now;

          
            if (Attacker is Kingdom)
            {
                float fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(this, Attacker).ResultNumber;
                KingdomDiplomacy diplo = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(Attacker as Kingdom);
                diplo.AddFatigue(fatigue * 0.02f);
            }

            if (Defender is Kingdom)
            {
                float fatigue = BannerKingsConfig.Instance.WarModel.CalculateFatigue(this, Defender).ResultNumber;
                KingdomDiplomacy diplo = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(Defender as Kingdom);
                diplo.AddFatigue(fatigue * 0.02f);
            }
        }

        public void EndWar()
        {
            if (Demand != null)
            {
                bool success = Attacker.GetStanceWith(Defender).GetDailyTributePaid(Defender) > 0;
                Demand.EndRebellion(Attacker as Kingdom, Defender as Kingdom, success);
            }
        }
    }
}
