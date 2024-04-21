using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using static BannerKings.Utils.PerksHelpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKInventoryCapacityModel : DefaultInventoryCapacityModel
    {
        private static readonly TextObject _textTroops = new TextObject("{=5k4dxUEJ}Troops");

        private static readonly TextObject _textHorses = new TextObject("{=1B8ZDOLs}Horses");

        private static readonly TextObject _textBase = new TextObject("{=basevalue}Base");

        private static readonly TextObject _textSpareMounts = new TextObject("{=rCiKbsyW}Spare Mounts");

        private static readonly TextObject _textPackAnimals = new TextObject("{=dI1AOyqh}Pack Animals");

        public override ExplainedNumber CalculateInventoryCapacity(MobileParty mobileParty,
            bool includeDescriptions = false, int additionalTroops = 0, int additionalSpareMounts = 0,
            int additionalPackAnimals = 0, bool includeFollowers = false)
        {
            ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);
            PartyBase party = mobileParty.Party;
            int num = party.NumberOfMounts;
            int num2 = party.NumberOfHealthyMembers;
            int num3 = party.NumberOfPackAnimals;
            if (includeFollowers)
            {
                foreach (MobileParty mobileParty2 in mobileParty.AttachedParties)
                {
                    num += party.NumberOfMounts;
                    num2 += party.NumberOfHealthyMembers;
                    num3 += party.NumberOfPackAnimals + additionalPackAnimals;
                }
            }
            ExplainedNumber partyCarryingCapacity = new ExplainedNumber(mobileParty.PrisonRoster.TotalHealthyCount, includeDescriptions, null);

            #region DefaultPerks.Steward.ArenicosHorses
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (mobileParty.PrisonRoster.TotalHealthyCount > 0)
                {
                  
                    DefaultPerks.Steward.ForcedLabor.AddScaledPerkBonus(ref partyCarryingCapacity, false, mobileParty);
                    result.Add(partyCarryingCapacity.ResultNumber, DefaultPerks.Steward.ForcedLabor.Name);
                }
            }
            else
            {
                if (mobileParty.HasPerk(DefaultPerks.Steward.ArenicosHorses, false))
                {
                    num2 += MathF.Round((float)num2 * DefaultPerks.Steward.ArenicosHorses.PrimaryBonus);
                }
            }
            #endregion           
            #region DefaultPerks.Steward.ForcedLabor
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (mobileParty.PrisonRoster.TotalHealthyCount>0)
                {
                    ExplainedNumber prisonersCarryingCapacity = new ExplainedNumber(mobileParty.PrisonRoster.TotalHealthyCount, includeDescriptions, null);
                    DefaultPerks.Steward.ForcedLabor.AddScaledPerkBonus(ref prisonersCarryingCapacity, false, mobileParty);
                    result.Add(prisonersCarryingCapacity.ResultNumber, DefaultPerks.Steward.ForcedLabor.Name);
                }              
            }
            else
            {
                if (mobileParty.HasPerk(DefaultPerks.Steward.ForcedLabor, false))
                {
                    num2 += party.PrisonRoster.TotalHealthyCount;
                }
            }
            #endregion
            
          
            ExplainedNumber explainedNumber = new ExplainedNumber((float)num3 * 10f * 10f, false, null);
            if (mobileParty.HasPerk(DefaultPerks.Scouting.BeastWhisperer, true))
            {
                explainedNumber.AddFactor(DefaultPerks.Scouting.BeastWhisperer.SecondaryBonus, DefaultPerks.Scouting.BeastWhisperer.Name);
            }
            if (mobileParty.HasPerk(DefaultPerks.Riding.DeeperSacks, false))
            {
                explainedNumber.AddFactor(DefaultPerks.Riding.DeeperSacks.PrimaryBonus, DefaultPerks.Riding.DeeperSacks.Name);
            }
            #region DefaultPerks.Steward.ArenicosMules
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                DefaultPerks.Steward.ArenicosMules.AddScaledPerkBonus(ref explainedNumber, false, mobileParty);
            }
            else
            {
                if (mobileParty.HasPerk(DefaultPerks.Steward.ArenicosMules, false))
                {
                    explainedNumber.AddFactor(DefaultPerks.Steward.ArenicosMules.PrimaryBonus, DefaultPerks.Steward.ArenicosMules.Name);
                }
            }
            #endregion

            result.Add(10f, _textBase, null);
            result.Add((float)num2 * 2f * 10f, _textTroops, null);
            result.Add((float)num * 2f * 10f, _textSpareMounts, null);
            result.Add(explainedNumber.ResultNumber, _textPackAnimals, null);

            if (mobileParty.HasPerk(DefaultPerks.Trade.CaravanMaster, false))
            {
                result.AddFactor(DefaultPerks.Trade.CaravanMaster.PrimaryBonus, DefaultPerks.Trade.CaravanMaster.Name);
            }
            result.LimitMin(10f);
            return result;
        }
    }
}