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
            if (hero == Hero.MainHero && target.StringId == "town_A1")
            {
                this.CreateMaster(target);
            }
        }

        private void CreateMaster(Settlement target)
        {
            CharacterObject character = CharacterObject.All.FirstOrDefault(x => x.StringId == "bannerkings_preacher_aserai_1");
            Hero hero = HeroCreator.CreateSpecialHero(character, target);
            AgentData agent = new AgentData(new SimpleAgentOrigin(hero.CharacterObject, 0, null, default(UniqueTroopDescriptor)));
            LocationCharacter locCharacter = new LocationCharacter(agent, 
                new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddFixedCharacterBehaviors), 
                "sp_throne", true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);

            target.LocationComplex.GetLocationWithId("lordshall")
                .AddLocationCharacters(delegate { return locCharacter; }, target.Culture,
                LocationCharacter.CharacterRelations.Neutral, 1);
        }
    }
}
