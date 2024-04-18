using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using BannerKings.Utils;
using BannerKings.Settings;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using BannerKings.Managers.Skills;

namespace BannerKings.Patches
{
    internal class PerksPatches
    {
        [HarmonyPatch(typeof(CampaignUIHelper), "GetPerkRoleText")]
        class GetPerkRoleTextPatch
        {
            static void Postfix(ref TextObject __result, PerkObject perk, bool getSecondary)
            {
                TextObject textObject = null;
                var rolesText = new List<TextObject>();
                textObject = GameTexts.FindText("str_perk_one_role", null);
                if (!getSecondary && perk.PrimaryRole != SkillEffect.PerkRole.None)
                {
                    rolesText.Add(GameTexts.FindText("role", perk.PrimaryRole.ToString()));
                    if (RegisterPerkPatch.PerksAdditionalPrimaryRoles.ContainsKey(perk.StringId))
                    {
                        rolesText.AddRange(RegisterPerkPatch.PerksAdditionalPrimaryRoles[perk.StringId].Select(d => GameTexts.FindText("role", d.ToString())));
                    }
                    textObject.SetTextVariable("PRIMARY_ROLE", NormalizeAdditionalRoles(perk.PrimaryRole, string.Join(" - ", rolesText)));
                }
                else if (getSecondary && perk.SecondaryRole != SkillEffect.PerkRole.None)
                {
                    rolesText.Add(GameTexts.FindText("role", perk.SecondaryRole.ToString()));
                    if (RegisterPerkPatch.PerksAdditionalSecondaryRoles.ContainsKey(perk.StringId))
                    {
                        rolesText.AddRange(RegisterPerkPatch.PerksAdditionalSecondaryRoles[perk.StringId].Select(d => GameTexts.FindText("role", d.ToString())));
                    }
                    textObject.SetTextVariable("PRIMARY_ROLE", NormalizeAdditionalRoles(perk.SecondaryRole, string.Join(" - ", rolesText)));
                }

                __result = textObject;
            }
            static string NormalizeAdditionalRoles(SkillEffect.PerkRole perkRole, string text)
            {
                if (perkRole == SkillEffect.PerkRole.Governor)
                {
                    var partyOwner = GameTexts.FindText("role", SkillEffect.PerkRole.PartyOwner.ToString()).ToString();
                    var partyMember = GameTexts.FindText("role", SkillEffect.PerkRole.PartyMember.ToString()).ToString();
                    text = text.Replace(partyOwner, "Town/Castle Owner");
                    text = text.Replace(partyMember, "Town/Castle Member");
                }
                if (perkRole == SkillEffect.PerkRole.Personal|| perkRole == SkillEffect.PerkRole.ClanLeader)
                {
                    var familyMember = GameTexts.FindText("role", SkillEffect.PerkRole.Captain.ToString()).ToString();
                    var clanMember = GameTexts.FindText("role", SkillEffect.PerkRole.PartyMember.ToString()).ToString();
                    text = text.Replace(familyMember, "Family Member");
                    text = text.Replace(clanMember, "Clan Member");
                }
                return text;
            }
        }
        [HarmonyPatch(typeof(DefaultPerks), "RegisterAll")]
        class RegisterPerkPatch
        {
            public static MBReadOnlyList<PerkObject> AllPerks => Game.Current.ObjectManager.GetObjectTypeList<PerkObject>();
            public static Dictionary<string, List<SkillEffect.PerkRole>> PerksAdditionalPrimaryRoles { get; set; } = new Dictionary<string, List<SkillEffect.PerkRole>>();
            public static Dictionary<string, List<SkillEffect.PerkRole>> PerksAdditionalSecondaryRoles { get; set; } = new Dictionary<string, List<SkillEffect.PerkRole>>();
            static void Postfix()
            {
                if (BannerKingsSettings.Instance.EnableUsefulPerks)
                {
                    if (BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                    {
                        #region Steward
                        #region StewardFrugal (done)
                        ChangePerkRequirement("StewardFrugal", 1);
                        ChangePerk("StewardFrugal", false, -0.01f,
                            "Reduce party wages by {VALUE}% for every 20 levels of steward skill if hero is the party quartermaster,\nReduce party wages by {VALUE}% for every 100 levels of steward skill if hero is a party member. (max -30%)",
                            "Reduce party wages by {VALUE}% for every 20 levels of steward skill if hero is the party quartermaster. (max -40%)"
                            , SkillEffect.PerkRole.PartyMember);

                        ChangePerk("StewardFrugal", true, -0.01f,
                            "Reduce recruitment costs by {VALUE}% for every 20 levels of steward skill if hero is the party leader,\nReduce recruitment costs by {VALUE}% for every 100 levels of steward skill if hero is a party member. (max -40%)",
                            "Reduce recruitment costs by {VALUE}% for every 20 levels of steward skill if hero is the party party leader. (max -40%)",
                            SkillEffect.PerkRole.PartyMember);
                        #endregion
                        #region StewardWarriorsDiet (done)    

                        ChangePerk("StewardWarriorsDiet", false, -0.01f,
                           "Reduce party food consumption by {VALUE}% for every 15 levels of steward skill if hero is the party quartermaster,\nReduce party food consumption by {VALUE}% for every 100 levels of steward skill if hero is a party member. (max -30%)",
                           "Reduce party food consumption by {VALUE}% for every 15 levels of steward skill if hero is the party quartermaster. (max -30%)",
                           SkillEffect.PerkRole.PartyMember);
                        #endregion
                        #region StewardDrillSergant (done)
                        ChangePerkRequirement("StewardDrillSergant", 2);
                        ChangePerk("StewardDrillSergant", false, 1f,
                            "{VALUE} daily experience to troops in your party for every 25 levels of steward skill if hero is the party quartermaster or party leader,\n{VALUE} daily experience to troops in your party for every 100 levels of steward skill if hero is a party member. (max +30)",
                            "{VALUE} daily experience to troops in your party for every 25 levels of steward skill if hero is the party quartermaster or party leader. (max +30)"
                            , SkillEffect.PerkRole.PartyLeader, SkillEffect.PerkRole.PartyMember);

                        ChangePerk("StewardDrillSergant", true, -0.01f,
                            "{VALUE}% garrison wages in the town/castle for every 20 levels of steward skill if the hero is the town/castle governer,\n{VALUE}% garrison wages in the town/castle for every 40 levels of steward skill if the hero is a town owner, \n{VALUE}% garrison wages in the town/castle for every 100 levels of steward skill if the hero is staying in town that belongs to his clan.(max -30%)",
                            "{VALUE}% garrison wages in the town/castle for every 20 levels of steward skill if the hero is the town/castle governer,\n{VALUE}% garrison wages in the town/castle for every 40 levels of steward skill if the hero is a town owner.(max -30%)"
                            , SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember);

                        #endregion
                        #region StewardSevenVeterans (done)
                        //this._stewardSevenVeterans.Initialize("{=2ryLuN2i}Seven Veterans", DefaultSkills.Steward, this.GetTierCost(2), this._stewardDrillSergant, "{=gX0edfpK}{VALUE} daily experience for tier 4+ troops in your party.", SkillEffect.PerkRole.Quartermaster, 4f, SkillEffect.EffectIncrementType.Add, "{=g9gTYB8u}{VALUE} militia recruitment in the governed settlement.", SkillEffect.PerkRole.Governor, 1f, SkillEffect.EffectIncrementType.Add, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        ChangePerk("StewardSevenVeterans", false, 2f,
                            "{VALUE} daily experience to tier 4+ troops in your party for every 25 levels of steward skill if hero is the party quartermaster or party leader,\n{VALUE} daily experience to tier 4+ troops in your party for every 100 levels of steward skill if hero is a party member. (max +60)",
                            "{VALUE} daily experience to tier 4+ troops in your party for every 25 levels of steward skill if hero is the party quartermaster or party leader. (max +30)"
                            , SkillEffect.PerkRole.PartyLeader, SkillEffect.PerkRole.PartyMember);

                        ChangePerk("StewardSevenVeterans", true, 1f,
                            "{VALUE} militia recruitment in the town/castle for every 50 levels of steward skill if the hero is the town/castle governer,\n{VALUE} militia recruitment in the town/castle for every 100 levels of steward skill if the hero is a town owner, \n{VALUE} militia recruitment in the town/castle for every 150 levels of steward skill if the hero is staying in town that belongs to his clan.(max +10)",
                            "{VALUE} militia recruitment in the town/castle for every 50 levels of steward skill if the hero is the town/castle governer,\n{VALUE} militia recruitment in the town/castle for every 100 levels of steward skill if the hero is a town owner.(max +10)"
                             , SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember);

                        #endregion
                        #region StewardStiffUpperLip (not tested)
                        //this._stewardStiffUpperLip.Initialize("{=QUeJ4gc3}Stiff Upper Lip", DefaultSkills.Steward, this.GetTierCost(3), this._stewardSweatshops, "{=y9AsEMnV}{VALUE}% food consumption in your party while it is part of an army.", SkillEffect.PerkRole.Quartermaster, -0.1f, SkillEffect.EffectIncrementType.AddFactor, "{=1FPpHasQ}{VALUE}% garrison wages in the governed castle.", SkillEffect.PerkRole.Governor, -0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);          
                        ChangePerk("StewardStiffUpperLip", false, -0.01f,
                           "Reduce party food consumption while it is part of an army by {VALUE}% for every 15 levels of steward skill if hero is the party quartermaster,\nReduce party food consumption while it is part of an army by {VALUE}% for every 100 levels of steward skill if hero is a party member. (max -30%)",
                           "Reduce party food consumption while it is part of an army by {VALUE}% for every 15 levels of steward skill if hero is the party quartermaster. (max -30%)",
                           SkillEffect.PerkRole.PartyMember);

                        ChangePerk("StewardStiffUpperLip", true, -0.01f,
                            "{VALUE}% garrison wages in the castle for every 20 levels of steward skill if the hero is the castle governer,\n{VALUE}% garrison wages in the castle for every 40 levels of steward skill if the hero is a castle owner, \n{VALUE}% garrison wages in the castle for every 100 levels of steward skill if the hero is staying in castle that belongs to his clan.(max -30%)",
                            "{VALUE}% garrison wages in the castle for every 20 levels of steward skill if the hero is the castle governer,\n{VALUE}% garrison wages in the castle for every 40 levels of steward skill if the hero is a castle owner.(max -30%)"
                            , SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember);

                        #endregion
                        #region StewardSweatshops (not tested)
                        //this._stewardSweatshops.Initialize("{=jbAtOsIy}Sweatshops", DefaultSkills.Steward, this.GetTierCost(3), this._stewardStiffUpperLip, "{=6wqJA77K}{VALUE}% production rate to owned workshops.", SkillEffect.PerkRole.Personal, 0.2f, SkillEffect.EffectIncrementType.AddFactor, "{=rA9nzrAr}{VALUE}% siege engine build rate in your party.", SkillEffect.PerkRole.Quartermaster, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        ChangePerk("StewardSweatshops", false, 0.01f,
                          "{VALUE}% production rate to owned workshops for every 10 levels of steward skill,\n{VALUE}% production rate to owned workshops for every 40 levels of steward skill if the hero is family member,\n{VALUE}% production rate to owned workshops for every 100 levels of steward skill if the hero is clan member. (max +100%)",
                          "{VALUE}% production rate to owned workshops for every 10 levels of steward skill. (max +100%)",
                          SkillEffect.PerkRole.Captain, SkillEffect.PerkRole.PartyMember);

                        ChangePerk("StewardSweatshops", true, 0.01f,
                           "{VALUE}% siege engine build rate in your party for every 10 levels of steward skill if hero is the party quartermaster,\n{VALUE}% siege engine build rate in your party for every 100 levels of steward skill if hero is a party member. (max +50%)",
                           "{VALUE}% siege engine build rate in your party for every 10 levels of steward skill if hero is the party quartermaster. (max +50%)",
                           SkillEffect.PerkRole.PartyMember);

                        #endregion
                        #region StewardPaidInPromise (not tested)
                        //this._stewardPaidInPromise.Initialize("{=CPxbG7Zp}Paid in Promise", DefaultSkills.Steward, this.GetTierCost(4), this._stewardEfficientCampaigner, "{=H9tQfeBr}{VALUE}% companion wages and recruitment fees.", SkillEffect.PerkRole.PartyLeader, -0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=1eKRHLur}Discarded armors are donated to troops for increased experience.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        ChangePerkRole("StewardPaidInPromise", SkillEffect.PerkRole.ClanLeader);
                        ChangePerk("StewardPaidInPromise", false, -0.01f,
                        "Reduce all clan companions wages and recruitment fees by {VALUE}% for every 20 levels of steward skill if the hero is a clan leader,\nReduce all clan companions wages and recruitment fees by {VALUE}% for every 100 levels of steward skill if the hero is a family member.,\nReduce the companion wages by {VALUE}% for every 30 levels of steward skill if the hero is a companion. (max -40%)",
                        "Reduce all clan companions wages and recruitment fees by {VALUE}% for every 20 levels of steward skill if the hero is a clan leader. (max -40%)"
                        , SkillEffect.PerkRole.Captain, SkillEffect.PerkRole.Personal);
                        #endregion
                        #region StewardEfficientCampaigner (not tested)
                        //this._stewardEfficientCampaigner.Initialize("{=sC53NYcA}Efficient Campaigner", DefaultSkills.Steward, this.GetTierCost(4), this._stewardPaidInPromise, "{=5t6cveXT}{VALUE} extra food for each food taken during village raids for your party.", SkillEffect.PerkRole.PartyLeader, 1f, SkillEffect.EffectIncrementType.Add, "{=JhFCoWbE}{VALUE}% troop wages in your party while it is part of an army.", SkillEffect.PerkRole.Quartermaster, -0.25f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        //ChangePerk("StewardEfficientCampaigner", false, 1f,
                        // "{VALUE} extra food for each food taken during village raids for every 50 levels of steward skill if the hero is party leader,\n{VALUE} extra food taken during village raids for every 150 levels of steward skill if the hero is party member. (max 10)",
                        // "{VALUE} extra food for each food taken during village raids for every 50 levels of steward skill. (max 10)",
                        //  SkillEffect.PerkRole.PartyMember);

                        ChangePerk("StewardEfficientCampaigner", true, -0.02f,
                         "While the party is part of an army reduce its wages by {VALUE}% for every 30 levels of steward skill if hero is the party quartermaster,\nWhile the party is part of an army reduce its wages by by {VALUE}% for every 100 levels of steward skill if hero is a party member. (max -40%)",
                         "while the party is part of an army reduce its wages by {VALUE}% for every 30 levels of steward skill if hero is the party quartermaster. (max -40%)"
                         , SkillEffect.PerkRole.PartyMember);

                        #endregion
                        #region
                        //this._stewardGivingHands.Initialize("{=VsqyzWYY}Giving Hands", DefaultSkills.Steward, this.GetTierCost(5), this._stewardLogistician, "{=WaGKvsfc}Discarded weapons are donated to troops for increased experience.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, "{=Eo958e7R}{VALUE}% tariff income in the governed settlement.", SkillEffect.PerkRole.Governor, 0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardLogistician.Initialize("{=U2buPiec}Logistician", DefaultSkills.Steward, this.GetTierCost(5), this._stewardGivingHands, "{=sG9WGOeN}{VALUE} party morale when number of mounts is greater than number of foot troops in your party.", SkillEffect.PerkRole.Quartermaster, 4f, SkillEffect.EffectIncrementType.Add, "{=Z1n0w5Kc}{VALUE}% tax income.", SkillEffect.PerkRole.Governor, 0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardRelocation.Initialize("{=R6dnhblo}Relocation", DefaultSkills.Steward, this.GetTierCost(6), this._stewardAidCorps, "{=urSSNtUD}{VALUE}% influence gain from donating troops.", SkillEffect.PerkRole.Quartermaster, 0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=XmqJb7RN}{VALUE}% effect from boosting projects in the governed settlement.", SkillEffect.PerkRole.Governor, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardAidCorps.Initialize("{=4FdtVyj1}Aid Corps", DefaultSkills.Steward, this.GetTierCost(6), this._stewardRelocation, "{=ZLbCqt23}Wounded troops in your party are no longer paid wages.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, "{=ULY7byYc}{VALUE}% hearth growth in villages bound to the governed settlement.", SkillEffect.PerkRole.Governor, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardGourmet.Initialize("{=63lHFDSG}Gourmet", DefaultSkills.Steward, this.GetTierCost(7), this._stewardSoundReserves, "{=KDtcsKUs}Double the morale bonus from having diverse food in your party.", SkillEffect.PerkRole.Quartermaster, 2f, SkillEffect.EffectIncrementType.AddFactor, "{=q2ZDAm2v}{VALUE}% garrison food consumption during sieges in the governed settlement.", SkillEffect.PerkRole.Governor, -0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardSoundReserves.Initialize("{=O5dgeoss}Sound Reserves", DefaultSkills.Steward, this.GetTierCost(7), this._stewardGourmet, "{=RkYL5eaP}{VALUE}% troop upgrade costs.", SkillEffect.PerkRole.Quartermaster, -0.1f, SkillEffect.EffectIncrementType.AddFactor, "{=P10E5o9l}{VALUE}% food consumption during sieges in your party.", SkillEffect.PerkRole.Quartermaster, -0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardForcedLabor.Initialize("{=cWyqiNrf}Forced Labor", DefaultSkills.Steward, this.GetTierCost(8), this._stewardContractors, "{=HrOTTjgo}Prisoners in your party provide carry capacity as if they are standard troops.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, "{=T9Viygs8}{VALUE}% construction speed per every 3 prisoners.", SkillEffect.PerkRole.Governor, 0.01f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardContractors.Initialize("{=Pg5enC8c}Contractors", DefaultSkills.Steward, this.GetTierCost(8), this._stewardForcedLabor, "{=4220dQ4j}{VALUE}% wages and upgrade costs of the mercenary troops in your party.", SkillEffect.PerkRole.Quartermaster, -0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=xiTD2qUv}{VALUE}% town project effects in the governed settlement.", SkillEffect.PerkRole.Governor, 0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardArenicosMules.Initialize("{=qBx8UbUt}Arenicos' Mules", DefaultSkills.Steward, this.GetTierCost(9), this._stewardArenicosHorses, "{=Yp4zv2ib}{VALUE}% carrying capacity for pack animals in your party.", SkillEffect.PerkRole.Quartermaster, 0.2f, SkillEffect.EffectIncrementType.AddFactor, "{=fswrp38u}{VALUE}% trade penalty for trading pack animals.", SkillEffect.PerkRole.Quartermaster, -0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardArenicosHorses.Initialize("{=tbQ5bUzD}Arenicos' Horses", DefaultSkills.Steward, this.GetTierCost(9), this._stewardArenicosMules, "{=G9OTNRs4}{VALUE}% carrying capacity for troops in your party.", SkillEffect.PerkRole.Quartermaster, 0.1f, SkillEffect.EffectIncrementType.AddFactor, "{=xm4eEbQY}{VALUE}% trade penalty for trading mounts.", SkillEffect.PerkRole.Personal, -0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardMasterOfPlanning.Initialize("{=n5aT1Y7s}Master of Planning", DefaultSkills.Steward, this.GetTierCost(10), this._stewardMasterOfWarcraft, "{=KMmAG5bk}{VALUE}% food consumption while your party is in a siege camp.", SkillEffect.PerkRole.Quartermaster, -0.4f, SkillEffect.EffectIncrementType.AddFactor, "{=P5OjioRl}{VALUE}% effectiveness to continuous projects in the governed settlement. ", SkillEffect.PerkRole.Governor, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardMasterOfWarcraft.Initialize("{=MM0ARhGh}Master of Warcraft", DefaultSkills.Steward, this.GetTierCost(10), this._stewardMasterOfPlanning, "{=StzVsQ2P}{VALUE}% troop wages while your party is in a siege camp.", SkillEffect.PerkRole.Quartermaster, -0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=ya7alenH}{VALUE}% food consumption of town population in the governed settlement.", SkillEffect.PerkRole.Governor, -0.05f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #region
                        //this._stewardPriceOfLoyalty.Initialize("{=eVTnUmSB}Price of Loyalty", DefaultSkills.Steward, this.GetTierCost(11), null, "{=sYrG8rNy}{VALUE}% to food consumption, wages and combat related morale loss for each steward point above 250 in your party.", SkillEffect.PerkRole.Quartermaster, -0.005f, SkillEffect.EffectIncrementType.AddFactor, "{=lwp50FuF}{VALUE}% tax income for each skill point above 200 in the governed settlement", SkillEffect.PerkRole.Governor, 0.005f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                        #endregion
                        #endregion
                    }
                }
            }
            private static void ChangePerkRequirement(string perkId, int tierIndex)
            {
                var perk = AllPerks.FirstOrDefault(d => d.StringId == perkId);
                if (perk != null)
                {
                    perk.SetPrivatePropertyValue("RequiredSkillValue", (float)BKPerks.GetTierCost(tierIndex));
                    perk.AlternativePerk?.SetPrivatePropertyValue("RequiredSkillValue", (float)BKPerks.GetTierCost(tierIndex));
                }
            }
            private static void ChangePerkRole(string perkId, SkillEffect.PerkRole newRole , bool isSecondary=false)
            {
                var perk = AllPerks.FirstOrDefault(d => d.StringId == perkId);
                if (perk != null)
                {
                    if (isSecondary)
                    {
                        perk.SetPrivatePropertyValue("SecondaryRole", newRole);
                    }
                    else
                    {
                        perk.SetPrivatePropertyValue("PrimaryRole", newRole);
                    }                                     
                }
            }
            private static void ChangePerk(string perkId, bool isSecondary, float bonus, string description1, string description2, params SkillEffect.PerkRole[] additionalSecondaryRoles)
            {
                var perk = AllPerks.FirstOrDefault(d => d.StringId == perkId);
                if (perk != null)
                {
                    if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
                    {
                        if (isSecondary)
                        {
                            perk.SetPrivatePropertyValue("SecondaryBonus", bonus);
                            perk.SetPrivatePropertyValue("SecondaryDescription", new TextObject(description1, null));
                            PerkHelper.SetDescriptionTextVariable(perk.SecondaryDescription, perk.SecondaryBonus, perk.SecondaryIncrementType);
                            PerksAdditionalSecondaryRoles.Add(perkId, additionalSecondaryRoles.ToList());
                        }
                        else
                        {
                            perk.SetPrivatePropertyValue("PrimaryBonus", bonus);
                            perk.SetPrivatePropertyValue("PrimaryDescription", new TextObject(description1, null));
                            PerkHelper.SetDescriptionTextVariable(perk.PrimaryDescription, perk.PrimaryBonus, perk.PrimaryIncrementType);
                            PerksAdditionalPrimaryRoles.Add(perkId, additionalSecondaryRoles.ToList());
                        }
                    }
                    else
                    {
                        if (isSecondary)
                        {
                            perk.SetPrivatePropertyValue("SecondaryBonus", bonus);
                            perk.SetPrivatePropertyValue("SecondaryDescription", new TextObject(description2, null));
                            PerkHelper.SetDescriptionTextVariable(perk.SecondaryDescription, perk.SecondaryBonus, perk.SecondaryIncrementType);
                            PerksAdditionalSecondaryRoles.Add(perkId, additionalSecondaryRoles.ToList());
                        }
                        else
                        {
                            perk.SetPrivatePropertyValue("PrimaryBonus", bonus);
                            perk.SetPrivatePropertyValue("PrimaryDescription", new TextObject(description2, null));
                            PerkHelper.SetDescriptionTextVariable(perk.PrimaryDescription, perk.PrimaryBonus, perk.PrimaryIncrementType);
                            PerksAdditionalPrimaryRoles.Add(perkId, additionalSecondaryRoles.ToList());
                        }
                    }
                }
            }
        }
    }
}
