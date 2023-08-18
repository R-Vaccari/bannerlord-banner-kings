using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Innovations.Eras;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Innovations
{
    public class InnovationData : BannerKingsData
    {
        [SaveableField(5)] private readonly CultureObject culture;

        [SaveableField(4)] private readonly List<Innovation> innovations;

        [SaveableField(1)] private float research;

        public InnovationData(List<Innovation> innovations, CultureObject culture)
        {
            this.innovations = innovations;
            this.culture = culture;
        }

        [field: SaveableField(2)] public Clan CulturalHead { get; private set; }
        [field: SaveableField(3)] public Innovation Fascination { get; private set; }
        public Era Era { get; private set; }

        public MBReadOnlyList<Innovation> Innovations => new MBReadOnlyList<Innovation>(innovations);

        public void PostInitialize()
        {
            if (Fascination != null)
            {
                Fascination.PostInitialize();
            }

            foreach (var innovation in innovations)
            {
                innovation.PostInitialize();
            }
        }

        public List<BuildingType> GetAvailableBuildings()
        {
            List<BuildingType> buildings = new List<BuildingType>(20);
            buildings.AddRange(BKBuildings.AllBuildings);

            if (!HasFinishedInnovation(DefaultInnovations.Instance.Aqueducts))
            {
                buildings.Remove(DefaultBuildingTypes.SettlementAquaducts);
            }

            if (!HasFinishedInnovation(DefaultInnovations.Instance.Forum))
            {
                buildings.Remove(DefaultBuildingTypes.SettlementForum);
            }

            if (!HasFinishedInnovation(DefaultInnovations.Instance.Theater))
            {
                buildings.Remove(BKBuildings.Instance.Theater);
            }

            return buildings;
        }

        public bool HasFinishedInnovation(Innovation innovation)
        {
            return innovations.Contains(innovation) && (from i in innovations where i == innovation select i.Finished).FirstOrDefault();
        }

        public bool CanAssumeCulturalHead(Clan clan)
        {
            var renown = 0f;
            if (CulturalHead != null)
            {
                renown = CulturalHead.Renown;
            }

            return clan.Culture == culture && clan != CulturalHead && clan.Renown > renown;
        }

        public bool CanChangeFascination(Clan clan, Innovation fascination)
        {
            return clan == CulturalHead && !fascination.Finished && fascination != Fascination;
        }

        public void AssumeCulturalHead(Clan clan)
        {
            CulturalHead = clan;
            if (culture == Clan.PlayerClan.Culture)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=uZPepQjz}The {CLAN} has assumed the role of cultural head of the {CULTURE} culture.")
                        .SetTextVariable("CLAN", clan.Name)
                        .SetTextVariable("CULTURE", culture.Name), 0, null, "event:/ui/notification/relation");
            }
        }

        public void ChangeFascination(Innovation fascination)
        {
            Fascination = fascination;
            if (culture == Clan.PlayerClan.Culture)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=Hvt8EySp}The {CULTURE} is now fascinated by the {FASCINATION} innovation.")
                        .SetTextVariable("FASCINATION", fascination.Name)
                        .SetTextVariable("CULTURE", culture.Name), 0, null, "event:/ui/notification/relation");
            }
        }

        public void SetFascination(Innovation innovation)
        {
            Fascination = innovation;
        }

        public void AddInnovation(Innovation innov)
        {
            innovations.Add(innov);
        }

        public void AddResearch(float points)
        {
            research += points;
        }

        internal override void Update(PopulationData data = null)
        {
            if (CulturalHead == null)
            {
                var clans = new List<Clan>(Clan.All).FindAll(x =>
                    !x.IsEliminated && x.Culture == culture && x.Leader != null);
                if (clans.Count > 0)
                {
                    clans.Sort((x, y) => y.Renown.CompareTo(x.Renown));
                    AssumeCulturalHead(clans[0]);
                }
            }

            if (CulturalHead == null)
            {
                return;
            }

            var unfinished = innovations.FindAll(x => !x.Finished);
            if (unfinished.Count > 0)
            {
                if (Fascination == null)
                {
                    ChangeFascination(unfinished.GetRandomElement());
                }

                for (var i = 0; i < 10; i++)
                {
                    var random = unfinished.GetRandomElement();
                    var result = research * 0.1f;
                    if (random == Fascination)
                    {
                        var toAdd = 1.1f;
                        if (CulturalHead.Leader.GetPerkValue(BKPerks.Instance.ScholarshipWellRead))
                        {
                            toAdd += 0.2f;
                        }

                        result *= toAdd;
                    }

                    random.AddProgress(result);
                }
            }

            research = 0f;
        }
    }
}