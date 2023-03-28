using BannerKings.Components;
using BannerKings.Settings;
using HarmonyLib;
using Helpers;
using SandBox.CampaignBehaviors;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Behaviours
{
    public class BKBanditBehavior : BannerKingsBehavior
    {
        private Dictionary<Hero, MobileParty> bandits = new Dictionary<Hero, MobileParty>();
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnClanTick);
            CampaignEvents.DailyTickHeroEvent.AddNonSerializedListener(this, OnDailyTickHero);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener(this, OnPartyHourlyTick);
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, OnPartyDailyTick);
            CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompleted);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (bandits == null)
            {
                bandits = new Dictionary<Hero, MobileParty>();
            }
        }

        private void OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent raidEvent)
        {

        }

        private void OnDailyTickHero(Hero hero)
        {
            if (bandits.ContainsKey(hero) && hero.IsPrisoner && MobileParty.MainParty.Party != hero.PartyBelongedToAsPrisoner &&
                hero.PartyBelongedToAsPrisoner.LeaderHero != null)
            {
                KillCharacterAction.ApplyByExecution(hero, hero.PartyBelongedToAsPrisoner.LeaderHero);
            }
        }

        private void OnClanTick(Clan clan)
        {
            if (!clan.IsBanditFaction)
            {
                return;
            }

            RunWeekly(() =>
            {
                if (!clan.WarPartyComponents.Any(x => x.Leader != null) && MBRandom.RandomFloat < 0.05f)
                {
                    CreateBanditHero(clan);
                }
            },
            GetType().Name,
            false);
        }

        private void OnPartyHourlyTick(MobileParty party)
        {
            TickBandits(party);
            TickBanditHeroes(party);
        }

        private void OnPartyDailyTick(MobileParty party)
        {
            if (!party.IsBandit || party.PartyComponent is not BanditHeroComponent)
            {
                return;
            }

            BanditHeroComponent component = (BanditHeroComponent)party.PartyComponent;
            component.Tick();
        }

        public void UpgradeParty(MobileParty party)
        {
            string id = GetPartyTemplateId(party.ActualClan);
            PartyTemplateObject partyTemplate = Campaign.Current.ObjectManager.GetObjectTypeList<PartyTemplateObject>()
                .FirstOrDefault(x => x.StringId == id);

            int stacks = partyTemplate.Stacks.Count - 1;
            var template = partyTemplate.Stacks[MBRandom.RandomInt(0, stacks)];
            party.MemberRoster.AddToCounts(template.Character, MBRandom.RandomInt(2, 6));
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (!settlement.IsHideout)
            {
                return;
            }
        }

        private void TickBandits(MobileParty party)
        {
            if (!party.IsBandit || party.PartyComponent is BanditHeroComponent)
            {
                return;
            }

            foreach (var heroParty in bandits.Values)
            {
                if (Campaign.Current.Models.MapDistanceModel.GetDistance(party, heroParty) <= 10f)
                {
                    SetFollow(heroParty, party);
                }
            }
        }

        private void TickBanditHeroes(MobileParty party)
        {
            if (party.PartyComponent is not BanditHeroComponent)
            {
                return;
            }

            float partyLimit = Campaign.Current.Models.PartySizeLimitModel.GetPartyMemberSizeLimit(party.Party).ResultNumber;
            BanditHeroComponent component = (BanditHeroComponent)party.PartyComponent;
            if (party.MemberRoster.TotalManCount < partyLimit * 0.2f)
            {
                party.Ai.SetMoveGoToSettlement(component.Hideout.Settlement);
            }
        }

        public void SetFollow(MobileParty heroParty, MobileParty follower)
        {
            follower.Ai.DisableForHours(2);
            follower.Ai.SetMoveEscortParty(heroParty);
            follower.Ai.RecalculateShortTermAi();
        }

        public void CreateBanditHero(Clan clan)
        {
            Hideout hideout = Hideout.All.FirstOrDefault(x => x.Settlement.Culture == clan.Culture);
            Settlement settlement = null;
            if (hideout != null)
            {
                settlement = hideout.Settlement;
            }

            if (settlement == null)
            {
                hideout = Hideout.All.GetRandomElement();
                settlement = hideout.Settlement;
            }

            Settlement closest = SettlementHelper.FindNearestTown(x => x.IsTown, settlement);

            var templates = CharacterObject.All.ToList().FindAll(x =>
              x.StringId.Contains("bannerkings_bandithero") && x.Culture == closest.Culture);
            CharacterObject template = templates.GetRandomElement();
            if (template == null)
            {
                return;
            }

            var source = from e in MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>() where e.EquipmentCulture == clan.Culture select e;
            if (source == null)
            {
                return;
            }

            var roster = source.GetRandomElementInefficiently();
            if (roster == null)
            {
                return;
            }

            var partyTemplates = Campaign.Current.ObjectManager.GetObjectTypeList<PartyTemplateObject>();
            string id = GetPartyTemplateId(clan);
            PartyTemplateObject partyTemplate = partyTemplates.FirstOrDefault(x => x.StringId == id);
            if (partyTemplate == null)
            {
                return;
            }

            var hero = HeroCreator.CreateSpecialHero(template, 
                settlement, 
                clan, 
                null, 
                Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, roster.AllEquipments.GetRandomElement());
            var mainParty = hero.PartyBelongedTo == MobileParty.MainParty;

            MobileParty mobileParty = BanditHeroComponent.CreateParty(hideout, hero, partyTemplate);
            AddHeroToPartyAction.Apply(hero, mobileParty, false);
            mobileParty.ChangePartyLeader(hero);

            UpgradeParty(mobileParty);
            UpgradeParty(mobileParty);
            UpgradeParty(mobileParty);
            UpgradeParty(mobileParty);

            bandits.Add(hero, mobileParty);
            InfestHieout(hideout, clan);
            EnterSettlementAction.ApplyForParty(mobileParty, hideout.Settlement);

            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}A renowned criminal, {HERO}, has arisen among the {CLAN}! They were sighted in the vicinity of {TOWN}...")
                .SetTextVariable("HERO", hero.Name)
                .SetTextVariable("CLAN", clan.Name)
                .SetTextVariable("TOWN", closest.Name)
                .ToString()));
        }

        private string GetPartyTemplateId(Clan clan)
        {
            string id = "bandits_hero_{0}{1}";
            if (BannerKingsSettings.Instance.DRMBandits)
            {
                id = string.Format(id, clan.StringId, "_drm");
            }
            else
            {
                id = string.Format(id, clan.StringId, "");
            }
            return id;
        }

        private void InfestHieout(Hideout hideout, Clan clan)
        {
            int num = 0;
            while ((float)num < Campaign.Current.Models.BanditDensityModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt * 6)
            {
                Campaign.Current.GetCampaignBehavior<BanditsCampaignBehavior>()
                    .AddBanditToHideout(hideout, clan.DefaultPartyTemplate, false);
                num++;
            }
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(LordConversationsCampaignBehavior))]
        internal class BanditDialoguePatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("conversation_player_wants_to_make_peace_on_condition")]
            private static bool MakePeacePrefix(ref bool __result)
            {
                __result = !Hero.OneToOneConversationHero.MapFaction.IsBanditFaction &&
                    FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.OneToOneConversationHero.MapFaction);

                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterRelationCampaignBehavior))]
        internal class CharacterRelationCampaignBehaviorPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnRaidCompleted")]
            private static bool OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent raidEvent)
            {
                MapEvent mapEvent = raidEvent.MapEvent;
                PartyBase leaderParty = mapEvent.AttackerSide.LeaderParty;
                if (leaderParty != null && leaderParty.LeaderHero != null && leaderParty.MobileParty.PartyComponent is BanditHeroComponent)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
