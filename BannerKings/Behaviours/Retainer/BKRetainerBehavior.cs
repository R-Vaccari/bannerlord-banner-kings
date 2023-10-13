using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Retainer
{
    public class BKRetainerBehavior : BannerKingsBehavior
    {
        private Contract contract;
        private bool freelancer;
        private CharacterObject template;

        public Contract GetContract() => contract;

        public HashSet<CharacterObject> AdequateTroopOptions
        {
            get
            {
                HashSet<CharacterObject> options = new HashSet<CharacterObject>();
                Hero main = Hero.MainHero;
                int minLevel = 6;
                int maxLevel = 6;
                if (main.Level > 26)
                {
                    minLevel = 26;
                    maxLevel = 31;
                }
                else if (main.Level > 21)
                {
                    minLevel = 21;
                    maxLevel = 21;
                }
                else if (main.Level > 16)
                {
                    minLevel = 16;
                    maxLevel = 16;
                }
                else if (main.Level > 11)
                {
                    minLevel = 11;
                    maxLevel = 11;
                }

                if (Hero.OneToOneConversationHero != null)
                {
                    var culture = Hero.OneToOneConversationHero.MapFaction.Culture;
                    List<CharacterObject> forbidden = new List<CharacterObject>();
                    foreach (var clan in Clan.All)
                    {
                        if (clan.Culture != culture) continue;

                        if (clan.BasicTroop != null) forbidden.Add(clan.BasicTroop);
                        if (clan.IsMinorFaction && clan.DefaultPartyTemplate != null)
                        {
                            foreach (var stack in clan.DefaultPartyTemplate.Stacks)
                            {
                                forbidden.Add(stack.Character);
                            }
                        }
                    }
                    forbidden.Add(culture.MilitiaArcher);
                    forbidden.Add(culture.MilitiaSpearman);
                    forbidden.Add(culture.MilitiaVeteranArcher);
                    forbidden.Add(culture.MilitiaVeteranSpearman);

                    foreach (CharacterObject troop in CharacterObject.All)
                    {
                        if (troop.Culture == culture &&
                            troop.Occupation == Occupation.Soldier &&
                            !forbidden.Contains(troop) &&
                            !troop.HiddenInEncylopedia &&
                            troop.Level >= minLevel && troop.Level <= maxLevel)
                        {
                            if (!main.IsFemale && troop.IsFemale) continue;
                            options.Add(troop);
                        }  
                    }   
                }

                return options;
            }
        }

        private void FillTroops(HashSet<CharacterObject> options,
            CharacterObject basicTroop, int minLevel, int maxLevel)
        {
            foreach (CharacterObject target in basicTroop.UpgradeTargets)
            {
                if (target.Level >= minLevel && target.Level <= maxLevel) options.Add(target);
                FillTroops(options, target, minLevel, maxLevel);
            }
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            
        }

        private void OnLoaded(CampaignGameStarter starter)
        {
            SetCamera();
        }

        private void OnTick(float dt)
        {
            if (contract != null)
            {
                MobileParty contractor = contract.Contractor.PartyBelongedTo;
                if (contractor != null && contractor.LeaderHero == contract.Contractor)
                {
                    MapState mapState = Game.Current.GameStateManager.ActiveState as MapState;
                    if (!PlayerCaptivity.IsCaptive && (dt > 0f || (mapState != null && !mapState.AtMenu)))
                    {
                        if (contractor.IsActive)
                        {
                            PartyBase.MainParty.MobileParty.Position2D = contractor.Position2D;
                            if (TaleWorlds.CampaignSystem.Campaign.Current.CurrentMenuContext == null || 
                                TaleWorlds.CampaignSystem.Campaign.Current.CurrentMenuContext.StringId != "bk_retinue_wait")
                            {
                                GameMenu.ActivateGameMenu("bk_retinue_wait");
                            }
                        }
                    }
                    else if (PlayerCaptivity.IsCaptive)
                    {

                    }
                }
            }
        }

        private void SetCamera()
        {
            if (contract != null)
            {
                MobileParty contractor = contract.Contractor.PartyBelongedTo;
                if (contractor != null && contractor.LeaderHero == contract.Contractor)
                {
                    MobileParty.MainParty.IsActive = false;
                    PartyBase.MainParty.UpdateVisibilityAndInspected(0f, true);
                    contractor.Party.SetAsCameraFollowParty();
                    contractor.Party.UpdateVisibilityAndInspected(0f, false);
                }
            }
        }

        private void UpdateRetinueMenu()
        {
            if (contract != null)
            {
                var party = contract.Contractor.PartyBelongedTo;
                if (party != null)
                {
                    MBTextManager.SetTextVariable("RETINUE_PARTY_NAME", party.Name);
                    MBTextManager.SetTextVariable("MORALE", party.Morale);
                    MBTextManager.SetTextVariable("MAX", party.LimitedPartySize);
                    MBTextManager.SetTextVariable("SIZE", party.MemberRoster.TotalManCount);
                    MBTextManager.SetTextVariable("QUARTERMASTER", party.EffectiveQuartermaster.Name);
                    MBTextManager.SetTextVariable("ENGINEER", party.EffectiveEngineer.Name);
                    MBTextManager.SetTextVariable("SCOUT", party.EffectiveScout.Name);
                    MBTextManager.SetTextVariable("SURGEON", party.EffectiveSurgeon.Name);
                    MBTextManager.SetTextVariable("FOOD", party.Food);
                }
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            return;
            starter.AddWaitGameMenu("bk_retinue_wait",
                "{=Pz1Njwo9}{RETINUE_PARTY_NAME}\nRetinue size: {SIZE}/{MAX}\nMorale: {MORALE}\nFood: {FOOD}\nSurgeon: {SURGEON}\nQuartermaster: {QUARTERMASTER}\nScout: {SCOUT}\nEngineer: {ENGINEER}\n",
                (MenuCallbackArgs args) =>
                {
                    UpdateRetinueMenu();
                },
                (MenuCallbackArgs args) => true,
                null,
                (MenuCallbackArgs args, CampaignTime time) =>
                {
                    if (time.GetHourOfDay == 12)
                    {
                        UpdateRetinueMenu();
                    }
                },
                GameMenu.MenuAndOptionType.WaitMenuHideProgressAndHoursOption,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.Encounter);

            starter.AddGameMenu("bk_retinue_contract",
                "{=B0FvvUGf}Retainer service for {CONTRACTOR}\nContract type: {TYPE}\nRole: {ROLE}\nWage: {WAGE}{GOLD_ICON}/day\nLeaves: {LEAVES}(+1/season)",
                (MenuCallbackArgs args) =>
                {
                    MBTextManager.SetTextVariable("CONTRACTOR", contract.Contractor.Name);
                    MBTextManager.SetTextVariable("CONTRACTOR", contract.Contractor.Name);

                    TextObject role = new TextObject("{=koX9okuG}None");
                    var party = contract.Contractor.PartyBelongedTo;
                    if (party != null)
                    {
                        role = GameTexts.FindText("role", party.GetHeroPerkRole(Hero.MainHero).ToString());
                    }
                    
                    MBTextManager.SetTextVariable("ROLE", contract.IsFreelancer ? role : contract.Template.Name);
                    MBTextManager.SetTextVariable("WAGE", contract.Wage);
                    MBTextManager.SetTextVariable("LEAVES", contract.Leaves);
                });

            starter.AddGameMenuOption("bk_retinue_wait",
                "bk_retinue_wait_contract",
                new TextObject("{=Pso1RM9e}Review Contract").ToString(),
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return true;
                },
                (MenuCallbackArgs args) => GameMenu.SwitchToMenu("bk_retinue_contract"),
                true);

            starter.AddGameMenuOption("bk_retinue_wait",
                "bk_retinue_wait_escape",
                new TextObject("{=NgTzQHnJ}Take a leave").ToString(),
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    EndService(true);
                },
                true);

            starter.AddGameMenuOption("bk_retinue_wait",
                "bk_retinue_wait_escape",
                new TextObject("{=BPOGNynF}Flee your duty").ToString(),
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    EndService(true);
                },
                true);

            #region Dialogues

            starter.AddPlayerLine("bk_retainer_start", 
                "lord_talk_speak_diplomacy_2",
                "bk_retainer_reason",
                "{=a48kCRRf}I would like to join your service, {?CONVERSATION_NPC.GENDER}my lady{?}my lord{\\?}.", 
                () => CharacterObject.OneToOneConversationCharacter.HeroObject != null && 
                CharacterObject.OneToOneConversationCharacter.HeroObject.PartyBelongedTo != null && 
                CharacterObject.OneToOneConversationCharacter.HeroObject.PartyBelongedTo.LeaderHero == CharacterObject.OneToOneConversationCharacter.HeroObject && 
                !CharacterObject.OneToOneConversationCharacter.HeroObject.Clan.IsMinorFaction && 
                Hero.MainHero.Clan.Kingdom == null && 
                CharacterObject.OneToOneConversationCharacter.HeroObject.Clan.Kingdom != null,
                null);

            starter.AddDialogLine("bk_retainer_reason",
                "bk_retainer_reason",
                "bk_retainer_reason_response",
                "{=wPpWea6C}And why would I take thee, {PLAYER.NAME}, under my banner?",
                () => true,
                delegate ()
                {
                    template = null;
                    freelancer = false;
                });

            starter.AddPlayerLine("bk_retainer_reason_response1",
                "bk_retainer_reason_response",
                "bk_retainer_accepted",
                "{=0X9Cdyt3}I am an expert fighter, my blade will be yours to command.",
                () => true,
                null,
                100,
                delegate (out TextObject explanation)
                {
                    Hero main = Hero.MainHero;
                    bool skill = main.GetSkillValue(DefaultSkills.OneHanded) >= 75 ||
                    main.GetSkillValue(DefaultSkills.TwoHanded) >= 75 ||
                    main.GetSkillValue(DefaultSkills.Polearm) >= 75;

                    explanation = new TextObject("{=N6ugF9Ta}You need at least 75 levels in any melee skill.");
                    return skill;
                });

            starter.AddPlayerLine("bk_retainer_reason_response2",
                "bk_retainer_reason_response",
                "bk_retainer_accepted",
                "{=FibXNYb6}My eyes are sharp and my aim is true.",
                () => true,
                null,
                100,
                delegate (out TextObject explanation)
                {
                    Hero main = Hero.MainHero;
                    bool skill = main.GetSkillValue(DefaultSkills.Bow) >= 75 ||
                    main.GetSkillValue(DefaultSkills.Crossbow) >= 75 ||
                    main.GetSkillValue(DefaultSkills.Throwing) >= 75;

                    explanation = new TextObject("{=QxCBcQok}You need at least 75 levels in any ranged skill.");
                    return skill;
                });

            starter.AddPlayerLine("bk_retainer_reason_response3",
                "bk_retainer_reason_response",
                "bk_retainer_accepted",
                "{=SMRzyknm}My loyalty and word are not fickle. I give you my unwavering alliegance.",
                () => true,
                null,
                100,
                delegate (out TextObject explanation)
                {
                    Hero main = Hero.MainHero;
                    explanation = new TextObject("{=LEYFhuax}You must be known for a positive sense of Honor.");
                    return main.GetTraitLevel(DefaultTraits.Honor) > 0;
                });

            starter.AddPlayerLine("bk_retainer_reason_response4",
                "bk_retainer_reason_response",
                "bk_retainer_accepted",
                "{=6PR69j0c}I am known for good service in the {REALM}.",
                () => true,
                null,
                100,
                delegate (out TextObject explanation)
                {
                    IFaction faction = Hero.OneToOneConversationHero.MapFaction;
                    MBTextManager.SetTextVariable("REALM", faction.Name);
                    Hero main = Hero.MainHero;
                    explanation = new TextObject("{=5Dwqvsa7}You must have a friendly notable in the {REALM}.")
                    .SetTextVariable("REALM", faction.Name);
                    return faction.Settlements.Any(x => x.Notables.Any(x => x.IsFriend(Hero.MainHero)));
                });

            starter.AddDialogLine("bk_retainer_accepted",
                "bk_retainer_accepted",
                "bk_retainer_contract",
                "{=SJO4tdme}Very well, {PLAYER.NAME}. And how dost thou wish to serve me? Know that any men under your command will go their own way.", 
                () => true, 
                null);

            starter.AddPlayerLine("bk_retainer_contract",
                "bk_retainer_contract",
                "bk_retainer_troop_options",
                "{=5zMJAWZf}As your servant.",
                () => true,
                () => 
                {
                    freelancer = false;
                    ConversationSentence.SetObjectsToRepeatOver(AdequateTroopOptions.ToList(), 6);
                },
                100,
                delegate (out TextObject explanation)
                {
                    Hero main = Hero.MainHero;
                    explanation = new TextObject("{=XQFO8smt}As a servant, you serve as a troop. Your equipment and wage are on par with standard troops, and you may be promoted with time and good service.");
                    return AdequateTroopOptions.Count > 0;
                });

            starter.AddPlayerLine("bk_retainer_contract",
                "bk_retainer_contract",
                "bk_retainer_finish",
                "{=Vy4vy61d}As a freelancer.",
                () => true,
                () => freelancer = true,
                100,
                delegate (out TextObject explanation)
                {
                    Hero main = Hero.MainHero;
                    explanation = new TextObject("{=oayYgfEc}As a freelancer, you serve as a companion. Your wage is determined by your qualifications and your equipment is your responsibility.");
                    return true;
                });

            starter.AddDialogLine("bk_retainer_troop_options",
                "bk_retainer_troop_options",
                "bk_retainer_troop_options_select",
                "{=nwALXrcj}What position?",
                () =>
                {
                    return true;
                },
                null);

            starter.AddRepeatablePlayerLine("bk_retainer_troop_options_select",
                "bk_retainer_troop_options_select",
                "bk_retainer_finish",
                "{=pQTfmN8k}{TROOP} - Level: ({LEVEL}), Upgrades: ({UPGRADES})",
                "A different option",
                "bk_retainer_troop_options",
                delegate
                {
                    var troop = ((CharacterObject)ConversationSentence.CurrentProcessedRepeatObject);
                    ConversationSentence.SelectedRepeatLine.SetTextVariable("TROOP", troop.Name);
                    ConversationSentence.SelectedRepeatLine.SetTextVariable("LEVEL", troop.Level);
                    ConversationSentence.SelectedRepeatLine.SetTextVariable("UPGRADES", troop.UpgradeTargets.Length);
                    return true;
                },
                () =>
                {
                    template = (CharacterObject)ConversationSentence.SelectedRepeatObject;
                });

            starter.AddDialogLine("bk_retainer_finish",
                "bk_retainer_finish",
                "bk_retainer_proposal",
                "{=gei4i1r9}Very well, {PLAYER.NAME}. {CONTRACT_TEXT} {TIME_TEXT} {FIELTY_TEXT}",
                () => 
                {
                    TextObject text;
                    contract = new Contract(Hero.OneToOneConversationHero, freelancer, template);
                    if (freelancer)
                    {
                        text = new TextObject("{=8tejhA91}As a freelancer, I offer thee {HIRING}{GOLD_ICON} immediatly for thy service, as well as {WAGE}{GOLD_ICON} on a daily basis, adjusted regularly and accordingly to thy skills.")
                        .SetTextVariable("WAGE", contract.Wage)
                        .SetTextVariable("HIRING", contract.HiringCost);
                    }
                    else
                    {
                        text = new TextObject("{=MnmMrbWo}As my servant, under the role of {TROOP}, I offer thee {HIRING}{GOLD_ICON} immediatly for thy service, as well as {WAGE}{GOLD_ICON} on a daily basis, to be adjusted if and when thou deservest betterment.")
                        .SetTextVariable("TROOP", template.Name)
                        .SetTextVariable("WAGE", contract.Wage)
                        .SetTextVariable("HIRING", contract.HiringCost);
                    }

                    MBTextManager.SetTextVariable("CONTRACT_TEXT", text);
                    MBTextManager.SetTextVariable("TIME_TEXT", new TextObject("{=WVGhoW1b}This contract will hold until {DATE}, a year from now, given no extraordinary circunstances. At that time, thou mayest renew it with me.")
                        .SetTextVariable("DATE", CampaignTime.YearsFromNow(1f).ToString()));
                    MBTextManager.SetTextVariable("FIELTY_TEXT", new TextObject("{=cO1B7K4v}Know that thy utmost loyalty is expected. If our covenant were to be violated, know that I shall sentence and punish thee under the law."));

                    return true;
                },
                null);

            starter.AddPlayerLine("bk_retainer_proposal",
                "bk_retainer_proposal",
                "bk_retainer_proposal_accepted",
                "{=FBMpN2Bo}I accept your terms, and swear to you my loyalty.",
                () => true,
                () =>
                {
                    GiveGoldAction.ApplyBetweenCharacters(Hero.OneToOneConversationHero,
                        Hero.MainHero,
                        contract.HiringCost);
                },
                100,
                (out TextObject reason) =>
                {
                    reason = new TextObject("{=RcT8kw4F}{HERO} needs to have at least {HIRING} in money to pay you.")
                    .SetTextVariable("HERO", Hero.OneToOneConversationHero.Name)
                    .SetTextVariable("HIRING", contract.HiringCost);
                    return Hero.OneToOneConversationHero.Gold >= contract.HiringCost;
                });

            starter.AddPlayerLine("bk_retainer_proposal",
                "bk_retainer_proposal",
                "bk_retainer_proposal_accepted",
                "{=J4hOvKWn}I swear to you my loyalty, and ask no immediate pay.",
                () => true,
                null);

            starter.AddPlayerLine("bk_retainer_proposal",
                "bk_retainer_proposal",
                "lord_talk_speak_diplomacy_2",
                "{=1CGUK73B}I must refuse, for now.",
                () => true,
                () => contract = null);

            starter.AddDialogLine("bk_retainer_proposal_accepted",
               "bk_retainer_proposal_accepted",
               "close_window",
               "{=t0GpPrbd}Then I bid thee welcome, {PLAYER.NAME}.",
               () => true,
               () => StartService());

            #endregion Dialogues
        }

        private void EndService(bool fled)
        {

        }

        private void StartService()
        {
            MobileParty newParty = Hero.OneToOneConversationHero.PartyBelongedTo;
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }

            if (MobileParty.MainParty.CurrentSettlement != null)
            {
                LeaveSettlementAction.ApplyForParty(MobileParty.MainParty);
            }

            if (MobileParty.MainParty.Army != null)
            {
                if (MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty)
                {
                    DisbandArmyAction.ApplyByLeaderPartyRemoved(MobileParty.MainParty.Army);
                }
                MobileParty.MainParty.Army = null;
            }

            newParty.Party.AddMember(Hero.MainHero.CharacterObject, 1);
            MobileParty.MainParty.ChangePartyLeader(null);
            MobileParty.MainParty.MemberRoster.RemoveTroop(Hero.MainHero.CharacterObject, 
                1, 
                default(UniqueTroopDescriptor), 
                0);
            contract = new Contract(Hero.OneToOneConversationHero, freelancer, template);
            SetCamera();
        }
    }
}
