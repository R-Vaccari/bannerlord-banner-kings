using System;
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

        public InnovationData(List<Innovation> innovations, CultureObject culture)
        {
            this.innovations = innovations;
            this.culture = culture;
            Era = DefaultEras.Instance.FirstEra;
        }

        [field: SaveableField(2)] public Clan CulturalHead { get; private set; }
        [field: SaveableField(3)] public Innovation Fascination { get; private set; }
        [field: SaveableField(6)] public Era Era { get; private set; }

        public CultureObject Culture => culture;

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

            if (Era == null)
            {
                Era = DefaultEras.Instance.FirstEra;
            }
            else Era.PostInitialize();
        }

        public List<Innovation> GetEraInnovations(Era era) => innovations.FindAll(x => x.Era.Equals(era));

        public bool IsBuildingUpgradeAvailable(BuildingType building, int level)
        {
            if (building == DefaultBuildingTypes.Wall || building == DefaultBuildingTypes.Fortifications)
            {
                if (level == 2) return HasFinishedInnovation(DefaultInnovations.Instance.Masonry);
                else if (level == 3) return HasFinishedInnovation(DefaultInnovations.Instance.AdvancedMasonry);
            }

            return true;
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
            bool result = false;
            foreach (Innovation i in Innovations)
            {
                if (i.StringId == innovation.StringId && i.Finished)
                {
                    result = true;
                    break;
                }
            }

            return result;
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

        public void AssumeCulturalHead(Clan clan, bool announce = false)
        {
            CulturalHead = clan;
            if (culture == Clan.PlayerClan.Culture && announce)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=uZPepQjz}The {CLAN} has assumed the role of cultural head of the {CULTURE} culture.")
                        .SetTextVariable("CLAN", clan.Name)
                        .SetTextVariable("CULTURE", culture.Name), 0, null, "event:/ui/notification/relation");
            }
        }

        public void ChangeFascination(Innovation fascination, bool announce = false)
        {
            Fascination = fascination;
            if (culture == Clan.PlayerClan.Culture && announce)
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
            if (!innovations.Any(x => x.StringId == innov.StringId)) 
                innovations.Add(innov.GetCopy(culture));
        }

        public void RemoveInnovation(Innovation innov)
        {
            Innovation i = innovations.FirstOrDefault(x => x.StringId == innov.StringId);
            if (i != null && i.Equals(innov)) innovations.Remove(i);
        }

        public bool CanResearch(Innovation innovation)
        {
            bool era = innovation.Era.Equals(Era);
            if (innovation.Requirement != null) return HasFinishedInnovation(innovation.Requirement) && era;
            return era;
        }

        public void SetEra(Era era)
        {
            if (era == null) return;

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}The {CULTURE} culture is now on the {ERA}!")
                .SetTextVariable("CULTURE", culture.Name)
                .SetTextVariable("ERA", era.Name)
                .ToString(),
                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
            Era = era;
            Era.TriggerEra(culture);
        }

        public Era FindNextEra()
        {
            if (Era == null) return DefaultEras.Instance.FirstEra;
            return DefaultEras.Instance.All.FirstOrDefault(x => x.PreviousEra != null && x.PreviousEra.Equals(Era));
        }

        internal override void Update(PopulationData data = null)
        {
            var startInnovations = DefaultInnovations.Instance.GetCultureDefaultInnovations(culture);
            foreach (var innovation in innovations)
            {
                if (!innovation.Finished && startInnovations.Any(x => x.StringId == innovation.StringId))
                {
                    innovation.AddProgress(innovation.RequiredProgress - innovation.CurrentProgress);
                }
            }

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

            float research = BannerKingsConfig.Instance.InnovationsModel.CalculateCultureResearch(Culture).ResultNumber;
            var unfinished = innovations.FindAll(x => !x.Finished && CanResearch(x));
            if (unfinished.Count > 0)
            {
                if (Fascination == null)
                {
                    ChangeFascination(unfinished.GetRandomElement());
                }

                List<ValueTuple<Innovation, float>> candidates = new List<(Innovation, float)>();
                foreach (Innovation i in unfinished)
                {
                    float factor = 1f;
                    if (i == Fascination)
                    {
                        factor += 0.25f;
                        if (CulturalHead.Leader.GetPerkValue(BKPerks.Instance.ScholarshipWellRead))
                        {
                            factor += 0.2f;
                        }
                    }
                    candidates.Add(new(i, factor));
                }

                var random = MBRandom.ChooseWeighted(candidates);
                random.AddProgress(research);
            }
            else
            {
                SetEra(FindNextEra());
            }
        }
    }
}