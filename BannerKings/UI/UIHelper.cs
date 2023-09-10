using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Behaviours.PartyNeeds;
using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
using BannerKings.UI.TownManagement;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI
{
    public static class UIHelper
    {
        public static string GetWorkshopIconText(string workshopType)
        {
            if (workshopType == "weaponsmithy" || workshopType == "barding-smithy" || workshopType == "armorsmithy" || workshopType == "mines")
            {
                return "smithy";
            }

            if (workshopType == "fletcher")
            {
                return "wood_WorkshopType";
            }

            if (workshopType == "bakery")
            {
                return "pottery_shop";
            }

            if (workshopType == "butcher")
            {
                return "tannery";
            }

            if (workshopType == "meadery")
            {
                return "brewery";
            }

            return workshopType;
        }

        public static TextObject GetLanguageFluencyText(float fluency)
        {
            var text = fluency switch
            {
                >= 0.9f => new TextObject("{=KkbAZK46}Fluent"),
                >= 0.5f => new TextObject("{=Zwkc1Qbx}Capable"),
                >= 0.1f => new TextObject("{=oJCm9gDS}Novice"),
                _ => new TextObject("{=DAp3eXTr}Incompetent")
            };

            return text;
        }

        public static TextObject GetFaithTypeName(Faith faith)
        {
            TextObject text = null;
            if (faith is MonotheisticFaith)
            {
                text = new TextObject("{=x5cTibqS}Monotheism");
            }
            else
            {
                text = new TextObject("{=FUnQKZ8K}Polytheism");
            }

            return text;
        }

        public static TextObject GetFaithTypeDescription(Faith faith)
        {
            TextObject text = null;
            if (faith is MonotheisticFaith)
            {
                text = new TextObject("{=x5cTibqS}Monotheism");
            }
            else
            {
                text = new TextObject("{=FUnQKZ8K}Polytheism");
            }

            return text;
        }

        public static List<TooltipProperty> GetTownMaterialTooltip(MaterialItemVM material)
        {
            List<TooltipProperty> properties = new List<TooltipProperty>();
            properties.Add(new TooltipProperty(material.Material.Name.ToString() + "        ",
                string.Empty,
                0,
                onlyShowWhenExtended: false,
                TooltipProperty.TooltipPropertyFlags.Title));

            properties.Add(new TooltipProperty(string.Empty,
                GameTexts.FindText("str_bk_description", material.Material.StringId).ToString(),
                0,
                false,
                TooltipProperty.TooltipPropertyFlags.MultiLine));

            TooltipAddEmptyLine(properties);
            properties.Add(new TooltipProperty(new TextObject("{=zyG8TxaZ}Availability").ToString(), " ", 0));
            TooltipAddSeperator(properties);

            properties.Add(new TooltipProperty(new TextObject("{=iWcM5S0d}Stash").ToString(),
                material.StashAmount.ToString(),
                0));

            properties.Add(new TooltipProperty(new TextObject("{=QVOpQQ9V}Market").ToString(),
                material.MarketAmount.ToString(),
                0));

            Settlement source = SettlementHelper.FindNearestVillage(village =>
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village);
                return BannerKingsConfig.Instance.PopulationManager.GetProductions(data)
                .Any(production => production.Item1 == material.Material);
            });

            if (source != null)
            {
                properties.Add(new TooltipProperty(new TextObject("{=DKd1O8fp}Closest source").ToString(),
                               source.Name.ToString(),
                               0));
            }

            return properties;
        }

        public static List<TooltipProperty> GetEncyclopediaWorkshopTooltip(Workshop workshop)
        {
            List<TooltipProperty> properties = new List<TooltipProperty>();
            properties.Add(new TooltipProperty(workshop.Name.ToString() + "        ",
                string.Empty,
                0,
                onlyShowWhenExtended: false,
                TooltipProperty.TooltipPropertyFlags.Title));

            Hero hero = workshop.Owner;
            properties.Add(new TooltipProperty(new TextObject("{=qRqnrtdX}Owner").ToString(), 
                hero.Name.ToString(), 0));
            properties.Add(new TooltipProperty(GameTexts.FindText("str_enc_sf_occupation").ToString(),
                CampaignUIHelper.GetHeroOccupationName(hero), 0));
            if (workshop.Owner.Clan != null)
            {
                properties.Add(new TooltipProperty(new TextObject("{=j4F7tTzy}Clan").ToString(), 
                    hero.Clan.Name.ToString(), 0));
            }

            properties.Add(new TooltipProperty(new TextObject("Capital").ToString(),
                    MBRandom.RoundRandomized(workshop.Capital).ToString(), 0));

            ExplainedNumber result = BannerKingsConfig.Instance.WorkshopModel.GetBuyingCostExplained(workshop, Hero.MainHero, true);
            TooltipAddEmptyLine(properties);

            properties.Add(new TooltipProperty(new TextObject("{=f7t4saJu}Value").ToString(), " ", 0));
            properties.Add(new TooltipProperty("", string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));

            var explanation = CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult(String.Empty,
                MBRandom.RoundRandomized(result.ResultNumber),
                ref result);
            explanation.RemoveAt(0);
            properties.AddRange(explanation);

            return properties;
        }

        public static List<TooltipProperty> GetCouncilPositionTooltip(CouncilMember position)
        {
            List<TooltipProperty> properties = new List<TooltipProperty>();
            properties.Add(new TooltipProperty(position.Name.ToString() + "        ", 
                string.Empty, 
                0, 
                onlyShowWhenExtended: false, 
                TooltipProperty.TooltipPropertyFlags.Title));

            properties.Add(new TooltipProperty(string.Empty,
                position.Description.ToString(),
                0,
                false,
                TooltipProperty.TooltipPropertyFlags.MultiLine));

            TooltipAddEmptyLine(properties);
            properties.Add(new TooltipProperty(new TextObject("{=HsgYtcxu}Competence(s)").ToString(), " ", 0));
            TooltipAddSeperator(properties);

            properties.Add(new TooltipProperty(new TextObject("{=PcM7JDMu}Primary").ToString(),
                position.PrimarySkill.ToString(),
                0));

            if (position.SecondarySkill != null)
            {
                properties.Add(new TooltipProperty(new TextObject("{=sPsERGcn}Secondary").ToString(),
                                position.SecondarySkill.ToString(),
                                0));
            }

            TooltipAddEmptyLine(properties);
            properties.Add(new TooltipProperty(new TextObject("{=77D4i3pG}Privileges").ToString(), " ", 0));
            TooltipAddSeperator(properties);

            foreach (var privilege in position.AllPrivileges)
            {
                properties.Add(new TooltipProperty(GameTexts.FindText("str_bk_council_privilege", privilege.ToString().ToLower()).ToString(),
                    " ", 
                    0));
            }

            return properties;
        }

        public static List<TooltipProperty> GetPietyTooltip(Managers.Institutions.Religions.Religion rel, Hero hero, int piety)
        {
            var model = BannerKingsConfig.Instance.ReligionModel;
            var tooltipForAccumulatingProperty =
                CampaignUIHelper.GetTooltipForAccumulatingProperty(new TextObject("{=0EVuzzOU}Piety").ToString(), piety,
                    model.CalculatePietyChange(hero, true));

            TextObject relText = null;
            if (rel == null)
            {
                relText = new TextObject("{=jxXHurYn}You do not currently adhere to any faith");
            }
            else
            {
                relText = new TextObject("{=hK6ZeAcC}You are following the {FAITH} faith")
                    .SetTextVariable("FAITH", rel.Faith.GetFaithName());
            }

            tooltipForAccumulatingProperty.Add(new TooltipProperty("", relText.ToString(), 0));

            return tooltipForAccumulatingProperty;
        }

        public static void ShowSlaveTransferScreen()
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            var count = (int) (data.GetTypeCount(PopType.Slaves) * data.EconomicData.StateSlaves);

            var stlmtSlaves = new TroopRoster(null);
            stlmtSlaves.AddToCounts(CharacterObject.All.FirstOrDefault(x => x.StringId == "looter"), count);

            PartyScreenManager.OpenScreenAsLoot(TroopRoster.CreateDummyTroopRoster(), stlmtSlaves,
                Settlement.CurrentSettlement.Name, 0,
                delegate(PartyBase _, TroopRoster _, TroopRoster leftPrisonRoster,
                    PartyBase _, TroopRoster _, TroopRoster rightPrisonRoster,
                    bool _)
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

        public static void ShowSlaveDonationScreen(Hero notable)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            var stlmtSlaves = new TroopRoster(null);

            var slavesResult = 0f;
            PartyScreenManager.OpenScreenAsLoot(TroopRoster.CreateDummyTroopRoster(), stlmtSlaves,
                Settlement.CurrentSettlement.Name, 0,
                delegate (PartyBase _, TroopRoster _, TroopRoster leftPrisonRoster,
                    PartyBase _, TroopRoster _, TroopRoster rightPrisonRoster,
                    bool _)
                {
                    slavesResult = leftPrisonRoster.TotalRegulars;
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

                    data.UpdatePopType(PopType.Slaves, leftPrisonRoster.TotalRegulars, false);
                });

            if (slavesResult > 0f)
            {
                GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, slavesResult * 0.05f);
                int relation = (int)(slavesResult / 50f);
                if (relation > 0)
                {
                    ChangeRelationAction.ApplyPlayerRelation(notable, relation, false, true);
                }
            }
        }

        public static void ShowActionPopup(BannerKingsAction action, ViewModel vm = null)
        {
            TextObject description = null;
            TextObject affirmativeText = null;
            Hero receiver = null;

            if (action is TitleAction titleAction)
            {
                affirmativeText = GetActionText(titleAction.Type);

                switch (titleAction.Type)
                {
                    case ActionType.Grant:
                    {
                        description = new TextObject("{=d4p6yHON}Grant this title away to {RECEIVER}, making them the legal owner of it. If the receiver is in your kingdom and the title is landed (attached to a fief), they will also receive the direct ownership of that fief and it's revenue. Granting a title provides positive relations with the receiver.");
                        affirmativeText = new TextObject("{=dugq4xHo}Grant");
                        var options = new List<InquiryElement>();
                        foreach (var hero in BannerKingsConfig.Instance.TitleModel.GetGrantCandidates(titleAction.ActionTaker))
                        {
                            options.Add(new InquiryElement(hero, hero.Name.ToString(),
                                new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject))));
                        }

                        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                            new TextObject("{=dugq4xHo}Grant {TITLE}").SetTextVariable("TITLE", titleAction.Title.FullName).ToString(),
                            new TextObject("{=hzwZeQyE}Select a lord who you would like to grant this title to.").ToString(),
                            options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                            delegate(List<InquiryElement> x)
                            {
                                receiver = (Hero) x[0].Identifier;
                                description.SetTextVariable("RECEIVER", receiver.Name);
                            }, null, string.Empty));
                        break;
                    }
                    case ActionType.Revoke:
                        description =
                            new TextObject("{=2kxQvCVb}Revoking transfers the legal ownership of a vassal's title to the suzerain. The revoking restrictions are associated with the title's government type.");
                        affirmativeText = new TextObject("{=iLpAKttu}Revoke");
                        break;
                    case ActionType.Claim:
                        description =
                            new TextObject("{=BSX8rvCS}Claiming this title sets a legal precedence for you to legally own it, thus allowing it to be usurped. A claim takes 1 year to build. Claims last until they are pressed or until it's owner dies.");
                        affirmativeText = new TextObject("{=6hY9WysN}Claim");
                        break;
                    default:
                        description =
                            new TextObject("{=6cxTA139}Press your claim and usurp this title from it's owner, making you the lawful ruler of this title. Usurping from lords within your kingdom degrades your clan's reputation.");
                        affirmativeText = new TextObject("{=L3Jzg76z}Usurp");
                        break;
                }
            }

            InformationManager.ShowInquiry(new InquiryData(string.Empty, 
                description.ToString(),
                action.Possible, 
                true, 
                affirmativeText.ToString(),
                GameTexts.FindText("str_selection_widget_cancel").ToString(), 
                delegate
                {
                    action.TakeAction(receiver);
                    vm?.RefreshValues();
                }, 
                null, 
                string.Empty));
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

            list.Add(new TooltipProperty(new TextObject("{=j4F7tTzy}Clan").ToString(), hero.Clan.Name.ToString(), 0));

            list.Add(new TooltipProperty(new TextObject("{=uUmEcuV8}Age").ToString(), 
                MBRandom.RoundRandomized(hero.Age).ToString(), 0));

            TooltipAddEmptyLine(list);

            var claimants = BannerKingsConfig.Instance.TitleModel.GetClaimants(title);
            if (claimants.Count > 0)
            {
                list.Add(new TooltipProperty(new TextObject("{=nFcAkRcD}Possible Claimants").ToString(), " ", 0));
                TooltipAddSeperator(list);

                list.AddRange(claimants.Select(claimant => new TooltipProperty(claimant.Key.Name.ToString(),
                        claimant.Value.ToString(),
                        0)));

                TooltipAddEmptyLine(list);
            }

            list.Add(new TooltipProperty(new TextObject("{=T39jWYUx}Actions").ToString(), " ", 0));
            TooltipAddSeperator(list);
            list.Add(new TooltipProperty(string.Empty,
                new TextObject("{=rfwjouvE}Title actions allow you multiple ways to use them. Actions that actively undermine other lords are considered hostile actions, and often cost denars, influence, your clan's renown, and relations with the affected, so take them wisely. On the other hand, an action such as Grant of a title is considered amicable and grows relations instead, at the cost of your ownership of the title.").ToString(),
                0,
                false,
                TooltipProperty.TooltipPropertyFlags.MultiLine));

            foreach (var action in actions)
            {
                AddActionHint(ref list, action);
            }

            if (title.DeJureDrifts.Any())
            {
                TooltipAddEmptyLine(list);
                list.Add(new TooltipProperty(new TextObject("{=C99O2rGX}De Jure Drifts").ToString(), " ", 0));
                TooltipAddSeperator(list);

                foreach (var pair in title.DeJureDrifts)
                {
                    list.Add(new TooltipProperty(pair.Key.FullName.ToString(), new TextObject("{=qODLryGe}{PERCENTAGE} complete.")
                        .SetTextVariable("PERCENTAGE", (pair.Value * 100f).ToString("0.000") + '%')
                        .ToString(), 0));
                }
            }

            if (title.OngoingClaims.Count + title.Claims.Count > 0)
            {
                TooltipAddEmptyLine(list);
                list.Add(new TooltipProperty(new TextObject("{=6hY9WysN}Claimants").ToString(), " ", 0));
                TooltipAddSeperator(list);

                foreach (var pair in title.OngoingClaims)
                {
                    list.Add(new TooltipProperty(pair.Key.Name.ToString(),
                        new TextObject("{=CW56ko9B}{DAYS} days left to build claim.")
                            .SetTextVariable("DAYS", pair.Value.RemainingDaysFromNow)
                            .ToString(), 0));
                }

                list.AddRange(title.Claims.Select(pair => new TooltipProperty(pair.Key.Name.ToString(), GetClaimText(pair.Value).ToString(), 0)));
            }

            return list;
        }

        private static TextObject GetClaimText(ClaimType type)
        {
            if (type == ClaimType.Previous_Owner)
            {
                return new TextObject("{=NOYOqKSV}Previous title owner");
            }

            return new TextObject("{=0KERfXox}Fabricated claim");
        }

        private static TextObject GetActionText(ActionType type)
        {
            return type switch
            {
                ActionType.Usurp => new TextObject("{=L3Jzg76z}Usurp"),
                ActionType.Revoke => new TextObject("{=iLpAKttu}Revoke"),
                ActionType.Claim => new TextObject("{=6hY9WysN}Claim"),
                _ => new TextObject("{=dugq4xHo}Grant")
            };
        }

        private static void AddActionHint(ref List<TooltipProperty> list, TitleAction action)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(GetActionText(action.Type).ToString(), " ", 0));
            list.Add(new TooltipProperty("", string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));

            list.Add(new TooltipProperty(new TextObject("{=n4LgwLxB}Reason").ToString(), action.Reason.ToString(), 0));
            if (action.Gold > 0)
            {
                list.Add(new TooltipProperty(new TextObject("{=AsR4WzYu}Denars:").ToString(), new TextObject("{=7rA02JY3}{GOLD} coins.")
                    .SetTextVariable("GOLD", action.Gold.ToString("0.0"))
                    .ToString(), 0));
            }

            if (action.Influence > 0)
            {
                list.Add(new TooltipProperty(new TextObject("{=H0mrKw6B}Influence:").ToString(),
                    new TextObject("{=bqXrF5SC}{INFLUENCE} influence.")
                        .SetTextVariable("INFLUENCE", action.Influence.ToString("0.0"))
                        .ToString(), 0));
            }

            if (action.Renown > 0)
            {
                list.Add(new TooltipProperty(new TextObject("{=NqSLYe6b}Renown:").ToString(),
                    new TextObject("{=bW8pmr9u}{RENOWN} renown.")
                        .SetTextVariable("RENOWN", action.Renown.ToString("0.0"))
                        .ToString(), 0));
            }
        }

        public static void AddMoraleSuppliesHint(ref List<TooltipProperty> list, PartySupplies supplies)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=SaWHeTiw}Supplies").ToString(), " ", 0));
            TooltipAddSeperator(list);
            list.Add(new TooltipProperty(string.Empty,
              new TextObject("{=SaWHeTiw}Supplies such as alcohol and wood for firewood are required to preserve your retinue in good mental condition.").ToString(),
              0,
              false,
              TooltipProperty.TooltipPropertyFlags.MultiLine));

            list.Add(new TooltipProperty("", string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
            list.Add(new TooltipProperty(new TextObject("{=ex269b0j}Wood:").ToString(), 
                MBRandom.RoundRandomized(supplies.WoodNeed).ToString(), 
                0));
            list.Add(new TooltipProperty(new TextObject("{=yw4KTjMj}Textiles:").ToString(),
                MBRandom.RoundRandomized(supplies.ClothNeed).ToString(),
                0));
            list.Add(new TooltipProperty(new TextObject("{=1y4e5t97}Alcohol:").ToString(),
               MBRandom.RoundRandomized(supplies.AlcoholNeed).ToString(),
               0));
            list.Add(new TooltipProperty(new TextObject("{=xcEes2qY}Animal Products:").ToString(),
                MBRandom.RoundRandomized(supplies.AnimalProductsNeed).ToString(),
                0));
        }

        public static void AddWageSuppliesHint(ref List<TooltipProperty> list, PartySupplies supplies)
        {
            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=qBXbGjp1}Daily Supplies").ToString(), " ", 0));
            TooltipAddSeperator(list);
            list.Add(new TooltipProperty(string.Empty,
              new TextObject("{=SaWHeTiw}Supplies are required to preserve your retinue in maximum efficiency. Your quartermaster is able to handle supplies if you instruct them directly.").ToString(),
              0,
              false,
              TooltipProperty.TooltipPropertyFlags.MultiLine));

            list.Add(new TooltipProperty("", string.Empty, 0, false, TooltipProperty.TooltipPropertyFlags.RundownSeperator));

            list.Add(new TooltipProperty(new TextObject("{=j8R1v8fv}Weapons:").ToString(),
                supplies.GetWeaponsCurrentNeed().ResultNumber.ToString("0.00"),
               0));
            list.Add(new TooltipProperty(new TextObject("{=x4KhXV25}Ammunition:").ToString(),
                supplies.GetArrowsCurrentNeed().ResultNumber.ToString("0.00"),
                0));
            list.Add(new TooltipProperty(new TextObject("{=GgdSMucS}Mounts:").ToString(),
                supplies.GetMountsCurrentNeed().ResultNumber.ToString("0.00"),
              0));
            list.Add(new TooltipProperty(new TextObject("{=JC5JZRjY}Shields:").ToString(),
                supplies.GetShieldsCurrentNeed().ResultNumber.ToString("0.00"),
              0));
            list.Add(new TooltipProperty(new TextObject("{=1y4e5t97}Alcohol:").ToString(),
                supplies.GetAlcoholCurrentNeed().ResultNumber.ToString("0.00"),
               0));
            list.Add(new TooltipProperty(new TextObject("{=xcEes2qY}Animal Products:").ToString(),
                supplies.GetAnimalProductsCurrentNeed().ResultNumber.ToString("0.00"),
                0));
            list.Add(new TooltipProperty(new TextObject("{=ex269b0j}Wood:").ToString(),
                supplies.GetWoodCurrentNeed().ResultNumber.ToString("0.00"),
              0));
            list.Add(new TooltipProperty(new TextObject("{=yw4KTjMj}Textiles:").ToString(),
                supplies.GetTextileCurrentNeed().ResultNumber.ToString("0.00"),
                0));
            list.Add(new TooltipProperty(new TextObject("{=9C4tmyos}Tools:").ToString(),
                supplies.GetToolsCurrentNeed().ResultNumber.ToString("0.00"),
                0));
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
            list.Add(new TooltipProperty(new TextObject("{=uUmEcuV8}Age").ToString(), hero.Age.ToString(), 0));

            if (hero.CurrentSettlement != null)
            {
                list.Add(new TooltipProperty(new TextObject("{=J6oPqQmt}Settlement").ToString(),
                    hero.CurrentSettlement.Name.ToString(), 0));
            }

            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero);
            if (titles.Count > 0)
            {
                TooltipAddEmptyLine(list);
                list.Add(new TooltipProperty(new TextObject("{=2qXtnwSn}Titles").ToString(), " ", 0));
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

        public static List<TooltipProperty> GetHeroGovernorEffectsTooltip(Hero hero, CouncilMember position,
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
            list.Add(new TooltipProperty(new TextObject("{=RMUyXy4e}Competence").ToString(), FormatValue(competence * 100f), 0));

            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=J6oPqQmt}Settlement Effects").ToString(), " ", 0));

            TooltipAddEmptyLine(list);
            return list;
        }

        public static List<TooltipProperty> GetRecruitToolTip(CharacterObject character, Hero owner, int relation, bool canRecruit)
        {
            List<TooltipProperty> list = new List<TooltipProperty>();

            if (!canRecruit)
            {
                string text = "";
                list.Add(new TooltipProperty(text, character.ToString(), 1, false, TooltipProperty.TooltipPropertyFlags.None));
                list.Add(new TooltipProperty(text, text, -1, false, TooltipProperty.TooltipPropertyFlags.None));
                GameTexts.SetVariable("LEVEL", character.Level);
                GameTexts.SetVariable("newline", "\n");
                list.Add(new TooltipProperty(text, GameTexts.FindText("str_level_with_value", null).ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

                list.Add(new TooltipProperty(text, new TextObject("{=xa13n63V}You don't have access to this recruit.").ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.None));

                var explanation = BannerKingsConfig.Instance.VolunteerModel.CalculateMaximumRecruitmentIndex(Hero.MainHero, owner, relation, true);
                TooltipAddEmptyLine(list);
                list.Add(new TooltipProperty(new TextObject("{=vqP5R9TS}Explanations").ToString(), " ", 0));
                TooltipAddSeperator(list);

                list.Add(new TooltipProperty(text, explanation.GetExplanations(), 0, false, TooltipProperty.TooltipPropertyFlags.None));
            }

            return list;

        }

        public static List<TooltipProperty> GetHeirTooltip(Hero hero, ExplainedNumber score)
        {
            var list = new List<TooltipProperty>
            {
                new("", hero.Name.ToString(), 0, false, TooltipProperty.TooltipPropertyFlags.Title)
            };
            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_clan"));
            var definition = GameTexts.FindText("str_LEFT_ONLY").ToString();
            list.Add(new TooltipProperty(hero.Clan.Name.ToString(), string.Empty, 0));
            MBTextManager.SetTextVariable("LEFT", GameTexts.FindText("str_tooltip_label_type"));
            var definition2 = GameTexts.FindText("str_LEFT_ONLY").ToString();
            list.Add(new TooltipProperty(definition2, HeroHelper.GetCharacterTypeName(hero).ToString(), 0));
        

            TooltipAddEmptyLine(list);
            list.Add(new TooltipProperty(new TextObject("{=NeydSXjc}Score").ToString(), " ", 0));
            TooltipAddSeperator(list);
            list.Add(new TooltipProperty(string.Empty, score.GetExplanations(), 0));


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
}