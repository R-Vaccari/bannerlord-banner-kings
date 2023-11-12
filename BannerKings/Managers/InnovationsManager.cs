using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Innovations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class InnovationsManager
    {
        public InnovationsManager()
        {
            Innovations = new Dictionary<CultureObject, InnovationData>();
            InitializeInnovations();
        }

        [SaveableProperty(1)] private Dictionary<CultureObject, InnovationData> Innovations { get; set; }

        private void InitializeInnovations()
        {
            foreach (var culture in Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().Where(culture => !culture.IsBandit && culture.CanHaveSettlement))
            {
                Innovations.Add(culture, new InnovationData(new List<Innovation>(), culture));
                foreach (var innovation in DefaultInnovations.Instance.All)
                {
                    if (innovation.Culture != null && innovation.Culture != culture)
                    {
                        continue;
                    }

                    Innovations[culture].AddInnovation(innovation.GetCopy(culture));
                }
            }
        }

        public void PostInitialize()
        {
            foreach (var data in Innovations.Values)
            {
                foreach (var innovation in DefaultInnovations.Instance.All)
                {
                    if (innovation.Culture != null && data.CulturalHead != null && innovation.Culture != data.CulturalHead.Culture)
                    {
                        continue;
                    }

                    data.AddInnovation(innovation);
                }

                data.PostInitialize();
            }
        }

        public void UpdateInnovations()
        {
            foreach (var data in Innovations.Values)
            {
                data.Update();
            }
        }

        public void AddSettlementResearch(Settlement settlement)
        {
            if (!Innovations.ContainsKey(settlement.Culture))
            {
                return;
            }

            var data = Innovations[settlement.Culture];
            data.AddResearch(BannerKingsConfig.Instance.InnovationsModel.CalculateSettlementResearch(settlement).ResultNumber);
        }

        public InnovationData GetInnovationData(CultureObject culture)
        {
            if (culture == null) return null;

            InnovationData data = null;
            if (Innovations.ContainsKey(culture))
            {
                data = Innovations[culture];
            }

            return data;
        }
    }
}