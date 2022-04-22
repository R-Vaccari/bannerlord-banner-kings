using BannerKings.Managers.Court;
using Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Localization;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using BannerKings.Models.BKModels;

namespace BannerKings.UI
{
    public static class UIHelper
    {

		public static void ShowTitleActionPopup(TitleAction action, ViewModel vm = null)
		{
			BKTitleModel model = (BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel);
			TextObject description = null;
			TextObject affirmativeText = GetActionText(action.Type);
			Hero receiver = null;
			if (action.Type == ActionType.Grant)
            {
				description = new TextObject("{=!}Grant this title away to {RECEIVER}, making them the legal owner of it. If the receiver is in your kingdom and the title is landed (attached to a fief), they will also receive the direct ownership of that fief and it's revenue. Granting a title provides positive relations with the receiver.");
				affirmativeText = new TextObject("{=!}Grant");
				List<InquiryElement> options = new List<InquiryElement>();
				foreach (Hero hero in model.GetGrantCandidates(action.ActionTaker))
					options.Add(new InquiryElement(hero, hero.Name.ToString(), new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, false))));


				InformationManager.ShowMultiSelectionInquiry(
					new MultiSelectionInquiryData(
						new TextObject("{=!}Grant {TITLE}").SetTextVariable("TITLE", action.Title.FullName).ToString(), 
						new TextObject("{=!}Select a lord who you would like to grant this title to.").ToString(),
						options, true, 1, GameTexts.FindText("str_done", null).ToString(), string.Empty,
						new Action<List<InquiryElement>>(delegate (List<InquiryElement> x)
						{
							receiver = (Hero?)x[0].Identifier;
							description.SetTextVariable("RECEIVER", receiver.Name);
						}), null, string.Empty), false);
			}
			else if (action.Type == ActionType.Revoke)
            {
				description = new TextObject("{=!}Revoking transfers the legal ownership of a vassal's title to the suzerain. The revoking restrictions are associated with the title's government type.");
				affirmativeText = new TextObject("{=!}Revoke");
			} else if (action.Type == ActionType.Claim)
            {
				description = new TextObject("{=!}Claiming this title sets a legal precedence for you to legally own it, thus allowing it to be usurped. A claim takes 1 year to build. Claims last until they are pressed or until it's owner dies.");
				affirmativeText = new TextObject("{=!}Claim");
			}
			else
            {
				description = new TextObject("{=!}Press your claim and usurp this title from it's owner, making you the lawful ruler of this title. Usurping from lords within your kingdom degrades your clan's reputation.");
				affirmativeText = new TextObject("{=!}Usurp");
			}

			InformationManager.ShowInquiry(new InquiryData("", description.ToString(),
				true, true, affirmativeText.ToString(), "Cancel", delegate 
				{ 
					action.TakeAction(receiver);
					if (vm != null) vm.RefreshValues();
				}, null, string.Empty));
		}

		public static List<TooltipProperty> GetTitleTooltip(FeudalTitle title, List<TitleAction> actions)
		{
			Hero hero = title.deJure;
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
				foreach (FeudalTitle t in titles)
					list.Add(new TooltipProperty(t.FullName.ToString(), GetOwnership(hero, t), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			}

			foreach (TitleAction action in actions)
				AddActionHint(ref list, action);

			if (title.DeJureDrifts.Count() > 0)
            {
				TooltipAddEmptyLine(list, false);
				list.Add(new TooltipProperty(new TextObject("{=!}De Jure Drifts", null).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
				TooltipAddSeperator(list, false);

				foreach (KeyValuePair<FeudalTitle, float> pair in title.DeJureDrifts)
					list.Add(new TooltipProperty(pair.Key.FullName.ToString(), new TextObject("{=!}{PERCENTAGE} complete.")
						.SetTextVariable("PERCENTAGE", pair.Value.ToString("0.00") + '%')
						.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

			}

			if (title.OngoingClaims.Count() + title.Claims.Count() > 0)
            {
				TooltipAddEmptyLine(list, false);
				list.Add(new TooltipProperty(new TextObject("{=!}Claimants", null).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
				TooltipAddSeperator(list, false);
				foreach (KeyValuePair<Hero, CampaignTime> pair in title.OngoingClaims)
					list.Add(new TooltipProperty(pair.Key.Name.ToString(), new TextObject("{=!}{DAYS} days left to build claim.")
						.SetTextVariable("DAYS", pair.Value.RemainingDaysFromNow)
						.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

				foreach (KeyValuePair<Hero, ClaimType> pair in title.Claims)
					list.Add(new TooltipProperty(pair.Key.Name.ToString(), GetClaimText(pair.Value).ToString(), 
						0, false, TooltipProperty.TooltipPropertyFlags.None));
			}


			List<Hero> claimants = (BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel).GetClaimants(title);
			if (claimants != null && claimants.Count > 0)
			{
				TooltipAddEmptyLine(list, false);
				list.Add(new TooltipProperty(new TextObject("{=!}Possible Claimants", null).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
				TooltipAddSeperator(list, false);
				foreach (Hero claimant in claimants)
					list.Add(new TooltipProperty(claimant.Name.ToString(), new TextObject("").ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			}

			return list;
		}

		private static TextObject GetClaimText(ClaimType type)
        {
			if (type == ClaimType.Previous_Owner)
				return new TextObject("{=!}Previous title owner");
			else return new TextObject("{=!}Fabricated claim");
        }

		private static TextObject GetActionText(ActionType type)
        {
			if (type == ActionType.Usurp)
				return new TextObject("{=!}Usurp", null);
			else if (type == ActionType.Revoke)
				return new TextObject("{=!}Revoke", null);
			else if (type == ActionType.Claim)
				return new TextObject("{=!}Claim", null);
			else return new TextObject("{=!}Grant", null);
		}

		private static void AddActionHint(ref List<TooltipProperty> list, TitleAction action)
        {
			TooltipAddEmptyLine(list, false);
			list.Add(new TooltipProperty(GetActionText(action.Type).ToString(), " ", 0, false, TooltipProperty.TooltipPropertyFlags.None));
			TooltipAddSeperator(list, false);

			list.Add(new TooltipProperty(new TextObject("{=!}Reason").ToString(), action.Reason.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
			if (action.Gold > 0)
				list.Add(new TooltipProperty(new TextObject("{=!}Gold").ToString(), new TextObject("{=!}{GOLD} coins.")
					.SetTextVariable("GOLD", action.Gold.ToString("0.0"))
					.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

			if (action.Influence > 0)
				list.Add(new TooltipProperty(new TextObject("{=!}Influence").ToString(), new TextObject("{=!}{INFLUENCE} influence.")
					.SetTextVariable("INFLUENCE", action.Influence.ToString("0.0"))
					.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

			if (action.Renown > 0)
				list.Add(new TooltipProperty(new TextObject("{=!}Influence").ToString(), new TextObject("{=!}{RENOWN} renown.")
					.SetTextVariable("RENOWN", action.Renown.ToString("0.0"))
					.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

		}

		public static List<TooltipProperty> GetHeroCourtTooltip(Hero hero)
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
					list.Add(new TooltipProperty(title.FullName.ToString(), GetOwnership(hero, title), 0, false, TooltipProperty.TooltipPropertyFlags.None));
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
