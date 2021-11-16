using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace Populations.Components
{
    class PopulationPartyComponent : PartyComponent
    {
        private Settlement _origin;
        private Settlement _target;
        private string _nameTemplate;
        public PopulationPartyComponent(Settlement origin, Settlement target, string nameTemplate) : base()
        {
            _origin = origin;
            _target = target;
            _nameTemplate = nameTemplate;
        }

        public static MobileParty CreateParty(string id, Settlement origin, Settlement target, string nameTemplate)
        {
            return MobileParty.CreateParty(id + origin.Name.ToString() + target.Name.ToString(), new PopulationPartyComponent(origin, target, nameTemplate), delegate (MobileParty mobileParty)
            {
                mobileParty.SetPartyUsedByQuest(false);
            });
        }
        public override Hero PartyOwner => HomeSettlement.OwnerClan.Leader;

        public override TextObject Name
        {
            get
            {
                return new TextObject(String.Format(_nameTemplate, HomeSettlement.Name.ToString()));
            }
        }

        public override Settlement HomeSettlement
        {
            get => _origin;
        }

        public Settlement TargetSettlement
        {
            get => _target;
        }
    }
}
