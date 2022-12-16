using BannerKings.Managers.Titles.Laws;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class InterestGroup : BannerKingsObject
    {
        public InterestGroup(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, TraitObject mainTrait,
            bool demandsCouncil, bool allowsCommoners, bool allowsNobles, List<PolicyObject> supportedPolicy,
            List<PolicyObject> shunnedPolicies, List<DemesneLaw> supportedLaws, List<DemesneLaw> shunnedLaws)
        {
            Initialize(name, description);
            MainTrait = mainTrait;
            DemandsCouncil = demandsCouncil;
            AllowsCommoners = allowsCommoners;
            AllowsNobles = allowsNobles;
            SupportedPolicies = supportedPolicy;
            ShunnedPolicies = shunnedPolicies;
            SupportedLaws = supportedLaws;
            ShunnedLaws = shunnedLaws;
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
