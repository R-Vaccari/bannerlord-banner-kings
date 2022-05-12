using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKNotableBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGovernorChangedEvent.AddNonSerializedListener(this, new Action<Town, Hero, Hero>(OnGovernorChanged));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnGovernorChanged(Town town, Hero oldGovernor, Hero newGovernor)
        {
            if (oldGovernor == null || !oldGovernor.IsNotable) return;

            Hero owner = town.OwnerClan.Leader;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(owner, oldGovernor, -10, true);
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement.Town == null || settlement.OwnerClan == null) return;

            Hero governor = settlement.Town.Governor;
            if (governor == null || !governor.IsNotable) return;

            if (MBRandom.RandomInt(1, 100) < 5)
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.Town.OwnerClan.Leader, governor, 1, true);
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(GovernorCampaignBehavior), "DailyTickSettlement")]
        class DailyTickSettlementPatch
        {
            static bool Prefix(Settlement settlement)
            {
                if ((settlement.IsTown || settlement.IsCastle) && settlement.Town.Governor != null)
                {
                    Hero governor = settlement.Town.Governor;
                    if (governor.IsNotable || governor.Clan == null)
                    {

                        if (governor.GetPerkValue(DefaultPerks.Charm.MeaningfulFavors) && MBRandom.RandomFloat < 0.02f)
                            foreach (Hero hero in settlement.Notables)
                                if (hero.Power >= 200f)
                                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.OwnerClan.Leader, hero, (int)DefaultPerks.Charm.MeaningfulFavors.SecondaryBonus, true);

                        SkillLevelingManager.OnSettlementGoverned(governor, settlement);
                        return false;
                    }
                }
                
                return true;
            }
        }
    }
}
