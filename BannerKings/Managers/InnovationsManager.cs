using BannerKings.Managers.Innovations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Managers
{
    public class InnovationsManager
    {
        private Dictionary<CultureObject, List<Innovation>> Innovations { get; set; }

        public InnovationsManager()
        {
            Innovations = new Dictionary<CultureObject, List<Innovation>>();
            InitializeInnovations();
        }

        private void InitializeInnovations()
        {
            foreach (CultureObject culture in Game.Current.ObjectManager.GetObjectTypeList<CultureObject>())
            {
                if (culture.IsBandit || !culture.IsInitialized) continue;

                Innovations.Add(culture, new List<Innovation>());
                foreach (Innovation innovation in DefaultInnovations.Instance.All)
                {
                    if (innovation.Culture == null || innovation.Culture == culture)
                    {
                        Innovation newInnov = new Innovation(innovation.StringId);
                        newInnov.Initialize(innovation.Name, innovation.Description, innovation.Effects,
                            innovation.RequiredProgress, innovation.Culture, innovation.Requirement);

                        Innovations[culture].Add(newInnov);
                    }
                }
            }
        }
    }
}
