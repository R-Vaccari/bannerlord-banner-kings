using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations.Estates
{
    public class EstateData : BannerKingsData
    {

        public EstateData(Settlement settlement)
        {
            Settlement = settlement;
            Estates = new List<Estate>();
        }

        [SaveableProperty(1)] public Settlement Settlement { get; private set; }
        [SaveableProperty(2)]  public List<Estate> Estates { get; private set; }

        public bool HeroHasEstate(Hero hero) => Estates.Any(x => x.Owner == hero);

        public Estate GetHeroEstate(Hero hero) => Estates.FirstOrDefault(x => x.Owner == hero);

        public void UpdatePopulation(PopulationManager.PopType type, int quantity, int classTotal)
        {
            foreach (Estate estate in Estates)
            {
                if (estate.IsDisabled)
                {
                    continue;
                }

                float proportion = estate.GetPopulationClassQuantity(type) / (float)classTotal;
                int result = (int)(quantity * proportion);
                estate.AddPopulation(type, result);
            }
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


        internal override void Update(PopulationData data = null)
        {
            foreach (Estate estate in Estates)
            {
                if (estate.IsDisabled) 
                {
                    continue;
                }

                estate.Tick(data);
                if (estate.Owner.IsDead)
                {
                    InheritEstate(estate);
                }
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
                            Estates.Add(Estate.CreateNotableEstate(notable, data));
                        }
                    }
                }
            }
         

            if (Estates.Count < BannerKingsConfig.Instance.EstatesModel.CalculateEstatesMaximum(Settlement).ResultNumber)
            {
                Estates.Add(Estate.CreateNotableEstate(null, data));
            }
        }
    }
}
