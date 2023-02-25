using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Spiritual : CouncilPosition
    {
        public Spiritual() : base("Spiritual")
        {
        }

        public override SkillObject PrimarySkill => BKSkills.Instance.Theology;
        public override SkillObject SecondarySkill => BKSkills.Instance.Scholarship;

        public override IEnumerable<CouncilPrivileges> Privileges
        {
            get
            {
                yield return CouncilPrivileges.CLERGYMEN_EXCLUSIVE;
            }
        }

        public override IEnumerable<CouncilTask> Tasks
        {
            get
            {
                yield return new OverseeReligiousMattersTask();
            }
        }

        public override CouncilPosition GetCopy(Clan clan)
        {
            Spiritual pos = new Spiritual();
            pos.Initialize(null, clan);
            return pos;
        }

        public override bool IsAdequate(CouncilData data)
        {
            var clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(data.Clan.Leader);
            return clanReligion != null;
        }

        protected override bool IsValidCandidateInternal(Hero candidate)
        {
            bool matchingFaith = false;
            var clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Clan.Leader);
            if (clanReligion != null)
            {
                matchingFaith = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(candidate).Equals(clanReligion);
            }

            return BannerKingsConfig.Instance.ReligionsManager.IsPreacher(candidate) && matchingFaith;
        }

        public class OverseeReligiousMattersTask : CouncilTask
        {
            public override TextObject Name => new TextObject("{=!}Organize Militia");
            public override TextObject Description => new TextObject("{=!}");
            public override TextObject Effects => new TextObject("{=!}");

            public override float StandartBuildUp => 0f;
        }
    }
}
