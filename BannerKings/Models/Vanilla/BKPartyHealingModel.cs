using System;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyHealingModel : DefaultPartyHealingModel
    {
        private static readonly TextObject _starvingText = new TextObject("{=jZYUdkXF}Starving");

        public override int GetHeroesEffectedHealingAmount(Hero hero, float healingRate)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber(healingRate, false, null);

            #region DefaultPerks.Medicine
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulMedicinePerks)
            {
                DefaultPerks.Medicine.SelfMedication.AddScaledPersonalPerkBonus(ref explainedNumber, false, hero);
            }
            else
            {
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.SelfMedication, hero.CharacterObject, true, ref explainedNumber);
            }
            #endregion

            float resultNumber = explainedNumber.ResultNumber;
            if (resultNumber - (float)((int)resultNumber) > MBRandom.RandomFloat)
            {
                return (int)resultNumber + 1;
            }
            return (int)resultNumber;
        }

        public override ExplainedNumber GetDailyHealingHpForHeroes(MobileParty party, bool includeDescriptions = false)
        {
            ExplainedNumber result = base.GetDailyHealingHpForHeroes(party, includeDescriptions);
            #region DefaultPerks.Medicine.TriageTent && DefaultPerks.Medicine.WalkItOff
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (!party.IsMoving)
                {
                    DefaultPerks.Medicine.TriageTent.AddScaledPartyPerkBonus(ref result, false, party, removeOriginalValue: true);
                }
                else
                {
                    DefaultPerks.Medicine.WalkItOff.AddScaledPartyPerkBonus(ref result, false, party, removeOriginalValue: true);
                }
            }
            #endregion
            Hero leader = party.LeaderHero;
            if (leader != null && party.CurrentSettlement != null)
            {
                if (BannerKingsConfig.Instance.CourtManager.HasCurrentTask(leader.Clan, DefaultCouncilTasks.Instance.FamilyCare,
                    out float healCompetence))
                {
                    result.AddFactor(0.2f * healCompetence, DefaultCouncilTasks.Instance.FamilyCare.Name);
                }
            }

            return result;
        }
        public override ExplainedNumber GetDailyHealingForRegulars(MobileParty party, bool includeDescriptions = false)
        {
            ExplainedNumber bonuses = base.GetDailyHealingForRegulars(party, includeDescriptions);

            #region DefaultPerks.Medicine.TriageTent && DefaultPerks.Medicine.WalkItOff
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (!party.IsMoving)
                {
                    DefaultPerks.Medicine.TriageTent.AddScaledPartyPerkBonus(ref bonuses, false, party, removeOriginalValue: true);
                }
                else
                {
                    DefaultPerks.Medicine.WalkItOff.AddScaledPartyPerkBonus(ref bonuses, false, party, removeOriginalValue: true);
                }
            }
            #endregion

            Boolean isInBesiegedStarvingCity = party.CurrentSettlement != null && party.CurrentSettlement.IsUnderSiege && party.CurrentSettlement.IsStarving;
            if (isInBesiegedStarvingCity && !party.IsGarrison)
            {
                int num = MBRandom.RoundRandomized((float)party.MemberRoster.TotalRegulars * 0.1f);
                bonuses.Add(-num, _starvingText);
            }
            return bonuses;
        }
        public override int GetBattleEndHealingAmount(MobileParty party, CharacterObject character)
        {
            float num = 0f;
            if (character.IsHero)
            {
                Hero heroObject = character.HeroObject;
                if (heroObject.GetPerkValue(DefaultPerks.Medicine.PreventiveMedicine))
                {
                    #region DefaultPerks.Medicine.PreventiveMedicine
                    if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulMedicinePerks)
                    {
                        ExplainedNumber expnum = new ExplainedNumber(heroObject.MaxHitPoints - heroObject.HitPoints);
                        var healRate = DefaultPerks.Medicine.PreventiveMedicine.AddScaledPersonalPerkBonus(ref expnum, true, heroObject);
                        if (heroObject.MaxHitPoints > heroObject.HitPoints)
                        {
                            num += ((float)(heroObject.MaxHitPoints - heroObject.HitPoints)) * healRate;
                        }
                    }
                    else
                    {
                        num += (float)(heroObject.MaxHitPoints - heroObject.HitPoints) * DefaultPerks.Medicine.PreventiveMedicine.SecondaryBonus;
                    }
                    #endregion                   
                }
                #region DefaultPerks.Medicine.WalkItOff
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulMedicinePerks)
                {
                    if (party.MapEventSide == party.MapEvent.AttackerSide)
                    {
                        ExplainedNumber expnum = new ExplainedNumber(heroObject.MaxHitPoints - heroObject.HitPoints);
                        num += DefaultPerks.Medicine.WalkItOff.AddScaledPersonalPerkBonus(ref expnum, true, heroObject);
                    }
                }
                else
                {
                    if (party.MapEventSide == party.MapEvent.AttackerSide && heroObject.GetPerkValue(DefaultPerks.Medicine.WalkItOff))
                    {
                        num += DefaultPerks.Medicine.WalkItOff.SecondaryBonus;
                    }
                }
                #endregion

            }
            return MathF.Round(num);
        }


    }
}
