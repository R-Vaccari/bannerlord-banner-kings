using BannerKings.Managers.Policies;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations.Estates
{
    public class Estate
    {
        public Estate()
        {

        }

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
            if (!notable.IsNotable || notable.CurrentSettlement == null)
            {
                return null;
            }

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

        public Hero Owner { get; private set; }

        public TextObject Name => new TextObject("{=!}Estate of {OWNER}").SetTextVariable("OWNER", Owner.Name);

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


        public int GetTaxFromIncome(BKTaxPolicy.TaxType taxType)
        {
            int result = 0;
            if (taxType != BKTaxPolicy.TaxType.Exemption)
            {
                float factor = 0.45f;
                switch (taxType)
                {
                    case BKTaxPolicy.TaxType.Low:
                        factor = 0.25f;
                        break;
                    case BKTaxPolicy.TaxType.High:
                        factor = 0.65f;
                        break;
                }

                result = (int)(Income.ResultNumber * factor);
            }

            return result;
        }

        public ExplainedNumber Income => BannerKingsConfig.Instance.EstatesModel.CalculateEstateIncome(this, true);


        public EstateData EstatesData { get; private set; }
        public float Farmland { get; private set; }
        public float Pastureland { get; private set; }
        public float Woodland { get; private set; }

        public int Nobles { get; private set; }
        public int Craftsmen { get; private set; }
        public int Serfs { get; private set; }
        public int Slaves { get; private set; }



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
    }
}
