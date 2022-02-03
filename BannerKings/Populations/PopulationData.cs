using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Populations
{
    public class PopulationData
    {
        [SaveableProperty(1)]
        private List<PopulationClass> classes { get; set; }

        [SaveableProperty(2)]
        private int totalPop { get; set; }

        [SaveableProperty(3)]
        private float assimilation { get; set; }

        [SaveableProperty(4)]
        private float[] satisfactions { get; set; }

        [SaveableProperty(5)]
        private float stability { get; set; }

        public PopulationData(List<PopulationClass> classes, float assimilation)
        {
            this.classes = classes;
            classes.ForEach(popClass => TotalPop += popClass.count);
            this.assimilation = assimilation;
            this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
            this.stability = 0.5f;
        }

        public float Stability
        {
            get => this.stability;
            set
            {
                if (value != stability)
                    stability = value;
            }
        }

        public float Assimilation
        {
            get => assimilation;
            set
            {
                if (value != assimilation)
                    assimilation = value;
            }
        }

        public List<PopulationClass> Classes
        {
            get => classes;
            set
            {
                if (value != classes)
                    classes = value;
            }
        }

        public int TotalPop
        {
            get => totalPop;
            set
            {
                if (value != totalPop)
                    totalPop = value;
            }
        }

        public float[] GetSatisfactions()
        {
            if (satisfactions == null) this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
            return satisfactions;
        }
        public void UpdateSatisfaction(ConsumptionType type, float value)
        {
            if (this.satisfactions == null) this.satisfactions = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
            float current = this.satisfactions[(int)type];
            this.satisfactions[(int)type] = MathF.Clamp(current + value, 0f, 1f);
        }

        public void UpdatePopulation(Settlement settlement, int pops, PopType target)
        {
            if (target == PopType.None)
            {
                if (settlement.Owner == Hero.MainHero)
                    InformationManager.DisplayMessage(new InformationMessage());
                bool divisibleNegative = ((float)pops * -1f) > 20;
                if (pops > 20 || divisibleNegative)
                {
                    int fractions = (int)((float)pops / (divisibleNegative ? -20f : 20f));
                    int reminder = pops % 20;
                    for (int i = 0; i < fractions; i++)
                    {
                        SelectAndUpdatePop(settlement, divisibleNegative ? -20 : 20);
                    }
                    SelectAndUpdatePop(settlement, divisibleNegative ? -reminder : reminder);
                }
                else SelectAndUpdatePop(settlement, pops);
            }
            else UpdatePopType(target, pops);
        }

        private void SelectAndUpdatePop(Settlement settlement, int pops)
        {
            if (pops != 0)
            {
                Dictionary<PopType, float[]> desiredTypes = GetDesiredPopTypes(settlement);
                List<ValueTuple<PopType, float>> typesList = new List<ValueTuple<PopType, float>>();
                classes.ForEach(popClass =>
                {
                    PopType type = popClass.type;
                    if (pops < 0 && popClass.count >= pops)
                    {
                        bool hasExcess = GetCurrentTypeFraction(type) > desiredTypes[type][1];
                        typesList.Add(new ValueTuple<PopType, float>(popClass.type, desiredTypes[type][0] * (hasExcess ? 2f : 1f)));
                    }
                    else if (pops > 0)
                    {
                        bool isLacking = GetCurrentTypeFraction(type) < desiredTypes[type][0];
                        typesList.Add(new ValueTuple<PopType, float>(popClass.type, desiredTypes[type][0] * (isLacking ? 2f : 1f)));
                    }
                });

                PopType targetType = MBRandom.ChooseWeighted(typesList);
                UpdatePopType(targetType, pops);
            }
        }

        public void UpdatePopType(PopType type, int count)
        {
            if (type != PopType.None)
            {
                PopulationClass pops = classes.Find(popClass => popClass.type == type);
                if (pops == null)
                    pops = new PopulationClass(type, 0);

                pops.count += count;
                if (pops.count < 0) pops.count = 0;
                RefreshTotal();
            }
        }

        public int GetTypeCount(PopType type)
        {
            PopulationClass targetClass = classes.Find(popClass => popClass.type == type);
            return targetClass != null ? targetClass.count : 0;
        }

        public float GetCurrentTypeFraction(PopType type)
        {
            RefreshTotal();
            return (float)GetTypeCount(type) / (float)TotalPop;
        }

        private void RefreshTotal()
        {
            int pops = 0;
            classes.ForEach(popClass => pops += popClass.count);
            TotalPop = pops;
        }
    }

    public class PopulationClass
    {
        [SaveableProperty(1)]
        public PopType type { get; set; }

        [SaveableProperty(2)]
        public int count { get; set; }

        public PopulationClass(PopType type, int count)
        {
            this.type = type;
            this.count = count;
        }
    }

    public class CultureData
    {

    }

    public class EconomicData
    {
        private float productionEfficiency;
        private float productionQuality;
        private float mercantilism;
        private Guild guild;


    }

    public class LandData
    {
        private float acres;
        private float arable;
        private float pasture;
        private float meadow;
        private float woods;
    }

    public class Guild
    {
        private Hero guildMaster;
        private int capital;

    }
}
