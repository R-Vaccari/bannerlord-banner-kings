using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyWageModel : DefaultPartyWageModel
    {

        public override int GetCharacterWage(int tier)
        {
			int result;
			switch (tier)
			{
				case 0:
					result = 3;
					break;
				case 1:
					result = 6;
					break;
				case 2:
					result = 10;
					break;
				case 3:
					result = 15;
					break;
				case 4:
					result = 26;
					break;
				case 5:
					result = 38;
					break;
				case 6:
					result = 50;
					break;
				default:
					result = 60;
					break;
			}
			return result;
		}

        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
			ExplainedNumber result = base.GetTotalWage(mobileParty, includeDescriptions);

			Hero owner = mobileParty.LeaderHero != null ? mobileParty.LeaderHero : mobileParty.Owner;
			if (owner != null)
            {
				float totalCulture = 0f;
				for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
				{
					TroopRosterElement elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
					if (elementCopyAtIndex.Character.Culture == owner.Culture)
						totalCulture += elementCopyAtIndex.Number;

					if (elementCopyAtIndex.Character.IsHero)
                    {
						if (elementCopyAtIndex.Character.HeroObject == mobileParty.LeaderHero) continue;
						MBReadOnlyList<SkillObject> skills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
						BKCompanionPrices companionModel = new BKCompanionPrices();
						float totalCost = 0f;
						foreach (SkillObject skill in skills)
						{
							float skillValue = elementCopyAtIndex.Character.GetSkillValue(skill);
							if (skillValue > 30) totalCost += skillValue * companionModel.GetCostFactor(skill);
						}

						result.Add(totalCost * 0.005f, elementCopyAtIndex.Character.Name);
					}
				}

				float proportion = MBMath.ClampFloat(totalCulture / (float)mobileParty.MemberRoster.TotalManCount, 0f, 1f);
				if (proportion > 0f)
					result.AddFactor(proportion * -0.1f, GameTexts.FindText("str_culture"));

				
				if (mobileParty.IsGarrison)
					result.Add(result.ResultNumber * - 0.5f, null);
			}

			return result;
        }

        public override int GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
        {
            ExplainedNumber result = new ExplainedNumber(base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost));

			if (buyerHero != null)
            {
				if (Helpers.Helpers.IsRetinueTroop(troop))
					result.AddFactor(0.20f, null);

				if (troop.Culture == buyerHero.Culture)
					result.AddFactor(-0.05f, GameTexts.FindText("str_culture", null));

				if (buyerHero.Clan != null)
				{
					if (buyerHero.CurrentSettlement != null && buyerHero.CurrentSettlement.OwnerClan != null
						&& buyerHero.CurrentSettlement.OwnerClan == buyerHero.Clan)
						result.AddFactor(-0.15f, null);

					if (troop.IsInfantry)
						result.AddFactor(-0.05f, null);

					Kingdom buyerKingdom = buyerHero.Clan.Kingdom;
					if (buyerKingdom != null && troop.Culture != buyerHero.Culture)
						result.AddFactor(0.25f, GameTexts.FindText("str_kingdom", null));
					else result.AddFactor(-0.1f, GameTexts.FindText("str_kingdom", null));

					if (buyerHero.Clan.Tier >= 4)
						result.AddFactor((float)(buyerHero.Clan.Tier - 3) * 0.05f, null);
					else if (buyerHero.Clan.Tier <= 1)
						result.AddFactor((float)(buyerHero.Clan.Tier - 2) * 0.05f, null);
				}
			}

			return (int)result.ResultNumber;
        }
    }
}
