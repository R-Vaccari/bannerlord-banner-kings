using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BannerKings.Extensions;
using BannerKings.Managers;
using BannerKings.Managers.Goals.Decisions;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Traits;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using Color = TaleWorlds.Library.Color;

namespace BannerKings.Behaviours
{
    public class BKReligionsBehavior : BannerKingsBehavior
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
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnOwnerChanged);
            CampaignEvents.OnSiegeAftermathAppliedEvent.AddNonSerializedListener(this, OnSiegeAftermath);
            CampaignEvents.MapEventEnded.AddNonSerializedListener(this, EventEnded);
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, () =>
            {
                foreach (Religion religion in BannerKingsConfig.Instance.ReligionsManager.GetReligions())
                {
                    if (religion.Faith.FaithGroup.ShouldHaveLeader)
                    {
                        Hero leader = religion.Faith.FaithGroup.EvaluatePossibleLeaders(religion).GetRandomElement();
                        if (leader != null)
                            religion.Faith.FaithGroup.MakeHeroLeader(religion, leader, null, false);
                    }
                }
            });

            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public ValueTuple<bool, TextObject> IsInstallingPreacherPossible(Hero hero, Settlement settlement)
        {
            ValueTuple<bool, TextObject> result = new ValueTuple<bool, TextObject>(true, TextObject.Empty);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);

            int piety = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel.GetAppointCost(hero, data.ReligionData).ResultNumber);
            int cost = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel.GetAppointInfluence(hero, data.ReligionData).ResultNumber);

            if (BannerKingsConfig.Instance.ReligionsManager.GetPiety(hero) < piety)
            {
                return new ValueTuple<bool, TextObject>(false, new TextObject("Not enough piety."));
            }

            if (hero.Clan.Influence < cost)
            {
                return new ValueTuple<bool, TextObject>(false, new TextObject("Not enough influence."));
            }

            return result;
        }

        public void InstallPreacher(PopulationData data, Hero hero, Religion heroReligion)
        {
            int piety = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel.GetAppointCost(hero, data.ReligionData).ResultNumber);
            int cost = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel.GetAppointInfluence(hero, data.ReligionData).ResultNumber);
            Clergyman clergy = heroReligion.GenerateClergyman(data.Settlement);
            if (clergy != null)
            {
                ChangeClanInfluenceAction.Apply(hero.Clan, -cost);
                BannerKingsConfig.Instance.ReligionsManager.AddPiety(hero, -piety, true);
                hero.AddSkillXp(BKSkills.Instance.Theology, 2000f);
                if (hero == Hero.MainHero)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                                        new TextObject("{=cM7fEchf}{HERO} was installed as a preacher at {FIEF}")
                                        .SetTextVariable("HERO", clergy.Hero.Name)
                                        .SetTextVariable("FIEF", data.Settlement.Name)
                                        .ToString(),
                                        Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                }
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddDialogue(starter);
        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            if (BannerKingsConfig.Instance.ReligionsManager != null)
            {
                BannerKingsConfig.Instance.ReligionsManager.InitializeReligions();
            }
        }

        private void EventEnded(MapEvent mapEvent) 
        {
            if ((int)mapEvent.WinningSide >= 0)
            {
                foreach (var eventParty in mapEvent.PartiesOnSide(mapEvent.WinningSide))
                {
                    if (eventParty.GainedInfluence > 0f && eventParty.GainedInfluence < float.MaxValue && 
                        eventParty.Party.LeaderHero != null)
                    {
                        Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(eventParty.Party.LeaderHero);
                        if (rel != null && rel.HasDoctrine(DefaultDoctrines.Instance.Warlike))
                        {
                            BannerKingsConfig.Instance.ReligionsManager.AddPiety(
                                eventParty.Party.LeaderHero,
                                eventParty.GainedInfluence, 
                                true);
                        }
                    }
                }
            }
        }

        private void OnSiegeAftermath(MobileParty attackerParty, Settlement settlement,
           SiegeAftermathAction.SiegeAftermath aftermathType,
           Clan previousSettlementOwner,
           Dictionary<MobileParty, float> partyContributions)
        {
            if (aftermathType == SiegeAftermathAction.SiegeAftermath.ShowMercy)
            {
                foreach (MobileParty party in partyContributions.Keys)
                {
                    if (!party.IsLordParty || party.LeaderHero == null) continue;

                    Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(party.LeaderHero);
                    if (rel == null) continue;

                    if (rel.HasDoctrine(DefaultDoctrines.Instance.OsricsVengeance))
                    {
                        BannerKingsConfig.Instance.ReligionsManager.AddPiety(party.LeaderHero,
                            MBRandom.RoundRandomized(settlement.Town.Prosperity * 0.03f), 
                            true);
                    }

                    if (settlement.Culture.StringId == BannerKingsConfig.EmpireCulture &&
                        rel.HasDoctrine(DefaultDoctrines.Instance.RenovatioImperi))
                    {
                        foreach (Hero notable in settlement.Notables)
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(party.LeaderHero,
                                notable,
                                5);
                        }
                    }
                }
            }
        }

        private void OnOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner,
            Hero capturerHero,
            ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (capturerHero == null)
            {
                return;
            }
            
            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(capturerHero, DefaultDivinities.Instance.Osric))
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                if (title != null && title.deJure != capturerHero)
                {
                    title.AddClaim(capturerHero, ClaimType.Fabricated);
                }
            }
        }

        private void OnDailyTickSettlement(Settlement settlement)
        {
            if (settlement == null || settlement.Notables == null)
            {
                return;
            }

            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            if (data != null && data.ReligionData != null)
            {
                var religion = data.ReligionData.DominantReligion;

                if (religion != null)
                {
                    AddOwnerPreacher(data);
                    CleanClergymen(settlement, religion);
                }
            }
        }

        private void AddOwnerPreacher(PopulationData data)
        {
            RunWeekly(() =>
            {
                Settlement settlement = data.Settlement;
                Hero owner = settlement.IsVillage ? settlement.Village.GetActualOwner() : settlement.OwnerClan.Leader;
                if (owner == Hero.MainHero) return;

                bool shouldAdd = false;
                var ownerRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(owner);
                foreach (Hero notable in settlement.Notables)
                {
                    if (notable.IsPreacher)
                    {
                        var notableRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(notable);
                        if (notableRel == ownerRel)
                        {
                            shouldAdd = false;
                        }
                    }
                }

                if (shouldAdd && IsInstallingPreacherPossible(owner, settlement).Item1)
                {
                    InstallPreacher(data, owner, ownerRel);
                }
            },
            GetType().Name,
            false);      
        }

        private void CleanClergymen(Settlement settlement, Religion religion)
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
                religion.AddClergyman(settlement, hero);
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

        private void OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent mapEvent)
        {
            foreach (var mapEventParty in mapEvent.AttackerSide.Parties)
            {
                if (mapEventParty.Party.IsActive)
                {
                    var mobileParty = mapEventParty.Party.MobileParty;
                    if (mobileParty != null && mobileParty.LeaderHero != null)
                    {
                        var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(mobileParty.LeaderHero);
                        if (rel == null) continue;

                        var settlementCulture = mapEvent.MapEventSettlement.Culture;
                        if (settlementCulture.StringId != "battania")
                        {
                            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(mobileParty.LeaderHero,
                                DefaultDivinities.Instance.AmraSecondary2, rel))
                            {
                                GainRenownAction.Apply(mobileParty.LeaderHero, 10f);
                            }
                        }

                        if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(mobileParty.LeaderHero,
                                DefaultDivinities.Instance.TreeloreMain, rel) && !rel.FavoredCultures.Contains(settlementCulture))
                        {
                            GainRenownAction.Apply(mobileParty.LeaderHero, 10f);
                        }

                        if (rel.MainCulture != settlementCulture && rel.HasDoctrine(DefaultDoctrines.Instance.Reavers))
                        {
                            BannerKingsConfig.Instance.ReligionsManager.AddPiety(rel, mobileParty.LeaderHero,
                                mapEvent.MapEventSettlement.Village.Hearth, true);
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

            if (hero.Clan != null && hero.Clan.Leader != null )
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero.Clan.Leader);
                if (rel != null && rel.HasDoctrine(DefaultDoctrines.Instance.Childbirth))
                {
                    hero.Clan.AddRenown(25f, true);
                }
            }

            if (bornNaturally)
            {
                if (hero.Father != null)
                {
                    if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero.Father, DefaultDivinities.Instance.SheWolf))
                    {
                        TraitObject random = BKTraits.Instance.CongenitalTraits.GetRandomElementInefficiently();
                        hero.SetTraitLevel(random, 1);
                    }

                    if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero.Father, DefaultDivinities.Instance.WindNorth))
                    {
                        BannerKingsConfig.Instance.ReligionsManager.AddPiety(hero.Father, 150f, true);
                    }
                }

                if (hero.Mother != null)
                {
                    if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero.Mother, DefaultDivinities.Instance.SheWolf))
                    {
                        TraitObject random = BKTraits.Instance.CongenitalTraits.GetRandomElementInefficiently();
                        hero.SetTraitLevel(random, 1);
                    }

                    if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(hero.Mother, DefaultDivinities.Instance.WindNorth))
                    {
                        BannerKingsConfig.Instance.ReligionsManager.AddPiety(hero.Mother, 150f, true);
                    }
                }
            }
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
            if (DefaultReligions.Instance.All.Count() == 0) return;

            Religion startingReligion = null;
            if (hero.Clan != null && hero != hero.Clan.Leader && hero.Clan.Leader != null)
            {
                startingReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero.Clan.Leader);
            }
            else if (hero.IsNotable)
            {
                var settlement = hero.CurrentSettlement != null ? hero.CurrentSettlement : hero.BornSettlement;
                if (settlement == null)
                {
                    return;
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null && data.ReligionData != null)
                {
                    startingReligion = data.ReligionData.GetRandomReligion();
                }
            }

            ReligionsManager.InitializeHeroFaith(hero, startingReligion);
        }

        private void OnDailyTickHero(Hero hero)
        {
            if (hero == null || hero.IsChild) return;

            TickFaithXp(hero);

            if (hero.Clan != null && hero.Clan == Clan.PlayerClan) return;

            TickRuler(hero);

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
                            new InformationMessage(new TextObject("{=KGqPK07Z}{COUNT} {UNIT} zealots have joined your party!")
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
                                if (title.Sovereign == empireTitle)
                                {
                                    bonus += 2f / (float)title.TitleType;
                                }
                            }

                            GainRenownAction.Apply(hero, bonus);
                        }
                    }
                }
            }
        }

        private void TickRuler(Hero hero)
        {
            if (hero.MapFaction == null || hero != hero.MapFaction.Leader || !hero.MapFaction.IsKingdomFaction) return;

            RunWeekly(() =>
            {
                FaithLeaderDecision decision = new FaithLeaderDecision(hero);
                decision.DoAiDecision();
            },
            GetType().Name,
            false);
        }

        private void TickFaithXp(Hero hero)
        {
            float piety = BannerKingsConfig.Instance.ReligionModel.CalculatePietyChange(hero).ResultNumber;
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
            foreach (var religion in ReligionsManager.GetReligions())
            {
                religion.Faith.FaithGroup.TickLeadership(religion);
                foreach (var hero in ReligionsManager.GetFaithfulHeroes(religion))
                {
                    ReligionsManager.AddPiety(religion, hero, BannerKingsConfig.Instance.ReligionModel.CalculatePietyChange(hero).ResultNumber);
                }
            }
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero || target.Town == null || BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target))
            {
                return;
            }

            foreach (var notable in target.Notables)
            {
                if (notable.IsPreacher)
                    Utils.Helpers.AddCharacterToKeep(notable, target);
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
                "lord_pretalk",
                "{=KdVPngCa}{CLERGYMAN_PREACHING_LAST}",
                IsPreacher, null);

            starter.AddPlayerLine("bk_question_faith", "hero_main_options", "bk_preacher_asked_faith",
                "{=rhPmXyLC}How do I prove my faith?",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_1", "bk_preacher_asked_faith", "bk_preacher_asked_faith_last",
                "{=Oa86My4f}{CLERGYMAN_FAITH}",
                IsPreacher, null);

            starter.AddDialogLine("bk_answer_faith_2", "bk_preacher_asked_faith_last", "lord_pretalk",
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
                "lord_pretalk",
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
                "lord_pretalk",
                "{=6d9H5id0}{CLERGYMAN_INDUCTION_LAST}",
                IsPreacher, InductionOnConsequence);

            starter.AddPlayerLine("bk_question_induction", "hero_main_options", "bk_preacher_asked_topics",
               "{=MwCfjHL6}Can I learn about specific topics?",
               IsPreacher,
               () =>
               {
                   Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetClergymanReligion(
                   BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero));

                   List<Divinity> divinities = new List<Divinity>(4);
                   divinities.Add(rel.Faith.MainGod);
                   foreach (var god in rel.Faith.GetSecondaryDivinities()) divinities.Add(god);
                   ConversationSentence.SetObjectsToRepeatOver(divinities, 5);
               });

            starter.AddDialogLine("bk_answer_induction_1", 
                "bk_preacher_asked_topics",
                "bk_preacher_asked_topics_options",
                "{=69KyJPSL}{CLERGYMAN_TOPICS}",
                () =>
                {
                    MBTextManager.SetTextVariable("CLERGYMAN_TOPICS",
                       new TextObject("{=MnsCVxH9}Certainly, {PLAYER.NAME}. What can I help you with?"));
                    return true;
                },
                null);

            starter.AddRepeatablePlayerLine("bk_preacher_asked_topics_options",
                "bk_preacher_asked_topics_options",
                "bk_preacher_asked_topics_answer",
                "{=Dhz4U9OP}{DIVINITY_NAME}",
                "{=hnp2M53N}I was thinking of another option",
                "bk_preacher_asked_topics_options",
                () =>
                {
                    Divinity divinity = ConversationSentence.CurrentProcessedRepeatObject as Divinity;
                    ConversationSentence.SelectedRepeatLine.SetTextVariable("DIVINITY_NAME", divinity.Name);
                    return divinity.Dialogue != null;
                },
                () =>
                {
                    Divinity divinity = ConversationSentence.SelectedRepeatObject as Divinity;
                    MBTextManager.SetTextVariable("DIVINITY_TEXT", divinity.Dialogue);
                    MBTextManager.SetTextVariable("DIVINITY_LAST_TEXT", divinity.LastDialogue);
                }, 100,
                null);

            starter.AddPlayerLine("bk_preacher_asked_topics_options_cancel",
                "bk_preacher_asked_topics_options",
                "lord_pretalk",
                "{=D33fIGQe}Never mind.",
                () => true,
                null);

            starter.AddDialogLine("bk_preacher_asked_topics_answer", "bk_preacher_asked_topics_answer",
                "bk_preacher_asked_topics_answer_last",
                "{=91fF5d2N}{DIVINITY_TEXT}",
                () => true,
                null
                );

            starter.AddDialogLine("bk_preacher_asked_topics_answer_last", "bk_preacher_asked_topics_answer_last",
               "bk_preacher_asked_topics_options",
               "{=3wUbufLY}{DIVINITY_LAST_TEXT}",
               () => true,
               null);

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
                "lord_pretalk",
                "{=G4ALCxaA}Never mind.", null, null);

            starter.AddDialogLine("bk_boon_confirm", "bk_boon_confirm", "bk_boon_confirm",
                "{=VKujVK2V}{CLERGYMAN_BLESSING_CONFIRM}",
                null, null);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "hero_main_options", "{=LPVNjXpT}See it done.",
                () => selectedDivinity != null, BlessingConfirmOnConsequence);

            starter.AddPlayerLine("bk_boon_confirm", "bk_boon_confirm", "lord_pretalk",
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

            starter.AddDialogLine("bk_answer_rite_impossible", "bk_preacher_asked_rites", "lord_pretalk",
                "{=rfjH2WMz}I am afraid that won't be possible.",
                IsPreacher,
                null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer", "bk_rite_confirm",
                "{=8B2pePnb}I have decided.", null,
                null);

            starter.AddPlayerLine("bk_preacher_asked_rites_answer", "bk_preacher_asked_rites_answer",
                "lord_pretalk",
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
            starter.AddPlayerLine("bk_rite_confirm", "bk_rite_confirm", "lord_pretalk",
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
                faithText = new TextObject("{=WWkVwmPy}Lords of {CURRENT_FAITH} faith may disapprove your change")
                .SetTextVariable("CURRENT_FAITH", playerReligion.Faith.GetFaithName());
            }

            var result = religion.Faith.GetInductionAllowed(Hero.MainHero, clergyman.Rank);
            hintText = new TextObject("{=VqkEaJWp}{POSSIBLE}.\n\nChanging faiths will significantly impact your clan's renown, if you are converting from another faith. Your piety in the new faith will be zero. {FAITH_TEXT}")
                .SetTextVariable("POSSIBLE", result.Item2)
                .SetTextVariable("FAITH_TEXT", faithText);
            return true;
        }

        private void InductionOnConsequence()
        {
            var clergyman = ReligionsManager.GetClergymanFromHeroHero(Hero.OneToOneConversationHero);
            var religion = ReligionsManager.GetClergymanReligion(clergyman);

            var result = religion.Faith.GetInductionAllowed(Hero.MainHero, clergyman.Rank);
            if (result.Item1)
            {
                ReligionsManager.AddToReligion(Hero.MainHero, religion);
            }
        }

        private void BlessingPositiveAnswerOnConsequence()
        {
            var religion = ReligionsManager.GetHeroReligion(Hero.MainHero);
            var piety = ReligionsManager.GetPiety(religion, Hero.MainHero);

            var list = religion.Faith.GetSecondaryDivinities()
                .Select(div => new InquiryElement(div, 
                div.Name.ToString(), 
                null, 
                piety >= div.BlessingCost(Hero.MainHero, religion.Faith), 
                new TextObject("{=oFfExhaM}{DESCRIPTION}\n{EFFECTS}").SetTextVariable("DESCRIPTION", div.Description)
                    .SetTextVariable("EFFECTS", div.Effects)
                    .ToString()))
            .ToList();

            Divinity mainDivinity = religion.Faith.GetMainDivinity();
            list.Add(new InquiryElement(mainDivinity,
                mainDivinity.Name.ToString(),
                null,
                piety >= mainDivinity.BlessingCost(Hero.MainHero, religion.Faith),
                new TextObject("{=oFfExhaM}{DESCRIPTION}\n{EFFECTS}").SetTextVariable("DESCRIPTION", mainDivinity.Description)
                    .SetTextVariable("EFFECTS", mainDivinity.Effects)
                    .ToString()));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                religion.Faith.GetCultsDescription().ToString(),
                new TextObject("{=QUvKUF87}Select which of the {SECONDARIES} you would like to {BLESSING_ACTION}.")
                    .SetTextVariable("SECONDARIES", religion.Faith.GetCultsDescription())
                    .SetTextVariable("BLESSING_ACTION", religion.Faith.GetBlessingActionName())
                    .ToString(),
                list,
                false, 
                1,
                1,
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
                    var cost = divinity.BlessingCost(Hero.MainHero, playerReligion.Faith);
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

            StringBuilder sb = new StringBuilder();
            sb.Append(new TextObject("{=OXP2Kb4E}Each rite has different conditions to be fulfilled. They also have time intervals before being able to me performed again, check their details in the Religion tab."));
            foreach (Rite rite in religion.Rites)
            {
                TextObject reason;
                bool possible = rite.MeetsCondition(Hero.MainHero, out reason);
                if (!possible)
                {
                    sb.Append(new TextObject("{=UcLBbKzj}\n\n{RITE}: {REASON}")
                        .SetTextVariable("RITE", rite.GetName())
                        .SetTextVariable("REASON", reason));
                }
            }
            TextObject r;
            bool anyPossible = religion.Rites.Any(rite => rite.MeetsCondition(Hero.MainHero, out r));
            if (!anyPossible)
            {
                hintText = new TextObject("{=!}" + sb.ToString());
                return false;
            }

            hintText = new TextObject("{=2Q6R8xum}Perform a rite such as an offering in exchange for piety.");
            return true;
        }

        private void RitesPositiveAnswerOnConsequence()
        {
            var religion = ReligionsManager.GetHeroReligion(Hero.MainHero);
            var piety = ReligionsManager.GetPiety(religion, Hero.MainHero);

            var list = religion.Rites.Select(rite =>
            {
                TextObject reason;
                bool available = rite.MeetsCondition(Hero.MainHero, out reason);
                return new InquiryElement(rite,
                           rite.GetName().ToString(),
                           null,
                           available,
                           new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                           .SetTextVariable("TEXT", rite.GetDescription().ToString())
                           .SetTextVariable("EXPLANATIONS", reason).ToString());
            }
           ).ToList();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=Yy2s38FQ}Rites").ToString(),
                new TextObject("{=B4M6aqo5}Select what rite you would like to perform. Check their descriptions and entries on Religions tab for details.").ToString(), 
                list,
                false, 
                1,
                1,
                GameTexts.FindText("str_done").ToString(), 
                string.Empty,
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
            int count = religion.Rites.Count;
            for (int i = 0; i < count; i++)
            {
                var rite = religion.Rites.ElementAt(i);
                if (i != count - 1) 
                {
                    sb.Append(rite.GetName() + ", ");
                }
                else
                {
                    sb.Append(rite.GetName());
                }
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
