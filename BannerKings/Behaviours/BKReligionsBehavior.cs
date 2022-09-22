using System.Collections.Generic;
using System.Linq;
using System.Text;
using BannerKings.Managers;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Skills;
using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKReligionsBehavior : CampaignBehaviorBase
    {
        private static ReligionsManager ReligionsManager => BannerKingsConfig.Instance.ReligionsManager;
        private Divinity selectedDivinity;
        private Rite selectedRite;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, OnDailyTickSettlement);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
            CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnHeroComesOfAge);
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHero);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddDialogue(starter);
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
        }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            Util.TryCatch(() =>
            {
                if (settlement == null || settlement.Notables == null)
                {
                    return;
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null && data.ReligionData != null)
                {
                    var religion = data.ReligionData.DominantReligion;

                    if (religion != null)
                    {

                        List<Hero> toRemove = new List<Hero>();
                        int count = settlement.Notables.Count(x => x.IsPreacher);
                        if (count > 1)
                        {
                            foreach (var notable in settlement.Notables)
                            {
                                if (notable.IsPreacher)
                                {
                                    var preacher = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(notable);
                                    if (!preacher)
                                    {
                                        toRemove.Add(notable);
                                    }
                                }
                            }
                        }
                        else if (count == 1)
                        {
                            var hero = settlement.Notables.First(x => x.IsPreacher);
                            BannerKingsConfig.Instance.ReligionsManager.AddClergyman(religion, hero, settlement);
                        }

                        int heroesCount = settlement.HeroesWithoutParty.Count(x => x.IsPreacher);
                        if (count > 1)
                        {
                            foreach (var notable in settlement.HeroesWithoutParty)
                            {
                                if (notable.IsPreacher)
                                {
                                    var preacher = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(notable);
                                    if (!preacher)
                                    {
                                        toRemove.Add(notable);
                                    }
                                }
                            }
                        }


                        if (toRemove.Count > 0)
                        {
                            List<Hero> notables = (List<Hero>)AccessTools.Field(settlement.GetType(), "_notablesCache").GetValue(settlement);
                            foreach (var notable in toRemove)
                            {
                                if (notables.Contains(notable))
                                {
                                    notables.Remove(notable);
                                }

                                notable.AddPower(-10000f);
                                if (notable.CurrentSettlement != null)
                                {
                                    LeaveSettlementAction.ApplyForCharacterOnly(notable);
                                }
                                if (notable.IsAlive)
                                {
                                    KillCharacterAction.ApplyByRemove(notable);
                                }
                            }
                        }

                    }
                }
            });
            
          

           
            
        }

        private void OnRaidCompleted(BattleSideEnum winnerSide, MapEvent mapEvent)
        {
            foreach (var mapEventParty in mapEvent.AttackerSide.Parties)
            {
                if (mapEventParty.Party.IsActive)
                {
                    var mobileParty = mapEventParty.Party.MobileParty;
                    if (mobileParty != null && mobileParty.LeaderHero != null && 
                        mapEvent.MapEventSettlement.Culture.StringId != "battania")
                    {
                        if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(mobileParty.LeaderHero,
                            DefaultDivinities.Instance.AmraSecondary2))
                        {
                            GainRenownAction.Apply(mobileParty.LeaderHero, 10f);
                        }
                    }
                }
            }
        }

        private void OnHeroCreated(Hero hero, bool bornNaturally)
        {
            if (hero == null)
            {
                return;
            }

            InitializeFaith(hero);
        }

        private void OnHeroComesOfAge(Hero hero)
        {
            if (ReligionsManager.GetHeroReligion(hero) != null)
            {
                return;
            }

            InitializeFaith(hero);
        }
         
        private void InitializeFaith(Hero hero)
        {
            Religion startingReligion = null;
            if (hero.Clan != null && hero != hero.Clan.Leader && hero.Clan.Leader != null)
            {
                startingReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero.Clan.Leader);
            }

            ReligionsManager.InitializeHeroFaith(hero, startingReligion);
        }

        private void OnDailyTickHero(Hero hero)
        {
            if (hero == null || hero.IsChild)
            {
                return;
            }

            TickFaithXp(hero);

            if (hero.Clan != null && hero.Clan == Clan.PlayerClan)
            {
                return;
            }

            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel == null)
            {

                if (hero.Clan != null && hero != hero.Clan.Leader)
                {
                    var leaderRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero.Clan.Leader);
                    if (leaderRel != null)
                    {
                        ReligionsManager.AddToReligion(hero, leaderRel);
                        return;
                    }
                }


                AddHeroToIdealReligion(hero);
            } 
            else
            {
 
                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero,
                    DefaultDivinities.Instance.AseraSecondary2) && hero.IsPartyLeader)
                {
                    int aserai = 0;
                    foreach (TroopRosterElement element in hero.PartyBelongedTo.MemberRoster.GetTroopRoster())
                    {
                        if (element.Character.Culture.StringId == "aserai")
                        {
                            aserai += element.Number;
                        }
                    }

                    if (aserai == hero.PartyBelongedTo.MemberRoster.TotalManCount)
                    {
                        GainRenownAction.Apply(hero, 0.5f);
                    }
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero,
                    DefaultDivinities.Instance.AseraSecondary1) && MBRandom.RandomFloat < 0.3f) 
                {
                    Clan random = Clan.All.GetRandomElementWithPredicate(x => x.Culture.StringId == "aserai" && x != hero.Clan);
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, random.Leader, 2);
                }

                if (hero.IsPartyLeader && MBRandom.RandomFloat < 0.05f && 
                    BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero, DefaultDivinities.Instance.VlandiaSecondary2))
                {
                    int count = MBRandom.RandomInt(1, 4);
                    var character = Game.Current.ObjectManager.GetObject<CharacterObject>("canticles_zealot_tier4");
                    TroopRosterElement element = new TroopRosterElement(character);
                    element.Number = count;
                    hero.PartyBelongedTo.MemberRoster.Add(element);
                    
                    if (hero == Hero.MainHero)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(new TextObject("{=!}{COUNT} {UNIT} zealots have joined your party!")
                            .SetTextVariable("COUNT", count)
                            .SetTextVariable("UNIT", character.Name)
                            .ToString(), 
                            Color.ConvertStringToColor("#00CCFF")));
                    }
                }


                if (CampaignTime.Now.GetDayOfSeason == 1 && BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero,
                    DefaultDivinities.Instance.DarusosianSecondary1))
                {
                    var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero);
                    var kingdom = Kingdom.All.FirstOrDefault(x => x.StringId == "empire_s");
                    if (kingdom != null)
                    {
                        var empireTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                        if (empireTitle != null)
                        {
                            var bonus = 0f;
                            foreach (var title in titles)
                            {
                                if (title.sovereign == empireTitle)
                                {
                                    bonus += 2f / (float)title.type;
                                }
                            }

                            GainRenownAction.Apply(hero, bonus);
                        }
                    }
                }
            }
        }

        private void TickFaithXp(Hero hero)
        {
            float piety = BannerKingsConfig.Instance.PietyModel.CalculateEffect(hero).ResultNumber;
            if (piety > 0f)
            {
                hero.AddSkillXp(BKSkills.Instance.Theology, MathF.Clamp(piety / 2f, 1f, 10f));
            }
        }

        private void AddHeroToIdealReligion(Hero hero)
        {
            var ideal = BannerKingsConfig.Instance.ReligionsManager.GetIdealReligion(hero.Culture);
            if (ideal != null)
            {
                ReligionsManager.AddToReligion(hero, ideal);
            }
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
        {
            ReligionsManager.ExecuteRemoveHero(victim);
        }

        private void DailyTick()
        {
            Util.TryCatch(() =>
            {
                foreach (var religion in ReligionsManager.GetReligions())
                {
                    foreach (var hero in ReligionsManager.GetFaithfulHeroes(religion))
                    {
                        ReligionsManager.AddPiety(religion, hero, BannerKingsConfig.Instance.PietyModel.CalculateEffect(hero).ResultNumber);
                    }
                }
            });

            
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null || BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target))
            {
                return;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).ReligionData;

            if (data?.Clergyman != null)
            {
                Utils.Helpers.AddSellerToKeep(data.Clergyman.Hero, target);
            }
        }

        private void AddDialogue(CampaignGameStarter starter)
        {
            starter.AddDialogLine("bk_preacher_introduction", "lord_introduction", "lord_start",
                "{=erGoiFgH}{CLERGYMAN_GREETING}",
                OnConditionClergymanGreeting, null);

            starter.AddPlayerLine("bk_question_preaching", "hero_main_options", "bk_preacher_asked_preaching",
                "{=B9ei3RAA}What are you preaching?",
                OnConditionClergymanGreeting, null);

            starter.AddDialogLine("bk_answer_preaching_1", "bk_preacher_asked_preaching",
                "bk_preacher_asked_preaching_last",
                "{=vtbEXn9H}{CLERGYMAN_PREACHING}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_preaching_2", "bk_preacher_asked_preaching_last",
                "hero_main_options",
                "{=KdVPngCa}{CLERGYMAN_PREACHING_LAST}",
                IsPreacher, null);


            starter.AddPlayerLine("bk_question_faith", "hero_main_options", "bk_preacher_asked_faith",
                "{=rhPmXyLC}How do I prove my faith?",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_1", "bk_preacher_asked_faith", "bk_preacher_asked_faith_last",
                "{=Oa86My4f}{CLERGYMAN_FAITH}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_2", "bk_preacher_asked_faith_last", "hero_main_options",
                "{=Pv6nVzD1}{CLERGYMAN_FAITH_LAST}",
                IsPreacher, null);


            starter.AddPlayerLine("bk_question_faith_forbidden", "hero_main_options",
                "bk_preacher_asked_faith_forbidden",
                "{=s8dFuSef}What is forbidden to the faith?",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_1", "bk_preacher_asked_faith_forbidden",
                "bk_preacher_asked_faith_forbidden_last",
                "{=7E2WCYo7}{CLERGYMAN_FAITH_FORBIDDEN}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_forbidden_2", "bk_preacher_asked_faith_forbidden_last",
                "hero_main_options",
                "{=hZ06pC5W}{CLERGYMAN_FAITH__FORBIDDEN_LAST}",
                IsPreacher, null);

            starter.AddPlayerLine("bk_question_induction", "hero_main_options", "bk_preacher_asked_induction",
                "{=izA2jaF1}I would like to be inducted.",
                IsPreacher,
                null, 100,
                InductionOnClickable);

            starter.AddDialogLine("bk_answer_induction_1", "bk_preacher_asked_induction",
                "bk_preacher_asked_induction_last",
                "{=dX4YUFS6}{CLERGYMAN_INDUCTION}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_induction_2", "bk_preacher_asked_induction_last",
                "hero_main_options",
                "{=6d9H5id0}{CLERGYMAN_INDUCTION_LAST}",
                IsPreacher, InductionOnConsequence);


            starter.AddPlayerLine("bk_question_boon", "hero_main_options", "bk_preacher_asked_boon",
                "{=H9E58HNp}{CLERGYMAN_BLESSING_ACTION}",
                IsPreacher,
                BlessingOnConsequence,
                100,
                BlessingOnClickable);

            starter.AddDialogLine("bk_answer_boon", "bk_preacher_asked_boon", "bk_preacher_asked_boon_answer",
                "{=CHg5Rn5h}{CLERGYMAN_BLESSING_QUESTION}",
                null, BlessingPositiveAnswerOnConsequence);

            starter.AddPlayerLine("bk_preacher_asked_boon_answer", "bk_preacher_asked_boon_answer", "bk_boon_confirm",
                "{=8B2pePnb}I have decided.", null,
                null);

            starter.AddPlayerLine("bk_preacher_asked_boon_answer", "bk_preacher_asked_boon_answer",
                "hero_main_options",
                "{=G4ALCxaA}Never mind.", null, null);

            starter.AddDialogLine("bk_boon_confirm", "bk_boon_confirm", "bk_boon_confirm",
                "{=VKujVK2V}{CLERGYMAN_BLESSING_CONFIRM}",
                null, null);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "hero_main_options", "{=LPVNjXpT}See it done.",
                () => selectedDivinity != null, BlessingConfirmOnConsequence);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "hero_main_options",
                "{=G4ALCxaA}Never mind.",
                null, null);


            starter.AddPlayerLine("bk_question_rite", "hero_main_options", "bk_preacher_asked_rites",
                "{=hcK4qzV2}I would like to perform a rite.",
                IsPreacher,
                RitesOnConsequence,
                100,
                RitesOnClickable);

            starter.AddDialogLine("bk_answer_rite", "bk_preacher_asked_rites", "bk_preacher_asked_rites_answer",
                "{=XnzEHCmP}{CLERGYMAN_RITE}",
                null,
                RitesPositiveAnswerOnConsequence);

            starter.AddDialogLine("bk_answer_rite_impossible", "bk_preacher_asked_rites", "hero_main_options",
                "{=rfjH2WMz}I am afraid that won't be possible.",
                IsPreacher,
                null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer", "bk_rite_confirm",
                "{=8B2pePnb}I have decided.", null,
                null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer",
                "hero_main_options",
                "{=G4ALCxaA}Never mind.", null,
                null);

            starter.AddDialogLine("bk_rite_confirm", "bk_rite_confirm", "bk_rite_confirm",
                "{=JgeQ3a2u}{CLERGYMAN_RITE_CONFIRM}",
                null, null);
            starter.AddPlayerLine("bk_rite_confirm", "bk_rite_confirm", "hero_leave", "{=LPVNjXpT}See it done.",
                null, () =>
                {
                    selectedRite?.Complete(Hero.MainHero);
                });
            starter.AddPlayerLine("bk_rite_confirm", "bk_rite_confirm", "hero_main_options",
                "{=G4ALCxaA}Never mind.",
                null, null);


            starter.AddPlayerLine("bk_blessing_recruit_battania_bandits", "bandit_attacker", "common_encounter_ultimatum_answer",
                "{=2QtnvGFq}I am oathbound to the Na Sidhfir. As men of the wilds, will you join me?",
                RecruitBattaniaBanditsOnCondition,
                RecruitBattaniaBanditsOnConseqence, 
                100, 
                null, 
                null);


        }

        private bool RecruitBattaniaBanditsOnCondition()
        {
            var party = MobileParty.ConversationParty;
            var blessed = BannerKingsConfig.Instance.ReligionsManager.HasBlessing(Hero.MainHero, 
                DefaultDivinities.Instance.AmraSecondary1);
            return party.IsBandit && party.MapFaction.Culture.StringId == "forest_bandits" && 
                party.MemberRoster.Count < 21 && blessed;
        }

        private void RecruitBattaniaBanditsOnConseqence()
        {
            var list = new List<MobileParty>
            {
                MobileParty.MainParty
            };
            var list2 = new List<MobileParty>();
            if (PlayerEncounter.EncounteredMobileParty != null)
            {
                list2.Add(PlayerEncounter.EncounteredMobileParty);
            }
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.Current.FindAllNpcPartiesWhoWillJoinEvent(ref list, ref list2);
            }
            var troopsToJoinPlayerParty = GetTroopsToJoinPlayerParty(list2);
            PartyScreenManager.OpenScreenAsLoot(troopsToJoinPlayerParty, TroopRoster.CreateDummyTroopRoster(), PlayerEncounter.EncounteredParty.Name, troopsToJoinPlayerParty.TotalManCount, null);
            for (var i = list2.Count - 1; i >= 0; i--)
            {
                var mobileParty = list2[i];
                CampaignEventDispatcher.Instance.OnBanditPartyRecruited(mobileParty);
                DestroyPartyAction.Apply(MobileParty.MainParty.Party, mobileParty);
            }
            PlayerEncounter.LeaveEncounter = true;
        }


        private bool InductionOnClickable(out TextObject hintText)
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            var playerReligion = ReligionsManager.GetHeroReligion(Hero.MainHero);
            TextObject faithText = new TextObject();
            if (playerReligion != null)
            {
                if (playerReligion.Faith.GetId() == religion.Faith.GetId())
                {
                    hintText = new TextObject("{=ProkogUg}Already an adherent of this faith.");
                    return false;
                }
                faithText = new TextObject("{=!}Lords of {CURRENT_FAITH} faith may disapprove your change")
                .SetTextVariable("CURRENT_FAITH", playerReligion.Faith.GetFaithName());
            }
            

            var result = religion.Faith.GetInductionAllowed(Hero.MainHero, clergyman.Rank);
            if (!result.Item1)
            {
                hintText = result.Item2;
                return false;

                
            }

            hintText = new TextObject("{=!}{POSSIBLE}. Changing faiths will significantly impact your clan's renown, if you are converting from another faith. Your piety in the new faith will be zero. {FAITH_TEXT}")
                .SetTextVariable("POSSIBLE", result.Item2)
                .SetTextVariable("FAITH_TEXT", faithText);
            return true;
        }

        private void InductionOnConsequence()
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            ReligionsManager.AddToReligion(Hero.MainHero, religion);
        }

        private void BlessingPositiveAnswerOnConsequence()
        {
            var religion = ReligionsManager.GetHeroReligion(Hero.MainHero);
            var piety = ReligionsManager.GetPiety(religion, Hero.MainHero);

            var list = religion.Faith.GetSecondaryDivinities()
                .Select(div => new InquiryElement(div, div.Name.ToString(), null, piety >= div.BlessingCost(Hero.MainHero), new TextObject("{=oFfExhaM}{DESCRIPTION}\n{EFFECTS}").SetTextVariable("DESCRIPTION", div.Description)
                    .SetTextVariable("EFFECTS", div.Effects)
                    .ToString()))
                .ToList();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                religion.Faith.GetSecondaryDivinitiesDescription().ToString(),
                new TextObject("{=QUvKUF87}Select which of the {SECONDARIES} you would like to {BLESSING_ACTION}.")
                    .SetTextVariable("SECONDARIES", religion.Faith.GetSecondaryDivinitiesDescription())
                    .SetTextVariable("BLESSING_ACTION", religion.Faith.GetBlessingActionName())
                    .ToString(),
                list,
                false, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate(List<InquiryElement> x)
                {
                    var divinity = (Divinity?) x[0].Identifier;
                    selectedDivinity = divinity;
                },
                null,
                string.Empty));
        }

        private void BlessingConfirmOnConsequence()
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            ReligionsManager.AddBlessing(selectedDivinity, Hero.MainHero, religion, true);
        }

        private bool BlessingOnClickable(out TextObject hintText)
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            var playerReligion = ReligionsManager.GetHeroReligion(Hero.MainHero);


            if (playerReligion == null || religion.Faith.GetId() != playerReligion.Faith.GetId())
            {
                hintText = new TextObject("{=vE0bYBmL}You do not adhere to the {FAITH} faith.")
                    .SetTextVariable("FAITH", religion.Faith.GetFaithName());
                return false;
            }

            var anyPossible = false;
            var minPiety = 300f;
            if (playerReligion != null)
            {
                var piety = ReligionsManager.GetPiety(playerReligion, Hero.MainHero);
                
                foreach (var divinity in religion.Faith.GetSecondaryDivinities())
                {
                    var cost = divinity.BlessingCost(Hero.MainHero);
                    var optionPossible = piety >= cost;
                    if (optionPossible)
                    {
                        anyPossible = true;
                    }

                    if (cost < minPiety)
                    {
                        minPiety = cost;
                    }
                }
            }


            if (!anyPossible)
            {
                hintText = new TextObject("{=LCTBJedU}Not enough piety to receive any blessing (minimum {PIETY} piety).")
                    .SetTextVariable("PIETY", minPiety);
                return false;
            }

            hintText = new TextObject("{=TWny4y7r}Expend your piety in exchange of a blessing or inspiration.");
            return true;
        }

        private void BlessingOnConsequence()
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);

            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_CONFIRM", religion.Faith.GetBlessingConfirmQuestion());
            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_QUESTION", religion.Faith.GetBlessingQuestion());
        }

        private bool RitesOnClickable(out TextObject hintText)
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            Religion playerReligion = ReligionsManager.GetHeroReligion(Hero.MainHero);

            if (playerReligion == null || religion.Faith.GetId() != playerReligion.Faith.GetId())
            {
                hintText = new TextObject("{=vE0bYBmL}You do not adhere to the {FAITH} faith.")
                    .SetTextVariable("FAITH", religion.Faith.GetFaithName());
                return false;
            }

            bool anyPossible = religion.Rites.Any(rite => rite.MeetsCondition(Hero.MainHero));
            if (!anyPossible)
            {
                hintText = new TextObject("{=QbUTvLMt}No rite is currently possible to perform.");
                return false;
            }

            hintText = new TextObject("{=2Q6R8xum}Perform a rite such as an offering in exchange for piety.");
            return true;
        }

        private void RitesPositiveAnswerOnConsequence()
        {
            var religion = ReligionsManager.GetHeroReligion(Hero.MainHero);
            var piety = ReligionsManager.GetPiety(religion, Hero.MainHero);

            var list = religion.Rites.Select(rite => new InquiryElement(rite, rite.GetName().ToString(), null, rite.MeetsCondition(Hero.MainHero), rite.GetDescription().ToString())).ToList();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                religion.Faith.GetSecondaryDivinitiesDescription().ToString(),
                string.Empty, list,
                false, 1,
                GameTexts.FindText("str_done").ToString(), string.Empty,
                delegate(List<InquiryElement> x)
                {
                    var rite = (Rite?) x[0].Identifier;
                    selectedRite = rite;
                    rite.Execute(Hero.MainHero);
                },
                null,
                string.Empty));
        }

        private void RitesOnConsequence()
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);

            var faithText = new TextObject("{=eV3uOZCw}{FAITH} teaches us that we may perform {RITES}.");
            var riteText =
                new TextObject("{=1b08dLC7}Certainly, {HERO}. Remember that proving your devotion is a life-long process. Once a rite is done, some time is needed before it may be consummated again. {RITES}")
                    .SetTextVariable("HERO", Hero.MainHero.Name);

            var sb = new StringBuilder();
            foreach (var rite in religion.Rites)
            {
                sb.Append(rite.GetName() + ", ");
            }


            MBTextManager.SetTextVariable("CLERGYMAN_RITE", riteText.SetTextVariable("RITES", faithText
                .SetTextVariable("FAITH", religion.Faith.GetFaithName())
                .SetTextVariable("RITES", sb.ToString())));
        }


        private bool IsPreacher()
        {
            return Hero.OneToOneConversationHero.IsPreacher &&
                   BannerKingsConfig.Instance.ReligionsManager != null &&
                   ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
        }

        private bool OnConditionClergymanGreeting()
        {
            if (!IsPreacher())
            {
                return false;
            }

            InitializePreacherTexts();
            return true;

        }

        private void InitializePreacherTexts()
        {
            var clergyman =
                ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);
            var greeting = ReligionsManager.IsReligionMember(Hero.MainHero, religion)
                ? religion.Faith.GetClergyGreetingInducted(clergyman.Rank)
                : religion.Faith.GetClergyGreeting(clergyman.Rank);

            MBTextManager.SetTextVariable("CLERGYMAN_GREETING", greeting);
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING",
                religion.Faith.GetClergyPreachingAnswer(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_PREACHING_LAST",
                religion.Faith.GetClergyPreachingAnswerLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH", religion.Faith.GetClergyProveFaith(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_LAST",
                religion.Faith.GetClergyProveFaithLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH_FORBIDDEN",
                religion.Faith.GetClergyForbiddenAnswer(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_FAITH__FORBIDDEN_LAST",
                religion.Faith.GetClergyForbiddenAnswerLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION", religion.Faith.GetClergyInduction(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_INDUCTION_LAST",
                religion.Faith.GetClergyInductionLast(clergyman.Rank));
            MBTextManager.SetTextVariable("CLERGYMAN_BLESSING_ACTION", religion.Faith.GetBlessingAction());
        }

        private TroopRoster GetTroopsToJoinPlayerParty(List<MobileParty> parties)
        {
            var troopRoster = TroopRoster.CreateDummyTroopRoster();
            foreach (var mobileParty in parties)
            {
                if (mobileParty.IsBandit && !mobileParty.IsLordParty)
                {
                    for (var i = 0; i < mobileParty.MemberRoster.Count; i++)
                    {
                        if (!mobileParty.MemberRoster.GetCharacterAtIndex(i).IsHero)
                        {
                            troopRoster.AddToCounts(mobileParty.MemberRoster.GetCharacterAtIndex(i), mobileParty.MemberRoster.GetElementNumber(i), false, 0, 0, true, -1);
                        }
                    }
                    for (var j = 0; j < mobileParty.PrisonRoster.Count; j++)
                    {
                        if (!mobileParty.PrisonRoster.GetCharacterAtIndex(j).IsHero)
                        {
                            troopRoster.AddToCounts(mobileParty.PrisonRoster.GetCharacterAtIndex(j), mobileParty.PrisonRoster.GetElementNumber(j), false, 0, 0, true, -1);
                        }
                    }
                }
            }
            return troopRoster;
        }
    }

    namespace Patches
    {

        

        [HarmonyPatch(typeof(KingdomDecision), "GetInfluenceCostOfSupport")]
        internal class GetInfluenceCostOfSupportPatch
        {
            private static void Postfix(ref int __result, Clan clan, Supporter.SupportWeights supportWeight)
            {
                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(clan.Leader, 
                    DefaultDivinities.Instance.AseraSecondary1))
                {
                    var result = __result;
                    __result = (int)(result * 0.7f);
                }
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_puritan_preacher_introduction_on_condition")]
        internal class PuritanPreacherPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_minor_faction_preacher_introduction_on_condition")]
        internal class MinorFactionPreacherPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_mystic_preacher_introduction_on_condition")]
        internal class MysticPreacherPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior), "conversation_messianic_preacher_introduction_on_condition")]
        internal class MessianicPatch
        {
            private static void Postfix(ref bool __result)
            {
                if (BannerKingsConfig.Instance.ReligionsManager == null)
                {
                    return;
                }

                if (!Hero.OneToOneConversationHero.IsPreacher)
                {
                    return;
                }

                var bannerKings = BannerKingsConfig.Instance.ReligionsManager.IsPreacher(Hero.OneToOneConversationHero);
                __result = !bannerKings;
            }
        }
    }
}