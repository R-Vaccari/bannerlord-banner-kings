using BannerKings.Settings;
using System.Collections.Generic;
using System.Linq;
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
            Both
        }

        public static float AddScaledGovernerPerkBonusForTownWithTownHeros(this PerkObject perk, ref ExplainedNumber bonuses, Town town, SkillObject scaleSkill, float everySkillGoverner, float everySkillOwner, float everySkillMember, float? minValue = null, float? maxValue = null)
        {
            float value = 0;
            if (!(perk.PrimaryRole == SkillEffect.PerkRole.Governor || perk.SecondaryRole == SkillEffect.PerkRole.Governor))
            {
                return 0f;
            }

            value += GetGovernorPerkBonus(perk, town, scaleSkill, everySkillGoverner);
            value += GetOwnerClanPerkBonus(perk, town, scaleSkill, everySkillGoverner, everySkillOwner);
            value += GetGarrisonHerosPerkBonus(perk, town, scaleSkill, everySkillMember);

            return AddBonusToStat(perk, ref bonuses, perk.SecondaryRole == SkillEffect.PerkRole.Governor, minValue, maxValue, ref value);
        }

        public static float AddScaledPerkBonus(this PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, MobileParty mobileParty, SkillObject scaleSkill, float everySkillLeader, float everySkillQuartermaster, float everySkillMember, SkillScale skillScale, float? minValue = null, float? maxValue = null)
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
                        chosenValue = perkbouns * chosenHero.GetSkillValue(scaleSkill) / everySkillQuartermaster;
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
                        var newValue = perkbouns * chosenHero.GetSkillValue(scaleSkill) / everySkillLeader;
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

            value += GetPartyHerosPerkBonus(perk, mobileParty, scaleSkill, everySkillMember, chosenHero);

            return AddBonusToStat(perk, ref bonuses, isSecondary, minValue, maxValue, ref value);
        }

        public static float AddScaledPersonlOrClanLeaderPerkBonusWithClanAndFamilyMembers(this PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, Hero person, SkillObject scaleSkill, float everySkillPerson, float everySkillFamilyMembers, float everySkillClanMembers, float? minValue = null, float? maxValue = null)
        {
            if (person == null)
            {
                return 0f;
            }
            var value = 0f;
            var perkbouns = isSecondary ? perk.SecondaryBonus : perk.PrimaryBonus;

            value += GetFamilyMembersPerkBonus(perk, person, scaleSkill, everySkillFamilyMembers);

            value += GetOtherClanMembersPerkBonus(perk, person, scaleSkill, everySkillClanMembers);

            if (everySkillPerson > 0 && person.GetPerkValue(perk))
            {
                value += perkbouns * person.GetSkillValue(scaleSkill) / everySkillPerson;
            }

            return AddBonusToStat(perk, ref bonuses, isSecondary, minValue, maxValue, ref value);
        }



        private static float GetGarrisonHerosPerkBonus(PerkObject perk, Town town, SkillObject scaleSkill, float everySkillMember)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && town.OwnerClan != null && everySkillMember > 0)
            {
                var garrisonHeros = town.Settlement.HeroesWithoutParty.Where(d => d.Clan == town.OwnerClan && d.GetPerkValue(perk) && d != town.Governor && !d.IsClanLeader);
                if (garrisonHeros.Any())
                {
                    value += garrisonHeros.Sum(d => perk.PrimaryBonus * (d.GetSkillValue(scaleSkill) / everySkillMember));
                }
            }
            return value;
        }

        private static float GetOwnerClanPerkBonus(PerkObject perk, Town town, SkillObject scaleSkill, float everySkillGoverner, float everySkillOwner)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulGovernorPerksFromSettlementOwner && town.OwnerClan?.Leader != null && town.OwnerClan.Leader.GetPerkValue(perk))
            {
                Hero governor = town.Governor;
                if (governor == null && town.OwnerClan.Leader.CurrentSettlement != null && town.OwnerClan.Leader.CurrentSettlement == town.Settlement)
                {
                    if (everySkillGoverner > 0)
                    {
                        value += perk.PrimaryBonus * (town.OwnerClan.Leader.GetSkillValue(scaleSkill) / everySkillGoverner);
                    }
                }
                else
                {
                    if (everySkillOwner > 0)
                    {
                        value += perk.PrimaryBonus * (town.OwnerClan.Leader.GetSkillValue(scaleSkill) / everySkillOwner);
                    }
                }
            }
            return value;
        }

        private static float GetGovernorPerkBonus(PerkObject perk, Town town, SkillObject scaleSkill, float everySkillGoverner)
        {
            float value = 0;
            Hero governor = town.Governor;
            if (governor != null && governor.GetPerkValue(perk) && governor.CurrentSettlement != null && governor.CurrentSettlement == town.Settlement && everySkillGoverner > 0)
            {
                value += perk.PrimaryBonus * (town.Governor.GetSkillValue(scaleSkill) / everySkillGoverner);
            }
            return value;
        }


        private static float GetPartyHerosPerkBonus(PerkObject perk, MobileParty mobileParty, SkillObject scaleSkill, float everySkillMember, Hero choosenHero)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers && everySkillMember > 0)
            {
                var mobilePartyHeros = mobileParty.GetAllPartyHeros();
                var partyHeros = mobilePartyHeros.Where(d => d.GetPerkValue(perk) && d.HeroObject != choosenHero);
                value += partyHeros.Sum(d => perk.PrimaryBonus * (d.GetSkillValue(scaleSkill) / everySkillMember));
            }
            return value;
        }


        private static float GetFamilyMembersPerkBonus(PerkObject perk, Hero person, SkillObject scaleSkill, float everySkillFamilyMembers)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
            {
                var familyMembers = person.GetActiveFamilyMembers();
                value += CalculatePerkBonusForMembers(familyMembers, perk, perk.PrimaryBonus, scaleSkill, everySkillFamilyMembers);
            }
            return value;
        }

        private static float GetOtherClanMembersPerkBonus(PerkObject perk, Hero person, SkillObject scaleSkill, float everySkillClanMembers)
        {
            float value = 0;
            if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
            {
                var otherClanMembers = person.GetActiveClanCompanions();
                value += CalculatePerkBonusForMembers(otherClanMembers, perk, perk.PrimaryBonus, scaleSkill, everySkillClanMembers);
            }
            return value;
        }



        private static float CalculatePerkBonusForMembers(List<Hero> members, PerkObject perk, float perkbouns, SkillObject scaleSkill, float everySkill)
        {
            return members.Where(d => d.GetPerkValue(perk)).Sum(d => perkbouns * (d.GetSkillValue(scaleSkill) / everySkill));
        }

        private static float AddBonusToStat(PerkObject perk, ref ExplainedNumber bonuses, bool isSecondary, float? minValue, float? maxValue, ref float value)
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
                    AddToStat(ref bonuses, perk.SecondaryIncrementType, value, perk.Name);
                    return value;
                }
                else
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, value, perk.Name);
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