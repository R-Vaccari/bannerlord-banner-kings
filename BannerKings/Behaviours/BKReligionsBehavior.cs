using BannerKings.Managers.Institutions.Religions;
using SandBox.Source.Towns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.Towns;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKReligionsBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
            //CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void DailySettlementTick(Settlement settlement)
        {

        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (hero != Hero.MainHero && target.StringId != "town_A1") return;
            
            ReligionData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).ReligionData;
            if (data == null) return;

            this.AddClergymanToKeep(data, target);
        }

        private void AddClergymanToKeep(ReligionData data, Settlement settlement)
        {
            AgentData agent = new AgentData(new SimpleAgentOrigin(data.Clergyman.Hero.CharacterObject, 0, null, default(UniqueTroopDescriptor)));
            LocationCharacter locCharacter = new LocationCharacter(agent, 
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors), 
                null, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);

            settlement.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, settlement.Culture,
                LocationCharacter.CharacterRelations.Neutral, 1);
        }
    }
}
