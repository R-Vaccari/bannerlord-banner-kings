using BannerKings.Managers.Innovations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class InnovationsManager
    {
        [SaveableProperty(1)]
        private Dictionary<CultureObject, InnovationData> Innovations { get; set; }

        public InnovationsManager()
        {
            Innovations = new Dictionary<CultureObject, InnovationData>();
            InitializeInnovations();
        }

        private void InitializeInnovations()
        {
            foreach (CultureObject culture in Game.Current.ObjectManager.GetObjectTypeList<CultureObject>())
            {
                if (culture.IsBandit || !culture.IsInitialized) continue;

                Innovations.Add(culture, new InnovationData(new List<Innovation>(), culture));
                foreach (Innovation innovation in DefaultInnovations.Instance.All)
                {
                    if (innovation.Culture == null || innovation.Culture == culture)
                    {
                        Innovation newInnov = new Innovation(innovation.StringId);
                        newInnov.Initialize(innovation.Name, innovation.Description, innovation.Effects,
                            innovation.RequiredProgress, innovation.Culture, innovation.Requirement);

                        Innovations[culture].AddInnovation(newInnov);
                    }
                }
            }
        }

        public void PostInitialize()
        {
            foreach (InnovationData data in Innovations.Values)
                data.PostInitialize();
        }

        public void UpdateInnovations()
        {
            foreach (InnovationData data in Innovations.Values)
                data.Update();
        }


        public void AddSettlementResearch(Settlement settlement)
        {
            if (Innovations.ContainsKey(settlement.Culture))
            {
                InnovationData data = Innovations[settlement.Culture];
                data.AddResearch(BannerKingsConfig.Instance.InnovationsModel.CalculateSettlementResearch(settlement).ResultNumber);
            }
            
        }

        public InnovationData GetInnovationData(CultureObject culture)
        {
            InnovationData data = null;
            if (Innovations.ContainsKey(culture)) data = Innovations[culture];
            return data;
        }

        public MBReadOnlyList<Innovation> GetInnovations(CultureObject culture)
        {
            List<Innovation> list = new List<Innovation>();
            if (Innovations.ContainsKey(culture))
                foreach (Innovation innov in Innovations[culture].Innovations)
                    list.Add(innov);

            return list.GetReadOnlyList();
        }
    }
}
