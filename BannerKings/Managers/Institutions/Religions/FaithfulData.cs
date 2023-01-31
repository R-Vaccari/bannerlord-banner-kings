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
        [SaveableField(4)] private CampaignTime lastBlessing;
        [SaveableField(5)] private CampaignTime blessingEndDate;

        [SaveableField(3)] private readonly Dictionary<RiteType, CampaignTime> performedRites;

        public FaithfulData(float piety)
        {
            lastBlessing = CampaignTime.Never;
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

        public CampaignTime LastBlessing => lastBlessing;
        public CampaignTime BlessingEndDate => blessingEndDate;
        [field: SaveableField(2)] public Divinity Blessing { get; private set; }

        public float GetBlessingYearsWindow(Hero hero)
        {
            var result = 2f;
            if (hero.GetPerkValue(BKPerks.Instance.TheologyBlessed))
            {
                result += 0.25f;
            }

            return result;
        }

        public bool CanReceiveBlessing() => blessingEndDate.RemainingDaysFromNow <= 0f;

        [field: SaveableField(1)] public float Piety { get; private set; }

        public void AddPiety(float piety)
        {
            Piety += piety;
        }

        public void AddBlessing(Divinity blessing, Hero hero)
        {
            Blessing = blessing;
            if (blessing.IsIndefiniteMembership)
            {
                blessingEndDate = CampaignTime.Never;
            }
            else
            {
                blessingEndDate = CampaignTime.YearsFromNow(GetBlessingYearsWindow(hero));
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
            if (lastBlessing == null)
            {
                lastBlessing = CampaignTime.Never;
            }

            if (Blessing != null && BlessingEndDate != null && BlessingEndDate.IsPast)
            {
                Blessing = null;
            }
        }
    }
}