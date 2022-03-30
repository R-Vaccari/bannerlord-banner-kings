using BannerKings.Managers.Court;
using Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Utils
{
    public static class UIHelper
    {

		public static List<TooltipProperty> GetHeroCourtTooltip(Hero hero, UsurpData usurp = null, List<Hero> claimants = null)
		{
			List<TooltipProperty> list = new List<TooltipProperty>
			{
				new TooltipProperty("", hero.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
			};
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_relation", null), false);
			string definition = GameTexts.FindText("str_LEFT_ONLY", null).ToString();
			list.Add(new TooltipProperty(definition, ((int)hero.GetRelationWithPlayer()).ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type", null), false);
			string definition2 = GameTexts.FindText("str_LEFT_ONLY", null).ToString();
			list.Add(new TooltipProperty(definition2, GetCorrelation(hero), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			list.Add(new TooltipProperty(new TextObject("{=jaaQijQs}Age").ToString(), hero.Age.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

			if (hero.CurrentSettlement != null)
				list.Add(new TooltipProperty(new TextObject("{=!}Settlement").ToString(), hero.CurrentSettlement.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

			List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero);
			if (titles.Count > 0)
            {
				TooltipAddEmptyLine(list, false);
				list.Add(new TooltipProperty(new TextObject("{=!}Titles", null).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
				TooltipAddSeperator(list, false);
				foreach (FeudalTitle title in titles)
					list.Add(new TooltipProperty(title.name.ToString(), GetOwnership(hero, title), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			}

			if (usurp != null && !usurp.Usurpable)
			{
				TooltipAddEmptyLine(list, false);
				list.Add(new TooltipProperty(new TextObject("{=!}Usurp", null).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
				TooltipAddSeperator(list, false);
				TextObject gold = new TextObject("{=!}{GOLD} coins.");
				gold.SetTextVariable("GOLD", usurp.Gold.ToString("0.0"));

				TextObject influence = new TextObject("{=!}{INFLUENCE} influence.");
				influence.SetTextVariable("INFLUENCE", usurp.Influence.ToString("0.0"));

				TextObject renown = new TextObject("{=!}{RENOWN} renown.");
				renown.SetTextVariable("RENOWN", usurp.Renown.ToString("0.0"));

				list.Add(new TooltipProperty(new TextObject("{=!}Reason").ToString(), usurp.Reason.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
				list.Add(new TooltipProperty(new TextObject("{=!}Gold").ToString(), gold.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
				list.Add(new TooltipProperty(new TextObject("{=!}Influence").ToString(), influence.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
				list.Add(new TooltipProperty(new TextObject("{=!}Renown").ToString(), renown.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			}

			if (claimants != null && claimants.Count > 0)
			{
				TooltipAddEmptyLine(list, false);
				list.Add(new TooltipProperty(new TextObject("{=!}Claimants", null).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
				TooltipAddSeperator(list, false);
				foreach (Hero claimant in claimants)
					list.Add(new TooltipProperty(claimant.Name.ToString(), new TextObject("").ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			}

			return list;
		}

		private static string GetOwnership(Hero hero, FeudalTitle title)
        {
			string ownership = "";
			if (title.deJure == hero && title.deFacto == hero)
				ownership = "Full ownership";
			else if (title.deJure == hero)
				ownership = "De Jure ownership";
			else ownership = "De Facto ownership";

			return ownership;
        }

		private static string GetCorrelation(Hero hero)
        {
			string correlation = "";
			Clan playerClan = Clan.PlayerClan;
			Hero main = Hero.MainHero;
			if (hero.IsNotable)
				correlation = "Notable";
			else if (playerClan.Companions.Contains(hero) && BannerKingsConfig.Instance.TitleManager.IsHeroKnighted(hero))
				correlation = "Knight";
			else if (playerClan.Heroes.Contains(hero) && hero.Father == main || hero.Mother == main || hero.Siblings.Contains(main)
				|| hero.Spouse == main || hero.Children.Contains(main))
				correlation = "Family";
			else if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(hero))
				correlation = "Vassal Lord";

			return correlation;
        }

		public static List<TooltipProperty> GetHeroGovernorEffectsTooltip(Hero hero, CouncilPosition position, float competence)
		{
			List<TooltipProperty> list = new List<TooltipProperty>
			{
				new TooltipProperty("", hero.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
			};
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_relation", null), false);
			string definition = GameTexts.FindText("str_LEFT_ONLY", null).ToString();
			list.Add(new TooltipProperty(definition, ((int)hero.GetRelationWithPlayer()).ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type", null), false);
			string definition2 = GameTexts.FindText("str_LEFT_ONLY", null).ToString();
			list.Add(new TooltipProperty(definition2, HeroHelper.GetCharacterTypeName(hero).ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			list.Add(new TooltipProperty(new TextObject("{=!}Competence").ToString(), UIHelper.FormatValue(competence * 100f), 0, false, TooltipProperty.TooltipPropertyFlags.None));
		
			TooltipAddEmptyLine(list, false);
			list.Add(new TooltipProperty(new TextObject("{=!}Settlement Effects", null).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
			TooltipAddSeperator(list, false);
			List<ValueTuple<string, string>> councilEffects = UIHelper.GetCouncilMemberEffects(position, competence);
			foreach (ValueTuple<string, string> effect in councilEffects)
				list.Add(new TooltipProperty(effect.Item1.ToString(), effect.Item2.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			TooltipAddEmptyLine(list, false);
			return list;
		}

		private static List<ValueTuple<string, string>> GetCouncilMemberEffects(CouncilPosition position, float competence)
        {
			List<ValueTuple<string, string>> lines = new List<ValueTuple<string, string>>();
			if (position == CouncilPosition.Marshall)
            {
				lines.Add(("Militia", FormatDailyValue(1f * competence)));
				lines.Add(("Militarism", FormatValue(3f * competence)));
				lines.Add(("Draft Efficiency", FormatValue(25 * competence)));
			} else if (position == CouncilPosition.Steward) 
			{
				lines.Add(("Prosperity", FormatDailyValue(1f * competence)));
				lines.Add(("Production Efficiency", FormatValue(15f * competence)));
				lines.Add(("Caravan Attractiveness", FormatValue(15f * competence)));
			}
			else if (position == CouncilPosition.Chancellor)
			{
				lines.Add(("Loyalty", FormatDailyValue(1f * competence)));
				lines.Add(("Vassals Limit", FormatDailyValue((int)(4f * competence))));
				lines.Add(("Disagreement Impact", FormatValueNegative(30f * competence)));
			}
			else if (position == CouncilPosition.Spymaster)
			{
				lines.Add(("Security", FormatDailyValue(1f * competence)));
				lines.Add(("Crime Rating", FormatValue(5f * competence)));
				lines.Add(("Settle Issues", FormatValue(3f * competence)));
			}

			return lines;
		}

		private static string FormatValue(float value) => value.ToString("0.00") + '%';
		private static string FormatValueNegative(float value) => '-' + value.ToString("0.00") + '%';
		private static string FormatDailyValue(float value) => '+' + value.ToString("0.00");

		private static void TooltipAddEmptyLine(List<TooltipProperty> properties, bool onlyShowOnExtend = false)
		{
			properties.Add(new TooltipProperty(string.Empty, string.Empty, -1, onlyShowOnExtend, TooltipProperty.TooltipPropertyFlags.None));
		}

		private static void TooltipAddSeperator(List<TooltipProperty> properties, bool onlyShowOnExtend = false)
		{
			properties.Add(new TooltipProperty("", string.Empty, 0, onlyShowOnExtend, TooltipProperty.TooltipPropertyFlags.DefaultSeperator));
		}
	}

	
}
