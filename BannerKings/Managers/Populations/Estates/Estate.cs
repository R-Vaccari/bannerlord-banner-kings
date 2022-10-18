using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Populations.Estates
{
    public class Estate
    {


        private Estate(Hero owner, float farmland, float pastureland, float woodland,
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

            return new Estate(notable, farmland, pastureland, woodland, 0, 0);
        }

        public Hero Owner { get; private set; }

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
