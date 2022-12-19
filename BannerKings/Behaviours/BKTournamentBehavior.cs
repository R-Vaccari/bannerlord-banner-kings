using BannerKings.Managers.Skills;
using BannerKings.Utils;

using HarmonyLib;

using Helpers;

using SandBox.Tournaments.MissionLogics;

using System;
using System.Linq.Expressions;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKTournamentBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.TournamentFinished.AddNonSerializedListener(this, OnTournamentFinished);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town,
            ItemObject prize)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (BannerKingsConfig.Instance.PopulationManager == null)
                {
                    return;
                }

                if (participants.Contains(Hero.MainHero.CharacterObject))
                {
                    var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
                    if (education.HasPerk(BKPerks.Instance.GladiatorTourDeCalradia))
                    {
                        var resultTown = SettlementHelper.FindNearestTown((Settlement s) =>
                        {
                            return s.Town.HasTournament;
                        },
                        null
                        ).Town;

                        var game = Campaign.Current.TournamentManager.GetTournamentGame(resultTown);
                        if (resultTown != null)
                        {
                            InformationManager.ShowTextInquiry(new TextInquiryData(
                                new TextObject("{=HwBPn5so}Nearest Tournament").ToString(),
                                new TextObject("{=3abzCPc2}As a known gladiator, you are informed that {TOWN} holds the nearest tournament match. It's prize is {PRIZE}")
                                .SetTextVariable("TOWN", resultTown.Name)
                                .SetTextVariable("PRIZE", game.Prize.Name)
                                .ToString(),
                                true,
                                false,
                                GameTexts.FindText("str_ok").ToString(),
                                string.Empty,
                                null,
                                null
                                ));
                        }
                    }

                    if (winner == Hero.MainHero.CharacterObject && education.HasPerk(BKPerks.Instance.GladiatorPromisingAthlete))
                    {
                        var notable = town.Settlement.Notables.GetRandomElement();
                        ChangeRelationAction.ApplyPlayerRelation(notable, 2);
                    }
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var tournament = data.TournamentData;
                if (town.OwnerClan == Clan.PlayerClan && tournament is { Active: true })
                {
                    float price = town.MarketData.GetPrice(prize);
                    var renown = -10f;
                    if (price <= 10000)
                    {
                        renown += price / 1000f;
                    }
                    else
                    {
                        renown += price / 10000f;
                    }

                    GainRenownAction.Apply(Hero.MainHero, renown, true);
                    InformationManager.DisplayMessage(new InformationMessage(string
                        .Format("Your prize of choice for the tournament at {0} has awarded you {1} renown", renown,
                            town.Name)));
                    tournament.Active = false;
                }
            }, "OnTournamentFinished");
        }

    }

    namespace Patches
    {
        [HarmonyPatch(typeof(TournamentBehavior), "GetExpectedDenarsForBet")]
        internal class GetExpectedDenarsForBetlPatch
        {
            private static void Postfix(ref int __result, int bet)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
                if (education.HasPerk(BKPerks.Instance.GladiatorPromisingAthlete))
                {
                    var baseResult = __result;
                    __result = (int)(baseResult * 1.3f);
                }
            }
        }

        [HarmonyPatch(typeof(TournamentBehavior), "GetMaximumBet")]
        internal class GetMaximumBetlPatch
        {
            private static void Postfix(ref int __result)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(Hero.MainHero);
                if (education.HasPerk(BKPerks.Instance.GladiatorTourDeCalradia))
                {
                    __result *= MathF.Round(1.5);
                }
            }
        }
    }
}