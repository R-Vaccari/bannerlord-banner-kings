using BannerKings.Utils;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations.Estates
{
    public class EstateData : BannerKingsData
    {
        public EstateData(Settlement settlement, PopulationData data)
        {
            Settlement = settlement;
            Estates = new List<Estate>();
        }

        [SaveableProperty(1)] public Settlement Settlement { get; private set; }
        [SaveableProperty(2)]  public List<Estate> Estates { get; private set; }

        public bool HeroHasEstate(Hero hero) => Estates.Any(x => x.Owner == hero);

        public Estate GetHeroEstate(Hero hero) => Estates.FirstOrDefault(x => x.Owner == hero);

        public void AccumulateTradeTax(PopulationData data, int tradeTax)
        {
            int totalDeducted = 0;
            foreach (Estate estate in Estates)
            {
                if (estate.IsDisabled)
                {
                    continue;
                }

                var result = (int)(tradeTax * (BannerKingsConfig.Instance.EstatesModel.GetEstateWorkforceProportion(estate, data) * 
                    (1f - estate.TaxRatio.ResultNumber)));
                totalDeducted += result;
                estate.TaxAccumulated += result;
            }

            Settlement.Village.TradeTaxAccumulated += tradeTax - totalDeducted;
        }

        public void InheritEstate(Estate estate, Hero newOwner = null)
        {
            if (newOwner != null)
            {
                estate.SetOwner(newOwner);
            }
            else
            {
                var owner = estate.Owner;
                if (owner.IsNotable && owner.IsRuralNotable)
                {
                    var newNotable = Settlement.Notables.FirstOrDefault(x => !HeroHasEstate(x));
                    if (newNotable != null)
                    {
                        estate.SetOwner(newNotable);
                    }
                }
                else if (owner.Clan != null)
                {
                    var leader = owner.Clan.Leader;
                    if (leader != owner && leader.IsAlive)
                    {
                        estate.SetOwner(leader);
                    }
                    else
                    {
                        owner.Children.Sort((x, y) => x.Age.CompareTo(y.Age));
                        var child = owner.Children.FirstOrDefault(x => !x.IsChild && x.IsAlive);
                        if (child != null)
                        {
                            estate.SetOwner(child);
                        }
                    }
                }
            }

            if (estate.Owner.IsDead)
            {
                DestroyEstate(estate);
            }
        }

        public void DestroyEstate(Estate estate) => Estates.Remove(estate);

        public void CreateEstates(PopulationData data)
        {
            if (Estates.Count < BannerKingsConfig.Instance.EstatesModel.CalculateEstatesMaximum(Settlement).ResultNumber)
            {
                var estate = Estate.CreateNotableEstate(null, data, this);
                if (estate != null)
                {
                    Estates.Add(estate);
                }
            }
        }

        internal override void Update(PopulationData data = null)
        {
            ExceptionUtils.TryCatch(() =>
            {
                float growthFactor = 0f;
                if (Settlement.IsVillage)
                {
                    growthFactor = BannerKingsConfig.Instance.ProsperityModel.CalculateHearthChange(Settlement.Village).ResultNumber;
                }
                
                var dead = new List<Estate>();
                foreach (Estate estate in Estates)
                {
                    if (estate.IsDisabled) continue;
                    if (MBRandom.RandomFloat < growthFactor) estate.AddPopulation(1);

                    estate.Tick(data);
                    if (estate.Owner.IsDead)
                    {
                        dead.Add(estate);
                    }
                }

                foreach (var estate in dead)
                {
                    InheritEstate(estate);
                }

                if (Settlement.Notables != null)
                {
                    foreach (Hero notable in Settlement.Notables)
                    {
                        if (notable.IsRuralNotable && !HeroHasEstate(notable))
                        {
                            var vacantEstate = Estates.FirstOrDefault(x => x.Owner != null && x.Owner.IsDead && x.Owner.IsRuralNotable);
                            if (vacantEstate != null)
                            {
                                InheritEstate(vacantEstate, notable);
                            }
                            else
                            {
                                var estate = Estate.CreateNotableEstate(notable, data, this);
                                if (estate != null)
                                {
                                    Estates.Add(estate);
                                }
                            }
                        }
                    }
                }

                CreateEstates(data);
            }, 
            this.GetType().Name,
            false);
        }
    }
}
