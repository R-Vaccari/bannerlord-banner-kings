using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class CultureModel : GameModel
    {

        public void CalculateAssimilationChange(Settlement settlement)
        {

            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                float result = GetAssimilationChange(settlement);
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
                float finalResult = data.Assimilation + result;
                if (finalResult > 1f)
                    finalResult = 1f;
                else if (finalResult < 0f)
                    finalResult = 0f;
                data.Assimilation = finalResult;

                if (data.Assimilation == 1f && settlement.Owner != null)
                {
                    if (settlement.IsTown)
                    {
                        var remainingTowns = 0;
                        foreach (Settlement other in Campaign.Current.Settlements)
                        {
                            if (other.Culture == settlement.Culture)
                            {
                                remainingTowns++;
                            }
                        }
                        if (remainingTowns == 1)
                        {
                            // can not convert last town in culture, or will cause CTD for companion respawns:
                            // borrowed from https://github.com/Splintertx/ChangeSettlementCulture/blob/master/ChangeSettlementCulture/SettlementVariablesBehaviorMod.cs#L54
                            return;
                        }
                    }

                    // conversion
                    settlement.Culture = settlement.Owner.Culture;
                    if (settlement.BoundVillages != null)
                    {
                        foreach (Village attached in settlement.BoundVillages)
                        {
                            if (attached.Settlement == null)
                                continue;

                            attached.Settlement.Culture = settlement.Owner.Culture;
                            if (attached.Settlement.Notables != null)
                            {
                                foreach (Hero notable in attached.Settlement.Notables)
                                {
                                    notable.Culture = settlement.Owner.Culture;
                                }
                            }
                        }
                    }
                    // convert any notables
                    if (settlement.Notables != null)
                    {
                        foreach (Hero notable in settlement.Notables)
                        {
                            notable.Culture = settlement.Owner.Culture;
                        }
                    }
                }
            }
        }

        public float GetAssimilationChange(Settlement settlement)
        {
            CultureObject ownerCulture = settlement.OwnerClan.Culture;
            float change = -0.005f;

            if (!settlement.IsVillage && settlement.Town != null)
            change += 0.005f * (1f * (settlement.Town.Security * 0.01f));

            if (settlement.Culture != ownerCulture)
            {
                if (!settlement.IsVillage && settlement.Town != null)
                    if (settlement.Town.Governor != null && settlement.Town.Governor.Culture == ownerCulture)
                    {
                        change += 0.005f;
                        int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                        float effect = (float)skill * 0.00005f;
                        if (effect > 0.015f)
                            effect = 0.015f;
                        change += effect;
                    }
                else if (settlement.IsVillage)
                        if (settlement.Village.MarketTown.Governor != null && settlement.Village.MarketTown.Governor.Culture == ownerCulture)
                        {
                            change += 0.005f;
                            int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                            float effect = (float)skill * 0.00005f;
                            if (effect > 0.015f)
                                effect = 0.015f;
                            change += effect;
                        }

            } else change = 0f;
            return change;
        }
    }
}
