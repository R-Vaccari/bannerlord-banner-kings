using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

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
                if (capitals.ContainsKey(kingdom))
                {
                    var town = GetIdealCapital(kingdom);
                    if (town != null)
                    {
                        capitals.Add(kingdom, town);
                    }
                }
                else if (capitals[kingdom].MapFaction != kingdom)
                {
                    capitals[kingdom] = GetIdealCapital(kingdom);
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
                if (town.IsTown)
                {
                    score += 50f;
                }

                if (town.OwnerClan == kingdom.RulingClan)
                {
                    score += 100f;
                }

                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(town.Settlement);
                if (sovereign != null && title.sovereign != sovereign)
                {
                    score -= 150f;
                }

                if (town.StringId == GetCapitalId(kingdom))
                {
                    score += 1000f;
                }

                candidates.Add(new(town, score));
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
                return "";
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
