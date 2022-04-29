using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles;
using BannerKings.Populations;
using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Policies.BKDraftPolicy;

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

                return true;
            }
            return false;
        }

        public override CharacterObject GetBasicVolunteer(Hero sellerHero)
        {
            Settlement settlement = sellerHero.CurrentSettlement;
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                float power = sellerHero.Power;
                float chance = settlement.IsTown ? power * 0.03f : power * 0.05f;
                float random = MBRandom.RandomFloatRanged(1f, 100f);
                if (data.MilitaryData.NobleManpower > 0 && chance >= random)
                    return sellerHero.Culture.EliteBasicTroop;
                return sellerHero.Culture.BasicTroop;
            }
            return base.GetBasicVolunteer(sellerHero);
        }

        public ExplainedNumber GetDraftEfficiency(Hero hero, int index, Settlement settlement)
        {
            if (hero != null)
            {
                float num = 0.7f;
                int num2 = 0;
                foreach (Town town in hero.CurrentSettlement.MapFaction.Fiefs)
                    num2 += (town.IsTown ? (((town.Settlement.Prosperity < 3000f) ? 1 : ((town.Settlement.Prosperity < 6000f) ? 2 : 3)) + town.Villages.Count<Village>()) : town.Villages.Count<Village>());

                float num3 = (num2 < 46) ? (num2 / 46f * (num2 / 46f)) : 1f;
                num += ((hero.CurrentSettlement != null && num3 < 1f) ? ((1f - num3) * 0.2f) : 0f);
                float baseNumber = 0.75f * MathF.Clamp(MathF.Pow(num, index + 1), 0f, 1f);
                ExplainedNumber explainedNumber = new ExplainedNumber(baseNumber, true);
                Clan clan = hero.Clan;
                if (((clan != null) ? clan.Kingdom : null) != null && hero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.Cantons))
                    explainedNumber.AddFactor(0.2f, new TextObject("Cantons kingdom policy"));

                if (hero.VolunteerTypes != null)
                    if (hero.VolunteerTypes[index] != null && hero.VolunteerTypes[index].IsMounted &&
                        PerkHelper.GetPerkValueForTown(DefaultPerks.Riding.CavalryTactics, settlement.IsVillage ? settlement.Village.TradeBound.Town : settlement.Town))
                        explainedNumber.AddFactor(DefaultPerks.Riding.CavalryTactics.PrimaryBonus * 0.01f, DefaultPerks.Riding.CavalryTactics.PrimaryDescription);

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref explainedNumber, settlement.OwnerClan.Leader, CouncilPosition.Marshall, 0.25f, true);
                DraftPolicy draftPolicy = ((BKDraftPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "draft")).Policy;
                if (draftPolicy == DraftPolicy.Conscription)
                    explainedNumber.Add(0.15f, new TextObject("{=!}Draft policy"));
                else if (draftPolicy == DraftPolicy.Demobilization)
                    explainedNumber.Add(-0.15f, new TextObject("{=!}Draft policy"));

                GovernmentType government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Tribal)
                    explainedNumber.AddFactor(0.2f, new TextObject("{=!}Government"));

                return explainedNumber;
            }
            return new ExplainedNumber(0f);
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

            if (type == PopType.Craftsmen)
            {
                return 0.03f;
            }

            if (type == PopType.Nobles)
            {
                return 0.12f;
            }

            return 0;
        }
    }
}
