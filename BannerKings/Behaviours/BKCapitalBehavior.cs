using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKCapitalBehavior : CampaignBehaviorBase
    {
        private Dictionary<Kingdom, Town> capitals = new Dictionary<Kingdom, Town>();

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-capitals", ref capitals);

            if (dataStore.IsLoading && capitals == null)
            {
                capitals = new Dictionary<Kingdom, Town>();
            }
        }

        public void ChangeCapital(Kingdom kingdom, Town town)
        {
            if (town.MapFaction == kingdom)
            {
                if (!capitals.ContainsKey(kingdom))
                {
                    capitals.Add(kingdom, town);
                }
                else
                {
                    capitals[kingdom] = town;
                }

                if (kingdom == Clan.PlayerClan.Kingdom)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=5gcHVbpa}{CAPITAL} is now the capital of {KINGDOM}")
                        .SetTextVariable("CAPITAL", town.Name)
                        .SetTextVariable("KINGDOM", kingdom.Name),
                        0,
                        null,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public Town GetCapital(Kingdom kingdom)
        {
            if (kingdom == null)
            {
                return null;
            }

            if (capitals.ContainsKey(kingdom))
            {
                return capitals[kingdom];
            }

            return null;
        }

        private void OnDailyTick()
        {
            foreach (var kingdom in Kingdom.All)
            {
                if (kingdom.Fiefs.Count > 0)
                {
                    if (!capitals.ContainsKey(kingdom))
                    {
                        var town = GetIdealCapital(kingdom);
                        if (town != null)
                        {
                            ChangeCapital(kingdom, town);
                        }
                    }
                    else if (capitals[kingdom].MapFaction != kingdom)
                    {
                        ChangeCapital(kingdom, GetIdealCapital(kingdom));
                    }
                } 
            }
        }

        public Town GetIdealCapital(Kingdom kingdom)
        {
            Town result = null;
            List<ValueTuple<Town, float>> candidates = new List<(Town, float)>();

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            foreach (var town in kingdom.Fiefs)
            {
                float score = 10f;
                if (!town.IsTown)
                {
                    continue;
                }

                if (town.OwnerClan == kingdom.RulingClan)
                {
                    score += 100f;
                }
                else
                {
                    continue;
                }

                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(town.Settlement);
                if (sovereign != null && title.sovereign != sovereign)
                {
                    score -= 150f;
                }

                if (town.StringId == GetCapitalId(kingdom))
                {
                    return town;
                }

                candidates.Add(new(town, score));
            }
            result = MBRandom.ChooseWeighted(candidates);
            if (result == null)
            {
                result = kingdom.Fiefs.GetRandomElement();
            }

            return result;
        }

        public string GetCapitalId(Kingdom kingdom)
        {
            if (kingdom.StringId == "battania")
            {
                return "town_B2";
            }

            if (kingdom.StringId == "aserai")
            {
                return "town_A6";
            }

            if (kingdom.StringId == "vlandia")
            {
                return "town_V3";
            }

            if (kingdom.StringId == "sturgia")
            {
                return "town_S2";
            }

            if (kingdom.StringId == "khuzait")
            {
                return "town_K5";
            }

            if (kingdom.StringId == "empire")
            {
                return "town_EN2";
            }

            if (kingdom.StringId == "empire_w")
            {
                return "town_EW2";
            }

            if (kingdom.StringId == "empire_s")
            {
                return "town_ES4";
            }

            return "";
        }
    }
}
