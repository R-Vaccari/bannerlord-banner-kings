using BannerKings.Extensions;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Populations.Estates.Estate;

namespace BannerKings.Models.BKModels
{
    public class BKEstatesModel
    {
        public int MinimumEstateAcreage => 120;

        public float MaximumEstateAcreagePercentage => 0.18f;

        public int CalculateEstateGrantRelation(Estate estate, Hero grantor)
        {
            int result = 8;

            if (!estate.Owner.IsClanLeader())
            {
                result += 10;
            }

            return result;
        }

        public EstateAction GetAction(ActionType type, Estate estate, Hero actionTaker, Hero actionTarget = null)
        {
            if (type == ActionType.Buy)
            {
                return GetBuy(estate, actionTaker);
            }
            else if (type == ActionType.Grant)
            {
                return GetGrant(estate, actionTaker, actionTarget);
            }
            else
            {
                return null;
            }
        }

        public EstateAction GetGrant(Estate estate, Hero actionTaker, Hero actionTarget)
        {
            EstateAction action = new EstateAction(estate, actionTaker, ActionType.Grant, actionTarget);

            var settlement = estate.EstatesData.Settlement;
            var owner = settlement.IsVillage ? settlement.Village.GetActualOwner() : settlement.Owner;

            if (actionTaker != owner)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}You don't own this settlement.");
                return action;
            }

            if (actionTarget != null)
            {
                if (actionTarget.IsNotable)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Cannot grant to notables.");
                    return action;
                }

                if (actionTarget.MapFaction != actionTaker.MapFaction)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Cannot grant to foreign lords.");
                    return action;
                }

                Clan clan = actionTarget.Clan;
                if (clan == null)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=Uw7dMzA4}No clan.");
                    return action;
                }
            }
            else
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}No defined target for granting.");
                return action;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (title.deJure != actionTaker)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=CK4rr7yZ}Not legal owner.");
                    return action;
                }

                if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.EstateTenureQuiaEmptores))
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Cannot grant estates under {LAW} law.")
                        .SetTextVariable("LAW", DefaultDemesneLaws.Instance.EstateTenureQuiaEmptores.Name);
                    return action;
                }
            }

            var candidates = GetGrantCandidates(action);
            if (!candidates.Contains(actionTarget))
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Not a granting candidate. Clan leaders and companions (except under {LAW} law) may be granted to.")
                    .SetTextVariable("LAW", DefaultDemesneLaws.Instance.EstateTenureQuiaEmptores.Name);
                return action;
            }

            action.Possible = true;
            action.Reason = new TextObject("{=bjJ99NEc}Action can be taken.");
            return action;
        }

        public EstateAction GetBuy(Estate estate, Hero actionTaker)
        {
            EstateAction action = new EstateAction(estate, actionTaker, ActionType.Buy);

            var settlement = estate.EstatesData.Settlement;
            var owner = settlement.IsVillage ? settlement.Village.GetActualOwner() : settlement.Owner;

            if (actionTaker == owner)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Already settlement owner.");
                return action;
            }

            if (actionTaker == estate.Owner)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Already estate owner.");
                return action;
            }

            if (estate.Owner != null)
            {
                if (estate.Owner.IsNotable)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Cannot buy notable estates.");
                    return action;
                }
            }

            Clan clan = actionTaker.Clan;
            if (clan == null)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=Uw7dMzA4}No clan.");
                return action;
            }
            else if (actionTaker != clan.Leader)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Not clan leader.");
                return action;
            }

            int value = (int)estate.EstateValue.ResultNumber;
            if (actionTaker.Gold < value)
            {
                action.Possible = false;
                action.Reason = GameTexts.FindText("str_warning_you_dont_have_enough_money");
                return action;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (title.deJure != owner)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Cannot buy from an illegal settlement owner. The owner must hold it de jure in order to sell estates.");
                    return action;
                }

                if (!title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.EstateTenureAllodial))
                {
                    if (owner.MapFaction != actionTaker.MapFaction)
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=!}Cannot buy foreign kingdom estates except if they are under Allodial tenure law.");
                        return action;
                    }
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=bjJ99NEc}Action can be taken.");
            return action;
        }

        public EstateAction GetReclaim(Estate estate, Hero actionTaker)
        {
            EstateAction action = new EstateAction(estate, actionTaker, ActionType.Reclaim);

            var settlement = estate.EstatesData.Settlement;
            var owner = settlement.IsVillage ? settlement.Village.GetActualOwner() : settlement.Owner;

            if (actionTaker != owner)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Not settlement owner.");
                return action;
            }

            if (actionTaker == estate.Owner)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Already estate owner.");
                return action;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (title.deJure != actionTaker)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Must be de jure owner to reclaim estates.");
                    return action;
                }
            }

            if (estate.Owner != null)
            {
                if (estate.Owner.MapFaction == actionTaker.MapFaction)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Cannot reclaim an estate from your faction Peers.");
                    return action;
                }
            }
            else
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Cannot reclaim a vacant estate.");
                return action;
            }

            int value = (int)estate.EstateValue.ResultNumber;
            if (actionTaker.Gold < value)
            {
                action.Possible = false;
                action.Reason = GameTexts.FindText("str_warning_you_dont_have_enough_money");
                return action;
            }

            action.Influence = estate.EstateValue.ResultNumber / 2000f;
            if (actionTaker.Clan.Influence < action.Influence)
            {
                action.Possible = false;
                action.Reason = GameTexts.FindText("str_decision_not_enough_influence");
                return action;
            }

            action.Possible = true;
            action.Reason = new TextObject("{=bjJ99NEc}Action can be taken.");
            return action;
        }

        public List<Hero> GetGrantCandidates(EstateAction action)
        {
            List<Hero> list = new List<Hero>();

            var clan = action.ActionTaker.Clan;
            if (clan != null)
            {
                if (clan.Kingdom != null)
                {
                    foreach (Clan targetClan in clan.Kingdom.Clans)
                    {
                        if (clan != targetClan && !targetClan.IsUnderMercenaryService)
                        {
                            list.Add(targetClan.Leader);
                        }
                    }
                }
            }

            return list;
        }

        public ExplainedNumber GetTaxRatio(Estate estate, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            var settlement = estate.EstatesData.Settlement;

            var taxType = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager
                    .GetPolicy(settlement, "tax")).Policy;
            
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.EstateTenureAllodial))
                {
                    result.AddFactor(-1f, DefaultDemesneLaws.Instance.EstateTenureAllodial.Name);
                }
                else if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.EstateTenureFeeTail) && estate.Duty == EstateDuty.Taxation)
                {
                    result.Add(0.075f, DefaultDemesneLaws.Instance.EstateTenureFeeTail.Name);
                }
            }

            float factor;
            switch (taxType)
            {
                case BKTaxPolicy.TaxType.Low:
                    factor = 0.15f;
                    break;
                case BKTaxPolicy.TaxType.High:
                    factor = 0.45f;
                    break;
                case BKTaxPolicy.TaxType.Exemption:
                    factor = 0f;
                    break;
                default:
                    factor = 0.3f;
                    break;
            }

            result.Add(factor, new TextObject("{=L7QhNa6a}Tax policy"));
            return result;
        }

        public ExplainedNumber CalculateEstateProduction(Estate estate, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(0f);

            var settlement = estate.EstatesData.Settlement;
            if (settlement.IsVillage)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(estate.EstatesData.Settlement);
                float proportion = MathF.Clamp(estate.AvailableWorkForce / (float)(data.GetTypeCount(PopType.Slaves) + data.GetTypeCount(PopType.Serfs)), 
                    0f, 1f);
                float production = BannerKingsConfig.Instance.VillageProductionModel.CalculateProductionsExplained(settlement.Village).ResultNumber;

                result.Add(production * proportion, new TextObject("{=!}Total production proportion"));
            }

            return result;
        }

        public ExplainedNumber CalculateAcrePrice(Settlement settlement, bool explanations = false)
        {
            var result = new ExplainedNumber(450f, explanations);
            if (settlement.IsVillage)
            {
                result.Add(settlement.Village.Hearth * 0.1f, GameTexts.FindText("str_map_tooltip_hearths"));
            }

            return result;
        }

        public ExplainedNumber CalculateEstatePrice(Estate estate, bool explanations = false)
        {
            var result = new ExplainedNumber(500f, explanations);
            var settlement = estate.EstatesData.Settlement;

            float acrePrice = CalculateAcrePrice(settlement).ResultNumber;
            result.Add(acrePrice * estate.Farmland, new TextObject("{=zMPm162W}Farmlands"));
            result.Add(acrePrice * estate.Pastureland * 0.5f, new TextObject("{=ngRhXYj1}Pasturelands"));
            result.Add(acrePrice * estate.Woodland * 0.15f, new TextObject("{=qPQ7HKgG}Woodlands"));

            result.Add(estate.TaxAccumulated, new TextObject("{=!}Accumulated taxes"));
            /*var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.EstateTenureAllodial))
                {
                    result.Add(1f, DefaultDemesneLaws.Instance.EstateTenureAllodial.Name);
                }
            }*/

            return result;
        }

        public ExplainedNumber CalculateEstatesMaximum(Settlement settlement, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            if (settlement.IsVillage)
            {
                var landOwners = settlement.Notables.Count(x => x.Occupation == Occupation.RuralNotable);
                result.Add(landOwners);
                result.Add(1);
            }

            return result;
        }

        public float GetEstateWorkforceProportion(Estate estate, PopulationData data)
        {
            float serfs = data.GetTypeCount(Managers.PopulationManager.PopType.Serfs);
            float slaves = data.GetTypeCount(Managers.PopulationManager.PopType.Slaves);

            return MathF.Clamp((estate.Serfs + estate.Slaves) / (serfs + slaves), 0f, 1f);
        }
    }
}
