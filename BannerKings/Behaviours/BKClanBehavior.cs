using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Managers.Court;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Settings;
using HarmonyLib;
using Helpers;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.CampaignSystem.SkillEffect;

namespace BannerKings.Behaviours
{
    public class BKClanBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, DailyClanTick);
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
            CampaignEvents.OnHeroGetsBusyEvent.AddNonSerializedListener(this, OnHeroGetsBusy);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddDialogLine("ally_thanks_after_helping_in_battle", 
                "start", 
                "close_window",
                "{=!}Thank you, {?PLAYER.GENDER}madam{?}sir{\\?}. I will tell the {CLAN} of your deeds.", 
                () =>
                {
                    if (Hero.OneToOneConversationHero == null)
                    {
                        return false;
                    }

                    if (Hero.OneToOneConversationHero.Clan != null)
                    {
                        MBTextManager.SetTextVariable("CLAN", Hero.OneToOneConversationHero.Clan.Name);
                    }

                    return IsCompanionOfAnotherClan() && MapEvent.PlayerMapEvent != null && 
                    !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.OneToOneConversationHero.MapFaction) && 
                    MapEvent.PlayerMapEvent.WinningSide == PartyBase.MainParty.Side;
                }, 
                () =>
                {
                    int playerGainedRelationAmount = 2;
                    ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, playerGainedRelationAmount, true, true);
                    if (Hero.OneToOneConversationHero.IsPrisoner)
                    {
                        EndCaptivityAction.ApplyByReleasedAfterBattle(Hero.OneToOneConversationHero);
                    }
                },
                110, null);

            starter.AddDialogLine("default_conversation_for_wrongly_created_heroes", "start", "close_window", 
                "{=!}I am under your mercy.", 
                null,
                () =>
                {
                    TakePrisonerAction.Apply(Campaign.Current.MainParty.Party, CharacterObject.OneToOneConversationCharacter.HeroObject);
                }, 
                0, null);

            starter.AddDialogLine("companion_captured",
              "companion_captured",
              "close_window",
              "{=!}As you say.",
              null,
              () =>
              {
                  TakePrisonerAction.Apply(Campaign.Current.MainParty.Party, CharacterObject.OneToOneConversationCharacter.HeroObject);
              });

            starter.AddPlayerLine("default_conversation_for_wrongly_created_heroes",
              "start",
              "companion_captured",
              "{=!}You'll be coming with me now.",
              () => IsCompanionOfAnotherClan() && Campaign.Current.CurrentConversationContext == ConversationContext.CapturedLord,
              null);

            starter.AddPlayerLine("meet_wanderer_different_clan", 
                "wanderer_meet_player_response", 
                "wanderer_different_clan_response",
                "{=!}My name is {PLAYER.NAME}, {?PLAYER.GENDER}madam{?}sir{\\?}. Tell me about yourself.",
                IsCompanionOfAnotherClan,
                null);

            starter.AddDialogLine("wanderer_different_clan_response_first_time",
                "wanderer_different_clan_response", 
                "wanderer_different_clan_options",
                "{=hBCC1HRS}{WANDERER_OTHER_CLAN}",
                CompanionOfAnotherClanIntroduction,
                null);

            starter.AddPlayerLine("wanderer_different_clan_options",
                "wanderer_different_clan_options",
                "hero_leave",
                "{=9mBy0qNW}I must leave now.",
                null,
                null);

            starter.AddDialogLine("wanderer_different_clan_response_first_time",
                "wanderer_preintroduction",
                "wanderer_different_clan_options",
                "{=hBCC1HRS}{WANDERER_OTHER_CLAN}",
                CompanionOfAnotherClanIntroduction,
                null);

            starter.AddDialogLine("companion_hire_different_clan", 
                "companion_hire",
                "wanderer_different_clan_options",
                "{=znLZBRbD}{WANDERER_OTHER_CLAN_HIRE}", 
                () =>
                {
                    var clan = Hero.OneToOneConversationHero.Clan;
                    if (clan != null)
                    {
                        MBTextManager.SetTextVariable("WANDERER_OTHER_CLAN_HIRE", new TextObject("{=kf3MeZSH}I currently serve the {CLAN} and so I'm not available for hire.")
                            .SetTextVariable("CLAN", clan.Name));
                    }

                    return Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan;
                },
                null, 100, null);
        }

        private bool IsCompanionOfAnotherClan() => CharacterObject.OneToOneConversationCharacter != null && CharacterObject.OneToOneConversationCharacter.IsHero && 
                    CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer && 
                    CharacterObject.OneToOneConversationCharacter.HeroObject.HeroState != Hero.CharacterStates.Prisoner &&
                    Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan &&
                    Hero.OneToOneConversationHero.Clan.StringId != "neutral";

        private bool CompanionOfAnotherClanIntroduction()
        {
            if (Hero.OneToOneConversationHero.Clan != null)
            {
                var purposeText = new TextObject("{=kxWQ9L87}What can I help you with?");
                if (Hero.OneToOneConversationHero.Clan.MapFaction != Clan.PlayerClan.MapFaction)
                {
                    purposeText = new TextObject("{=Ff9zAHiQ}So then, what is it?");
                }

                MBTextManager.SetTextVariable("WANDERER_OTHER_CLAN",
                    new TextObject("{=c3b1OA4H}I am {HERO}, a servant of the {CLAN}. I am here under business of {LEADER_NAME}. {PURPOSE_TEXT}")
                    .SetTextVariable("HERO", Hero.OneToOneConversationHero.Name)
                    .SetTextVariable("CLAN", Hero.OneToOneConversationHero.Clan.Name)
                    .SetTextVariable("LEADER_NAME", Hero.OneToOneConversationHero.Clan.Name)
                    .SetTextVariable("PURPOSE_TEXT", purposeText));
            }
            
            return IsCompanionOfAnotherClan();
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
        {
            if (newKingdom != null)
            {
                var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
                if (council != null)
                {
                    if (detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinKingdom)
                    {
                        if (council.Peerage == null || !council.Peerage.CanVote)
                        {
                            council.SetPeerage(new Peerage(new TextObject("{=rmxeMFzz}Lesser Peerage"),
                                true,
                                false,
                                false,
                                false,
                                true,
                                true));

                            if (clan == Clan.PlayerClan)
                            {
                                var peerage = council.Peerage;
                                InformationManager.ShowInquiry(new InquiryData(
                                    peerage.Name.ToString(),
                                    new TextObject("{=dVTqLz5i}As part of joinning a realm, the {CLAN} is receiving {PEERAGE}. {TEXT}")
                                    .SetTextVariable("CLAN", Clan.PlayerClan.Name)
                                    .SetTextVariable("PEERAGE", peerage.Name)
                                    .SetTextVariable("TEXT", peerage.PeerageGrantedText())
                                    .ToString(),
                                    true,
                                    false,
                                    GameTexts.FindText("str_ok").ToString(),
                                    String.Empty,
                                    null,
                                    null));
                            }
                        }
                    }

                    if (detail == ChangeKingdomAction.ChangeKingdomActionDetail.CreateKingdom)
                    {
                        if (council.Peerage == null || !council.Peerage.CanStartElection)
                        {
                            council.SetPeerage(new Peerage(new TextObject("{=9OhMK2Wk}Full Peerage"), true, 
                                true, true, true, true, false));

                            if (clan == Clan.PlayerClan)
                            {
                                var peerage = council.Peerage;
                                InformationManager.ShowInquiry(new InquiryData(
                                    peerage.Name.ToString(),
                                    new TextObject("{=9hmdku7t}As part of creating a realm, the {CLAN} is now considered to have {PEERAGE}. {TEXT}")
                                    .SetTextVariable("CLAN", Clan.PlayerClan.Name)
                                    .SetTextVariable("PEERAGE", peerage.Name)
                                    .SetTextVariable("TEXT", peerage.PeerageGrantedText())
                                    .ToString(),
                                    true,
                                    false,
                                    GameTexts.FindText("str_ok").ToString(),
                                    String.Empty,
                                    null,
                                    null));
                            }
                        }
                    }
                }
            }
        }

        private void OnHeroGetsBusy(Hero hero, HeroGetsBusyReasons heroGetsBusyReason)
        {
            if (hero.CompanionOf != null)
            {
                RemovePartyRoleIfExist(hero);
            }
        }

        private void OnHeroPrisonerTaken(PartyBase party, Hero prisoner)
        {
            if (prisoner.CompanionOf != null)
            {
                RemovePartyRoleIfExist(prisoner);
            }
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail,
            bool showNotification = true)
        {
            if (victim.CompanionOf != null)
            {
                RemovePartyRoleIfExist(victim);
            }
        }

        private void RemovePartyRoleIfExist(Hero hero)
        {
            foreach (var warPartyComponent in hero.Clan.WarPartyComponents)
            {
                if (warPartyComponent.MobileParty.GetHeroPerkRole(hero) != PerkRole.None)
                {
                    warPartyComponent.MobileParty.RemoveHeroPerkRole(hero);
                }
            }
        }

        private void DailyClanTick(Clan clan)
        {
            if (clan.IsEliminated || clan.IsBanditFaction || clan.Kingdom == null || clan.Leader == null)
            {
                return;
            }

            BannerKingsConfig.Instance.CourtManager.UpdateCouncil(clan);

            var councillours = BannerKingsConfig.Instance.CourtManager.GetCouncilloursCount(clan);
            if (councillours != 0)
            {
                clan.Leader.AddSkillXp(BKSkills.Instance.Lordship, councillours * 2f);
            }

            if (clan == Clan.PlayerClan || clan.IsUnderMercenaryService || clan.IsMinorFaction || clan.IsBanditFaction)
            {
                return;
            }

            EvaluateRecruitKnight(clan);
            EvaluateRecruitCompanion(clan);
            SetCompanionParty(clan);
        }

        private void SetCompanionParty(Clan clan)
        {
            if (clan.Companions == null || clan.Companions.Count == 0)
            {
                return;
            }

            foreach (var companion in clan.Companions)
            {
                if (!companion.IsWanderer || companion.IsPrisoner || !companion.IsReady)
                {
                    continue;
                }

                if (companion.PartyBelongedTo == null || companion.PartyBelongedTo.LeaderHero == null)
                {
                    goto Skills;
                }

                if (companion.PartyBelongedTo.LeaderHero == companion ||
                    companion.PartyBelongedTo.LeaderHero.Clan != companion.Clan)
                {
                    continue;
                }

                Skills:
                var role = companion.PartyBelongedTo != null ? companion.PartyBelongedTo.GetHeroPerkRole(companion) : PerkRole.None;
                if (role != PerkRole.None)
                {
                    continue;
                }

                if (companion.GetSkillValue(DefaultSkills.Medicine) >= 60)
                {
                    role = PerkRole.Surgeon;
                }
                else if (companion.GetSkillValue(DefaultSkills.Engineering) >= 60)
                {
                    role = PerkRole.Engineer;
                }
                else if (companion.GetSkillValue(DefaultSkills.Steward) >= 60)
                {
                    role = PerkRole.Quartermaster;
                }
                else if (companion.GetSkillValue(DefaultSkills.Scouting) >= 60)
                {
                    role = PerkRole.Scout;
                }

                if (clan.WarPartyComponents.Count <= 0)
                {
                    continue;
                }

                var warParty = clan.WarPartyComponents.GetRandomElementWithPredicate(x => IsRoleFree(x.MobileParty, role));
                if (warParty != null)
                {
                    AssignToRole(warParty.MobileParty, role, companion);
                }
                else
                {
                    AssignToRole(clan.WarPartyComponents.GetRandomElement().MobileParty, PerkRole.None, companion);
                }
            }
        }

        private bool IsRoleFree(MobileParty party, PerkRole role)
        {
            if (role == PerkRole.None)
            {
                return true;
            }

            switch (role)
            {
                case PerkRole.Scout:
                    return party.EffectiveScout == party.LeaderHero || party.EffectiveScout == null;
                case PerkRole.Engineer:
                    return party.EffectiveEngineer == party.LeaderHero || party.EffectiveEngineer == null;
                case PerkRole.Quartermaster:
                    return party.EffectiveQuartermaster == party.LeaderHero || party.EffectiveQuartermaster == null;
                case PerkRole.Surgeon:
                    return party.EffectiveSurgeon == party.LeaderHero || party.EffectiveSurgeon == null;
            }

            return true;
        }

        private void AssignToRole(MobileParty party, PerkRole role, Hero hero)
        {
            AddHeroToPartyAction.Apply(hero, party, false);
            switch (role)
            {
                case PerkRole.Scout when party.EffectiveScout != party.LeaderHero:
                    party.SetPartyScout(hero);
                    break;
                case PerkRole.Engineer when party.EffectiveEngineer != party.LeaderHero:
                    party.SetPartyEngineer(hero);
                    break;
                case PerkRole.Quartermaster when party.EffectiveQuartermaster != party.LeaderHero:
                    party.SetPartyQuartermaster(hero);
                    break;
                case PerkRole.Surgeon when party.EffectiveSurgeon != party.LeaderHero:
                    party.SetPartySurgeon(hero);
                    break;
            }
        }

        private void EvaluateRecruitCompanion(Clan clan)
        {
            if (clan.Leader.PartyBelongedTo == null || clan.Leader.IsPrisoner || clan.Companions.Count >= clan.CompanionLimit)
            {
                return;
            }

            var warParty = clan.WarPartyComponents.FirstOrDefault(x => x.Leader == clan.Leader);
            if (warParty?.MobileParty == null)
            {
                return;
            }

            var mobileParty = warParty.MobileParty;
            if (!mobileParty.IsActive || !mobileParty.IsReady)
            {
                return;
            }

            var candidates = new List<(PerkRole, float)>();

            if (IsRoleFree(mobileParty, PerkRole.Scout))
            {
                candidates.Add(new ValueTuple<PerkRole, float>(PerkRole.Scout, 1f));
            }

            if (IsRoleFree(mobileParty, PerkRole.Surgeon))
            {
                candidates.Add(new ValueTuple<PerkRole, float>(PerkRole.Surgeon, 1f));
            }

            if (IsRoleFree(mobileParty, PerkRole.Engineer))
            {
                candidates.Add(new ValueTuple<PerkRole, float>(PerkRole.Engineer, 1f));
            }

            if (IsRoleFree(mobileParty, PerkRole.Quartermaster))
            {
                candidates.Add(new ValueTuple<PerkRole, float>(PerkRole.Quartermaster, 1f));
            }

            if (candidates.Count == 0)
            {
                return;
            }

            var result = MBRandom.ChooseWeighted(candidates);
            var traits = new Dictionary<PerkRole, List<TraitObject>>
            {
                {
                    PerkRole.Scout,
                    new List<TraitObject>
                    {
                        DefaultTraits.WoodsScoutSkills,
                        DefaultTraits.SteppeScoutSkills,
                        DefaultTraits.HillScoutSkills,
                        DefaultTraits.DesertScoutSkills
                    }
                },
                {PerkRole.Surgeon, new List<TraitObject> {DefaultTraits.Surgery}},
                {PerkRole.Engineer, new List<TraitObject> {DefaultTraits.Siegecraft}},
                {PerkRole.Quartermaster, new List<TraitObject> {DefaultTraits.Manager}}
            };

            var template = GetAdequateTemplate(traits[result], clan.Culture);
            if (template == null)
            {
                return;
            }

            var equipment = GetEquipmentIfPossible(clan, false);
            if (equipment == null)
            {
                return;
            }

            var hero = HeroCreator.CreateSpecialHero(template, null, null, null, Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment);
            hero.CompanionOf = clan;
            AssignToRole(mobileParty, result, hero);
        }

        private CharacterObject GetAdequateTemplate(List<TraitObject> traits, CultureObject culture)
        {
            CharacterObject template = null;
            foreach (var trait in traits.Where(_ => template == null))
            {
                template = (from x in culture.NotableAndWandererTemplates
                    where x.Occupation == Occupation.Wanderer && x.GetTraitLevel(trait) >= 2
                    select x).GetRandomElementInefficiently();
            }

            return template;
        }

        private Equipment GetEquipmentIfPossible(Clan clan, bool noble, Town town = null)
        {
            var source = from e in MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>()
                where e.EquipmentCulture == clan.Culture
                select e;
            if (source == null)
            {
                return null;
            }

            var roster = (from e in source where e.EquipmentCulture == clan.Culture select e).ToList()
                .GetRandomElementWithPredicate(x =>
                    noble
                        ? x.HasEquipmentFlags(EquipmentFlags.IsMediumTemplate)
                        : x.StringId.Contains("bannerkings_companion"));

            if (roster == null)
            {
                return null;
            }

            town ??= Town.AllTowns.FirstOrDefault(x => x.Culture == clan.Culture);

            if (town == null)
            {
                return null;
            }

            var price = GetPrice(town.Settlement, roster);
            return clan.Leader.Gold >= price * 2f 
                ? roster.AllEquipments.GetRandomElement() 
                : null;
        }

        private void EvaluateRecruitKnight(Clan clan)
        {
            if (clan.WarPartyComponents.Count >= clan.CommanderLimit || clan.Companions.Count >= clan.CompanionLimit || clan.Settlements.Count(x => x.IsVillage) <= 1 || !(clan.Influence >= BannerKingsConfig.Instance.TitleModel.GetGrantKnighthoodCost(clan.Leader).ResultNumber))
            {
                return;
            }

            var village = clan.Settlements.FirstOrDefault(x => x.IsVillage);
            if (village == null)
            {
                return;
            }

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (council == null || council.Peerage == null || !council.Peerage.CanGrantKnighthood)
            {
                return;
            }

            var clanTitles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(village);
            if (clanTitles.Count == 0 || title == null || !clanTitles.Contains(title) || title.deJure != clan.Leader)
            {
                return;
            }

            CharacterObject template;
            var genderLaw = title.contract.GenderLaw;
            if (genderLaw == GenderLaw.Agnatic)
            {
                template = (from e in clan.Culture.NotableAndWandererTemplates
                    where e.Occupation == Occupation.Wanderer && !e.IsFemale
                    select e).GetRandomElementInefficiently();
            }

            else
            {
                template = (from e in clan.Culture.NotableAndWandererTemplates
                    where e.Occupation == Occupation.Wanderer
                    select e).GetRandomElementInefficiently();
            }

            if (template == null)
            {
                return;
            }

            var settlement = clan.Settlements.FirstOrDefault() ?? Town.AllTowns.FirstOrDefault(x => x.Culture == clan.Culture).Settlement;
            var source = from e in MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>() where e.EquipmentCulture == clan.Culture select e;
            if (source == null)
            {
                return;
            }

            var roster = (from e in source
                where e.HasEquipmentFlags(EquipmentFlags.IsMediumTemplate)
                select e
                into x
                orderby MBRandom.RandomInt()
                select x).FirstOrDefault();
            if (roster == null)
            {
                return;
            }

            var price = GetPrice(village.Village.Bound, roster);
            if (clan.Leader.Gold < price * 2f)
            {
                return;
            }

            var hero = HeroCreator.CreateSpecialHero(template, settlement, clan, null, Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
            BannerKingsConfig.Instance.TitleManager.GrantKnighthood(title, hero, title.deJure);
            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, roster.AllEquipments.GetRandomElement());
            var mainParty = hero.PartyBelongedTo == MobileParty.MainParty;
            MobilePartyHelper.CreateNewClanMobileParty(hero, clan, out mainParty);
            var component = clan.WarPartyComponents.FirstOrDefault(x => x.Leader == hero);
            if (component != null)
            {
                EnterSettlementAction.ApplyForParty(component.MobileParty, settlement);
            }
        }

        private float GetPrice(Settlement settlement, MBEquipmentRoster roster)
        {
            float price = 0;
            if (settlement == null)
            {
                return price * 0.5f;
            }

            var equip = roster.AllEquipments.GetRandomElement();
            for (var i = 0; i < 12; i++)
            {
                var element = new EquipmentElement(equip[i].Item, equip[i].ItemModifier);
                if (!element.IsEmpty && element.Item != null)
                {
                    price += settlement.Town.MarketData.GetPrice(element.Item);
                }
            }

            return price * 0.5f;
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(SettlementClaimantDecision))]
        internal class FiefOwnerPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("DetermineInitialCandidates")]
            private static bool DetermineInitialCandidatesPrefix(SettlementClaimantDecision __instance, 
                ref IEnumerable<DecisionOutcome> __result)
            {
                Kingdom kingdom = (Kingdom)__instance.Settlement.MapFaction;
                List<SettlementClaimantDecision.ClanAsDecisionOutcome> list = new List<SettlementClaimantDecision.ClanAsDecisionOutcome>();
                foreach (Clan clan in kingdom.Clans)
                {
                    if (clan != __instance.ClanToExclude && !clan.IsUnderMercenaryService && !clan.IsEliminated && !clan.Leader.IsDead)
                    {
                        var peerage = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan).Peerage;
                        if (peerage == null || !peerage.CanHaveFief) continue;

                        list.Add(new SettlementClaimantDecision.ClanAsDecisionOutcome(clan));
                    }
                }
                __result = list;
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("CalculateMeritOfOutcome")]
            private static void CalculateMeritOfOutcomePostfix(SettlementClaimantDecision __instance,
               DecisionOutcome candidateOutcome, ref float __result)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    SettlementClaimantDecision.ClanAsDecisionOutcome clanAsDecisionOutcome = (SettlementClaimantDecision.ClanAsDecisionOutcome)candidateOutcome;
                    Clan clan = clanAsDecisionOutcome.Clan;

                    var limit = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(clan.Leader).ResultNumber;
                    var current = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(clan).ResultNumber;
                    float factor = current / limit;
                    __result *= 1f - factor;
                }
            }
        }

        [HarmonyPatch(typeof(LordConversationsCampaignBehavior))]
        internal class LordDialoguePatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("conversation_player_ask_prisoners_on_consequence")]
            private static bool LordPrisonersPrefix()
            {
                List<Hero> list = new List<Hero>();
                foreach (TroopRosterElement troopRosterElement in Hero.OneToOneConversationHero.PartyBelongedTo.PrisonRoster.GetTroopRoster())
                {
                    if (troopRosterElement.Character.IsHero && troopRosterElement.Character.HeroObject.IsLord)
                    {
                        list.Add(troopRosterElement.Character.HeroObject);
                    }
                }
                ConversationSentence.SetObjectsToRepeatOver(list, 5);

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("conversation_wanderer_meet_player_on_condition")]
            private static bool MeetCompanionPrefix(ref bool __result)
            {
                __result = CharacterObject.OneToOneConversationCharacter != null && 
                    CharacterObject.OneToOneConversationCharacter.IsHero && 
                    CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer && 
                    CharacterObject.OneToOneConversationCharacter.HeroObject.HeroState != Hero.CharacterStates.Prisoner && 
                    Hero.OneToOneConversationHero.Clan == null;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("conversation_wanderer_preintroduction_on_condition")]
            private static void PreIntroductionPostfix(ref bool __result)
            {
                if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan)
                {
                    __result = false;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("conversation_companion_hire_gold_on_condition")]
            private static void HirePostfix(ref bool __result)
            {
                if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(ClanVariablesCampaignBehavior), "MakeClanFinancialEvaluation")]
        internal class MakeClanFinancialEvaluationPatch
        {
            private static bool Prefix(Clan clan)
            {
                if (clan.IsMinorFaction)
                {
                    return true;
                }

                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var war = false;
                    if (clan.Kingdom != null)
                    {
                        war = FactionManager.GetEnemyKingdoms(clan.Kingdom).Any();
                    }

                    var income = Campaign.Current.Models.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber *
                                 (war ? 0.5f : 0.2f);
                    if (war)
                    {
                        income += clan.Gold * 0.05f;
                    }

                    if (income > 0f)
                    {
                        var knights = 0f;
                        foreach (var partyComponent in clan.WarPartyComponents)
                        {
                            if (partyComponent.Leader != null && partyComponent.Leader != clan.Leader)
                            {
                                var title =
                                    BannerKingsConfig.Instance.TitleManager.GetHighestTitle(partyComponent.Leader);
                                if (title is {fief: { }})
                                {
                                    knights++;
                                    var limit = 0f;
                                    if (title.fief.IsVillage)
                                    {
                                        limit = BannerKingsConfig.Instance.TaxModel.CalculateVillageTaxFromIncome(title.fief.Village).ResultNumber;
                                    }
                                    else if (title.fief.Town != null)
                                    {
                                        limit = Campaign.Current.Models.SettlementTaxModel
                                            .CalculateTownTax(title.fief.Town).ResultNumber;
                                    }

                                    partyComponent.MobileParty.PaymentLimit = (int) (50f + limit);
                                }
                            }
                        }

                        foreach (var partyComponent in clan.WarPartyComponents)
                        {
                            var share = income / clan.WarPartyComponents.Count - knights;
                            partyComponent.MobileParty.PaymentLimit = (int) (300f + share);
                        }

                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ClanVariablesCampaignBehavior), "UpdateClanSettlementAutoRecruitment")]
        internal class AutoRecruitmentPatch
        {
            private static bool Prefix(Clan clan)
            {
                if (clan.MapFaction is {IsKingdomFaction: true})
                {
                    var enemies = FactionManager.GetEnemyKingdoms(clan.Kingdom);
                    foreach (var settlement in clan.Settlements)
                    {
                        if (settlement.IsFortification && settlement.Town.GarrisonParty != null)
                        {
                            if (enemies.Count() >= 0 && settlement.Town.GarrisonParty.MemberRoster.TotalManCount < 500)
                            {
                                settlement.Town.GarrisonAutoRecruitmentIsEnabled = true;
                            }

                            settlement.Town.GarrisonAutoRecruitmentIsEnabled = false;
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(DefaultClanFinanceModel))]
        internal class ClanFinancesPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("AddIncomeFromKingdomBudget", MethodType.Normal)]
            private static bool KingdomBudgetPrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                    return title is {contract: { }} &&
                           title.contract.Rights.Contains(FeudalRights.Assistance_Rights);
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddIncomeFromParty", MethodType.Normal)]
            private static bool AddIncomeFromPartyPrefix(MobileParty party, Clan clan, ref ExplainedNumber goldChange,
                bool applyWithdrawals)
            {
                if (BannerKingsConfig.Instance.TitleManager != null && party.LeaderHero != null &&
                    party.LeaderHero != clan.Leader)
                {
                    return BannerKingsConfig.Instance.TitleManager.GetHighestTitle(party.LeaderHero) == null;
                }

                if (party.IsCaravan)
                {
                    return !BannerKingsSettings.Instance.RealisticCaravanIncome;
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddExpensesFromGarrisons", MethodType.Normal)]
            private static bool GarrisonsPrefix(Clan clan, ref ExplainedNumber goldChange,
                bool applyWithdrawals = false)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var model = new DefaultClanFinanceModel();
                    var calculateWage = model.GetType().GetMethod("CalculatePartyWage",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (clan == Clan.PlayerClan)
                    {
                        Console.WriteLine();
                    }

                    foreach (var town in clan.Fiefs)
                    {
                        var garrisonParty = town.GarrisonParty;

                        if (garrisonParty is {IsActive: true})
                        {
                            var wage = (int) calculateWage.Invoke(model,
                                new object[] {garrisonParty, clan.Gold, applyWithdrawals});
                            if (wage > 0)
                            {
                                goldChange.Add(-wage, new TextObject("{=tqCSk7ya}Party wages {A0}"),
                                    garrisonParty.Name);
                            }
                        }
                    }

                    return false;
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddExpensesFromParties", MethodType.Normal)]
            private static bool PartyExpensesPrefix(Clan clan, ref ExplainedNumber goldChange,
                bool applyWithdrawals = false)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var list = new List<MobileParty>();
                    foreach (var hero in clan.Lords)
                    foreach (var caravanPartyComponent in hero.OwnedCaravans)
                    {
                        list.Add(caravanPartyComponent.MobileParty);
                    }

                    foreach (var hero2 in clan.Companions)
                    foreach (var caravanPartyComponent2 in hero2.OwnedCaravans)
                    {
                        list.Add(caravanPartyComponent2.MobileParty);
                    }

                    foreach (var warPartyComponent in clan.WarPartyComponents)
                    {
                        list.Add(warPartyComponent.MobileParty);
                    }

                    var model = new DefaultClanFinanceModel();
                    var addExpense = model.GetType()
                        .GetMethod("AddPartyExpense", BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (var mobileParty in list)
                    {
                        if (mobileParty.LeaderHero != null && mobileParty.LeaderHero != clan.Leader)
                        {
                            object[] array = {mobileParty, clan, new ExplainedNumber(), applyWithdrawals};
                            addExpense.Invoke(model, array);
                            if (BannerKingsConfig.Instance.TitleManager.GetHighestTitle(mobileParty.LeaderHero) == null)
                            {
                                goldChange.Add(((ExplainedNumber) array[2]).ResultNumber,
                                    new TextObject("{=tqCSk7ya}Party wages {A0}"), mobileParty.Name);
                            }
                            else
                            {
                                var calculateWage = model.GetType().GetMethod("CalculatePartyWage",
                                    BindingFlags.Instance | BindingFlags.NonPublic);
                                var wage = (int) calculateWage.Invoke(model,
                                    new object[] {mobileParty, mobileParty.LeaderHero.Gold, applyWithdrawals});
                                if (applyWithdrawals)
                                {
                                    mobileParty.LeaderHero.Gold -= MathF.Min(mobileParty.LeaderHero.Gold, wage);
                                }
                            }
                        }
                    }

                    return false;
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddVillagesIncome", MethodType.Normal)]
            private static bool VillageIncomePrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    int totalGold = 0;
                    var lordships = BannerKingsConfig.Instance.TitleManager
                        .GetAllDeJure(clan)
                        .FindAll(x => x.type == TitleType.Lordship);
                    foreach (var village in clan.Villages)
                    {
                        var title = lordships.FirstOrDefault(x => x.fief.Village == village);
                        if (title == null)
                        {
                            title = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
                        }
                        else
                        {
                            lordships.Remove(title);
                        }

                        var result = CalculateVillageIncome(ref goldChange, village, clan, applyWithdrawals);

                        if (title != null)
                        {
                            var deJure = title.deJure;
                            var knightOwned = title.deJure != clan.Leader && title.deJure.Clan == clan;
                            if (knightOwned)
                            {
                                deJure.Gold += result;
                                continue;
                            }

                            if (deJure.Clan.Kingdom == clan.Kingdom)
                            {
                                continue;
                            }
                        }

                        totalGold += result;
                    }

                    foreach (var lordship in lordships)
                    {
                        var village = lordship.fief.Village;
                        var ownerClan = village.Settlement.OwnerClan;
                        if (ownerClan.Kingdom == clan.Kingdom)
                        {
                            var result = CalculateVillageIncome(ref goldChange, village, clan, applyWithdrawals);
                            var leaderOwned = lordship.deJure == clan.Leader;
                            if (!leaderOwned)
                            {
                                var deJure = lordship.deJure;
                                deJure.Gold += result;
                            }
                            else
                            {
                                totalGold += result;
                            }
                        }
                    }

                    goldChange.Add(totalGold, new TextObject("{=GikQuojv}Village Demesnes"));
                    return false;
                }

                return true;
            }

            private static int CalculateVillageIncome(ref ExplainedNumber goldChange, Village village, Clan clan,
                bool applyWithdrawals)
            {
                var total = (int)BannerKingsConfig.Instance.TaxModel.CalculateVillageTaxFromIncome(village).ResultNumber;
                if (applyWithdrawals)
                {
                    village.TradeTaxAccumulated -= MathF.Min(village.TradeTaxAccumulated, total);
                }

                return total;
            }
        }
    }
}