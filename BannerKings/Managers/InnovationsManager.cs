using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Innovations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
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
            foreach (var culture in Game.Current.ObjectManager.GetObjectTypeList<CultureObject>().Where(culture => !culture.IsBandit && culture.IsInitialized))
            {
                Innovations.Add(culture, new InnovationData(new List<Innovation>(), culture));
                foreach (var innovation in DefaultInnovations.Instance.All)
                {
                    if (innovation.Culture != null && innovation.Culture != culture)
                    {
                        continue;
                    }

                    var newInnovation = new Innovation(innovation.StringId);
                    newInnovation.Initialize(innovation.Name, innovation.Description, innovation.Effects, innovation.RequiredProgress, innovation.Culture, innovation.Requirement);

                    Innovations[culture].AddInnovation(newInnovation);
                }
            }
        }

        public void PostInitialize()
        {
            foreach (var data in Innovations.Values)
            {
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
            InnovationData data = null;
            if (Innovations.ContainsKey(culture))
            {
                data = Innovations[culture];
            }

            return data;
        }

        public MBReadOnlyList<Innovation> GetInnovations(CultureObject culture)
        {
            var list = new List<Innovation>();
            if (!Innovations.ContainsKey(culture))
            {
                return new MBReadOnlyList<Innovation>(list);
            }

            list.AddRange(Innovations[culture].Innovations);

            return new MBReadOnlyList<Innovation>(list);
        }
    }
}