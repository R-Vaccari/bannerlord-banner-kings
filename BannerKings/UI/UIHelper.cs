using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI;

public static class UIHelper
{
    public static TextObject GetLanguageFluencyText(float fluency)
    {
        TextObject text = null;
        switch (fluency)
        {
            case >= 0.9f:
                text = new TextObject("{=!}Fluent");
                break;
            case >= 0.5f:
                text = new TextObject("{=!}Capable");
                break;
            case >= 0.1f:
                text = new TextObject("{=!}Novice");
                break;
            default:
                text = new TextObject("{=!}Incompetent");
                break;
        }

        return text;
    }

    public static TextObject GetFaithTypeName(Faith faith)
    {
        TextObject text = null;
        if (faith is MonotheisticFaith)
        {
            text = new TextObject("{=!}Monotheism");
        }
        else
        {
            text = new TextObject("{=!}Polytheism");
        }

        return text;
    }

    public static TextObject GetFaithTypeDescription(Faith faith)
    {
        TextObject text = null;
        if (faith is MonotheisticFaith)
        {
            text = new TextObject("{=!}Monotheism");
        }
        else
        {
            text = new TextObject("{=!}Polytheism");
        }

        return text;
    }

    public static List<TooltipProperty> GetPietyTooltip(Religion rel, Hero hero, int piety)
    {
        var model = (BKPietyModel) BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKPietyModel));
        var tooltipForAccumulatingProperty =
            CampaignUIHelper.GetTooltipForAccumulatingProperty(new TextObject("Piety").ToString(), piety,
                model.CalculateEffect(hero));

        TextObject relText = null;
        if (rel == null)
        {
            relText = new TextObject("{=!}You do not currently adhere to any faith");
        }
        else
        {
            relText = new TextObject("{=!}You are following the {FAITH} faith")
                .SetTextVariable("FAITH", rel.Faith.GetFaithName());
        }

        tooltipForAccumulatingProperty.Add(new TooltipProperty("", relText.ToString(), 0));

        return tooltipForAccumulatingProperty;
    }

    public static void ShowSlaveTransferScreen()
    {
        var leftMemberRoster = TroopRoster.CreateDummyTroopRoster();
        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
        var count = (int) (data.GetTypeCount(PopType.Slaves) * data.EconomicData.StateSlaves);
        var playerPrisonerCount = MobileParty.MainParty.PrisonRoster.TotalRegulars;
        var stlmtSlaves = new TroopRoster(null);
        stlmtSlaves.AddToCounts(CharacterObject.All.FirstOrDefault(x => x.StringId == "looter"), count);
        PartyScreenManager.OpenScreenAsLoot(TroopRoster.CreateDummyTroopRoster(), stlmtSlaves,
            Settlement.CurrentSettlement.Name, 0,
            delegate(PartyBase partyBase, TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster,
                PartyBase rightOwnerParty, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster,
                bool fromCancel)
            {
                if (leftPrisonRoster.TotalHeroes > 0)
                {
                    var heroes = new List<CharacterObject>();
                    foreach (var element in leftPrisonRoster.GetTroopRoster())
                    {
                        if (element.Character.IsHero)
                        {
                            heroes.Add(element.Character);
                        }
                    }

                    foreach (var hero in heroes)
                    {
                        leftPrisonRoster.RemoveTroop(hero);
                        rightPrisonRoster.AddToCounts(hero, 1);
                    }
                }

                data.UpdatePopType(PopType.Slaves, leftPrisonRoster.TotalRegulars - count, true);
            });
    }

    public static void ShowActionPopup(BannerKingsAction action, ViewModel vm = null)
    {
        TextObject description = null;
        TextObject affirmativeText = null;
        Hero receiver = null;

        if (action is TitleAction)
        {
            var titleAction = (TitleAction) action;
            affirmativeText = GetActionText(titleAction.Type);

            if (titleAction.Type == ActionType.Grant)
            {
                description = new TextObject(
                    "{=!}Grant this title away to {RECEIVER}, making them the legal owner of it. If the receiver is in your kingdom and the title is landed (attached to a fief), they will also receive the direct ownership of that fief and it's revenue. Granting a title provides positive relations with the receiver.");
                affirmativeText = new TextObject("{=!}Grant");
                var options = new List<InquiryElement>();
                foreach (var hero in BannerKingsConfig.Instance.TitleModel.GetGrantCandidates(action.ActionTaker))
                {
                    options.Add(new InquiryElement(hero, hero.Name.ToString(),
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject))));
                }


                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=!}Grant {TITLE}").SetTextVariable("TITLE", titleAction.Title.FullName).ToString(),
                    new TextObject("{=!}Select a lord who you would like to grant this title to.").ToString(),
                    options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                    delegate(List<InquiryElement> x)
                    {
                        receiver = (Hero?) x[0].Identifier;
                        description.SetTextVariable("RECEIVER", receiver.Name);
                    }, null, string.Empty));
            }
            else if (titleAction.Type == ActionType.Revoke)
            {
                description =
                    new TextObject(
                        "{=!}Revoking transfers the legal ownership of a vassal's title to the suzerain. The revoking restrictions are associated with the title's government type.");
                affirmativeText = new TextObject("{=!}Revoke");
            }
            else if (titleAction.Type == ActionType.Claim)
            {
                description =
                    new TextObject(
                        "{=!}Claiming this title sets a legal precedence for you to legally own it, thus allowing it to be usurped. A claim takes 1 year to build. Claims last until they are pressed or until it's owner dies.");
                affirmativeText = new TextObject("{=!}Claim");
            }
            else
            {
                description =
                    new TextObject(
                        "{=!}Press your claim and usurp this title from it's owner, making you the lawful ruler of this title. Usurping from lords within your kingdom degrades your clan's reputation.");
                affirmativeText = new TextObject("{=!}Usurp");
            }
        }
        else
        {
            var councilAction = (CouncilAction) action;
            var accept = new TextObject("{=!}Will accept: {ANSWER}")
                .SetTextVariable("ANSWER", councilAction.Possible
                    ? GameTexts.FindText("str_yes")
                    : new TextObject("{=!}{NO}.\n{REASON}")
                        .SetTextVariable("NO", GameTexts.FindText("str_no"))
                        .SetTextVariable("REASON", councilAction.Reason));

            if (councilAction.Type == CouncilActionType.REQUEST)
            {
                description =
                    new TextObject(
                            "{=!}Request your liege to grant you this position in the council. This action will cost {INFLUENCE} influence.\n\n{ACCEPT}")
                        .SetTextVariable("INFLUENCE", councilAction.Influence)
                        .SetTextVariable("ACCEPT", accept);
                affirmativeText = new TextObject("{=!}Request");
            }
            else if (councilAction.Type == CouncilActionType.SWAP)
            {
                description =
                    new TextObject(
                            "{=!}Request to swap your current position with {COUNCILMAN} position of {POSITION}. This action will cost {INFLUENCE} influence.\n\n{ACCEPT}")
                        .SetTextVariable("COUNCILMAN", councilAction.TargetPosition.Member.Name)
                        .SetTextVariable("POSITION", councilAction.TargetPosition.GetName())
                        .SetTextVariable("INFLUENCE", councilAction.Influence)
                        .SetTextVariable("ACCEPT", accept);
                affirmativeText = new TextObject("{=!}Swap");
            }
            else
            {
                description =
                    new TextObject(
                            "{=!}Relinquish your position in the council. It will cost no influence and exempt you of any council privileges.\n\n{ACCEPT}")
                        .SetTextVariable("ACCEPT", accept);
                affirmativeText = new TextObject("{=!}Relinquish");
            }
        }


        InformationManager.ShowInquiry(new InquiryData("", description.ToString(),
            action.Possible, true, affirmativeText.ToString(),
            GameTexts.FindText("str_selection_widget_cancel").ToString(), delegate
            {
                action.TakeAction(receiver);
                if (vm != null)
                {
                    vm.RefreshValues();
                }
            }, null, string.Empty));
    }


    public static List<TooltipProperty> GetTitleTooltip(FeudalTitle title, List<TitleAction> actions)
    {
        var hero = title.deJure;
        var list = new List<TooltipProperty>
        {
            new("", hero.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
        };
        MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_relation"));
        var definition = GameTexts.FindText("str_LEFT_ONLY").ToString();
        list.Add(new TooltipProperty(definition, ((int) hero.GetRelationWithPlayer()).ToString(), 0));
        MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
        var definition2 = GameTexts.FindText("str_LEFT_ONLY").ToString();
        list.Add(new TooltipProperty(definition2, GetCorrelation(hero), 0));
        list.Add(new TooltipProperty(new TextObject("{=jaaQijQs}Age").ToString(), hero.Age.ToString(), 0));

        if (hero.CurrentSettlement != null)
        {
            list.Add(new TooltipProperty(new TextObject("{=!}Settlement").ToString(),
                hero.CurrentSettlement.Name.ToString(), 0));
        }

        var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero);
        if (titles.Count > 0)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=!}Titles").ToString(), " ", 0));
            TooltipAddSeperator(list);
            foreach (var t in titles)
            {
                list.Add(new TooltipProperty(t.FullName.ToString(), GetOwnership(hero, t), 0));
            }
        }

        foreach (var action in actions)
        {
            AddActionHint(ref list, action);
        }

        if (title.DeJureDrifts.Count() > 0)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=!}De Jure Drifts").ToString(), " ", 0));
            TooltipAddSeperator(list);

            foreach (var pair in title.DeJureDrifts)
            {
                list.Add(new TooltipProperty(pair.Key.FullName.ToString(), new TextObject("{=!}{PERCENTAGE} complete.")
                    .SetTextVariable("PERCENTAGE", (pair.Value * 100f).ToString("0.000") + '%')
                    .ToString(), 0));
            }
        }

        if (title.OngoingClaims.Count() + title.Claims.Count() > 0)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=!}Claimants").ToString(), " ", 0));
            TooltipAddSeperator(list);
            foreach (var pair in title.OngoingClaims)
            {
                list.Add(new TooltipProperty(pair.Key.Name.ToString(),
                    new TextObject("{=!}{DAYS} days left to build claim.")
                        .SetTextVariable("DAYS", pair.Value.RemainingDaysFromNow)
                        .ToString(), 0));
            }

            foreach (var pair in title.Claims)
            {
                list.Add(new TooltipProperty(pair.Key.Name.ToString(), GetClaimText(pair.Value).ToString(),
                    0));
            }
        }


        var claimants =
            (BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel).GetClaimants(title);
        if (claimants != null && claimants.Count > 0)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=!}Possible Claimants").ToString(), " ", 0));
            TooltipAddSeperator(list);
            foreach (var claimant in claimants)
            {
                list.Add(new TooltipProperty(claimant.Name.ToString(), new TextObject().ToString(), 0));
            }
        }

        return list;
    }

    private static TextObject GetClaimText(ClaimType type)
    {
        if (type == ClaimType.Previous_Owner)
        {
            return new TextObject("{=!}Previous title owner");
        }

        return new TextObject("{=!}Fabricated claim");
    }

    private static TextObject GetActionText(ActionType type)
    {
        if (type == ActionType.Usurp)
        {
            return new TextObject("{=!}Usurp");
        }

        if (type == ActionType.Revoke)
        {
            return new TextObject("{=!}Revoke");
        }

        if (type == ActionType.Claim)
        {
            return new TextObject("{=!}Claim");
        }

        return new TextObject("{=!}Grant");
    }

    private static void AddActionHint(ref List<TooltipProperty> list, TitleAction action)
    {
        TooltipAddEmptyLine(list);
        list.Add(new TooltipProperty(GetActionText(action.Type).ToString(), " ", 0));
        TooltipAddSeperator(list);

        list.Add(new TooltipProperty(new TextObject("{=!}Reason").ToString(), action.Reason.ToString(), 0));
        if (action.Gold > 0)
        {
            list.Add(new TooltipProperty(new TextObject("{=!}Gold").ToString(), new TextObject("{=!}{GOLD} coins.")
                .SetTextVariable("GOLD", action.Gold.ToString("0.0"))
                .ToString(), 0));
        }

        if (action.Influence > 0)
        {
            list.Add(new TooltipProperty(new TextObject("{=!}Influence").ToString(),
                new TextObject("{=!}{INFLUENCE} influence.")
                    .SetTextVariable("INFLUENCE", action.Influence.ToString("0.0"))
                    .ToString(), 0));
        }

        if (action.Renown > 0)
        {
            list.Add(new TooltipProperty(new TextObject("{=!}Influence").ToString(),
                new TextObject("{=!}{RENOWN} renown.")
                    .SetTextVariable("RENOWN", action.Renown.ToString("0.0"))
                    .ToString(), 0));
        }
    }

    public static List<TooltipProperty> GetHeroCourtTooltip(Hero hero)
    {
        var list = new List<TooltipProperty>
        {
            new("", hero.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
        };
        MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_relation"));
        var definition = GameTexts.FindText("str_LEFT_ONLY").ToString();
        list.Add(new TooltipProperty(definition, ((int) hero.GetRelationWithPlayer()).ToString(), 0));
        MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
        var definition2 = GameTexts.FindText("str_LEFT_ONLY").ToString();
        list.Add(new TooltipProperty(definition2, GetCorrelation(hero), 0));
        list.Add(new TooltipProperty(new TextObject("{=jaaQijQs}Age").ToString(), hero.Age.ToString(), 0));

        if (hero.CurrentSettlement != null)
        {
            list.Add(new TooltipProperty(new TextObject("{=!}Settlement").ToString(),
                hero.CurrentSettlement.Name.ToString(), 0));
        }

        var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero);
        if (titles.Count > 0)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=!}Titles").ToString(), " ", 0));
            TooltipAddSeperator(list);
            foreach (var title in titles)
            {
                list.Add(new TooltipProperty(title.FullName.ToString(), GetOwnership(hero, title), 0));
            }
        }

        return list;
    }

    private static string GetOwnership(Hero hero, FeudalTitle title)
    {
        var ownership = "";
        if (title.deJure == hero && title.deFacto == hero)
        {
            ownership = "Full ownership";
        }
        else if (title.deJure == hero)
        {
            ownership = "De Jure ownership";
        }
        else
        {
            ownership = "De Facto ownership";
        }

        return ownership;
    }

    private static string GetCorrelation(Hero hero)
    {
        var correlation = "";
        var playerClan = Clan.PlayerClan;
        var main = Hero.MainHero;
        if (hero.IsNotable)
        {
            correlation = "Notable";
        }
        else if (playerClan.Companions.Contains(hero) && BannerKingsConfig.Instance.TitleManager.IsHeroKnighted(hero))
        {
            correlation = "Knight";
        }
        else if ((playerClan.Heroes.Contains(hero) && hero.Father == main) || hero.Mother == main ||
                 hero.Siblings.Contains(main)
                 || hero.Spouse == main || hero.Children.Contains(main))
        {
            correlation = "Family";
        }
        else if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(hero))
        {
            correlation = "Vassal Lord";
        }

        return correlation;
    }

    public static List<TooltipProperty> GetHeroGovernorEffectsTooltip(Hero hero, CouncilPosition position,
        float competence)
    {
        var list = new List<TooltipProperty>
        {
            new("", hero.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
        };
        MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_relation"));
        var definition = GameTexts.FindText("str_LEFT_ONLY").ToString();
        list.Add(new TooltipProperty(definition, ((int) hero.GetRelationWithPlayer()).ToString(), 0));
        MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
        var definition2 = GameTexts.FindText("str_LEFT_ONLY").ToString();
        list.Add(new TooltipProperty(definition2, HeroHelper.GetCharacterTypeName(hero).ToString(), 0));
        list.Add(new TooltipProperty(new TextObject("{=!}Competence").ToString(), FormatValue(competence * 100f), 0));

        TooltipAddEmptyLine(list);
        list.Add(new TooltipProperty(new TextObject("{=!}Settlement Effects").ToString(), " ", 0));

        TooltipAddEmptyLine(list);
        return list;
    }

    private static string FormatValue(float value)
    {
        return value.ToString("0.00") + '%';
    }

    private static string FormatValueNegative(float value)
    {
        return '-' + value.ToString("0.00") + '%';
    }

    private static string FormatDailyValue(float value)
    {
        return '+' + value.ToString("0.00");
    }

    public static void TooltipAddEmptyLine(List<TooltipProperty> properties, bool onlyShowOnExtend = false)
    {
        properties.Add(new TooltipProperty(string.Empty, string.Empty, -1, onlyShowOnExtend));
    }

    public static void TooltipAddSeperator(List<TooltipProperty> properties, bool onlyShowOnExtend = false)
    {
        properties.Add(new TooltipProperty("", string.Empty, 0, onlyShowOnExtend,
            TooltipProperty.TooltipPropertyFlags.DefaultSeperator));
    }
}