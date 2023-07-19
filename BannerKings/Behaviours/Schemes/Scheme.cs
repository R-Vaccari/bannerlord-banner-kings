using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Schemes
{
    public class Scheme : BannerKingsObject
    {
        private Func<CouncilData, bool> isAdequate;
        private Func<CouncilData, List<Hero>> getTargetList;
        private Func<CouncilData, Hero, bool> canContinue;

        public Scheme(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, Secret secret,
            SkillObject skill, SchemeType type,
            Func<CouncilData, bool> isAdequate, 
            Func<CouncilData, List<Hero>> getTargetList,
            Func<CouncilData, Hero, bool> canContinue)
        {
            Initialize(name, description);
            this.isAdequate = isAdequate;
            this.getTargetList = getTargetList;
            this.canContinue = canContinue;
        }

        public void PostInitialize()
        {

        }

        public Hero Agent { get; private set; }
        public Hero Target { get; private set; }
        public List<Hero> Conspirators { get; private set; }
        public float Progress { get; private set; }

        public SkillObject Skill { get; private set; }
        public Secret Secret { get; private set; }
        public bool IsSecret => Secret != null;

        public enum SchemeType
        {
            Diplomatic,
            Criminal,
            Title
        }
    }
}
