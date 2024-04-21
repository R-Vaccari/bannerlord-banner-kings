using HarmonyLib;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using BannerKings.Utils;
using BannerKings.Settings;
using TaleWorlds.Library;
using BannerKings.Managers.Skills;
using static BannerKings.Utils.PerksHelpers;
using BannerKings.Patches.Perks;

namespace BannerKings.Patches
{
    internal partial class PerksAndSkillsPatches
    {
        #region DefaultPerks.Steward
        public static Dictionary<string, PerkData> StewardPerksData { get; set; } = new Dictionary<string, PerkData>()
            {
                //Steward.Frugal               
                {"StewardFrugal",
                   //this._stewardFrugal.Initialize("{=eJIbMa8P}Frugal", DefaultSkills.Steward, this.GetTierCost(1), this._stewardWarriorsDiet, "{=CJB5HCsI}{VALUE} wages in your party.", SkillEffect.PerkRole.Quartermaster, -0.05f, SkillEffect.EffectIncrementType.AddFactor, "{=OTyYJ2Bt}{VALUE} recruitment costs.", SkillEffect.PerkRole.PartyLeader, -0.15f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                   new PerkData (){
                        PrimaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} party wages for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} party wages for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                      Description2 = "{VALUE} party wages for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyPartyLeader,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} recruitment costs for every {EVERYSKILLMAIN} steward point if the hero is the party leader,\n{VALUE} recruitment costs for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                      Description2 = "{VALUE} recruitment costs for every {EVERYSKILLMAIN} steward point if the hero is the party leader. (max {MINMAXVALUE})"}
                   }
                },
                //Steward.WarriorsDiet                
                {"StewardWarriorsDiet",
                   //this._stewardWarriorsDiet.Initialize("{=mIDsxe1O}Warrior's Diet", DefaultSkills.Steward, this.GetTierCost(1), this._stewardFrugal, "{=6NHvsrrx}{VALUE} food consumption in your party.", SkillEffect.PerkRole.Quartermaster, -0.1f, SkillEffect.EffectIncrementType.AddFactor, "{=mSvfxXVW}No morale penalty from having single type of food.", SkillEffect.PerkRole.PartyLeader, 0f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                   new PerkData(){
                        PrimaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =15 ,EverySkillSecondary = 15 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.Quartermaster },
                                      Description1 = "{VALUE} party food consumption for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} party food consumption for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                      Description2 = "{VALUE} party food consumption for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})" }
                   }
                },
                //Steward.DrillSergant               
                {"StewardDrillSergant",
                  //this._stewardSevenVeterans.Initialize("{=2ryLuN2i}Seven Veterans", DefaultSkills.Steward, this.GetTierCost(2), this._stewardDrillSergant, "{=gX0edfpK}{VALUE} daily experience for tier 4+ troops in your party.", SkillEffect.PerkRole.Quartermaster, 4f, SkillEffect.EffectIncrementType.Add, "{=g9gTYB8u}{VALUE} militia recruitment in the governed settlement.", SkillEffect.PerkRole.Governor, 1f, SkillEffect.EffectIncrementType.Add, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 1f ,MinBonus=0 ,MaxBonus = 30 ,EverySkillMain =25 ,EverySkillSecondary = 25 ,EverySkillOthers = 100 ,SkillScale = SkillScale.Both,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyLeader, SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} daily experience to troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader,\n{VALUE} daily experience to troops in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                      Description2 = "{VALUE} daily experience to troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader. (max {MINMAXVALUE})",
                                               },
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 40 ,EverySkillOthers = 100 ,SkillScale = SkillScale.Other,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} garrison wages in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} garrison wages in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} garrison wages in the settlement for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                      Description2 = "{VALUE} garrison wages in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} garrison wages in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner.(max {MINMAXVALUE})"}}
                },
                //Steward.SevenVeterans
                {"StewardSevenVeterans",
                   //this._stewardSevenVeterans.Initialize("{=2ryLuN2i}Seven Veterans", DefaultSkills.Steward, this.GetTierCost(2), this._stewardDrillSergant, "{=gX0edfpK}{VALUE} daily experience for tier 4+ troops in your party.", SkillEffect.PerkRole.Quartermaster, 4f, SkillEffect.EffectIncrementType.Add, "{=g9gTYB8u}{VALUE} militia recruitment in the governed settlement.", SkillEffect.PerkRole.Governor, 1f, SkillEffect.EffectIncrementType.Add, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                   new PerkData(){
                        PrimaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 2f ,MinBonus=0 ,MaxBonus = 60 ,EverySkillMain =25 ,EverySkillSecondary = 25 ,EverySkillOthers = 100 ,SkillScale = SkillScale.Both,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyLeader, SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} daily experience to tier 4+ troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader,\n{VALUE} daily experience to tier 4+ troops in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                      Description2 = "{VALUE} daily experience to tier 4+ troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader. (max {MINMAXVALUE})",
                                                                      },
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 1f ,MinBonus=0 ,MaxBonus = 10 ,EverySkillMain =50 ,EverySkillSecondary = 100 ,EverySkillOthers = 150 ,SkillScale = SkillScale.Other,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} militia recruitment in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} militia recruitment in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} militia recruitment in the settlement for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                      Description2 = "{VALUE} militia recruitment in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement"}}
                },
                //Steward.StiffUpperLip
                {"StewardStiffUpperLip",
                  //this._stewardStiffUpperLip.Initialize("{=QUeJ4gc3}Stiff Upper Lip", DefaultSkills.Steward, this.GetTierCost(3), this._stewardSweatshops, "{=y9AsEMnV}{VALUE} food consumption in your party while it is part of an army.", SkillEffect.PerkRole.Quartermaster, -0.1f, SkillEffect.EffectIncrementType.AddFactor, "{=1FPpHasQ}{VALUE} garrison wages in the governed castle.", SkillEffect.PerkRole.Governor, -0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);          
                  new PerkData(){
                        PrimaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =15 ,EverySkillSecondary = 15 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} party food consumption while it is part of an army for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} party food consumption while it is part of an army for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                      Description2 = "{VALUE} party food consumption while it is part of an army for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})" },
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 40 ,EverySkillOthers = 100 ,SkillScale = SkillScale.Other,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} garrison wages in the castle for every {EVERYSKILLMAIN} steward point if the hero is the castle governer,\n{VALUE} garrison wages in the castle for every {EVERYSKILLSECONDARY} steward point if the hero is the castle owner, \n{VALUE} garrison wages in the castle for every {EVERYSKILLOTHERS} steward point if the hero is staying in castle that belongs to his clan.(max {MINMAXVALUE})",
                                      Description2 = "{VALUE} garrison wages in the castle for every {EVERYSKILLMAIN} steward point if the"}}
                },              
                //Steward.Sweatshops                
                {"StewardSweatshops",
                  //this._stewardSweatshops.Initialize("{=jbAtOsIy}Sweatshops", DefaultSkills.Steward, this.GetTierCost(3), this._stewardStiffUpperLip, "{=6wqJA77K}{VALUE} production rate to owned workshops.", SkillEffect.PerkRole.Personal, 0.2f, SkillEffect.EffectIncrementType.AddFactor, "{=rA9nzrAr}{VALUE} siege engine build rate in your party.", SkillEffect.PerkRole.Quartermaster, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 1f ,EverySkillMain =10 ,EverySkillSecondary = 40 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                      Description1 = "{VALUE} production rate to owned workshops for every {EVERYSKILLMAIN} steward point,\n{VALUE} production rate to owned workshops for every {EVERYSKILLSECONDARY} steward point if the hero is a family member,\n{VALUE} production rate to owned workshops for every {EVERYSKILLOTHERS} steward point if the hero is clan member. (max {MINMAXVALUE})",
                                      Description2 = "{VALUE} production rate to owned workshops for every {EVERYSKILLMAIN} steward point,\n{VALUE} production rate to owned workshops for every {EVERYSKILLSECONDARY} steward point if the hero is a family member. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.5f ,EverySkillMain =10 ,EverySkillSecondary = 10 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                      Description1 = "Increase siege engine build rate in your party by {VALUE} for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\nIncrease siege engine build rate in your party by {VALUE} for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                      Description2 = "Increase siege engine build rate in your party by {VALUE} for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"}}
                },
                //Steward.PaidInPromise               
                {"StewardPaidInPromise",
                  //this._stewardPaidInPromise.Initialize("{=CPxbG7Zp}Paid in Promise", DefaultSkills.Steward, this.GetTierCost(4), this._stewardEfficientCampaigner, "{=H9tQfeBr}{VALUE} companion wages and recruitment fees.", SkillEffect.PerkRole.PartyLeader, -0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=1eKRHLur}Discarded armors are donated to troops for increased experience.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.4f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 100 ,EverySkillOthers = 30 ,SkillScale = SkillScale.Other,Role =SkillEffect.PerkRole.ClanLeader,
                                                             AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.ClanLeader, SkillEffect.PerkRole.PartyMember },
                                                             Description1 = "Reduce all clan companions wages and recruitment fees by {VALUE} for every {EVERYSKILLMAIN} steward point if the hero is a clan leader,\nReduce all clan companions wages and recruitment fees by {VALUE} for every {EVERYSKILLOTHERS} steward point if the hero is a family member,\nReduce the companion wages by {VALUE} for every {EVERYSKILLOTHERS} steward point if the hero is the companion. (max {MINMAXVALUE})",
                                                             Description2 = "Reduce all clan companions wages and recruitment fees by {VALUE} for every {EVERYSKILLMAIN} steward point if the hero is a clan leader,\nReduce all clan companions wages and recruitment fees by {VALUE} for every {EVERYSKILLOTHERS} steward point if the hero is a family member. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.02f ,MinBonus=0 ,MaxBonus = 1.5f ,EverySkillMain =10 ,EverySkillSecondary = 10 ,EverySkillOthers = 50 ,SkillScale = SkillScale.Both,
                                                             AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyLeader },
                                                             Description1 = "Discarded armors are donated to troops for increased experience.\n{VALUE} bonus experience from donated armors for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader,\n{VALUE} bonus experience from donated armors for every {EVERYSKILLOTHERS} steward point if the hero is a party member.(max {MINMAXVALUE})",
                                                             Description2 = "Discarded armors are donated to troops for increased experience.\n{VALUE} bonus experience from donated armors for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader.(max {MINMAXVALUE})"}
                                                         }
                }, 
                //Steward.ForeseeableFuture               
                {"StewardForeseeableFuture",
                  //this._stewardEfficientCampaigner.Initialize("{=sC53NYcA}Efficient Campaigner", DefaultSkills.Steward, this.GetTierCost(4), this._stewardPaidInPromise, "{=5t6cveXT}{VALUE} extra food for each food taken during village raids for your party.", SkillEffect.PerkRole.PartyLeader, 1f, SkillEffect.EffectIncrementType.Add, "{=JhFCoWbE}{VALUE} troop wages in your party while it is part of an army.", SkillEffect.PerkRole.Quartermaster, -0.25f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.02f ,MinBonus=0 ,MaxBonus = 1.5f ,EverySkillMain =10 ,EverySkillSecondary = 10 ,EverySkillOthers = 50 ,SkillScale = SkillScale.Both,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "Discarded weapons are donated to troops for increased experience.\n{VALUE} bonus experience from donated weapons for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader,\n{VALUE} bonus experience from donated weapons for every {EVERYSKILLOTHERS} steward point if the hero is a party member.(max {MINMAXVALUE})",
                                       Description2 = "Discarded weapons are donated to troops for increased experience"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.3f ,EverySkillMain =30 ,EverySkillSecondary = 90 ,EverySkillOthers = 120 ,SkillScale = SkillScale.Other,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} tariff income in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} tariff income in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} tariff income in the settlement for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                       Description2 = "{VALUE} tariff income in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} tariff income in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner.(max {MINMAXVALUE})" }}
                },
                //Steward.EfficientCampaigner               
                {"StewardEfficientCampaigner",
                  //this._stewardGivingHands.Initialize("{=VsqyzWYY}Giving Hands", DefaultSkills.Steward, this.GetTierCost(5), this._stewardLogistician, "{=WaGKvsfc}Discarded weapons are donated to troops for increased experience.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, "{=Eo958e7R}{VALUE} tariff income in the governed settlement.", SkillEffect.PerkRole.Governor, 0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        SecondaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.02f ,MinBonus=-0.4f ,MaxBonus = 0 ,EverySkillMain =30 ,EverySkillSecondary = 30 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "While the party is part of an army reduce its wages by {VALUE} for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\nWhile the party is part of an army reduce its wages by {VALUE} for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "while the party is part of an army reduce its wages by {VALUE} for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"}}
                },
                //Steward.Logistician                
                {"StewardLogistician",
                  //this._stewardLogistician.Initialize("{=U2buPiec}Logistician", DefaultSkills.Steward, this.GetTierCost(5), this._stewardGivingHands, "{=sG9WGOeN}{VALUE} party morale when number of mounts is greater than number of foot troops in your party.", SkillEffect.PerkRole.Quartermaster, 4f, SkillEffect.EffectIncrementType.Add, "{=Z1n0w5Kc}{VALUE} tax income.", SkillEffect.PerkRole.Governor, 0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 1 ,MinBonus=0 ,MaxBonus = 20f ,EverySkillMain =60 ,EverySkillSecondary = 60 ,EverySkillOthers = 150 ,SkillScale = SkillScale.Both,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyLeader, SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} party morale when number of mounts is greater than number of foot troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader,\n{VALUE} party morale when number of mounts is greater than number of foot troops in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "{VALUE} party morale when number of mounts is greater than number of foot troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster or the party leader. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.3f ,EverySkillMain =30 ,EverySkillSecondary = 90 ,EverySkillOthers = 120 ,SkillScale = SkillScale.Other,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} tax income in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} tax income in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} tax income in the settlement for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                       Description2 = "{VALUE} tax income in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} tax income in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner.(max {MINMAXVALUE})"}
                                     }
                },
                //Steward.Relocation
                {"StewardRelocation",
                  //this._stewardRelocation.Initialize("{=R6dnhblo}Relocation", DefaultSkills.Steward, this.GetTierCost(6), this._stewardAidCorps, "{=urSSNtUD}{VALUE} influence gain from donating troops.", SkillEffect.PerkRole.Quartermaster, 0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=XmqJb7RN}{VALUE} effect from boosting projects in the governed settlement.", SkillEffect.PerkRole.Governor, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.5f ,EverySkillMain =10 ,EverySkillSecondary = 10 ,EverySkillOthers = 50 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} influence gain from donating troops for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} influence gain from donating troops for every {EVERYSKILLOTHERS} steward point if the hero is a party member.(max {MINMAXVALUE})",
                                       Description2 = "{VALUE} influence gain from donating troops for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.4f ,EverySkillMain =15 ,EverySkillSecondary = 50 ,EverySkillOthers = 80 ,SkillScale = SkillScale.Other,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} effect from boosting projects for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} effect from boosting projects for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} effect from boosting projects for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                        Description2 = "{VALUE} effect from boosting projects for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} effect from boosting projects for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner.(max {MINMAXVALUE})"}}
                },
                //Steward.AidCorps                
                {"StewardAidCorps",
                  //this._stewardAidCorps.Initialize("{=4FdtVyj1}Aid Corps", DefaultSkills.Steward, this.GetTierCost(6), this._stewardRelocation, "{=ZLbCqt23}Wounded troops in your party are no longer paid wages.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, "{=ULY7byYc}{VALUE} hearth growth in villages bound to the governed settlement.", SkillEffect.PerkRole.Governor, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        SecondaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.4f ,EverySkillMain =15 ,EverySkillSecondary = 50 ,EverySkillOthers = 80 ,SkillScale = SkillScale.Other,
                                         AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                         Description1 = "{VALUE} hearth growth in villages bound to the settlement for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} hearth growth for every {EVERYSKILLOTHERS} steward point in villages bound to the settlement owned by the hero, \n{VALUE} hearth growth for every {EVERYSKILLOTHERS} steward point in villages bound to the settlement where the hero is staying if settlement belong to his clan.(max {MINMAXVALUE})",
                                         Description2 = "{VALUE} hearth growth in villages bound to the settlement for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} hearth growth for every {EVERYSKILLOTHERS} steward point in villages bound to the settlement owned by the hero.(max {MINMAXVALUE})"}}
                 },
                //Steward.Gourmet             
                {"StewardGourmet",
                  //this._stewardGourmet.Initialize("{=63lHFDSG}Gourmet", DefaultSkills.Steward, this.GetTierCost(7), this._stewardSoundReserves, "{=KDtcsKUs}Double the morale bonus from having diverse food in your party.", SkillEffect.PerkRole.Quartermaster, 2f, SkillEffect.EffectIncrementType.AddFactor, "{=q2ZDAm2v}{VALUE} garrison food consumption during sieges in the governed settlement.", SkillEffect.PerkRole.Governor, -0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.05f ,MinBonus=0 ,MaxBonus = 1f ,EverySkillMain =0 ,EverySkillSecondary = 0 ,EverySkillOthers = 20 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "Double the morale bonus from having diverse food in your party if the hero is the party quartermaster,\n{VALUE} morale bonus from having diverse food in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "Double the morale bonus from having diverse food in your party if the hero is the party quartermaster."},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =15 ,EverySkillSecondary = 60 ,EverySkillOthers = 90 ,SkillScale = SkillScale.Other,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} garrison food consumption during sieges in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} garrison food consumption during sieges in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} garrison food consumption during sieges in the settlement for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                        Description2 = "{VALUE} garrison food consumption during sieges in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} garrison food consumption during sieges in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner.(max {MINMAXVALUE})"}}
                },
                //Steward.SoundReserves                
                {"StewardSoundReserves",
                  //this._stewardSoundReserves.Initialize("{=O5dgeoss}Sound Reserves", DefaultSkills.Steward, this.GetTierCost(7), this._stewardGourmet, "{=RkYL5eaP}{VALUE} troop upgrade costs.", SkillEffect.PerkRole.Quartermaster, -0.1f, SkillEffect.EffectIncrementType.AddFactor, "{=P10E5o9l}{VALUE} food consumption during sieges in your party.", SkillEffect.PerkRole.Quartermaster, -0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} troop upgrade costs for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} troop upgrade costs for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "{VALUE} troop upgrade costs for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} food consumption during sieges in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} food consumption during sieges in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                        Description2 = "{VALUE} food consumption during sieges in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"}}
                },
                //Steward.ForcedLabor 
                {"StewardForcedLabor",
                  //this._stewardForcedLabor.Initialize("{=cWyqiNrf}Forced Labor", DefaultSkills.Steward, this.GetTierCost(8), this._stewardContractors, "{=HrOTTjgo}Prisoners in your party provide carry capacity as if they are standard troops.", SkillEffect.PerkRole.Quartermaster, 0f, SkillEffect.EffectIncrementType.AddFactor, "{=T9Viygs8}{VALUE} construction speed per every 3 prisoners.", SkillEffect.PerkRole.Governor, 0.01f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.3f ,EverySkillMain =10 ,EverySkillSecondary = 10 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "Prisoners in your party provide carry capacity as if they are standard troops.\n{VALUE} extra prisoners carry capacity for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} extra prisoners carry capacity for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "Prisoners in your party provide carry capacity as if they are standard troops.\n{VALUE} extra prisoners carry capacity for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 2f ,EverySkillMain =30 ,EverySkillSecondary = 90 ,EverySkillOthers = 120 ,SkillScale = SkillScale.Other,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} construction speed per 5 prisoners for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} construction speed per 5 prisoners for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} construction speed per 5 prisoners for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                        Description2 = "{VALUE} construction speed per 5 prisoners for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer.(max {MINMAXVALUE})"}}
                },
                //Steward.Contractors
                {"StewardContractors",
                  //this._stewardContractors.Initialize("{=Pg5enC8c}Contractors", DefaultSkills.Steward, this.GetTierCost(8), this._stewardForcedLabor, "{=4220dQ4j}{VALUE} wages and upgrade costs of the mercenary troops in your party.", SkillEffect.PerkRole.Quartermaster, -0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=xiTD2qUv}{VALUE} town project effects in the governed settlement.", SkillEffect.PerkRole.Governor, 0.1f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);     
                  new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} wages and upgrade costs of the mercenary troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} wages and upgrade costs of the mercenary troops in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "{VALUE} wages and upgrade costs of the mercenary troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.3f ,EverySkillMain =30 ,EverySkillSecondary = 90 ,EverySkillOthers = 120 ,SkillScale = SkillScale.Other,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} town project effects in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} town project effects in the settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner, \n{VALUE} town project effects in the settlement for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan.(max {MINMAXVALUE})",
                                        Description2 = "{VALUE} town project effects in the settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer.(max {MINMAXVALUE})"}}
                },
                //Steward.ArenicosMules
                {"StewardArenicosMules",
                //this._stewardArenicosMules.Initialize("{=qBx8UbUt}Arenicos' Mules", DefaultSkills.Steward, this.GetTierCost(9), this._stewardArenicosHorses, "{=Yp4zv2ib}{VALUE} carrying capacity for pack animals in your party.", SkillEffect.PerkRole.Quartermaster, 0.2f, SkillEffect.EffectIncrementType.AddFactor, "{=fswrp38u}{VALUE} trade penalty for trading pack animals.", SkillEffect.PerkRole.Quartermaster, -0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);      
                new PerkData{
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.8f ,EverySkillMain =10 ,EverySkillSecondary = 10 ,EverySkillOthers = 50 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} carrying capacity for pack animals in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} carrying capacity for pack animals in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "{VALUE} carrying capacity for pack animals in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} trade penalty for trading pack animals for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} trade penalty for trading pack animals for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                        Description2 = "{VALUE} trade penalty for trading pack animals for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"}}
                },
                //Steward.ArenicosHorses
                {"StewardArenicosHorses",
                //this._stewardArenicosHorses.Initialize("{=tbQ5bUzD}Arenicos' Horses", DefaultSkills.Steward, this.GetTierCost(9), this._stewardArenicosMules, "{=G9OTNRs4}{VALUE} carrying capacity for troops in your party.", SkillEffect.PerkRole.Quartermaster, 0.1f, SkillEffect.EffectIncrementType.AddFactor, "{=xm4eEbQY}{VALUE} trade penalty for trading mounts.", SkillEffect.PerkRole.Personal, -0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);         
                new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.4f ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} carrying capacity for troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} carrying capacity for troops in your party for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "{VALUE} carrying capacity for troops in your party for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =20 ,EverySkillSecondary = 20 ,EverySkillOthers = 100 ,SkillScale = SkillScale.OnlyQuartermaster,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                        Role = SkillEffect.PerkRole.Quartermaster,
                                        Description1 = "{VALUE} trade penalty for trading mounts for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} trade penalty for trading mounts for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                        Description2 = "{VALUE} trade penalty for trading mounts for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"}}
                },
                //Steward.MasterOfPlanning
                {"StewardMasterOfPlanning",
                //this._stewardMasterOfPlanning.Initialize("{=n5aT1Y7s}Master of Planning", DefaultSkills.Steward, this.GetTierCost(10), this._stewardMasterOfWarcraft, "{=KMmAG5bk}{VALUE} food consumption while your party is in a siege camp.", SkillEffect.PerkRole.Quartermaster, -0.4f, SkillEffect.EffectIncrementType.AddFactor, "{=P5OjioRl}{VALUE} effectiveness to continuous projects in the governed settlement. ", SkillEffect.PerkRole.Governor, 0.2f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);          
                new PerkData{
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.5f ,MaxBonus = 0 ,EverySkillMain =15 ,EverySkillSecondary = 15 ,EverySkillOthers = 90 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} food consumption while your party is in a siege camp for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} food consumption while your party is in a siege camp for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "{VALUE} food consumption while your party is in a siege camp for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.01f ,MinBonus=0 ,MaxBonus = 0.4f ,EverySkillMain =15 ,EverySkillSecondary = 15 ,EverySkillOthers = 90 ,SkillScale = SkillScale.Other,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner,SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} effectiveness to continuous projects in the governed settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} effectiveness to continuous projects for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner,\n{VALUE} effectiveness to continuous projects for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan. (max {MINMAXVALUE})",
                                        Description2 = "{VALUE} effectiveness to continuous projects in the governed settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} effectiveness to continuous projects for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner. (max {MINMAXVALUE})"}}
                },                              
                //Steward.MasterOfWarcraft
                {"StewardMasterOfWarcraft",
                //this._stewardMasterOfWarcraft.Initialize("{=MM0ARhGh}Master of Warcraft", DefaultSkills.Steward, this.GetTierCost(10), this._stewardMasterOfPlanning, "{=StzVsQ2P}{VALUE} troop wages while your party is in a siege camp.", SkillEffect.PerkRole.Quartermaster, -0.25f, SkillEffect.EffectIncrementType.AddFactor, "{=ya7alenH}{VALUE} food consumption of town population in the governed settlement.", SkillEffect.PerkRole.Governor, -0.05f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);          
                new PerkData(){
                        PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.3f ,MaxBonus = 0 ,EverySkillMain =30 ,EverySkillSecondary = 30 ,EverySkillOthers = 120 ,SkillScale = SkillScale.OnlyQuartermaster,
                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                       Description1 = "{VALUE} troop wages while your party is in a siege camp for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster,\n{VALUE} troop wages while your party is in a siege camp for every {EVERYSKILLOTHERS} steward point if the hero is a party member. (max {MINMAXVALUE})",
                                       Description2 = "{VALUE} troop wages while your party is in a siege camp for every {EVERYSKILLMAIN} steward point if the hero is the party quartermaster. (max {MINMAXVALUE})"},
                        SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.01f ,MinBonus=-0.15f ,MaxBonus = 0 ,EverySkillMain =50 ,EverySkillSecondary = 100 ,EverySkillOthers = 150 ,SkillScale = SkillScale.Other,
                                        AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                        Description1 = "{VALUE} food consumption of town population in the governed settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} food consumption of town population in the governed settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner,\n{VALUE} food consumption of town population for every {EVERYSKILLOTHERS} steward point if the hero is staying in a settlement that belongs to his clan. (max {MINMAXVALUE})",
                                        Description2 = "{VALUE} food consumption of town population in the governed settlement for every {EVERYSKILLMAIN} steward point if the hero is the settlement governer,\n{VALUE} food consumption of town population in the governed settlement for every {EVERYSKILLSECONDARY} steward point if the hero is the settlement owner. (max {MINMAXVALUE})"}}
                },
                //Steward.PriceOfLoyalty
                {"StewardPriceOfLoyalty",
                //this._stewardPriceOfLoyalty.Initialize("{=eVTnUmSB}Price of Loyalty", DefaultSkills.Steward, this.GetTierCost(11), null, "{=sYrG8rNy}{VALUE} to food consumption, wages and combat related morale loss for each steward point above 250 in your party.", SkillEffect.PerkRole.Quartermaster, -0.005f, SkillEffect.EffectIncrementType.AddFactor, "{=lwp50FuF}{VALUE} tax income for each skill point above 200 in the governed settlement", SkillEffect.PerkRole.Governor, 0.005f, SkillEffect.EffectIncrementType.AddFactor, TroopUsageFlags.Undefined, TroopUsageFlags.Undefined);
                new PerkData{
                    PrimaryPerk  = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = -0.005f ,MinBonus=-20f ,MaxBonus = 0 ,EverySkillMain =1 ,EverySkillSecondary = 10 ,EverySkillOthers = 30 ,StartSkillLevel=200,SkillScale = SkillScale.OnlyQuartermaster,
                                                      AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyMember },
                                                      Description1 = "{VALUE} to food consumption, wages and combat related morale loss for each steward point above {STARTSKILLLEVEL} in your party if the hero is the party quartermaster,\n{VALUE} to food consumption and wages for each {EVERYSKILLSECONDARY} steward point above {STARTSKILLLEVEL} in your party if the hero is a party member.",
                                                      Description2 = "{VALUE} to food consumption, wages and combat related morale loss for each steward point above {STARTSKILLLEVEL} in your party if the hero is the party quartermaster."},
                    SecondaryPerk = new PerkSubData(){ ScaleOnSkill =DefaultSkills.Steward ,BonusEverySkill = 0.005f ,MinBonus=0 ,MaxBonus = 20 ,EverySkillMain =1 ,EverySkillSecondary = 1 ,EverySkillOthers = 10 ,StartSkillLevel=200,SkillScale = SkillScale.Other,
                                                       AdditionalRoles = new List<SkillEffect.PerkRole>(){ SkillEffect.PerkRole.PartyOwner, SkillEffect.PerkRole.PartyMember },
                                                       Description1 = "{VALUE} tax income for each {EVERYSKILLMAIN} steward point above {STARTSKILLLEVEL} in the governed settlement by the hero,\n{VALUE} tax income for each {EVERYSKILLSECONDARY} steward point above {STARTSKILLLEVEL} in the settlement if the hero is the settlement owner,\n{VALUE} tax income for each {EVERYSKILLSECONDARY} steward point above {STARTSKILLLEVEL} if the hero is staying in a settlement that belongs to his clan.",
                                                       Description2 = "{VALUE} tax income for each {EVERYSKILLMAIN} steward point above {STARTSKILLLEVEL} in the governed settlement by the hero,\n{VALUE} tax income for each {EVERYSKILLSECONDARY} steward point above {STARTSKILLLEVEL} in the settlement if the hero is the settlement owner."}}
                },
            };

        #endregion

        [HarmonyPatch(typeof(DefaultPerks), "RegisterAll")]
        class RegisterPerkPatch
        {
            public static MBReadOnlyList<PerkObject> AllPerks => Game.Current.ObjectManager.GetObjectTypeList<PerkObject>();

            static void Postfix()
            {
                if (BannerKingsSettings.Instance.EnableUsefulPerks)
                {

                    MakeStewardPerksBetter();
                }
            }
            private static void MakeStewardPerksBetter()
            {
                if (BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    foreach (var perkData in StewardPerksData)
                    {
                        var perk = AllPerks.FirstOrDefault(p => p.StringId == perkData.Key);
                        if (perk != null)
                        {
                            perkData.Value.ChangePerk(perk);
                        }
                    }
                }
            }
            private static void ChangePerkRequirement(string perkId, int tierIndex)
            {
                var perk = AllPerks.FirstOrDefault(d => d.StringId == perkId);
                if (perk != null)
                {
                    ChangePerkRequirement(perk, tierIndex);
                }
            }
            private static void ChangePerkRequirement(PerkObject perk, int tierIndex)
            {
                if (perk != null)
                {
                    perk.SetPrivatePropertyValue("RequiredSkillValue", (float)BKPerks.GetTierCost(tierIndex));
                    perk.AlternativePerk?.SetPrivatePropertyValue("RequiredSkillValue", (float)BKPerks.GetTierCost(tierIndex));
                }
            }

            private static void ChangePerk(string perkId, bool isSecondary, float bonus, string description1, string description2, params SkillEffect.PerkRole[] additionalSecondaryRoles)
            {
                var perk = AllPerks.FirstOrDefault(d => d.StringId == perkId);
                if (perk != null)
                {
                    ChangePerk(perk, isSecondary, bonus, description1, description2, additionalSecondaryRoles);
                }
            }
            private static void ChangePerk(PerkObject perk, bool isSecondary, float bonus, string description1, string description2, params SkillEffect.PerkRole[] additionalSecondaryRoles)
            {
                if (perk != null)
                {
                    if (BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
                    {
                        if (isSecondary)
                        {
                            perk.SetPrivatePropertyValue("SecondaryBonus", bonus);
                            perk.SetPrivatePropertyValue("SecondaryDescription", new TextObject(description1, null));
                            PerkHelper.SetDescriptionTextVariable(perk.SecondaryDescription, perk.SecondaryBonus, perk.SecondaryIncrementType);
                            // PerksAdditionalSecondaryRoles.Add(perk.StringId, additionalSecondaryRoles.ToList());
                        }
                        else
                        {
                            perk.SetPrivatePropertyValue("PrimaryBonus", bonus);
                            perk.SetPrivatePropertyValue("PrimaryDescription", new TextObject(description1, null));
                            PerkHelper.SetDescriptionTextVariable(perk.PrimaryDescription, perk.PrimaryBonus, perk.PrimaryIncrementType);
                            // PerksAdditionalPrimaryRoles.Add(perk.StringId, additionalSecondaryRoles.ToList());
                        }
                    }
                    else
                    {
                        if (isSecondary)
                        {
                            perk.SetPrivatePropertyValue("SecondaryBonus", bonus);
                            perk.SetPrivatePropertyValue("SecondaryDescription", new TextObject(description2, null));
                            PerkHelper.SetDescriptionTextVariable(perk.SecondaryDescription, perk.SecondaryBonus, perk.SecondaryIncrementType);
                            //PerksAdditionalSecondaryRoles.Add(perk.StringId, additionalSecondaryRoles.ToList());
                        }
                        else
                        {
                            perk.SetPrivatePropertyValue("PrimaryBonus", bonus);
                            perk.SetPrivatePropertyValue("PrimaryDescription", new TextObject(description2, null));
                            PerkHelper.SetDescriptionTextVariable(perk.PrimaryDescription, perk.PrimaryBonus, perk.PrimaryIncrementType);
                            // PerksAdditionalPrimaryRoles.Add(perk.StringId, additionalSecondaryRoles.ToList());
                        }
                    }
                }
            }
        }
    }
}
