using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKLordPropertyBehavior : BannerKingsBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party?.LeaderHero == null || !party.IsLordParty)
            {
                return;
            }

            var lord = party.LeaderHero;
            if (lord.Occupation != Occupation.Lord || lord.Clan == null)
            {
                return;
            }

            var kingdom = lord.Clan.Kingdom;
            if (lord == Hero.MainHero || kingdom == null || target.OwnerClan == null ||
                target.OwnerClan.Kingdom != kingdom ||
                FactionManager.GetEnemyKingdoms(kingdom).Any())
            {
                return;
            }

            RunWeekly(() =>
            {
                var caravanCost = BannerKingsConfig.Instance.EconomyModel.GetCaravanPrice(target, lord).ResultNumber;
                if (ShouldHaveCaravan(lord, (int)caravanCost))
                {
                    lord.ChangeHeroGold(-(int)caravanCost);
                    CaravanPartyComponent.CreateCaravanParty(lord, target);
                }

                if (target.IsTown && !target.Town.Workshops.Any(x => x.Owner == lord))
                {
                    var random = target.Town.Workshops.GetRandomElement();
                    if (random != null)
                    {
                        float workshopCost = BannerKingsConfig.Instance.WorkshopModel.GetCostForPlayer(random);
                        if (ShouldHaveWorkshop(lord, (int)workshopCost))
                        {

                            if (random.Owner == Hero.MainHero)
                            {
                                InformationManager.ShowInquiry(new InquiryData(new TextObject("{=HGHxECuY}Workshop Acquisition").ToString(),
                                    new TextObject("{=Q19XEcNq}The {CLAN} proposes to buy your {WORKSHOP} at {TOWN}. They offer you {GOLD}{GOLD_ICON}")
                                    .SetTextVariable("CLAN", lord.Clan.Name)
                                    .SetTextVariable("WORKSHOP", random.WorkshopType.Name)
                                    .SetTextVariable("TOWN", target.Name)
                                    .SetTextVariable("GOLD", (int)workshopCost).ToString(),
                                    true,
                                    true,
                                    GameTexts.FindText("str_accept").ToString(),
                                    GameTexts.FindText("str_reject").ToString(),
                                    () => BuyWorkshop(random, lord, kingdom, workshopCost),
                                    null),
                                    true);
                            }
                            else
                            {
                                BuyWorkshop(random, lord, kingdom, workshopCost);
                            }
                        }
                    }
                }
            },
            GetType().Name,
            false);
        }

        private void BuyWorkshop(Workshop wk, Hero buyer, Kingdom kingdom, float cost)
        {
            if (kingdom == Clan.PlayerClan.Kingdom)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=gQYsK1AT}The {CLAN} now own {WORKSHOP} at {TOWN}.")
                        .SetTextVariable("CLAN", buyer.Clan.Name)
                        .SetTextVariable("WORKSHOP", wk.Name)
                        .SetTextVariable("TOWN", wk.Settlement.Name)
                        .ToString()));
            }

            GiveGoldAction.ApplyBetweenCharacters(buyer, wk.Owner, (int)cost, false);
            CampaignEventDispatcher.Instance.OnWorkshopOwnerChanged(wk, buyer);
            wk.ChangeOwnerOfWorkshop(buyer, wk.WorkshopType, TaleWorlds.CampaignSystem.Campaign.Current.Models.WorkshopModel.InitialCapital);
        }

        private bool ShouldHaveCaravan(Hero hero, int cost)
        {
            CharacterObject master = CharacterObject.All.FirstOrDefault((CharacterObject character) => 
                character.Occupation == Occupation.CaravanGuard && 
                character.IsInfantry && 
                character.Level == 26 && 
                character.Culture == hero.Culture);
            return master != null && hero == hero.Clan.Leader && hero.Clan.Gold >= (int) (cost * 2f) &&
                   hero.OwnedCaravans.Count < (int) (hero.Clan.Tier / 3f);
        }

        private bool ShouldHaveWorkshop(Hero hero, int cost)
        {
            return hero == hero.Clan.Leader && hero.Clan.Gold >= (int) (cost * 2f) &&
                   hero.OwnedWorkshops.Count < 1 + hero.Clan.Tier;
        }
    }
}