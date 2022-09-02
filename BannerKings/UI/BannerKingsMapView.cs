using System;
using BannerKings.Managers.Populations;
using BannerKings.UI.CampaignStart;
using BannerKings.UI.Management;
using BannerKings.UI.Management.BannerKings.UI.Panels;
using BannerKings.UI.Panels;
using BannerKings.UI.Titles;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using ReligionVM = BannerKings.UI.Religion.ReligionVM;

namespace BannerKings.UI
{
    public class BannerKingsMapView : MapView
    {
        public string id;

        public BannerKingsMapView(string id)
        {
            this.id = id;
            CreateLayout();
        }

        protected override void CreateLayout()
        {
            base.CreateLayout();
            var tuple = GetVM(id);
            UIManager.Instance.BKScreen.LoadLayer(tuple.Item1, tuple.Item2);
        }

        private (BannerKingsViewModel, string) GetVM(string id)
        {
            PopulationData data = null;
            if (Settlement.CurrentSettlement != null)
            {
                data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            }

            switch (id)
            {
                case "population":
                    return (new PopulationVM(data), "PopulationWindow");
                case "guild":
                    return (new GuildVM(data), "GuildWindow");
                case "vilage_project":
                    return (new VillageProjectVM(data), "VillageProjectWindow");
                case "titles":
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
                    if (title == null)
                    {
                        title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom);
                    }

                    return (new DemesneHierarchyVM(title.sovereign ?? title, Clan.PlayerClan.Kingdom),
                        "TitlesWindow");
                }
                case "religions":
                    return (new ReligionVM(data), "ReligionPopup");
                case "campaignStart":
                    return new ValueTuple<BannerKingsViewModel, string>(new CampaignStartVM(), "CampaignStartPopup");
                default:
                    return (new PopulationVM(data), "PopulationWindow");
            }
        }

        public void Close()
        {
            UIManager.Instance.BKScreen.CloseLayer();
        }

        public void Refresh()
        {
            UIManager.Instance.BKScreen.Refresh();
        }
    }
}