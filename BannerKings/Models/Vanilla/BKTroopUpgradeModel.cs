using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Models.Vanilla
{
    public class BKTroopUpgradeModel : DefaultPartyTroopUpgradeModel
    {
        public override int GetXpCostForUpgrade(PartyBase party, CharacterObject characterObject,
            CharacterObject upgradeTarget)
        {
            var result = base.GetXpCostForUpgrade(party, characterObject, upgradeTarget) * BannerKingsSettings.Instance.TroopUpgradeXp;
            if (party != null && party.MobileParty != null && party.MobileParty.LeaderHero != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(party.MobileParty.LeaderHero);
                if (education != null && education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Cataphract))
                {
                    result *= 1.25f;
                }
            }

            return (int)result;
        }

        public override int GetGoldCostForUpgrade(PartyBase party, CharacterObject characterObject, CharacterObject upgradeTarget)
        {
            if (TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyWageModel is BKPartyWageModel partyWageModel)
            {
                int troopRecruitmentCost = partyWageModel.GetTroopRecruitmentCost(upgradeTarget, null, true);
                int troopRecruitmentCost2 = partyWageModel.GetTroopRecruitmentCost(characterObject, null, true);
                bool isMercenary = characterObject.Occupation == Occupation.Mercenary || characterObject.Occupation == Occupation.Gangster;
                var initCost = (troopRecruitmentCost - troopRecruitmentCost2) / ((!isMercenary) ? 2f : 3f);
                ExplainedNumber explainedNumber = new ExplainedNumber((float)(troopRecruitmentCost - troopRecruitmentCost2) / ((!isMercenary) ? 2f : 3f), false, null);

                #region DefaultPerks.Steward.SoundReserves
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    DefaultPerks.Steward.SoundReserves.AddScaledPartyPerkBonus(ref explainedNumber, false, party.MobileParty);
                }
                else
                {
                    if (party.MobileParty.HasPerk(DefaultPerks.Steward.SoundReserves, false))
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.SoundReserves, party.MobileParty, true, ref explainedNumber);
                    }
                }
                #endregion

                if (characterObject.IsRanged && party.MobileParty.HasPerk(DefaultPerks.Bow.RenownedArcher, true))
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Bow.RenownedArcher, party.MobileParty, false, ref explainedNumber);
                }
                if (characterObject.IsInfantry && party.MobileParty.HasPerk(DefaultPerks.Throwing.ThrowingCompetitions, false))
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Throwing.ThrowingCompetitions, party.MobileParty, true, ref explainedNumber);
                }
                if (characterObject.IsMounted && PartyBaseHelper.HasFeat(party, DefaultCulturalFeats.KhuzaitRecruitUpgradeFeat))
                {
                    explainedNumber.AddFactor(DefaultCulturalFeats.KhuzaitRecruitUpgradeFeat.EffectBonus, GameTexts.FindText("str_culture", null));
                }
                else if (characterObject.IsInfantry && PartyBaseHelper.HasFeat(party, DefaultCulturalFeats.SturgianRecruitUpgradeFeat))
                {
                    explainedNumber.AddFactor(DefaultCulturalFeats.SturgianRecruitUpgradeFeat.EffectBonus, GameTexts.FindText("str_culture", null));
                }
                #region DefaultPerks.Steward.Contractors
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    if (isMercenary )
                    {
                        DefaultPerks.Steward.Contractors.AddScaledPartyPerkBonus(ref explainedNumber, false, party.MobileParty);
                    }
                }
                else
                {
                    if (isMercenary && party.MobileParty.HasPerk(DefaultPerks.Steward.Contractors, false))
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.Contractors, party.MobileParty, true, ref explainedNumber);
                    }
                }
                #endregion
                //make sure the cost is at least 1/4 of the initial cost
                return (int)(MBMath.ClampFloat(explainedNumber.ResultNumber, initCost / 4, explainedNumber.ResultNumber));
            }
            return base.GetGoldCostForUpgrade(party, characterObject, upgradeTarget);
        }
    }
}