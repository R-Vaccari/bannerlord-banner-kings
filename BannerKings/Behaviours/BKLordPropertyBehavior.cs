using HarmonyLib;
using System.Linq;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using SandBox.CampaignBehaviors;

namespace BannerKings.Behaviours
{
    public class BKLordPropertyBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party == null || party.LeaderHero == null || !party.IsLordParty) return;

            Hero lord = party.LeaderHero;
            Kingdom kingdom = lord.Clan.Kingdom;
            if (lord == Hero.MainHero || kingdom == null || target.OwnerClan == null || target.OwnerClan.Kingdom != kingdom ||
                FactionManager.GetEnemyKingdoms(kingdom).Count() > 0) return;

            float caravanCost = BannerKingsConfig.Instance.EconomyModel.GetCaravanPrice(target, lord, false).ResultNumber;
            if (ShouldHaveCaravan(lord, (int)caravanCost))
            {
                lord.ChangeHeroGold(-(int)caravanCost);
                CaravanPartyComponent.CreateCaravanParty(lord, target, false, null, null, 0);
            }

            if (target.IsTown && !target.Town.Workshops.Any(x => x.Owner == lord))
            {
                Workshop random = target.Town.Workshops.GetRandomElement();
                if (random != null)
                {
                    float workshopCost = BannerKingsConfig.Instance.WorkshopModel.GetBuyingCostForPlayer(random);
                    if (ShouldHaveWorkshop(lord, (int)workshopCost))
                    {
                        if (kingdom == Clan.PlayerClan.Kingdom)
                            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}The {CLAN} now own {WORKSHOP} at {TOWN}.")
                                .SetTextVariable("CLAN", lord.Clan.Name)
                                .SetTextVariable("WORKSHOP", random.Name)
                                .SetTextVariable("TOWN", random.Settlement.Name)
                                .ToString()));
                        ChangeOwnerOfWorkshopAction.ApplyByTrade(random, lord, random.WorkshopType, 
                            Campaign.Current.Models.WorkshopModel.GetInitialCapital(1), true,
                            (int)workshopCost, null);
                    }
                }
            }
        }

        private bool ShouldHaveCaravan(Hero hero, int cost) => hero == hero.Clan.Leader && hero.Clan.Gold >= (int)(cost * 2f) && 
            hero.OwnedCaravans.Count < (int)(hero.Clan.Tier / 3f);

        private bool ShouldHaveWorkshop(Hero hero, int cost) => hero == hero.Clan.Leader && hero.Clan.Gold >= (int)(cost * 2f) &&
            hero.OwnedCaravans.Count < (1 + hero.Clan.Tier);
    }

    namespace Patches
    {

        [HarmonyPatch(typeof(CaravansCampaignBehavior))]
        class CaravansCampaignBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("SpawnCaravan", MethodType.Normal)]
            static bool SpawnCaravan(Hero hero, bool initialSpawn = false) => hero.CurrentSettlement != null && hero.CurrentSettlement.IsTown;

        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior))]
        class LordConversationsCampaignBehaviorPatches
        {

            [HarmonyPostfix]
            [HarmonyPatch("SmallCaravanFormingCost", MethodType.Getter)]
            static void SmallCaravanFormingCost(ref int __result) => __result = (int)BannerKingsConfig.Instance.EconomyModel
                .GetCaravanPrice(Settlement.CurrentSettlement, Hero.MainHero, false).ResultNumber;

            [HarmonyPostfix]
            [HarmonyPatch("LargeCaravanFormingCost", MethodType.Getter)]
            static void LargeCaravanFormingCost(ref int __result) => __result = (int)BannerKingsConfig.Instance.EconomyModel
                .GetCaravanPrice(Settlement.CurrentSettlement, Hero.MainHero, true).ResultNumber;

        }
    }
}
