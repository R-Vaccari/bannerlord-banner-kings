using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions;

public class FaithfulData : BannerKingsData
{
    private Hero curse;

    [SaveableField(4)] private CampaignTime lastBlessing;

    [SaveableField(3)] private readonly Dictionary<RiteType, CampaignTime> performedRites;

    public FaithfulData(float piety)
    {
        this.Piety = piety;
        Blessing = null;
        curse = null;
        performedRites = new Dictionary<RiteType, CampaignTime>();
    }

    public CampaignTime LastBlessing => lastBlessing;
    [field: SaveableField(2)] public Divinity Blessing { get; private set; }

    public float BlessingYearsWindow => 2f;

    public bool CanReceiveBlessing
    {
        get
        {
            if (lastBlessing == null)
            {
                lastBlessing = CampaignTime.Never;
            }

            return lastBlessing.ElapsedYearsUntilNow >= BlessingYearsWindow;
        }
    }

    [field: SaveableField(1)] public float Piety { get; private set; }

    public void AddPiety(float piety)
    {
        this.Piety += piety;
    }

    public void AddBlessing(Divinity blessing)
    {
        this.Blessing = blessing;
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
    }
}