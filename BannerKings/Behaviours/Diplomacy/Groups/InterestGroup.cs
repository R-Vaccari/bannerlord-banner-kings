using BannerKings.Managers.Titles.Laws;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class InterestGroup : BannerKingsObject
    {
        public InterestGroup(string stringId) : base(stringId)
        {
        }

        public TraitObject MainTrait { get; private set; }
        public Hero Leader { get; private set; }
        public List<Hero> Members { get; private set; }
        public bool DemandsCouncil { get; private set; }
        public bool AllowsCommoners { get; private set; }
        public bool AllowsNobles { get; private set; }
        public List<PolicyObject> SupportedPolicies { get; private set; }
        public List<PolicyObject> ShunnedPolicies { get; private set; }
        public List<DemesneLaw> SupportedLaws { get; private set; }
        public List<DemesneLaw> ShunnedLaws { get; private set; }
    }
}
