using HarmonyLib;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using SandBox.GameComponents;
using Helpers;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using BannerKings.Settings;
using BannerKings.Utils;

namespace BannerKings.Patches
{
    internal partial class PerksAndSkillsPatches
    {

        [HarmonyPatch(typeof(SandboxAgentStatCalculateModel), "SetPerkAndBannerEffectsOnAgent")]
        class SetPerkAndBannerEffectsOnAgentPatch
        {
            static bool Prefix(Agent agent, CharacterObject agentCharacter, AgentDrivenProperties agentDrivenProperties, WeaponComponentData equippedWeaponComponent)
            {
                Formation formation = agent.Formation;
                object obj;
                if (formation == null)
                {
                    obj = null;
                }
                else
                {
                    Agent captain = formation.Captain;
                    obj = ((captain != null) ? captain.Character : null);
                }
                CharacterObject characterObject = obj as CharacterObject;
                Formation formation2 = agent.Formation;
                if (((formation2 != null) ? formation2.Captain : null) == agent)
                {
                    characterObject = null;
                }
                ItemObject itemObject = null;
                EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
                if (wieldedItemIndex != EquipmentIndex.None)
                {
                    itemObject = agent.Equipment[wieldedItemIndex].Item;
                }
                BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(agent.Formation);
                bool flag = equippedWeaponComponent != null && equippedWeaponComponent.IsRangedWeapon;
                bool flag2 = equippedWeaponComponent != null && equippedWeaponComponent.IsMeleeWeapon;
                bool flag3 = itemObject != null && itemObject.PrimaryWeapon.IsShield;
                ExplainedNumber explainedNumber = new ExplainedNumber(agentDrivenProperties.CombatMaxSpeedMultiplier, false, null);
                ExplainedNumber explainedNumber2 = new ExplainedNumber(agentDrivenProperties.MaxSpeedMultiplier, false, null);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.FleetOfFoot, agentCharacter, true, ref explainedNumber);
                ExplainedNumber explainedNumber3 = new ExplainedNumber(agentDrivenProperties.KickStunDurationMultiplier, false, null);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.DirtyFighting, agentCharacter, true, ref explainedNumber3);
                agentDrivenProperties.KickStunDurationMultiplier = explainedNumber3.ResultNumber;
                if (equippedWeaponComponent != null)
                {
                    ExplainedNumber explainedNumber4 = new ExplainedNumber(agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier, false, null);
                    if (flag2)
                    {
                        ExplainedNumber explainedNumber5 = new ExplainedNumber(agentDrivenProperties.SwingSpeedMultiplier, false, null);
                        ExplainedNumber explainedNumber6 = new ExplainedNumber(agentDrivenProperties.HandlingMultiplier, false, null);
                        if (!agent.HasMount)
                        {
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Fury, agentCharacter, true, ref explainedNumber6);
                            if (characterObject != null)
                            {
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.Fury, characterObject, ref explainedNumber6);
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.OnTheEdge, characterObject, ref explainedNumber5);
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.BladeMaster, characterObject, ref explainedNumber5);
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.SwiftSwing, characterObject, ref explainedNumber5);
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.BladeMaster, characterObject, ref explainedNumber4);
                            }
                        }
                        if (equippedWeaponComponent.RelevantSkill == DefaultSkills.OneHanded)
                        {
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.SwiftStrike, agentCharacter, true, ref explainedNumber5);
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.OneHanded.WayOfTheSword, agentCharacter, DefaultSkills.OneHanded, true, ref explainedNumber5, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.OneHanded.WayOfTheSword, agentCharacter, DefaultSkills.OneHanded, true, ref explainedNumber4, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.WrappedHandles, agentCharacter, true, ref explainedNumber6);
                        }
                        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.TwoHanded)
                        {
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.OnTheEdge, agentCharacter, true, ref explainedNumber5);
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.TwoHanded.WayOfTheGreatAxe, agentCharacter, DefaultSkills.TwoHanded, true, ref explainedNumber5, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.TwoHanded.WayOfTheGreatAxe, agentCharacter, DefaultSkills.TwoHanded, true, ref explainedNumber4, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.StrongGrip, agentCharacter, true, ref explainedNumber6);
                        }
                        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Polearm)
                        {
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.Footwork, agentCharacter, true, ref explainedNumber);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.SwiftSwing, agentCharacter, true, ref explainedNumber5);
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Polearm.WayOfTheSpear, agentCharacter, DefaultSkills.Polearm, true, ref explainedNumber5, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Polearm.WayOfTheSpear, agentCharacter, DefaultSkills.Polearm, true, ref explainedNumber4, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            if (equippedWeaponComponent.SwingDamageType != DamageTypes.Invalid)
                            {
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.CounterWeight, agentCharacter, true, ref explainedNumber6);
                            }
                        }
                        agentDrivenProperties.SwingSpeedMultiplier = explainedNumber5.ResultNumber;
                        agentDrivenProperties.HandlingMultiplier = explainedNumber6.ResultNumber;
                    }
                    if (flag)
                    {
                        ExplainedNumber explainedNumber7 = new ExplainedNumber(agentDrivenProperties.WeaponInaccuracy, false, null);
                        ExplainedNumber explainedNumber8 = new ExplainedNumber(agentDrivenProperties.WeaponMaxMovementAccuracyPenalty, false, null);
                        ExplainedNumber explainedNumber9 = new ExplainedNumber(agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty, false, null);
                        ExplainedNumber explainedNumber10 = new ExplainedNumber(agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians, false, null);
                        ExplainedNumber explainedNumber11 = new ExplainedNumber(agentDrivenProperties.WeaponUnsteadyBeginTime, false, null);
                        ExplainedNumber explainedNumber12 = new ExplainedNumber(agentDrivenProperties.WeaponUnsteadyEndTime, false, null);
                        ExplainedNumber explainedNumber13 = new ExplainedNumber(agentDrivenProperties.ReloadMovementPenaltyFactor, false, null);
                        ExplainedNumber explainedNumber14 = new ExplainedNumber(agentDrivenProperties.ReloadSpeed, false, null);
                        ExplainedNumber explainedNumber15 = new ExplainedNumber(agentDrivenProperties.MissileSpeedMultiplier, false, null);
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.NockingPoint, agentCharacter, true, ref explainedNumber13);
                        if (characterObject != null)
                        {
                            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.LooseAndMove, characterObject, ref explainedNumber2);
                        }
                        if (activeBanner != null)
                        {
                            BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedRangedAccuracyPenalty, activeBanner, ref explainedNumber7);
                        }
                        if (agent.HasMount)
                        {
                            if (agentCharacter.GetPerkValue(DefaultPerks.Riding.Sagittarius))
                            {
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.Sagittarius, agentCharacter, true, ref explainedNumber8);
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.Sagittarius, agentCharacter, true, ref explainedNumber9);
                            }
                            if (characterObject != null && characterObject.GetPerkValue(DefaultPerks.Riding.Sagittarius))
                            {
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.Sagittarius, characterObject, ref explainedNumber8);
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.Sagittarius, characterObject, ref explainedNumber9);
                            }
                            if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Bow && agentCharacter.GetPerkValue(DefaultPerks.Bow.MountedArchery))
                            {
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.MountedArchery, agentCharacter, true, ref explainedNumber8);
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.MountedArchery, agentCharacter, true, ref explainedNumber9);
                            }
                            if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing && agentCharacter.GetPerkValue(DefaultPerks.Throwing.MountedSkirmisher))
                            {
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.MountedSkirmisher, agentCharacter, true, ref explainedNumber8);
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.MountedSkirmisher, agentCharacter, true, ref explainedNumber9);
                            }
                        }
                        bool flag4 = false;
                        if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Bow)
                        {
                            flag4 = true;
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.BowControl, agentCharacter, true, ref explainedNumber8);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.RapidFire, agentCharacter, true, ref explainedNumber14);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.QuickAdjustments, agentCharacter, true, ref explainedNumber10);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.Discipline, agentCharacter, true, ref explainedNumber11);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.Discipline, agentCharacter, true, ref explainedNumber12);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.QuickDraw, agentCharacter, true, ref explainedNumber4);
                            if (characterObject != null)
                            {
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.RapidFire, characterObject, ref explainedNumber14);
                                if (!agent.HasMount)
                                {
                                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.NockingPoint, characterObject, ref explainedNumber2);
                                }
                            }
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Bow.Deadshot, agentCharacter, DefaultSkills.Bow, true, ref explainedNumber14, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                        }
                        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Crossbow)
                        {
                            flag4 = true;
                            if (agent.HasMount)
                            {
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Steady, agentCharacter, true, ref explainedNumber8);
                                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Steady, agentCharacter, true, ref explainedNumber10);
                            }
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.WindWinder, agentCharacter, true, ref explainedNumber14);
                            if (characterObject != null)
                            {
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.WindWinder, characterObject, ref explainedNumber14);
                            }
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.DonkeysSwiftness, agentCharacter, true, ref explainedNumber8);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Marksmen, agentCharacter, true, ref explainedNumber4);
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Crossbow.MightyPull, agentCharacter, DefaultSkills.Crossbow, true, ref explainedNumber14, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                        }
                        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing)
                        {
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.QuickDraw, agentCharacter, true, ref explainedNumber14);
                            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.PerfectTechnique, agentCharacter, true, ref explainedNumber15);
                            if (characterObject != null)
                            {
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.QuickDraw, characterObject, ref explainedNumber14);
                                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.PerfectTechnique, characterObject, ref explainedNumber15);
                            }
                            PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Throwing.UnstoppableForce, agentCharacter, DefaultSkills.Throwing, true, ref explainedNumber15, TaleWorlds.CampaignSystem.Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                        }
                        if (flag4 && TaleWorlds.CampaignSystem.Campaign.Current.Models.MapWeatherModel.GetWeatherEffectOnTerrainForPosition(MobileParty.MainParty.Position2D) == MapWeatherModel.WeatherEventEffectOnTerrain.Wet)
                        {
                            explainedNumber15.AddFactor(-0.2f, null);
                        }
                        agentDrivenProperties.ReloadMovementPenaltyFactor = explainedNumber13.ResultNumber;
                        agentDrivenProperties.ReloadSpeed = explainedNumber14.ResultNumber;
                        agentDrivenProperties.MissileSpeedMultiplier = explainedNumber15.ResultNumber;
                        agentDrivenProperties.WeaponInaccuracy = explainedNumber7.ResultNumber;
                        agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = explainedNumber8.ResultNumber;
                        agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = explainedNumber9.ResultNumber;
                        agentDrivenProperties.WeaponUnsteadyBeginTime = explainedNumber11.ResultNumber;
                        agentDrivenProperties.WeaponUnsteadyEndTime = explainedNumber12.ResultNumber;
                        agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = explainedNumber10.ResultNumber;
                    }
                    agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = explainedNumber4.ResultNumber;
                }
                if (flag3)
                {
                    ExplainedNumber explainedNumber16 = new ExplainedNumber(agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder, false, null);
                    if (characterObject != null)
                    {
                        Formation formation3 = agent.Formation;
                        if (formation3 != null && formation3.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.ShieldWall)
                        {
                            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.ShieldWall, characterObject, ref explainedNumber16);
                        }
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.ArrowCatcher, characterObject, ref explainedNumber16);
                    }
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.ArrowCatcher, agentCharacter, true, ref explainedNumber16);
                    agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = explainedNumber16.ResultNumber;
                    ExplainedNumber explainedNumber17 = new ExplainedNumber(agentDrivenProperties.ShieldBashStunDurationMultiplier, false, null);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.Basher, agentCharacter, true, ref explainedNumber17);
                    agentDrivenProperties.ShieldBashStunDurationMultiplier = explainedNumber17.ResultNumber;
                }
                else
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.MorningExercise, agentCharacter, true, ref explainedNumber2);
                    #region DefaultPerks.Medicine.SelfMedication
                    if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulMedicinePerks)
                    {
                        DefaultPerks.Medicine.SelfMedication.AddScaledPersonalPerkBonus(ref explainedNumber2, true, agentCharacter.HeroObject);
                    }
                    else
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.SelfMedication, agentCharacter, false, ref explainedNumber2);
                    }
                    #endregion
                    if (!flag3 && !flag)
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Sprint, agentCharacter, true, ref explainedNumber2);
                    }
                    if (equippedWeaponComponent == null && itemObject == null)
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.FleetFooted, agentCharacter, true, ref explainedNumber2);
                    }
                    if (characterObject != null)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.MorningExercise, characterObject, ref explainedNumber2);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.ShieldBearer, characterObject, ref explainedNumber2);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.FleetOfFoot, characterObject, ref explainedNumber2);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.RecklessCharge, characterObject, ref explainedNumber2);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Footwork, characterObject, ref explainedNumber2);
                        if (agentCharacter.Tier >= 3)
                        {
                            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.FormFittingArmor, characterObject, ref explainedNumber2);
                        }
                        if (agentCharacter.IsInfantry)
                        {
                            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.Sprint, characterObject, ref explainedNumber2);
                        }
                    }
                }
                if (agent.IsHero)
                {
                    ItemObject item = (Mission.Current.DoesMissionRequireCivilianEquipment ? agentCharacter.FirstCivilianEquipment : agentCharacter.FirstBattleEquipment)[EquipmentIndex.Body].Item;
                    if (item != null && item.IsCivilian && agentCharacter.GetPerkValue(DefaultPerks.Roguery.SmugglerConnections))
                    {
                        agentDrivenProperties.ArmorTorso += DefaultPerks.Roguery.SmugglerConnections.PrimaryBonus;
                    }
                }
                float num = 0f;
                float num2 = 0f;
                bool flag5 = false;
                if (characterObject != null)
                {
                    if (agent.HasMount && characterObject.GetPerkValue(DefaultPerks.Riding.DauntlessSteed))
                    {
                        num += DefaultPerks.Riding.DauntlessSteed.SecondaryBonus;
                        flag5 = true;
                    }
                    else if (!agent.HasMount && characterObject.GetPerkValue(DefaultPerks.Athletics.IgnorePain))
                    {
                        num += DefaultPerks.Athletics.IgnorePain.SecondaryBonus;
                        flag5 = true;
                    }
                    if (characterObject.GetPerkValue(DefaultPerks.Engineering.Metallurgy))
                    {
                        num += DefaultPerks.Engineering.Metallurgy.SecondaryBonus;
                        flag5 = true;
                    }
                }
                if (!agent.HasMount && agentCharacter.GetPerkValue(DefaultPerks.Athletics.IgnorePain))
                {
                    num2 += DefaultPerks.Athletics.IgnorePain.PrimaryBonus;
                    flag5 = true;
                }
                if (flag5)
                {
                    float num3 = 1f + num2;
                    agentDrivenProperties.ArmorHead = MathF.Max(0f, (agentDrivenProperties.ArmorHead + num) * num3);
                    agentDrivenProperties.ArmorTorso = MathF.Max(0f, (agentDrivenProperties.ArmorTorso + num) * num3);
                    agentDrivenProperties.ArmorArms = MathF.Max(0f, (agentDrivenProperties.ArmorArms + num) * num3);
                    agentDrivenProperties.ArmorLegs = MathF.Max(0f, (agentDrivenProperties.ArmorLegs + num) * num3);
                }
                if (Mission.Current != null && Mission.Current.HasValidTerrainType)
                {
                    TerrainType terrainType = Mission.Current.TerrainType;
                    if (terrainType == TerrainType.Snow || terrainType == TerrainType.Forest)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.ExtendedSkirmish, characterObject, ref explainedNumber2);
                    }
                    else if (terrainType == TerrainType.Plain || terrainType == TerrainType.Steppe || terrainType == TerrainType.Desert)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.DecisiveBattle, characterObject, ref explainedNumber2);
                    }
                }
                if (agentCharacter.Tier >= 3 && agentCharacter.IsInfantry)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.FormFittingArmor, characterObject, ref explainedNumber2);
                }
                if (agent.Formation != null && agent.Formation.CountOfUnits <= 15)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.SmallUnitTactics, characterObject, ref explainedNumber2);
                }
                if (activeBanner != null)
                {
                    BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedTroopMovementSpeed, activeBanner, ref explainedNumber2);
                }
                agentDrivenProperties.MaxSpeedMultiplier = explainedNumber2.ResultNumber;
                agentDrivenProperties.CombatMaxSpeedMultiplier = explainedNumber.ResultNumber;

                return false;
            }

        }
    }
}
