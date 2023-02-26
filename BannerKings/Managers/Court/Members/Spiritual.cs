using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Court.Members
{
    public class Spiritual : CouncilMember
    {
        public Spiritual() : base("Spiritual")
        {
        }

        public override SkillObject PrimarySkill => BKSkills.Instance.Theology;
        public override SkillObject SecondarySkill => BKSkills.Instance.Scholarship;
        public override TextObject GetCulturalName()
        {
            var id = Culture.StringId;
            if (IsRoyal)
            {
                
                if (id == "battania") return new TextObject("{=!}Ard Draoidh");
                if (id == "sturgia") return new TextObject("{=!}Volkhvs");
                if (id == "aserai") return new TextObject("{=!}Murshid");

                return new TextObject("{=!}High Seneschal");
            }

            if (id == "battania") return new TextObject("{=!}Draoidh");
            if (id == "sturgia") return new TextObject("{=!}Volkhvs");
            if (id == "aserai") return new TextObject("{=!}Murshid");

            return new TextObject("{=!}Seneschal");
        }

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
                yield return DefaultCouncilTasks.Instance.OrganizeMiltia.GetCopy();
            }
        }

        public override CouncilMember GetCopy(Clan clan)
        {
            Spiritual pos = new Spiritual();
            pos.Initialize(clan);
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
    }
}
