using BannerKings.Patches;
using BannerKings.Settings;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Utils
{
    internal static class PerksHelpers
    {
        public enum SkillScale
        {
            None,

            OnlyQuartermaster,
            OnlyPartyLeader,
            QuartermasterFirst,
            PartyLeaderFirst,
            TheGreater,
            Both,
            Other,
        }
        public static float AddScaledGovernerPerkBonusForTownWithTownHeros(this PerkObject perk,
                                                                           ref ExplainedNumber bonuses, bool isSecondary,
                                                                           Town town, float? factor = null)
        {
            if (perk != null && PerksAndSkillsPatches.StewardPerksData.ContainsKey(perk.StringId))
            {
                var data = isSecondary ? PerksAndSkillsPatches.StewardPerksData[perk.StringId].SecondaryPerk : PerksAndSkillsPatches.StewardPerksData[perk.StringId].PrimaryPerk;
                if (data != null)
                {
                    return AddScaledGovernerPerkBonusForTownWithTownHeros(perk, ref bonuses, isSecondary, town, data.ScaleOnSkill, data.EverySkillMain, data.EverySkillSecondary, data.EverySkillOthers, data.StartSkillLevel, data.MinBonus, data.MaxBonus, factor);
                }
            }
            return 0;
        }
        public static float AddScaledPerkBonus(this PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary,
                                               MobileParty mobileParty, float? factor = null,
                                               TextObject nameOverride = null)
        {
            if (perk != null && PerksAndSkillsPatches.StewardPerksData.ContainsKey(perk.StringId))
            {
                var data = isSecondary ? PerksAndSkillsPatches.StewardPerksData[perk.StringId].SecondaryPerk : PerksAndSkillsPatches.StewardPerksData[perk.StringId].PrimaryPerk;
                if (data != null)
                {
                    return AddScaledPerkBonus(perk, ref bonuses, isSecondary, mobileParty, data.ScaleOnSkill, data.StartSkillLevel, data.EverySkillMain, data.EverySkillSecondary, data.EverySkillOthers, data.SkillScale, data.MinBonus, data.MaxBonus, nameOverride, factor);
                }
            }
            return 0;
        }

        public static float AddScaledPersonlOrClanLeaderPerkBonusWithClanAndFamilyMembers(this PerkObject perk,
                                                                                          ref ExplainedNumber bonuses,
                                                                                          bool isSecondary, Hero person,
                                                                                          float? factor = null)
        {
            if (perk != null && PerksAndSkillsPatches.StewardPerksData.ContainsKey(perk.StringId))
            {
                var data = isSecondary ? PerksAndSkillsPatches.StewardPerksData[perk.StringId].SecondaryPerk : PerksAndSkillsPatches.StewardPerksData[perk.StringId].PrimaryPerk;
                if (data != null)
                {
                    return AddScaledPersonlOrClanLeaderPerkBonusWithClanAndFamilyMembers(perk, ref bonuses, isSecondary, person, data.ScaleOnSkill, data.EverySkillMain, data.EverySkillSecondary, data.EverySkillOthers, data.SkillScale, data.StartSkillLevel, data.MinBonus, data.MaxBonus, factor);
                }
            }
            return 0;
        }

        static float AddScaledGovernerPerkBonusForTownWithTownHeros(PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, Town town, SkillObject scaleSkill, float everySkillGoverner, float everySkillOwner, float everySkillMember, int startAtSkill, float? minValue = null, float? maxValue = null, float? factor = null)
        {
            float value = 0;
            if (!(perk.PrimaryRole == SkillEffect.PerkRole.Governor || perk.SecondaryRole == SkillEffect.PerkRole.Governor))
            {
                return 0f;
            }
            var bonus = isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus;
            value += GetGovernorPerkBonus(perk, town, scaleSkill, everySkillGoverner, bonus, startAtSkill);
            value += GetOwnerClanPerkBonus(perk, town, scaleSkill, everySkillGoverner, everySkillOwner, bonus, startAtSkill);
            value += GetGarrisonHerosPerkBonus(perk, perk.SecondaryRole == SkillEffect.PerkRole.Governor, town, scaleSkill, everySkillMember, startAtSkill);
            if (factor.HasValue)
            {
                value = value * factor.Value;
            }
            return AddBonusToStat(perk, ref bonuses, isSecondary, minValue, maxValue, ref value);
        }
        static float AddScaledPerkBonus(PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, MobileParty mobileParty, SkillObject scaleSkill, int startAtSkill, float everySkillLeader, float everySkillQuartermaster, float everySkillMember, SkillScale skillScale, float? minValue = null, float? maxValue = null, TextObject name = null, float? factor = null)
        {
            var value = 0f;
            var perkbouns = isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus;
            var perkRole = isSecondary ? perk.SecondaryRole : perk.PrimaryRole;

            Hero chosenHero = null;

            if (skillScale != SkillScale.None)
            {
                float? chosenValue = null;

                if (everySkillQuartermaster > 0 && skillScale != SkillScale.OnlyPartyLeader &&
                    (skillScale == SkillScale.Both ||
                    skillScale == SkillScale.TheGreater ||
                    skillScale == SkillScale.QuartermasterFirst ||
                    skillScale == SkillScale.PartyLeaderFirst ||
                    (perkRole == SkillEffect.PerkRole.Quartermaster && skillScale == SkillScale.OnlyQuartermaster)))
                {
                    chosenHero = mobileParty.EffectiveQuartermaster;
                    if (chosenHero != null && chosenHero.GetPerkValue(perk))
                    {
                        chosenValue = perkbouns * MathF.Max(0, chosenHero.GetSkillValue(scaleSkill) - startAtSkill) / everySkillQuartermaster;
                    }
                }

                if (everySkillLeader > 0 && skillScale != SkillScale.OnlyQuartermaster &&
                    (skillScale == SkillScale.Both ||
                    skillScale == SkillScale.TheGreater ||
                    skillScale == SkillScale.QuartermasterFirst ||
                    skillScale == SkillScale.PartyLeaderFirst ||
                    (perkRole == SkillEffect.PerkRole.PartyLeader && skillScale == SkillScale.OnlyPartyLeader)))
                {
                    chosenHero = mobileParty.LeaderHero;
                    if (chosenHero != null && chosenHero.GetPerkValue(perk))
                    {
                        var newValue = perkbouns * MathF.Max(0, chosenHero.GetSkillValue(scaleSkill) - startAtSkill) / everySkillLeader;
                        if (skillScale == SkillScale.QuartermasterFirst && !chosenValue.HasValue)
                        {
                            chosenValue = newValue;
                        }
                        else if (skillScale == SkillScale.TheGreater && newValue > chosenValue)
                        {
                            chosenValue = newValue;
                        }
                        else if (skillScale == SkillScale.Both && mobileParty.LeaderHero != mobileParty.EffectiveQuartermaster)
                        {
                            if (!chosenValue.HasValue)
                            {
                                chosenValue = newValue;
                            }
                            else
                            {
                                chosenValue += newValue;
                            }
                        }
                        else if (skillScale == SkillScale.OnlyPartyLeader || skillScale == SkillScale.PartyLeaderFirst)
                        {
                            chosenValue = newValue;
                        }
                    }
                }

                if (chosenValue.HasValue)
                {
                    value += chosenValue.Value;
                }
            }

            value += GetPartyHerosPerkBonus(perk, isSecondary, mobileParty, scaleSkill, everySkillMember, chosenHero, startAtSkill);
            if (factor.HasValue)
            {
                value = value * factor.Value;
            }
            return AddBonusToStat(perk, ref bonuses, isSecondary, minValue, maxValue, ref value, name);
        }
        static float AddScaledPersonlOrClanLeaderPerkBonusWithClanAndFamilyMembers(PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, Hero person, SkillObject scaleSkill, float everySkillPerson, float everySkillFamilyMembers, float everySkillClanMembers, SkillScale skillScale, int startAtSkill, float? minValue = null, float? maxValue = null, float? factor = null)
        {
            if (person == null)
            {
                return 0f;
            }
            var value = 0f;
            var perkbouns = isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus;

            value += GetFamilyMembersPerkBonus(perk, isSecondary, person, scaleSkill, everySkillFamilyMembers, startAtSkill);

            value += GetOtherClanMembersPerkBonus(perk, isSecondary, person, scaleSkill, everySkillClanMembers, startAtSkill);

            if (everySkillPerson > 0 && person.GetPerkValue(perk))
            {
                value += perkbouns * MathF.Max(0, person.GetSkillValue(scaleSkill) - startAtSkill) / everySkillPerson;
            }
            if (factor.HasValue)
            {
                value = value * factor.Value;
            }
            return AddBonusToStat(perk, ref bonuses, isSecondary, minValue, maxValue, ref value);
        }

        private static float GetGarrisonHerosPerkBonus(PerkObject perk, bool isSecondary, Town town, SkillObject scaleSkill, float everySkillMember, int startAtSkill)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && town.OwnerClan != null && everySkillMember > 0)
            {
                IEnumerable<Hero> heros1 = town?.Settlement?.HeroesWithoutParty.Where(d => d.Clan == town.OwnerClan && d.GetPerkValue(perk) && d != town.Governor && !d.IsClanLeader);
                IEnumerable<Hero> heros2 = town?.Settlement?.Parties.SelectMany(d => d.GetAllPartyHeros().Select(d=>d.HeroObject)).Where(d => d.Clan == town.OwnerClan && d.GetPerkValue(perk) && d != town.Governor && !d.IsClanLeader);
                var allHeros = heros1.Union(heros2);
                if (allHeros.Any())
                {
                    value += allHeros.Sum(d => (isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus) * (MathF.Max(0, d.GetSkillValue(scaleSkill) - startAtSkill) / everySkillMember));
                }
            }
            return value;
        }

        private static float GetOwnerClanPerkBonus(PerkObject perk, Town town, SkillObject scaleSkill, float everySkillGoverner, float everySkillOwner, float bonus, int startAtSkill)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulGovernorPerksFromSettlementOwner && town.OwnerClan?.Leader != null && town.OwnerClan.Leader.GetPerkValue(perk))
            {
                Hero governor = town.Governor;
                if (governor == null && town.OwnerClan?.Leader?.CurrentSettlement != null && town.OwnerClan?.Leader?.CurrentSettlement == town?.Settlement)
                {
                    // if town has no governor, but owner is in town it means that owner is governor
                    if (everySkillGoverner > 0)
                    {
                        value += bonus * (MathF.Max(0, town.OwnerClan.Leader.GetSkillValue(scaleSkill) - startAtSkill) / everySkillGoverner);
                    }
                }
                else
                {
                    //if town has a governor owner bonus applied
                    if (everySkillOwner > 0 && town.OwnerClan?.Leader !=null)
                    {
                        value += bonus * (MathF.Max(0, town.OwnerClan.Leader.GetSkillValue(scaleSkill) - startAtSkill) / everySkillOwner);
                    }
                }
            }
            return value;
        }

        private static float GetGovernorPerkBonus(PerkObject perk, Town town, SkillObject scaleSkill, float everySkillGoverner, float bonus, int startAtSkill)
        {
            float value = 0;
            Hero governor = town.Governor;
            if (governor != null && governor.GetPerkValue(perk) && governor.CurrentSettlement != null && governor.CurrentSettlement == town.Settlement && everySkillGoverner > 0)
            {
                value += bonus * (MathF.Max(0, town.Governor.GetSkillValue(scaleSkill) - startAtSkill) / everySkillGoverner);
            }
            return value;
        }

        private static float GetPartyHerosPerkBonus(PerkObject perk, bool isSecondary, MobileParty mobileParty, SkillObject scaleSkill, float everySkillMember, Hero choosenHero, int startAtSkill)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && everySkillMember > 0)
            {
                var mobilePartyHeros = mobileParty.GetAllPartyHeros();
                var partyHeros = mobilePartyHeros.Where(d => (d.HeroObject?.IsActive ?? false) && d.GetPerkValue(perk) && d.HeroObject != choosenHero);
                value += partyHeros.Sum(d => (isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus) * (MathF.Max(0, d.GetSkillValue(scaleSkill) - startAtSkill) / everySkillMember));
            }
            return value;
        }


        private static float GetFamilyMembersPerkBonus(PerkObject perk, bool isSecondary, Hero person, SkillObject scaleSkill, float everySkillFamilyMembers, int startAtSkill)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && everySkillFamilyMembers > 0)
            {
                var familyMembers = person.GetActiveFamilyMembers().Where(d => d.IsActive).ToList();
                value += CalculatePerkBonusForMembers(familyMembers, perk, (isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus), scaleSkill, everySkillFamilyMembers, startAtSkill);
            }
            return value;
        }

        private static float GetOtherClanMembersPerkBonus(PerkObject perk, bool isSecondary, Hero person, SkillObject scaleSkill, float everySkillClanMembers, int startAtSkill)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && everySkillClanMembers > 0)
            {
                var otherClanMembers = person.GetActiveClanCompanions();
                value += CalculatePerkBonusForMembers(otherClanMembers, perk, (isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus), scaleSkill, everySkillClanMembers, startAtSkill);
                otherClanMembers = person.GetActiveClanLordsNotFamilyMemebrs();
                value += CalculatePerkBonusForMembers(otherClanMembers, perk, (isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus), scaleSkill, everySkillClanMembers, startAtSkill);
            }
            return value;
        }

        private static float CalculatePerkBonusForMembers(List<Hero> members, PerkObject perk, float perkbouns, SkillObject scaleSkill, float everySkill, int startAtSkill)
        {
            float value = 0;
            if (everySkill > 0)
            {
                value = members.Where(d => d.IsActive && d.GetPerkValue(perk)).Sum(d => perkbouns * (MathF.Max(0, d.GetSkillValue(scaleSkill) - startAtSkill) / everySkill));
            }
            return value;
        }

        private static float AddBonusToStat(PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, float? minValue, float? maxValue, ref float value, TextObject name = null)
        {
            if (!value.ApproximatelyEqualsTo(0f))
            {
                if (minValue.HasValue && value < minValue.Value)
                {
                    value = minValue.Value;
                }
                if (maxValue.HasValue && value > maxValue.Value)
                {
                    value = maxValue.Value;
                }

                if (isSecondary)
                {
                    AddToStat(ref bonuses, perk.SecondaryIncrementType, value, name ?? perk.Name);
                    return value;
                }
                else
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, value, name ?? perk.Name);
                    return value;
                }
            }
            return 0f;
        }

        private static void AddToStat(ref ExplainedNumber stat, SkillEffect.EffectIncrementType effectIncrementType, float number, TextObject text)
        {
            switch (effectIncrementType)
            {
                case SkillEffect.EffectIncrementType.Add:
                    stat.Add(number, text);
                    break;
                case SkillEffect.EffectIncrementType.AddFactor:
                    stat.AddFactor(number, text);
                    break;
            }
        }
    }
}