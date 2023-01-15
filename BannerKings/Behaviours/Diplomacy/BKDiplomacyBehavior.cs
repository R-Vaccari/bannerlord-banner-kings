using BannerKings.Behaviours.Diplomacy.Wars;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Diplomacy;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy
{
    public class BKDiplomacyBehavior : BannerKingsBehavior
    {
        private Dictionary<Kingdom, KingdomDiplomacy> kingdomDiplomacies = new Dictionary<Kingdom, KingdomDiplomacy>();

        public KingdomDiplomacy GetKingdomDiplomacy(Kingdom kingdom)
        {
            if (kingdom == null)
            {
                return null;
            }

            if (kingdomDiplomacies.ContainsKey(kingdom))
            {
                return kingdomDiplomacies[kingdom];
            }

            return null;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.WarDeclared.AddNonSerializedListener(this, OnWarDeclared);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnDailyTickClan);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnNewGameCreated);
            CampaignEvents.KingdomCreatedEvent.AddNonSerializedListener(this, OnKingdomCreated);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (kingdomDiplomacies == null)
            {
                kingdomDiplomacies = new Dictionary<Kingdom, KingdomDiplomacy>();
            }
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            InitializeDiplomacies();
        }

        private void InitializeDiplomacies()
        {
            if (kingdomDiplomacies == null)
            {
                kingdomDiplomacies = new Dictionary<Kingdom, KingdomDiplomacy>();
            }

            foreach (var kingdom in Kingdom.All)
            {
                if (!kingdomDiplomacies.ContainsKey(kingdom))
                {
                    kingdomDiplomacies.Add(kingdom, new KingdomDiplomacy(kingdom));
                }
            }
        }

        private void OnKingdomCreated(Kingdom kingdom)
        {
            kingdomDiplomacies.Add(kingdom, new KingdomDiplomacy(kingdom));
        }

        private void OnDailyTickClan(Clan clan)
        {
            if (clan.Kingdom == null || clan == Clan.PlayerClan || clan != clan.Kingdom.RulingClan)
            {
                return;
            }

            RunWeekly(() =>
            {
                KingdomDiplomacy diplomacy = GetKingdomDiplomacy(clan.Kingdom);
                if (diplomacy == null)
                {
                    return;
                }

                DiplomacyModel diplomacyModel = Campaign.Current.Models.DiplomacyModel;
                if (clan.Influence < (float)diplomacyModel.GetInfluenceCostOfProposingWar(clan.Kingdom))
                {
                    return;
                }

                List<CasusBelli> casi = diplomacy.GetAvailableCasusBelli();
                if (casi.Count == 0)
                {
                    return;
                }

                foreach (CasusBelli casus in casi)
                {
                    if (clan.Kingdom.UnresolvedDecisions.Any(x => x.GetType() is DeclareWarDecision || x.GetType() is BKDeclareWarDecision))
                    {
                        break;
                    }

                    BKDeclareWarDecision declareWarDecision = new BKDeclareWarDecision(casus, clan, casus.Defender);
                    float support = new KingdomElection(declareWarDecision).GetLikelihoodForOutcome(0);
                    if (support > 0.4f)
                    {
                        clan.Kingdom.AddDecision(declareWarDecision);
                        break;
                    }
                }
            },
            GetType().Name,
            false);
        }
      
        private void OnWarDeclared(IFaction faction1, IFaction faction2)
        {
            if (faction1.IsKingdomFaction && faction2.IsKingdomFaction)
            {

            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(KingdomDecisionProposalBehavior), "ConsiderWar")]
        internal class ConsiderWarPatch
        {
            private static bool Prefix(Clan clan, Kingdom kingdom, IFaction otherFaction, ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(KingdomDiplomacyVM), "OnDeclareWar")]
        internal class DeclareWarVMPatch
        {
            private static bool Prefix(KingdomDiplomacyVM __instance, KingdomTruceItemVM item)
            {
                IFaction enemy = item.Faction2;
                if (!enemy.IsKingdomFaction)
                {
                    return true;
                }

                Kingdom enemyKingdom = enemy as Kingdom;
                Kingdom kingdom = item.Faction1 as Kingdom;
                KingdomDiplomacy diplomacy = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom);
                if (diplomacy == null)
                {
                    return true;
                }

                if (kingdom.UnresolvedDecisions.Any(x => x is DeclareWarDecision || x is BKDeclareWarDecision))
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=!}War declaration is already being voted upon.").ToString()));
                }
                else
                {
                    var list = new List<InquiryElement>();
                    foreach (var casusBelli in diplomacy.GetAvailableCasusBelli(enemyKingdom))
                    {
                        float support = new KingdomElection(new BKDeclareWarDecision(casusBelli, Clan.PlayerClan, enemy)).GetLikelihoodForOutcome(0);
                        list.Add(new InquiryElement(casusBelli,
                        new TextObject("{=!}{NAME} ({CHANCE}% approval)")
                        .SetTextVariable("NAME", casusBelli.QueryNameText)
                        .SetTextVariable("CHANCE", (support * 100).ToString("0.00")).ToString(),
                        null,
                        true,
                        casusBelli.Description.ToString()));
                    }

                    list.Add(new InquiryElement(null, new TextObject("{=!}No Casus Belli").ToString(), null));

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=!}Casus Belli").ToString(),
                        new TextObject("{=!}Select a justification for war.").ToString(),
                        list,
                        true,
                        1,
                        GameTexts.FindText("str_accept").ToString(),
                        GameTexts.FindText("str_selection_widget_cancel").ToString(),
                        (List<InquiryElement> list) =>
                        {
                            object identifier = list[0].Identifier;
                            if (identifier != null)
                            {
                                CasusBelli casusBelli = (CasusBelli)identifier;
                                var decision = new BKDeclareWarDecision(casusBelli, Clan.PlayerClan, enemy);
                                Clan.PlayerClan.Kingdom.AddDecision(decision, false);
                            }
                            else
                            {
                                DeclareWarDecision declareWarDecision = new DeclareWarDecision(Clan.PlayerClan, enemy);
                                Clan.PlayerClan.Kingdom.AddDecision(declareWarDecision, false);
                            }
                            __instance.RefreshValues();
                        },
                        null));
                }

                return false;
            }
        }
    }
}
