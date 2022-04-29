using BannerKings.Managers.Titles;
using BannerKings.Populations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Court
{
    public class CouncilData : BannerKingsData
    {
        [SaveableProperty(1)]
        private Clan clan { get; set; }

        [SaveableProperty(2)]
        private List<CouncilMember> members { get; set; }

        public CouncilData(Clan clan, Hero marshall = null, Hero chancellor = null, Hero steward = null, Hero spymaster = null)
        {
            this.clan = clan;
            members = new List<CouncilMember>();
            members.Add(new CouncilMember(marshall, CouncilPosition.Marshall));
            members.Add(new CouncilMember(chancellor, CouncilPosition.Chancellor));
            members.Add(new CouncilMember(steward, CouncilPosition.Steward));
            members.Add(new CouncilMember(spymaster, CouncilPosition.Spymaster));
        }

        internal override void Update(PopulationData data)
        {
            foreach (CouncilMember member in members)
            {
                if (member.Member != null && member.Member.IsDead)
                    member.Member = null;
            }
        }

        public List<Hero> GetCourtMembers()
        {
            List<Hero> heroes = new List<Hero>();

            MBReadOnlyList<Hero> members = Clan.PlayerClan.Heroes;
            if (members != null && members.Count > 0)
                foreach (Hero member in members)
                    if (member != clan.Leader && member.IsAlive && !heroes.Contains(member))
                        heroes.Add(member);

            if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(Hero.MainHero))
            {
                List<FeudalTitle> vassals = BannerKingsConfig.Instance.TitleManager.GetVassals(Hero.MainHero);
                if (vassals != null && vassals.Count > 0)
                    foreach (FeudalTitle vassal in vassals)
                        if (vassal.deJure != clan.Leader && !heroes.Contains(vassal.deJure)) 
                            heroes.Add(vassal.deJure);
            }

            MBReadOnlyList<Town> towns = clan.Fiefs;
            if (towns != null && towns.Count > 0)
            {
                foreach (Town town in towns)
                {
                    MBReadOnlyList<Hero> notables = town.Settlement.Notables;
                    if (notables != null && notables.Count > 0)
                        foreach (Hero notable in notables)
                            if (!heroes.Contains(notable))
                               heroes.Add(notable);
                                
                }
            }

            return heroes;
        }

        public List<Hero> GetMembers()
        {
            List<Hero> heroes = new List<Hero>();
            foreach (CouncilMember councilMember in members)
                if (councilMember.Member != null) heroes.Add(councilMember.Member);

            return heroes;
        }

        public bool IsMember(Hero hero)
        {
            bool member = false;
            foreach (CouncilMember councilMember in members)
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
            foreach (CouncilMember member in members)
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


        public float GetCompetence(CouncilPosition position)
        {
            float competence = 0f;
            foreach (CouncilMember member in members)
                if (member.Position == position)
                {
                    competence = member.Competence;
                    break;
                }
            return competence;
        }

        public Hero Marshall
        {
            get => members.First(x => x.Position == CouncilPosition.Marshall).Member;
            set =>  members.First(x => x.Position == CouncilPosition.Marshall).Member = value;
        }
        public Hero Chancellor
        {
            get => members.First(x => x.Position == CouncilPosition.Chancellor).Member;
            set => members.First(x => x.Position == CouncilPosition.Chancellor).Member = value;
        }
        public Hero Steward
        {
            get => members.First(x => x.Position == CouncilPosition.Steward).Member;
            set => members.First(x => x.Position == CouncilPosition.Steward).Member = value;
        }
        public Hero Spymaster
        {
            get => members.First(x => x.Position == CouncilPosition.Spymaster).Member;
            set => members.First(x => x.Position == CouncilPosition.Spymaster).Member = value;
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
        [SaveableProperty(1)]
        private Hero member { get; set; }

        [SaveableProperty(2)]
        private CouncilPosition position { get; set; }

        public CouncilMember(Hero member, CouncilPosition position)
        {
            this.member = member;
            this.position = position;
        }

        public Hero Member
        {
            get => member;
            set => member = value;
        }
        public CouncilPosition Position => position;
        public float Competence
        {
            get
            {
                if (member != null)
                {
                    int targetCap = 300;
                    float primarySkill = 0f;
                    float secondarySkill = 0f;

                    targetCap += 15 * (member.GetAttributeValue(DefaultCharacterAttributes.Intelligence) - 5);
                    if (position == CouncilPosition.Marshall)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Leadership);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Tactics);
                    }
                    else if (position == CouncilPosition.Chancellor)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Charm);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Charm);
                    }
                    else if (position == CouncilPosition.Steward)
                    {
                        primarySkill = member.GetSkillValue(DefaultSkills.Steward);
                        secondarySkill = member.GetSkillValue(DefaultSkills.Trade);
                    }
                    else if (position == CouncilPosition.Spymaster)
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
