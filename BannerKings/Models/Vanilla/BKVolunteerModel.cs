using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Library;
using System.Linq;
using Helpers;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using BannerKings.Managers.Court;

namespace BannerKings.Models.Vanilla
{
    class BKVolunteerModel : DefaultVolunteerProductionModel
    {

        public override bool CanHaveRecruits(Hero hero)
        {
            Occupation occupation = hero.Occupation;
            bool valid = (occupation == Occupation.Mercenary || occupation - Occupation.Artisan <= 5);
            if (valid)
            {
                Settlement settlement = hero.CurrentSettlement;
                if (settlement != null && BannerKingsConfig.Instance.PopulationManager != null &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    return data.MilitaryData.Manpower > 0;
                }
                else return true;
            }
            return false;
        }

        public override CharacterObject GetBasicVolunteer(Hero sellerHero)
        {
            return base.GetBasicVolunteer(sellerHero);
        }

        public override float GetDailyVolunteerProductionProbability(Hero hero, int index, Settlement settlement)
        {
            if (BannerKingsConfig.Instance.PopulationManager != null)
                return this.GetDraftEfficiency(hero, index, settlement).ResultNumber;
            else 
                return base.GetDailyVolunteerProductionProbability(hero, index, settlement);
        }

        public ExplainedNumber GetDraftEfficiency(Hero hero, int index, Settlement settlement)
        {
            float num = 0.7f;
            int num2 = 0;
            foreach (Town town in hero.CurrentSettlement.MapFaction.Fiefs)
                num2 += (town.IsTown ? (((town.Settlement.Prosperity < 3000f) ? 1 : ((town.Settlement.Prosperity < 6000f) ? 2 : 3)) + town.Villages.Count<Village>()) : town.Villages.Count<Village>());
            
            float num3 = (num2 < 46) ? ((float)num2 / 46f * ((float)num2 / 46f)) : 1f;
            num += ((hero.CurrentSettlement != null && num3 < 1f) ? ((1f - num3) * 0.2f) : 0f);
            float baseNumber = 0.75f * MathF.Clamp(MathF.Pow(num, (float)(index + 1)), 0f, 1f);
            ExplainedNumber explainedNumber = new ExplainedNumber(baseNumber, true, null);
            Clan clan = hero.Clan;
            if (((clan != null) ? clan.Kingdom : null) != null && hero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Cantons))
                explainedNumber.AddFactor(0.2f, new TextObject("Cantons kingdom policy"));
            
            if (hero.VolunteerTypes[index] != null && hero.VolunteerTypes[index].IsMounted && 
                PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.CavalryTactics, settlement.IsTown ? settlement.Town : settlement.Village.TradeBound.Town))
                explainedNumber.AddFactor(DefaultPerks.Riding.CavalryTactics.PrimaryBonus * 0.01f, DefaultPerks.Riding.CavalryTactics.PrimaryDescription);

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, settlement.OwnerClan.Leader, CouncilPosition.Marshall, 0.25f, true);
            return explainedNumber;
        }

        public ExplainedNumber GetMilitarism(Settlement settlement)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber(0f, true);
            float serfMilitarism = GetClassMilitarism(PopType.Serfs);
            float craftsmenMilitarism = GetClassMilitarism(PopType.Craftsmen);
            float nobleMilitarism = GetClassMilitarism(PopType.Nobles);

            explainedNumber.Add((serfMilitarism + craftsmenMilitarism + nobleMilitarism) / 3f, new TextObject("Base"));

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, settlement.OwnerClan.Leader, CouncilPosition.Marshall, 0.03f, false);
            return explainedNumber;
        }

        public float GetClassMilitarism(PopType type)
        {
            if (type == PopType.Serfs)
            {
                return 0.1f;
            }
            else if (type == PopType.Craftsmen)
            {
                return 0.03f;
            }
            else if (type == PopType.Nobles)
            {
                return 0.12f;
            }
            else return 0;
        }
    }
}
