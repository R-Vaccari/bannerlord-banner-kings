using BannerKings.Components;
using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnClanTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnClanTick(Clan clan)
        {
            if (!clan.IsBanditFaction)
            {
                return;
            }

            if (!clan.WarPartyComponents.Any(x => x.Leader != null))
            {
                CreateBanditHero(clan);
            }
        }

        private void CreateBanditHero(Clan clan)
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
            
            var templates = CharacterObject.All.ToList().FindAll(x =>
               x.Occupation == Occupation.Bandit && x.StringId.Contains("bannerkings_bandithero") && x.Culture == clan.Culture);
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

            var hero = HeroCreator.CreateSpecialHero(template, 
                settlement, 
                clan, 
                null, 
                Campaign.Current.Models.AgeModel.HeroComesOfAge + 5 + MBRandom.RandomInt(27));
            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, roster.AllEquipments.GetRandomElement());
            var mainParty = hero.PartyBelongedTo == MobileParty.MainParty;

            MobileParty mobileParty = BanditHeroComponent.CreateParty(hideout, hero);
            AddHeroToPartyAction.Apply(hero, mobileParty, false);
            mobileParty.ChangePartyLeader(hero);

            Settlement closest = SettlementHelper.FindNearestTown(x => x.IsTown, settlement);
            InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=!}A renowned criminal, {HERO}, has arisen among the {CLAN}! They were sighted in the vicinity of {TOWN}...")
                .SetTextVariable("HERO", hero.Name)
                .SetTextVariable("CLAN", clan.Name)
                .SetTextVariable("TOWN", closest.Name)
                .ToString()));
        }
    }
}
