using BannerKings.Behaviours.PartyNeeds;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKPartyNeedsModel : IPartyNeedsModel
    {
        public float ArrowsPerSoldier => 0.01f;

        public float ShieldsPerSoldier => 0.01f;

        public float WeaponsPerSoldier => 0.015f;

        public float HorsesPerSoldier => 0.01f;

        public float ClothPerSoldier => 0.01f;

        public float ToolsPerSoldier => 0.01f;

        public float WoodPerSoldier => 0.02f;

        public float AnimalProductsPerSoldier => 0.01f;

        public float AlcoholPerSoldier => 0.025f;

        public ExplainedNumber MinimumSoldiersThreshold(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(5f, descriptions);
            if (needs.Party.LeaderHero == null)
            {
                result.Add(float.MaxValue);
                return result;
            }

            result.Add(needs.Party.LeaderHero.GetSkillValue(DefaultSkills.Leadership) / 10f,
                DefaultSkills.Leadership.Name);
            return result;
        }

        public ExplainedNumber CalculateAlcoholNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                result.Add(element.Number * AlcoholPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                    .SetTextVariable("TROOP_NAME", element.Character.Name)
                    .SetTextVariable("COUNT", element.Number));
            }

            return result;
        }

        public ExplainedNumber CalculateArrowsNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                FormationClass formation = element.Character.GetFormationClass();
                if (formation == FormationClass.Ranged || formation == FormationClass.HorseArcher)
                {
                    result.Add(element.Number * ArrowsPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                                                       .SetTextVariable("TROOP_NAME", element.Character.Name)
                                                       .SetTextVariable("COUNT", element.Number));
                }
            }

            return result;
        }

        public ExplainedNumber CalculateClothNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                result.Add(element.Number * ClothPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                                    .SetTextVariable("TROOP_NAME", element.Character.Name)
                                    .SetTextVariable("COUNT", element.Number));
            }

            return result;
        }

        public ExplainedNumber CalculateHorsesNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                if (element.Character.IsMounted)
                {
                    result.Add(element.Number * HorsesPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                                                        .SetTextVariable("TROOP_NAME", element.Character.Name)
                                                        .SetTextVariable("COUNT", element.Number));
                }
            }

            return result;
        }

        public ExplainedNumber CalculateAnimalProductsNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                result.Add(element.Number * AnimalProductsPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                    .SetTextVariable("TROOP_NAME", element.Character.Name)
                    .SetTextVariable("COUNT", element.Number));
            }

            return result;
        }

        public ExplainedNumber CalculateShieldsNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                if (element.Character.Equipment.HasWeaponOfClass(WeaponClass.SmallShield) ||
                    element.Character.Equipment.HasWeaponOfClass(WeaponClass.LargeShield))
                {
                    result.Add(element.Number * ShieldsPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                                        .SetTextVariable("TROOP_NAME", element.Character.Name)
                                        .SetTextVariable("COUNT", element.Number));
                }
            }

            return result;
        }

        public ExplainedNumber CalculateWeaponsNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                if (!element.Character.IsRanged)
                {
                    result.Add(element.Number * WeaponsPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                        .SetTextVariable("TROOP_NAME", element.Character.Name)
                        .SetTextVariable("COUNT", element.Number));
                }
            }

            return result;
        }

        public ExplainedNumber CalculateToolsNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            float siege = 0f;
            TextObject siegeText = null;
            if (needs.Party.SiegeEvent != null)
            {
                if (needs.Party.BesiegerCamp != null)
                {
                    if (!needs.Party.BesiegerCamp.IsPreparationComplete)
                    {
                        siege = 5f;
                        siegeText = new TextObject("{=!}Camp under construction");
                    }

                    var engines = needs.Party.BesiegerCamp.SiegeEngines;
                    var preparations = engines?.SiegePreparations;
                    if (preparations != null && !preparations.IsConstructed)
                    {
                        siege = 10f;
                        siegeText = new TextObject("{=!}Siege engine under construction");
                    }
                }
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                result.Add(element.Number * ToolsPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                    .SetTextVariable("TROOP_NAME", element.Character.Name)
                    .SetTextVariable("COUNT", element.Number));
            }

            result.AddFactor(siege, siegeText);
            return result;
        }

        public ExplainedNumber CalculateWoodNeed(PartyNeeds needs, bool descriptions)
        {
            ExplainedNumber result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            if (needs.Party.CurrentSettlement != null && needs.Party.CurrentSettlement.Town != null)
            {
                result.AddFactor(-1f, new TextObject("{=!}In a town or castle"));
            }

            float siege = 0f;
            TextObject siegeText = null;
            if (needs.Party.SiegeEvent != null)
            {
                if (needs.Party.BesiegerCamp != null)
                {
                    if (!needs.Party.BesiegerCamp.IsPreparationComplete)
                    {
                        siege = 5f;
                        siegeText = new TextObject("{=!}Camp under construction");
                    }

                    var engines = needs.Party.BesiegerCamp.SiegeEngines;
                    var preparations = engines?.SiegePreparations;
                    if (preparations != null && !preparations.IsConstructed)
                    {
                        siege = 10f;
                        siegeText = new TextObject("{=!}Siege engine under construction");
                    }
                }
            }

            foreach (TroopRosterElement element in needs.Party.MemberRoster.GetTroopRoster())
            {
                result.Add(element.Number * WoodPerSoldier, new TextObject("{=!}{TROOP_NAME}(x{COUNT})")
                    .SetTextVariable("TROOP_NAME", element.Character.Name)
                    .SetTextVariable("COUNT", element.Number));
            }

            result.AddFactor(siege, siegeText);
            return result;
        }
    }
}
