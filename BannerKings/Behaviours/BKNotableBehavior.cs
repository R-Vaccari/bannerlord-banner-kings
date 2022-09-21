﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.UI;
using BannerKings.Utils;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKNotableBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnGovernorChangedEvent.AddNonSerializedListener(this, OnGovernorChanged);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddDialogue(starter);
        }

        private void OnGovernorChanged(Town town, Hero oldGovernor, Hero newGovernor)
        {
            if (oldGovernor == null || !oldGovernor.IsNotable)
            {
                return;
            }

            var owner = town.OwnerClan.Leader;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(owner, oldGovernor, -10);
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement.Town == null || settlement.OwnerClan == null)
            {
                return;
            }

            if (settlement.IsCastle)
            {
                SettlementHelper.SpawnNotablesIfNeeded(settlement);
                UpdateVolunteers(settlement);
            }

            var governor = settlement.Town.Governor;
            if (governor == null || !governor.IsNotable)
            {
                return;
            }

            if (MBRandom.RandomInt(1, 100) < 5)
            {
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.Town.OwnerClan.Leader, governor, 1);
            }
        }

        private void UpdateVolunteers(Settlement settlement)
        {
            if (settlement.Notables.Count == 0 || settlement.Notables[0].IsDead)
            {
                return;
            }

            var hero = settlement.Notables[0];
            if (!hero.CanHaveRecruits)
            {
                return;
            }

            var flag = false;
            var basicVolunteer = Campaign.Current.Models.VolunteerModel.GetBasicVolunteer(hero);
            for (var i = 0; i < 6; i++)
            {
                if (!(MBRandom.RandomFloat < Campaign.Current.Models.VolunteerModel.GetDailyVolunteerProductionProbability(hero, i, settlement)))
                {
                    continue;
                }

                var characterObject = hero.VolunteerTypes[i];
                if (characterObject == null)
                {
                    hero.VolunteerTypes[i] = basicVolunteer;
                    flag = true;
                }
                else if (characterObject.UpgradeTargets.Length != 0 && characterObject.Tier <= 3)
                {
                    var num = MathF.Log(hero.Power / (float)characterObject.Tier, 2f) * 0.01f;
                    if (!(MBRandom.RandomFloat < num))
                    {
                        continue;
                    }

                    hero.VolunteerTypes[i] = characterObject.UpgradeTargets[MBRandom.RandomInt(characterObject.UpgradeTargets.Length)];
                    flag = true;
                }
            }

            if (!flag)
            {
                return;
            }

            var volunteerTypes = hero.VolunteerTypes;
            for (var j = 1; j < 6; j++)
            {
                var characterObject2 = volunteerTypes[j];
                if (characterObject2 == null)
                {
                    continue;
                }

                var num2 = 0;
                var num3 = j - 1;
                var characterObject3 = volunteerTypes[num3];
                while (num3 >= 0 && (characterObject3 == null || (float)characterObject2.Level + (characterObject2.IsMounted ? 0.5f : 0f) < (float)characterObject3.Level + (characterObject3.IsMounted ? 0.5f : 0f)))
                {
                    if (characterObject3 == null)
                    {
                        num3--;
                        num2++;
                        if (num3 >= 0)
                        {
                            characterObject3 = volunteerTypes[num3];
                        }
                    }
                    else
                    {
                        volunteerTypes[num3 + 1 + num2] = characterObject3;
                        num3--;
                        num2 = 0;
                        if (num3 >= 0)
                        {
                            characterObject3 = volunteerTypes[num3];
                        }
                    }
                }
                volunteerTypes[num3 + 1 + num2] = characterObject2;
            }
        }


        private void AddDialogue(CampaignGameStarter starter)
        {

            starter.AddPlayerLine("bk_question_give_slaves", "hero_main_options", "bk_answer_give_slaves",
                "{=!}I would like to offer you slaves.",
                IsPlayerNotable,
                delegate { UIHelper.ShowSlaveDonationScreen(Hero.OneToOneConversationHero); });

            starter.AddDialogLine("bk_answer_give_slaves", "bk_answer_give_slaves", "hero_main_options",
                "{=!}My suzerain, I would be honored. Extra workforce will benefit our community.",
                ConvertCultureAnswerOnCondition, null);



            starter.AddPlayerLine("bk_question_convert_culture", "hero_main_options", "bk_answer_convert_culture",
                "{=!}{NOTABLE_CONVERT_CULTURE}",
                ConvertCultureOnCondition, 
                null,
                100,
                CultureConversionOnClickable);

            starter.AddDialogLine("bk_answer_convert_culture", "bk_answer_convert_culture", "bk_convert_culture_confirm",
                "{=!}{NOTABLE_ANSWER_CONVERT_CULTURE}",
                ConvertCultureAnswerOnCondition, null);

            starter.AddPlayerLine("bk_convert_culture_confirm", "bk_convert_culture_confirm", "hero_main_options",
                "{=LPVNjXpT}See it done.",
                null, 
                CultureConversionAcceptedConsequence);

            starter.AddPlayerLine("bk_convert_culture_confirm", "bk_convert_culture_confirm", "hero_main_options",
                "{=G4ALCxaA}Never mind.",
                null, null);




            starter.AddPlayerLine("bk_question_convert_faith", "hero_main_options", "bk_answer_convert_faith",
                "{=!}{NOTABLE_CONVERT_FAITH}",
                ConvertFaithOnCondition,
                null,
                100,
                FaithConversionOnClickable);

            starter.AddDialogLine("bk_answer_convert_faith", "bk_answer_convert_faith", "bk_convert_faith_confirm",
                "{=!}{NOTABLE_ANSWER_CONVERT_FAITH}",
                FaithConvertAnswerOnCondition, null);

            starter.AddPlayerLine("bk_convert_faith_confirm", "bk_convert_faith_confirm", "hero_main_options",
                "{=LPVNjXpT}See it done.",
                null,
                FaithConversionAcceptedConsequence);

            starter.AddPlayerLine("bk_convert_faith_confirm", "bk_convert_faith_confirm", "hero_main_options",
                "{=G4ALCxaA}Never mind.",
                null, null);
        }

        public void ApplyNotableCultureConversion(Hero notable, Hero converter)
        {
            notable.Culture = converter.Culture;
            GainKingdomInfluenceAction.ApplyForDefault(converter, -BannerKingsConfig.Instance.CultureModel.GetConversionCost(Hero.OneToOneConversationHero,
                Hero.MainHero).ResultNumber);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, converter, -8);
            if (converter == Hero.MainHero)
            {
                NotificationsHelper.AddQuickNotificationWithSound(new TextObject("{=!}{HERO} has assumed the {CULTURE} culture.")
                    .SetTextVariable("HERO", notable.Name)
                    .SetTextVariable("CULTURE", converter.Culture.Name));
            }
        }

        private void CultureConversionAcceptedConsequence()
        {
            ApplyNotableCultureConversion(Hero.OneToOneConversationHero, Hero.MainHero);
        }

        private bool CultureConversionOnClickable(out TextObject hintText)
        {
            var model = BannerKingsConfig.Instance.CultureModel;

            var influence = model.GetConversionCost(Hero.OneToOneConversationHero, Hero.MainHero).ResultNumber;
            if (Clan.PlayerClan.Influence < influence)
            {
                hintText = new TextObject("{=hVJNXynE}Not enough influence.");
                return false;
            }

            if (Hero.OneToOneConversationHero.IsEnemy(Hero.MainHero))
            {
                hintText = new TextObject("{=!}{HERO} does not like you enough. Gain their trust first.")
                    .SetTextVariable("HERO", Hero.OneToOneConversationHero.Name);
                return false;
            }

            hintText = new TextObject("{=!}Conversion is possible.");
            return true;
        }

  
        private bool ConvertCultureOnCondition() 
        {
            var notable = Hero.OneToOneConversationHero;
            if (!notable.IsNotable || notable.CurrentSettlement == null)
            {
                return false;
            }

            MBTextManager.SetTextVariable("NOTABLE_CONVERT_CULTURE", 
                new TextObject("I would like you to convert to my culture ({INFLUENCE} influence).")
                .SetTextVariable("INFLUENCE", BannerKingsConfig.Instance.CultureModel.GetConversionCost(notable,
                Hero.MainHero).ResultNumber.ToString("0")));
            return IsPlayerNotable() && IsCultureDifferent();
        }

        private bool ConvertCultureAnswerOnCondition()
        {
            MBTextManager.SetTextVariable("NOTABLE_ANSWER_CONVERT_CULTURE",
                new TextObject("If that is your bidding, I would not deny it. Folks at {SETTLEMENT} might not like this. Over time however, they may accept it."));
            return IsPlayerNotable();
        }

        public void ApplyNotableFaithConversion(Hero notable, Hero converter)
        {
            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(converter);
            BannerKingsConfig.Instance.ReligionsManager.AddToReligion(notable, rel);   

            var influence = BannerKingsConfig.Instance.ReligionModel.GetConversionInfluenceCost(Hero.OneToOneConversationHero, Hero.MainHero).ResultNumber;
            var piety = BannerKingsConfig.Instance.ReligionModel.GetConversionPietyCost(Hero.OneToOneConversationHero, Hero.MainHero).ResultNumber;
            BannerKingsConfig.Instance.ReligionsManager.AddPiety(converter, -piety, true);
            GainKingdomInfluenceAction.ApplyForDefault(converter, -influence);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(notable, converter, -8);
            if (converter == Hero.MainHero)
            {
                NotificationsHelper.AddQuickNotificationWithSound(new TextObject("{=!}{HERO} has converted to the {FAITH} faith.")
                    .SetTextVariable("HERO", notable.Name)
                    .SetTextVariable("FAITH", rel.Faith.GetFaithName()));
            }
        }

        private void FaithConversionAcceptedConsequence()
        {
            ApplyNotableFaithConversion(Hero.OneToOneConversationHero, Hero.MainHero);
        }

        private bool FaithConvertAnswerOnCondition()
        {

            MBTextManager.SetTextVariable("NOTABLE_ANSWER_CONVERT_FAITH",
                new TextObject("If that is your bidding, I am inclined to accept it. The people {SETTLEMENT} might not like this. Over time however, they may accept it."));
            return IsPlayerNotable();
        }

        private bool FaithConversionOnClickable(out TextObject hintText)
        {

            var influence = BannerKingsConfig.Instance.ReligionModel.GetConversionInfluenceCost(Hero.OneToOneConversationHero, Hero.MainHero).ResultNumber;
            if (Clan.PlayerClan.Influence < influence)
            {
                hintText = new TextObject("{=hVJNXynE}Not enough influence.");
                return false;
            }

            var piety = BannerKingsConfig.Instance.ReligionModel.GetConversionPietyCost(Hero.OneToOneConversationHero, Hero.MainHero).ResultNumber;
            if (BannerKingsConfig.Instance.ReligionsManager.GetPiety(Hero.MainHero) < piety)
            {
                hintText = new TextObject("{=!}Not enough piety.");
                return false;
            }

            if (Hero.OneToOneConversationHero.IsEnemy(Hero.MainHero))
            {
                hintText = new TextObject("{=!}{HERO} does not like you enough. Gain their trust first.")
                    .SetTextVariable("HERO", Hero.OneToOneConversationHero.Name);
                return false;
            }

            hintText = new TextObject("{=!}Conversion is possible.");
            return true;
        }

        private bool ConvertFaithOnCondition()
        {
            var notable = Hero.OneToOneConversationHero;
            if (!notable.IsNotable || notable.CurrentSettlement == null)
            {
                return false;
            }

            MBTextManager.SetTextVariable("NOTABLE_CONVERT_FAITH",
                new TextObject("I would like you to convert to my faith ({INFLUENCE} influence, {PIETY} piety).")
                .SetTextVariable("INFLUENCE", BannerKingsConfig.Instance.ReligionModel.GetConversionInfluenceCost(notable,
                Hero.MainHero).ResultNumber.ToString("0"))
                .SetTextVariable("PIETY", BannerKingsConfig.Instance.ReligionModel.GetConversionPietyCost(notable,
                Hero.MainHero).ResultNumber.ToString("0")));
            return IsPlayerNotable() && IsFaithDifferent();
        }

        private bool IsPlayerNotable()
        {
            var hero = Hero.OneToOneConversationHero;
            var settlement = hero.CurrentSettlement;
            return hero.IsNotable && settlement != null && (settlement.OwnerClan == Clan.PlayerClan || 
                (BannerKingsConfig.Instance.TitleManager.GetTitle(settlement).deJure == Hero.MainHero &&
                settlement.MapFaction == Clan.PlayerClan.MapFaction));
        }

        private bool IsCultureDifferent()
        {
            return Hero.OneToOneConversationHero.Culture != Hero.MainHero.Culture;
        }

        private bool IsFaithDifferent()
        {
            var player = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            return player != null && player != BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.OneToOneConversationHero);
        }
    }

    namespace Patches
    {


        [HarmonyPatch(typeof(SettlementHelper), "SpawnNotablesIfNeeded")]
        internal class SpawnNotablesIfNeededPatch
        {
            private static bool Prefix(Settlement settlement)
            {
                var list = new List<Occupation>();
                if (settlement.IsTown)
                {
                    list = new List<Occupation>
                    {
                        Occupation.GangLeader,
                        Occupation.Artisan,
                        Occupation.Merchant
                    };
                }
                else if (settlement.IsVillage)
                {
                    list = new List<Occupation>
                    {
                        Occupation.RuralNotable,
                        Occupation.Headman
                    };
                }
                else if (settlement.IsCastle)
                {
                    list = new List<Occupation>
                    {
                        Occupation.Headman
                    };
                }

                var randomFloat = MBRandom.RandomFloat;
                var num = 0;
                foreach (var occupation in list)
                {
                    num += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
                        occupation);
                }

                var count = settlement.Notables.Count;
                var num2 = settlement.Notables.Any() ? (num - settlement.Notables.Count) / (float) num : 1f;
                num2 *= MathF.Pow(num2, 0.36f);
                if (randomFloat <= num2 && count < num)
                {
                    var list2 = new List<Occupation>();
                    foreach (var occupation2 in list)
                    {
                        var num3 = 0;
                        using (var enumerator2 = settlement.Notables.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (enumerator2.Current.CharacterObject.Occupation == occupation2)
                                {
                                    num3++;
                                }
                            }
                        }

                        var targetNotableCountForSettlement =
                            Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement,
                                occupation2);
                        if (num3 < targetNotableCountForSettlement)
                        {
                            list2.Add(occupation2);
                        }
                    }

                    if (list2.Count > 0)
                    {
                        EnterSettlementAction.ApplyForCharacterOnly(
                            HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement(), settlement), settlement);
                    }
                }

                return false;
            }
        }


        // Fix perk crash due to notable not having a Clan.
        [HarmonyPatch(typeof(GovernorCampaignBehavior), "DailyTickSettlement")]
        internal class DailyTickSettlementPatch
        {
            private static bool Prefix(Settlement settlement)
            {
                if ((settlement.IsTown || settlement.IsCastle) && settlement.Town.Governor != null)
                {
                    var governor = settlement.Town.Governor;
                    if (governor.IsNotable || governor.Clan == null)
                    {
                        if (governor.GetPerkValue(DefaultPerks.Charm.MeaningfulFavors) && MBRandom.RandomFloat < 0.02f)
                        {
                            foreach (var hero in settlement.Notables)
                            {
                                if (hero.Power >= 200f)
                                {
                                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(settlement.OwnerClan.Leader,
                                        hero, (int) DefaultPerks.Charm.MeaningfulFavors.SecondaryBonus);
                                }
                            }
                        }

                        SkillLevelingManager.OnSettlementGoverned(governor, settlement);
                        return false;
                    }
                }

                return true;
            }
        }

        // Fix perk crash due to notable not having a Clan.
        [HarmonyPatch(typeof(Town), "DailyTick")]
        internal class TownDailyTicktPatch
        {
            private static bool Prefix(Town __instance)
            {
                try{
                    if (__instance.Governor != null && __instance.Governor is {IsNotable: true} && __instance.OwnerClan != null &&
                        __instance.OwnerClan.Leader != null)
                    {
                        __instance.Loyalty += __instance.LoyaltyChange;
                        __instance.Security += __instance.SecurityChange;
                        __instance.FoodStocks += __instance.FoodChange;
                        if (__instance.FoodStocks < 0f)
                        {
                            __instance.FoodStocks = 0f;
                            __instance.Owner.RemainingFoodPercentage = -100;
                        }
                        else
                        {
                            __instance.Owner.RemainingFoodPercentage = 0;
                        }

                        if (__instance.FoodStocks > __instance.FoodStocksUpperLimit())
                        {
                            __instance.FoodStocks = __instance.FoodStocksUpperLimit();
                        }

                        if (!__instance.CurrentBuilding.BuildingType.IsDefaultProject)
                        {
                            __instance.GetType().GetMethod("TickCurrentBuilding",
                                    BindingFlags.Instance | BindingFlags.NonPublic)
                                .Invoke(__instance, null);
                        }

                        else if (__instance.Governor.GetPerkValue(DefaultPerks.Charm.Virile) && MBRandom.RandomFloat < 0.1f)
                        {
                            var randomElement = __instance.Settlement.Notables.GetRandomElement();
                            if (randomElement != null)
                            {
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.OwnerClan.Leader,
                                    randomElement, MathF.Round(DefaultPerks.Charm.Virile.SecondaryBonus), false);
                            }
                        }

                        if (__instance.Governor.GetPerkValue(DefaultPerks.Roguery.WhiteLies) &&
                            MBRandom.RandomFloat < 0.02f)
                        {
                            var randomElement2 = __instance.Settlement.Notables.GetRandomElement();
                            if (randomElement2 != null)
                            {
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.Governor,
                                    randomElement2, MathF.Round(DefaultPerks.Roguery.WhiteLies.SecondaryBonus));
                            }
                        }

                        if (__instance.Governor.GetPerkValue(DefaultPerks.Roguery.Scarface) &&
                            MBRandom.RandomFloat < 0.05f)
                        {
                            var randomElementWithPredicate =
                                __instance.Settlement.Notables.GetRandomElementWithPredicate(x => x.IsGangLeader);
                            if (randomElementWithPredicate != null)
                            {
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(__instance.Governor,
                                    randomElementWithPredicate,
                                    MathF.Round(DefaultPerks.Roguery.Scarface.SecondaryBonus));
                            }
                        }
                        

                        __instance.Owner.Settlement.Prosperity += __instance.ProsperityChange;
                        if (__instance.Owner.Settlement.Prosperity < 0f)
                        {
                            __instance.Owner.Settlement.Prosperity = 0f;
                        }

                        __instance.GetType().GetMethod("HandleMilitiaAndGarrisonOfSettlementDaily",
                                BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(__instance, null);
                        __instance.GetType().GetMethod("RepairWallsOfSettlementDaily",
                                BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(__instance, null);
                    }
                    }
                catch(System.Exception ex)
                {

                }
                return true;
            }

            private static System.Exception Finalize()
            {
                return  null;
            }
        }
    }
}