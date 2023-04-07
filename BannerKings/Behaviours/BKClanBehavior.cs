using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BannerKings.Extensions;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Settings;
using BannerKings.UI.Court;
using BannerKings.Utils;
using BannerKings.Utils.Extensions;
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
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.CampaignSystem.SkillEffect;
using static TaleWorlds.Core.ItemObject;

namespace BannerKings.Behaviours
{
    public class BKClanBehavior : BannerKingsBehavior
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
            starter.AddPlayerLine("conversation_prisoner_chat_player",
                "prisoner_recruit_start_player",
                "companion_freed_after_battle",
                "{=mgmaiTbH}You are free to go.",
                null,
                null,
                100);

            starter.AddDialogLine("companion_freed_after_battle",
                "companion_freed_after_battle", 
                "close_window",
                "{=jm464D6p}Thank you, {?PLAYER.GENDER}madam{?}sir{\\?}. I will tell the {CLAN} of your deed.", 
                null, 
                () =>
                {
                    int playerGainedRelationAmount = 5;
                    ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, playerGainedRelationAmount, true, true);
                    if (Hero.OneToOneConversationHero.IsPrisoner)
                    {
                        EndCaptivityAction.ApplyByReleasedAfterBattle(Hero.OneToOneConversationHero);
                    }
                },
                110, null);

            starter.AddDialogLine("default_conversation_for_wrongly_created_heroes", "start", "close_window", 
                "{=fAXkhM0C}I am under your mercy.", 
                () => Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero.CompanionOf != null,
                () =>
                {
                    TakePrisonerAction.Apply(Campaign.Current.MainParty.Party, CharacterObject.OneToOneConversationCharacter.HeroObject);
                }, 
                0, null);

            starter.AddDialogLine("companion_captured",
              "companion_captured",
              "close_window",
              "{=odndVRfW}As you say.",
              null,
              () =>
              {
                  TakePrisonerAction.Apply(Campaign.Current.MainParty.Party, CharacterObject.OneToOneConversationCharacter.HeroObject);
              });

            starter.AddPlayerLine("default_conversation_for_wrongly_created_heroes",
              "start",
              "companion_captured",
              "{=5S2TAT0h}You'll be coming with me now.",
              () => IsCompanionOfAnotherClan() && Campaign.Current.CurrentConversationContext == ConversationContext.CapturedLord,
              null);

            starter.AddPlayerLine("meet_wanderer_different_clan", 
                "wanderer_meet_player_response", 
                "wanderer_different_clan_response",
                "{=ZCdo5ZKf}My name is {PLAYER.NAME}, {?PLAYER.GENDER}madam{?}sir{\\?}. Tell me about yourself.",
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
                clan.Leader.AddSkillXp(BKSkills.Instance.Lordship, councillours * 10f);
            }

            if (!clan.IsUnderMercenaryService && clan.Kingdom != null)
            {
                var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
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

                if (clan.Kingdom.RulingClan == clan && (council.Peerage == null || !council.Peerage.CanHaveFief))
                {
                    council.SetPeerage(new Peerage(new TextObject("{=9OhMK2Wk}Full Peerage"), true,
                                true, true, true, true, false));

                    if (clan == Clan.PlayerClan)
                    {
                        var peerage = council.Peerage;
                        InformationManager.ShowInquiry(new InquiryData(
                            peerage.Name.ToString(),
                            new TextObject("{=tEWSy6Na}As part of beng a ruling clan, the {CLAN} is now considered to have {PEERAGE}. {TEXT}")
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

            if (clan == Clan.PlayerClan || clan.IsUnderMercenaryService || clan.IsMinorFaction || clan.IsBanditFaction)
            {
                return;
            }

            EvaluateRecruitKnight(clan);
            EvaluateRecruitCompanion(clan);
            SetCompanionParty(clan);
            RunCouncilTasks(clan);
        }

        private void RunCouncilTasks(Clan clan)
        {
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            HandleSpiritual(clan, council);

            if (BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.FamilyCare,
                out float healCompetence))
            {
                foreach (var member in clan.Heroes)
                {
                    member.AddSkillXp(DefaultSkills.Medicine, 5 * healCompetence);
                }
            }

            if (MBRandom.RandomFloat < 0.02f &&
            BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.PromoteCulture,
            out float cultureCompetence) &&
            MBRandom.RandomFloat < cultureCompetence)
            {
                Hero notable = null;
                foreach (var settlement in clan.Settlements)
                {
                    notable = settlement.Notables.GetRandomElementWithPredicate(x => x.Culture != clan.Culture);
                }

                if (notable != null)
                {
                    Campaign.Current.GetCampaignBehavior<BKNotableBehavior>()
                    .ApplyNotableCultureConversion(notable, council.Owner);

                    if (clan == Clan.PlayerClan)
                    {
                        CouncilMember steward = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Steward);
                        MBInformationManager.AddQuickInformation(
                            new TextObject("{=pwcJeEaS}{?PLAYER.GENDER}My lady{?}My lord{\\?}, {HERO} has converted to your culture!")
                            .SetTextVariable("HERO", notable.Name),
                            0,
                            steward.Member.CharacterObject,
                            Utils.Helpers.GetKingdomDecisionSound());
                    }
                }
            }

            if (MBRandom.RandomFloat < 0.03f &&
            BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.OverseeDignataries,
            out float dignatariesCompetence) &&
            MBRandom.RandomFloat < dignatariesCompetence)
            {
                Hero notable = null;
                foreach (var settlement in clan.Settlements)
                {
                    notable = settlement.Notables.GetRandomElement();
                }

                if (notable != null)
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, notable, (int)(8 * dignatariesCompetence), false);
                }

                if (clan == Clan.PlayerClan)
                {
                    CouncilMember chancellor = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Chancellor);
                    MBInformationManager.AddQuickInformation(
                        new TextObject("{=ZwwfKpGu}{?PLAYER.GENDER}My lady{?}My lord{\\?}, {NOTABLE} is now more favorable to us.")
                        .SetTextVariable("NOTABLE", notable.Name),
                        0,
                        chancellor.Member.CharacterObject,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }

            if (MBRandom.RandomFloat < 0.02f &&
            BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.RepressCriminality,
            out float criminalityCompetence) &&
            MBRandom.RandomFloat < criminalityCompetence)
            {
                Hideout hideout = null;
                Town town = null;
                foreach (var fief in clan.Fiefs)
                {
                    if (hideout != null)
                    {
                        break;
                    }

                    foreach (var h in Hideout.All)
                    {
                        if (hideout.IsInfested && fief.Settlement.Position2D.DistanceSquared(hideout.Settlement.Position2D) < 40f * 40f)
                        {
                            hideout = h;
                            town = fief;
                            break;
                        }
                    }
                }

                if (hideout != null)
                {
                    foreach (var party in hideout.Owner.Settlement.Parties)
                    {
                        DestroyPartyAction.Apply(null, party);
                    }

                    foreach (var notable in town.Settlement.Notables)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(clan.Leader, notable, 3, false);
                    }

                    if (clan == Clan.PlayerClan)
                    {
                        CouncilMember spymaster = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Spymaster);
                        MBInformationManager.AddQuickInformation(new TextObject("{=6vgsdLQp}{?PLAYER.GENDER}My lady{?}My lord{\\?}, the hideout near {FIEF} was exterminated.")
                            .SetTextVariable("FIEF", town.Name),
                            0,
                            spymaster.Member.CharacterObject,
                            Utils.Helpers.GetKingdomDecisionSound());
                    }
                }
            }

            if (MBRandom.RandomFloat < 0.02f && clan.Kingdom != null &&
               BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.ArbitrateRelations,
               out float relationsCompetence) &&
               MBRandom.RandomFloat < relationsCompetence)
            {
                Hero clanLeader = null;
                List<Hero> leaders = new List<Hero>();
                foreach (var c in clan.Kingdom.Clans)
                {
                    if (c != clan && !c.IsUnderMercenaryService)
                    {
                        leaders.Add(c.Leader);
                    }
                }

                clanLeader = leaders.GetRandomElement();
                if (clanLeader != null)
                {
                    ChangeRelationAction.ApplyPlayerRelation(clanLeader, 
                        (int)(10 * relationsCompetence), 
                        false,
                        false);

                    if (clan == Clan.PlayerClan)
                    {
                        CouncilMember chancellor = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Chancellor);
                        MBInformationManager.AddQuickInformation(
                            new TextObject("{=r8s1f28d}{?PLAYER.GENDER}My lady{?}My lord{\\?}, {HERO} is now more favorable to us.")
                            .SetTextVariable("HERO", clanLeader.Name),
                            0,
                            chancellor.Member.CharacterObject,
                            Utils.Helpers.GetKingdomDecisionSound());
                    }
                }
            }

            if (BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.EducateFamilyAntiquarian,
               out float antiquarianCompetence))
            {
                foreach (var member in clan.Lords)
                {
                    member.AddSkillXp(BKSkills.Instance.Scholarship, 10 * antiquarianCompetence);
                }
            }

            float smithCompetence;
            if (MBRandom.RandomFloat < 0.02f &&
               (BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.SmithArmors,
               out smithCompetence) ||
               BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.SmithBardings,
               out smithCompetence) ||
               BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.SmithWeapons,
               out smithCompetence)) &&
               MBRandom.RandomFloat < smithCompetence)
            {
                CouncilMember smith = council.GetCouncilPosition(DefaultCouncilPositions.Instance.CourtSmith);
                List<ItemTypeEnum> types = new List<ItemTypeEnum>();
                if (smith.CurrentTask.StringId == DefaultCouncilTasks.Instance.SmithArmors.StringId)
                {
                    types.Add(ItemTypeEnum.ChestArmor);
                    types.Add(ItemTypeEnum.HeadArmor);
                    types.Add(ItemTypeEnum.HandArmor);
                    types.Add(ItemTypeEnum.LegArmor);
                    types.Add(ItemTypeEnum.Cape);
                }

                if (smith.CurrentTask.StringId == DefaultCouncilTasks.Instance.SmithWeapons.StringId)
                {
                    types.Add(ItemTypeEnum.Polearm);
                    types.Add(ItemTypeEnum.OneHandedWeapon);
                    types.Add(ItemTypeEnum.TwoHandedWeapon);
                }

                if (smith.CurrentTask.StringId == DefaultCouncilTasks.Instance.SmithBardings.StringId)
                {
                    types.Add(ItemTypeEnum.HorseHarness);
                }

                int smithingSkill = smith.Member.GetSkillValue(DefaultSkills.Crafting);
                ItemObject item = null;
                List<ItemObject> items = new List<ItemObject>();
                foreach (var i in Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
                {
                    if (types.Contains(i.ItemType) && !i.IsUniqueItem && i.Tierf > 3f && 
                        BannerKingsConfig.Instance.SmithingModel.GetItemDifficulty(i) <= smithingSkill)
                    {
                        items.Add(i);
                    } 
                }

                item = items.GetRandomElement();
                if (item != null)
                {
                  

                    if (clan == Clan.PlayerClan)
                    {
                        MBInformationManager.AddQuickInformation(
                            new TextObject("{=4ztP2tnz}{?PLAYER.GENDER}My lady{?}My lord{\\?}, I forged you the {ITEM}.")
                            .SetTextVariable("ITEM", item.Name),
                            0,
                            smith.Member.CharacterObject,
                            Utils.Helpers.GetKingdomDecisionSound());
                    }
                }
            }
        }

        private void HandleSpiritual(Clan clan, CouncilData council)
        {
            ExceptionUtils.TryCatch(() =>
            {
                if (MBRandom.RandomFloat < 0.02f &&
                BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.PromoteFaith,
                out float faithCompetence) &&
                MBRandom.RandomFloat < faithCompetence)
                {
                    Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                    Hero notable = null;
                    foreach (var settlement in clan.Settlements)
                    {
                        notable = settlement.Notables.GetRandomElementWithPredicate(x =>
                        {
                            var notableRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(x);
                            return notableRel == null || !notableRel.Equals(rel);
                        });
                    }

                    if (notable != null)
                    {
                        Campaign.Current.GetCampaignBehavior<BKNotableBehavior>()
                        .ApplyNotableFaithConversion(notable, council.Owner, true);

                        if (clan == Clan.PlayerClan)
                        {
                            CouncilMember spiritual = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Spiritual);
                            MBInformationManager.AddQuickInformation(
                                new TextObject("{=MEby26tQ}{?PLAYER.GENDER}My lady{?}My lord{\\?}, {HERO} has converted to your faith!")
                                .SetTextVariable("HERO", notable.Name),
                                0,
                                spiritual.Member.CharacterObject,
                                Utils.Helpers.GetKingdomDecisionSound());
                        }
                    }
                }

                if (MBRandom.RandomFloat < 0.02f &&
                BannerKingsConfig.Instance.CourtManager.HasCurrentTask(council, DefaultCouncilTasks.Instance.CultivatePiety,
                out float pietyCompetence) &&
                MBRandom.RandomFloat < pietyCompetence)
                {
                    Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                    Hero hero = clan.Heroes.GetRandomElementWithPredicate(x =>
                    {
                        var heroRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(x);
                        return heroRel == null || !heroRel.Equals(rel);
                    });

                    if (hero != null)
                    {
                        Campaign.Current.GetCampaignBehavior<BKNotableBehavior>()
                        .ApplyNotableFaithConversion(hero, council.Owner, true);

                        if (clan == Clan.PlayerClan)
                        {
                            CouncilMember spiritual = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Spiritual);
                            MBInformationManager.AddQuickInformation(
                                new TextObject("{=MEby26tQ}{?PLAYER.GENDER}My lady{?}My lord{\\?}, {HERO} has converted to your faith!")
                                .SetTextVariable("HERO", hero.Name),
                                0,
                                spiritual.Member.CharacterObject,
                                Utils.Helpers.GetKingdomDecisionSound());
                        }
                    }
                }
            }, this.GetType().Name,
            false);
        }

        private void SetCompanionParty(Clan clan)
        {
            RunWeekly(() =>
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
            },
            GetType().Name,
            false);
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
            if (!BannerKingsSettings.Instance.AICompanions)
            {
                return;
            }

            RunWeekly(() =>
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
                        DefaultTraits.ScoutSkills
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
            },
            GetType().Name,
            false);
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
            if (!BannerKingsSettings.Instance.AIKnights)
            {
                return;
            }

            RunWeekly(() =>
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
                var genderLaw = title.Contract.GenderLaw;
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
            },
            GetType().Name,
            false);
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
                                 (war ? 0.45f : 0.15f);
                    if (war)
                    {
                        income += clan.Gold * 0.005f;
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
                                if (title is {Fief: { }})
                                {
                                    knights++;
                                    var limit = 0f;
                                    if (title.Fief.IsVillage)
                                    {
                                        limit = BannerKingsConfig.Instance.TaxModel.CalculateVillageTaxFromIncome(title.Fief.Village).ResultNumber;
                                    }
                                    else if (title.Fief.Town != null)
                                    {
                                        limit = Campaign.Current.Models.SettlementTaxModel
                                            .CalculateTownTax(title.Fief.Town).ResultNumber;
                                    }

                                    partyComponent.MobileParty.SetWagePaymentLimit((int)(50f + limit));
                                }
                            }
                        }

                        foreach (var partyComponent in clan.WarPartyComponents)
                        {
                            var share = income / clan.WarPartyComponents.Count - knights;
                            partyComponent.MobileParty.SetWagePaymentLimit((int) (300f + share));
                        }

                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ClanVariablesCampaignBehavior))]
        internal class AutoRecruitmentPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateClanSettlementAutoRecruitment", MethodType.Normal)]
            private static bool Prefix1(Clan clan)
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
                    return title is {Contract: { }} &&
                           title.Contract.Rights.Contains(FeudalRights.Assistance_Rights);
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
            [HarmonyPatch("AddExpensesFromPartiesAndGarrisons", MethodType.Normal)]
            private static bool PartyExpensesPrefix(Clan clan, ref ExplainedNumber goldChange,
                bool applyWithdrawals, bool includeDetails)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions, null);
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

                    foreach (Town town in clan.Fiefs)
                    {
                        if (town.GarrisonParty != null && town.GarrisonParty.IsActive)
                        {
                            list.Add(town.GarrisonParty);
                        }
                    }

                    var model = new DefaultClanFinanceModel();
                    var getWage = model.GetType()
                        .GetMethod("CalculatePartyWage", BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (var party in list)
                    {
                        int budget = clan.Gold + (int)goldChange.ResultNumber + (int)goldChange.ResultNumber;
                        object[] array = {party, budget, applyWithdrawals};
                        int expense = (int)getWage.Invoke(model, array);

                        if (applyWithdrawals)
                        {
                            if (party.IsLordParty)
                            {
                                if (party.LeaderHero != null)
                                {
                                    party.LeaderHero.Gold -= expense;
                                }
                                else
                                {
                                    party.ActualClan.Leader.Gold -= expense;
                                }
                            }
                            else
                            {
                                party.PartyTradeGold -= expense;
                            }
                        }

                        if (party.LeaderHero != null && party.LeaderHero != clan.Leader)
                        {
                            if (BannerKingsConfig.Instance.TitleManager.GetHighestTitle(party.LeaderHero) != null)
                            {
                                continue;
                            }
                        }

                        if (applyWithdrawals)
                        {
                            if (party.LeaderHero != null && party.LeaderHero.IsClanLeader())
                            {
                                continue;
                            }

                            bool needsExtra = false;
                            if (party.IsLordParty && party.LeaderHero != null)
                            {
                                needsExtra = party.LeaderHero.Gold < 5000;
                            }
                            else
                            {
                                needsExtra = party.PartyTradeGold< 5000;
                            }

                            if (needsExtra && (expense + 200) < budget)
                            {
                                expense += 200;
                            }

                            int refund = MathF.Min(expense, budget);
                            if (party.IsLordParty)
                            {
                                if (party.LeaderHero != null)
                                {
                                    party.LeaderHero.Gold += refund;
                                }
                                else
                                {
                                    party.ActualClan.Leader.Gold += refund;
                                }
                            }
                            else
                            {
                                party.PartyTradeGold += refund;
                            }
                        }

                        explainedNumber.Add(-expense,new TextObject("{=tqCSk7ya}Party wages {A0}"), party.Name);
                    }

                    if (!includeDetails)
                    {
                        goldChange.Add(explainedNumber.ResultNumber, new TextObject("{=ChUDSiJw}Garrison and Party Expense", null), null);
                        return false;
                    }

                    goldChange.AddFromExplainedNumber(explainedNumber, new TextObject("{=ChUDSiJw}Garrison and Party Expense", null));
                    return false;
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("AddSettlementIncome", MethodType.Normal)]
            private static bool VillageIncomePrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals, bool includeDetails)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    ExplainedNumber explainedNumber = new ExplainedNumber(0f, goldChange.IncludeDescriptions, null);
                    foreach (Town town in clan.Fiefs)
                    {
                        ExplainedNumber explainedNumber2 = new ExplainedNumber((float)((int)((float)town.TradeTaxAccumulated / 5f)), false, null);
                        int num = MathF.Round(explainedNumber2.ResultNumber);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Trade.ContentTrades, town, ref explainedNumber2);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Crossbow.Steady, town, ref explainedNumber2);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Roguery.SaltTheEarth, town, ref explainedNumber2);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.GivingHands, town, ref explainedNumber2);
                        if (applyWithdrawals)
                        {
                            town.TradeTaxAccumulated -= num;
                            if (clan == Clan.PlayerClan)
                            {
                                CampaignEventDispatcher.Instance.OnPlayerEarnedGoldFromAsset(DefaultClanFinanceModel.AssetIncomeType.Taxes, (int)explainedNumber2.ResultNumber);
                            }
                        }
                        int num2 = (int)Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(town, false).ResultNumber;
                        explainedNumber.Add((float)num2, new TextObject("{=TLuaPAIO}{A0} Taxes", null), town.Name);
                        explainedNumber.Add(explainedNumber2.ResultNumber, new TextObject("{=wVMPdc8J}{A0}'s tariff", null), town.Name);
                        if (town.CurrentDefaultBuilding != null && town.Governor != null && town.Governor.GetPerkValue(DefaultPerks.Engineering.ArchitecturalCommisions))
                        {
                            explainedNumber.Add(DefaultPerks.Engineering.ArchitecturalCommisions.SecondaryBonus, new TextObject("{=uixuohBp}Settlement Projects", null), null);
                        }
                    }

                    int villageTotal = 0;
                    foreach (Village village in clan.GetActualVillages())
                    {
                        FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
                        var income = CalculateVillageIncome(village);
                        if (title != null && title.deJure != clan.Leader && applyWithdrawals)
                        {
                            ApplyWithdrawal(village, income, title.deJure);
                        }
                        else
                        {
                            villageTotal += income;
                            if (applyWithdrawals)
                            {
                                ApplyWithdrawal(village, income);
                            }
                        }
                    }

                    goldChange.Add(villageTotal, new TextObject("{=GikQuojv}Village Demesnes"));
                    if (!includeDetails)
                    {
                        goldChange.Add(explainedNumber.ResultNumber, new TextObject("{=AewK9qME}Settlement Income", null), null);
                        return false;
                    }
                    goldChange.AddFromExplainedNumber(explainedNumber, new TextObject("{=AewK9qME}Settlement Income", null));
                    return false;
                }

                return true;
            }

            private static void ApplyWithdrawal(Village village, int income, Hero payTo = null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
                data.VillageData.LastPayment = income;
                village.TradeTaxAccumulated -= MathF.Min(village.TradeTaxAccumulated, income);
                if (payTo != null)
                {
                    payTo.Gold += income;
                }
            }

            private static int CalculateVillageIncome(Village village) => (int)BannerKingsConfig.Instance.TaxModel
                .CalculateVillageTaxFromIncome(village,
                    false,
                    false)
                    .ResultNumber;
        }
    }
}