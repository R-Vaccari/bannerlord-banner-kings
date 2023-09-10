using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class FaithfulData : BannerKingsData
    {
        [SaveableField(5)] private CampaignTime blessingEndDate;

        [SaveableField(3)] private readonly Dictionary<RiteType, CampaignTime> performedRites;

        public FaithfulData(float piety)
        {
            blessingEndDate = CampaignTime.Never;
            Piety = piety;
            Blessing = null;
            performedRites = new Dictionary<RiteType, CampaignTime>();
        }

        public void PostInitialize()
        {
            if (Blessing != null)
            {
                var bless = DefaultDivinities.Instance.GetById(Blessing.StringId);
                Blessing.Initialize(bless.Name, bless.Description, bless.Effects, bless.SecondaryTitle,
                    bless.BaseBlessingCost);
            }
        }
        public CampaignTime BlessingEndDate => blessingEndDate;
        [field: SaveableField(2)] public Divinity Blessing { get; private set; }

        public float GetBlessingYearsWindow(Divinity divinity, Hero hero)
        {
            var result = 1f;
            if (hero.GetPerkValue(BKPerks.Instance.TheologyBlessed))
            {
                result += 0.25f;
            }

            if (divinity.Shrine != null && hero.CurrentSettlement == divinity.Shrine)
            {
                result += 1f;
            }

            return result;
        }

        public bool CanReceiveBlessing() => blessingEndDate.RemainingDaysFromNow <= 0f;

        [field: SaveableField(1)] public float Piety { get; private set; }

        public void AddPiety(float piety)
        {
            Piety += piety;
        }

        public void AddBlessing(Divinity blessing, Hero hero, bool isIdefiniteMembership = false)
        {
            Blessing = blessing;
            if (isIdefiniteMembership)
            {
                blessingEndDate = CampaignTime.Never;
            }
            else
            {
                blessingEndDate = CampaignTime.YearsFromNow(GetBlessingYearsWindow(blessing, hero));
            }
        }

        public bool HasTimePassedForRite(RiteType type, float years)
        {
            if (performedRites.ContainsKey(type))
            {
                return performedRites[type].ElapsedYearsUntilNow >= years;
            }

            return true;
        }

        public void AddPerformedRite(RiteType type)
        {
            if (performedRites.ContainsKey(type))
            {
                performedRites[type] = CampaignTime.Now;
            }
            else
            {
                performedRites.Add(type, CampaignTime.Now);
            }
        }

        internal override void Update(PopulationData data)
        {
            if (Blessing != null && BlessingEndDate != null && BlessingEndDate.IsPast)
            {
                Blessing = null;
            }
        }
    }
}