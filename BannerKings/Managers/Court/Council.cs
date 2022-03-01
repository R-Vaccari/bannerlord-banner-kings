using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Managers.Court
{
    public class Council
    {
        private Hero lord;
        private List<CouncilMember> members;

        public Council(Hero lord, Hero marshall = null, Hero chancellor = null, Hero steward = null, Hero spymaster = null)
        {
            this.lord = lord;
            this.members = new List<CouncilMember>();
            this.members.Add(new CouncilMember(marshall, CouncilPosition.Marshall));
            this.members.Add(new CouncilMember(chancellor, CouncilPosition.Chancellor));
            this.members.Add(new CouncilMember(steward, CouncilPosition.Steward));
            this.members.Add(new CouncilMember(spymaster, CouncilPosition.Spymaster));
        }

        public List<Hero> GetMembers()
        {
            List<Hero> heroes = new List<Hero>();
            foreach (CouncilMember councilMember in this.members)
                if (councilMember.Member != null) heroes.Add(councilMember.Member);

            return heroes;
        }

        public bool IsMember(Hero hero)
        {
            bool member = false;
            foreach (CouncilMember councilMember in this.members)
                if (councilMember.Member == hero)
                {
                    member = true;
                    break;
                }
                    
            return member;  
        }

        public float GetCompetence(Hero hero, CouncilPosition position)
        {
            float competence = 0f;
            bool found = false;
            foreach (CouncilMember member in this.members)
                if (member.Member == hero)
                {
                    competence = member.Competence;
                    found = true;
                    break;
                }
                    

            if (!found)
                competence = new CouncilMember(hero, position).Competence;
            

            return competence;
        }

        public Hero Marshall
        {
            get => this.members.First(x => x.Position == CouncilPosition.Marshall).Member;
            set =>  this.members.First(x => x.Position == CouncilPosition.Marshall).Member = value;
        }
        public Hero Chancellor
        {
            get => this.members.First(x => x.Position == CouncilPosition.Chancellor).Member;
            set => this.members.First(x => x.Position == CouncilPosition.Chancellor).Member = value;
        }
        public Hero Steward
        {
            get => this.members.First(x => x.Position == CouncilPosition.Steward).Member;
            set => this.members.First(x => x.Position == CouncilPosition.Steward).Member = value;
        }
        public Hero Spymaster
        {
            get => this.members.First(x => x.Position == CouncilPosition.Spymaster).Member;
            set => this.members.First(x => x.Position == CouncilPosition.Spymaster).Member = value;
        }

        public float AdministrativeCosts
        {
            get
            {
                float costs = 0f;
                foreach (CouncilMember councilMember in members)
                    if (councilMember.Member != null)
                        costs += 0.03f;
                return costs;
            }
        }
    }

    public class CouncilMember
    {
        private Hero member;
        private CouncilPosition position;

        public CouncilMember(Hero member, CouncilPosition position)
        {
            this.member = member;
            this.position = position;
        }

        public Hero Member
        {
            get => this.member;
            set => this.member = value;
        }
        public CouncilPosition Position => this.position;
        public float Competence
        {
            get
            {
                if (this.member != null)
                {
                    int targetCap = 300;
                    float primarySkill = 0f;
                    float secondarySkill = 0f;

                    targetCap += 15 * (member.GetAttributeValue(DefaultCharacterAttributes.Intelligence) - 5);
                    if (this.position == CouncilPosition.Marshall)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Leadership);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Tactics);
                    }
                    else if (this.position == CouncilPosition.Chancellor)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Charm);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Charm);
                    }
                    else if (this.position == CouncilPosition.Steward)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Steward);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Trade);
                    }
                    else if (this.position == CouncilPosition.Spymaster)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Roguery);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Scouting);
                    }

                    return MBMath.ClampFloat((primarySkill + (secondarySkill / 2)) / targetCap, 0f, 1f);
                } return 0f; 
            }
        }
    }

    public enum CouncilPosition
    {
        Marshall,
        Chancellor,
        Steward,
        Spymaster
    }

}
