using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Core;
using BannerKings.Managers.Skills;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Innovations
{
    public class InnovationData : BannerKingsData
    {
        [SaveableField(1)]
        private float research;

        [SaveableField(2)]
        private Clan culturalHead;

        [SaveableField(3)]
        private Innovation fascination;

        [SaveableField(4)]
        private List<Innovation> innovations;

        [SaveableField(5)]
        private CultureObject culture;

        public InnovationData(List<Innovation> innovations, CultureObject culture)
        {
            this.innovations = innovations;
            this.culture = culture;
        }

        public void PostInitialize()
        {
            if (fascination != null)
            {
                Innovation fasc = DefaultInnovations.Instance.GetById(fascination);
                fascination.Initialize(fasc.Name, fasc.Description, fasc.Effects, fasc.RequiredProgress, fasc.Culture, fasc.Requirement);
            }

            foreach (Innovation innovation in innovations)
            {
                Innovation innov = DefaultInnovations.Instance.GetById(innovation);
                innovation.Initialize(innov.Name, innov.Description, innov.Effects, innov.RequiredProgress, innov.Culture, innov.Requirement);
            }
        }

        public void SetFascination(Innovation innovation) => fascination = innovation;
        public void AddInnovation(Innovation innov) => innovations.Add(innov);

        public Clan CulturalHead => culturalHead;
        public Innovation Fascination => fascination;
        public MBReadOnlyList<Innovation> Innovations => innovations.GetReadOnlyList();

        public void AddResearch(float points) => research += points;

        internal override void Update(PopulationData data = null)
        {
            if (culturalHead == null)
            {
                List<Clan> clans = new List<Clan>(Clan.All).FindAll(x => !x.IsEliminated && x.Culture == culture && x.Leader != null);
                if (clans.Count > 0)
                {
                    clans.Sort((x, y) => y.Renown.CompareTo(x.Renown));
                    culturalHead = clans[0];
                }
            }

            if (culturalHead == null) return;

            List<Innovation> unfinished = innovations.FindAll(x => !x.Finished);
            if (fascination == null) fascination = unfinished.GetRandomElement();
            for (int i = 0; i < 10; i++)
            {
                Innovation random = unfinished.GetRandomElement();
                float result = research * 0.1f;
                if (random == fascination)
                {
                    float toAdd = 1.1f;
                    if (culturalHead.Leader.GetPerkValue(BKPerks.Instance.ScholarshipWellRead))
                        toAdd += 0.2f;
                    result *= toAdd;
                }
                random.AddProgress(result);
            }

            

            research = 0f;
        }
    }
}
