﻿using BannerKings.Extensions;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles.Laws;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations.Estates
{
    public class Estate
    {
        public Estate(Hero owner, EstateData data, float farmland, float pastureland, float woodland,
            int serfs, int slaves, int nobles = 0, int craftsmen = 0)
        {
            Owner = owner;
            Farmland = farmland;
            Pastureland = pastureland;
            Woodland = woodland;
            Nobles = nobles;
            Craftsmen = craftsmen;
            Serfs = serfs;
            Slaves = slaves;
            EstatesData = data;
        }

        public static Estate CreateNotableEstate(Hero notable, PopulationData data)
        {
            float acreage = data.LandData.Acreage;
            float acres = MBRandom.RandomFloatRanged(BannerKingsConfig.Instance.EstatesModel.MinimumEstateAcreage, 
                BannerKingsConfig.Instance.EstatesModel.MaximumEstateAcreagePercentage * acreage);
            var composition = data.LandData.Composition;
            float farmland = acres * composition[0];
            float pastureland = acres * composition[1];
            float woodland = acres * composition[2];

            float totalSerfs = data.GetTypeCount(PopType.Serfs);
            float totalSlaves = data.GetTypeCount(PopType.Slaves) * (1f - data.EconomicData.StateSlaves);

            int desiredWorkforce = (int)(acres / 2f);
            float desiredSerfs = (int)(desiredWorkforce * 0.8f);
            float desiredSlaves = (int)(desiredWorkforce * 0.2f);

            return new Estate(notable, data.EstateData, farmland, pastureland, woodland, 
                (int)MathF.Min(desiredSerfs, totalSerfs * 0.15f),
                (int)MathF.Min(desiredSlaves, totalSlaves * 0.25f));
        }

        [SaveableProperty(1)] public Hero Owner { get; private set; }

        public TextObject Name => Owner != null ? new TextObject("{=!}Estate of {OWNER}").SetTextVariable("OWNER", Owner.Name) : new TextObject();

        public void SetOwner(Hero newOnwer)
        {
            Owner = newOnwer;
            if (newOnwer == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=!}You are now the owner of an estate at {SETTLEMENT}")
                    .SetTextVariable("SETTLEMENT", EstatesData.Settlement.Name),
                    0,
                    null,
                    "event:/ui/notification/relation");
            }
        }

        public int GetTaxFromIncome() => (int)(Income.ResultNumber * TaxRatio.ResultNumber);

        public ExplainedNumber TaxRatio => BannerKingsConfig.Instance.EstatesModel.GetTaxRatio(this, true);

        public bool IsDisabled 
        { 
            get
            {
                var settlement = EstatesData.Settlement;
                var fiefOwner = settlement.IsVillage ? settlement.Village.GetActualOwner() : settlement.Owner;
                return Owner == null || Owner == fiefOwner;
            } 
        }

        public ExplainedNumber Income => BannerKingsConfig.Instance.EstatesModel.CalculateEstateIncome(this, true);
        public ExplainedNumber AcrePrice => BannerKingsConfig.Instance.EstatesModel.CalculateAcrePrice(EstatesData.Settlement, true);
        public ExplainedNumber EstateValue => BannerKingsConfig.Instance.EstatesModel.CalculateEstatePrice(this, true);
        public ExplainedNumber AcreageGrowth => Task == EstateTask.Land_Expansion ? BannerKingsConfig.Instance.ConstructionModel
            .CalculateLandExpansion(BannerKingsConfig.Instance.PopulationManager.GetPopData(EstatesData.Settlement),
            LandExpansionWorkforce) : new ExplainedNumber(0f);

        public ExplainedNumber Production => BannerKingsConfig.Instance.EstatesModel.CalculateEstateProduction(this, true);

        public int Population => Nobles + Craftsmen + Serfs + Slaves;

        public int AvailableWorkForce
        {
            get
            {
                int toSubtract = 0;
                if (Task == EstateTask.Land_Expansion)
                {
                    toSubtract += LandExpansionWorkforce;
                }

                return Serfs + Slaves - toSubtract;
            }
        }

        public int LandExpansionWorkforce => (int)((Serfs + Slaves) * 0.5f);

        public float WorkforceSaturation
        {
            get
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(EstatesData.Settlement);
                float available = AvailableWorkForce;
                var farms = Farmland / data.LandData.GetRequiredLabor("farmland");
                var pasture = Pastureland / data.LandData.GetRequiredLabor("pasture");
                return available / (farms + pasture);
            }
        }


        public float Influence => BannerKingsConfig.Instance.InfluenceModel.GetNoblesInfluence(EstatesData.Settlement, Nobles);

        public float Acreage => Farmland + Pastureland + Woodland;

        [SaveableProperty(2)] public EstateData EstatesData { get; private set; }
        [SaveableProperty(3)] public float Farmland { get; private set; }
        [SaveableProperty(4)] public float Pastureland { get; private set; }
        [SaveableProperty(5)] public float Woodland { get; private set; }

        [SaveableProperty(6)] public int Nobles { get; private set; }
        [SaveableProperty(7)] public int Craftsmen { get; private set; }
        [SaveableProperty(8)] public int Serfs { get; private set; }
        [SaveableProperty(9)] public int Slaves { get; private set; }

     
        public void ChangeTask(EstateTask task) => Task = task;
        public void ChangeDuty(EstateDuty duty) => Duty = duty;

        [SaveableProperty(10)] public EstateDuty Duty { get; private set; }
        [SaveableProperty(11)] public EstateTask Task { get; private set; }

        public void Tick(PopulationData data)
        {
            if (IsDisabled)
            {
                return;
            }


            if (Task == EstateTask.Land_Expansion)
            {
                var progress = AcreageGrowth.ResultNumber;
                if (progress > 0f)
                {
                    var composition = data.LandData.Composition;
                    var list = new List<(int, float)>
                    {
                        new(0, composition[0]),
                        new(1, composition[1]),
                        new(2, composition[2])
                    };
                    var choosen = MBRandom.ChooseWeighted(list);

                    switch (choosen)
                    {
                        case 0:
                            Farmland += progress;
                            break;
                        case 1:
                            Pastureland += progress;
                            break;
                        default:
                            Woodland += progress;
                            break;
                    }
                }
            }
        }



        public void AddPopulation(PopType type, int toAdd)
        {
            if (type == PopType.Nobles)
            {
                Nobles += toAdd;
            }
            else if (type == PopType.Craftsmen)
            {
                Craftsmen += toAdd;
            }
            else if (type == PopType.Serfs)
            {
                Serfs += toAdd;
            }
            else if (type == PopType.Slaves)
            {
                Slaves += toAdd;
            }
        }

        public int GetPopulationClassQuantity(PopType type)
        {
            int result = 0;

            if (type == PopType.Nobles)
            {
                result = Nobles;
            }
            else if (type == PopType.Craftsmen)
            {
                result = Craftsmen;
            }
            else if (type == PopType.Serfs)
            {
                result = Serfs;
            }
            else if (type == PopType.Slaves)
            {
                result = Slaves;
            }

            return result;
        }


        public enum EstateDuty
        {
            Taxation,
            Military
        }

        public enum EstateTask
        {
            Prodution,
            Land_Expansion
        }
    }
}