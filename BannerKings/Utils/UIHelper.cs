using BannerKings.Managers.Court;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;

namespace BannerKings.Utils
{
    public static class UIHelper
    {

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
				lines.Add(("Militia", FormatDailyValue(1.5f * competence)));
				lines.Add(("Militarism", FormatValue(3f * competence)));
				lines.Add(("Draft Efficiency", FormatValue(25 * competence)));
			} else if (position == CouncilPosition.Steward) 
			{
				lines.Add(("Prosperity", FormatDailyValue(1.5f * competence)));
				lines.Add(("Production Efficiency", FormatValue(15f * competence)));
				lines.Add(("Caravan Attractiveness", FormatValue(15f * competence)));
			}
			else if (position == CouncilPosition.Chancellor)
			{
				lines.Add(("Loyalty", FormatDailyValue(1.5f * competence)));
				lines.Add(("Vassals Limit", FormatDailyValue((int)(4f * competence))));
				lines.Add(("Disagreement Impact", FormatValueNegative(30f * competence)));
			}
			else if (position == CouncilPosition.Spymaster)
			{
				lines.Add(("Security", FormatDailyValue(1.5f * competence)));
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
