using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Behaviours.Schemes
{
    public class Scheme : BannerKingsObject
    {
        public Scheme(string stringId) : base(stringId)
        {
        }

        public void PostInitialize()
        {

        }

        public Hero Agent { get; private set; }
        public Hero Target { get; private set; }
        public List<Hero> Conspirators { get; private set; }
        public float Progress { get; private set; }

        public SkillObject Skill { get; private set; }
        public bool IsSecret { get; private set; }



        public enum SchemeType
        {
            Diplomatic,
            Criminal
        }
    }
}
