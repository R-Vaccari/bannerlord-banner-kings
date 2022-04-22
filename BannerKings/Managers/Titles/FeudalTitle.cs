using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles
{
    public class FeudalTitle
    {
        [SaveableProperty(1)]
        public TitleType type { get; private set; }

        [SaveableProperty(2)]
        public Settlement fief { get; private set; }

        [SaveableProperty(3)]
        public List<FeudalTitle> vassals { get; private set; }

        [SaveableProperty(4)]
        public Hero deJure { get; internal set; }

        [SaveableProperty(5)]
        public Hero deFacto { get; internal set; }

        [SaveableProperty(6)]
        private TextObject name { get; set; }

        [SaveableProperty(7)]
        public TextObject shortName { get; private set; }

        [SaveableProperty(8)]
        public float dueTax { get; set; }

        [SaveableProperty(9)]
        public FeudalTitle sovereign { get; private set; }

        [SaveableProperty(10)]
        public FeudalContract contract { get; private set; }

        [SaveableProperty(11)]
        private Dictionary<Hero, ClaimType> claims { get; set; }

        [SaveableProperty(12)]
        private Dictionary<Hero, CampaignTime> ongoingClaims { get; set; }

        private Dictionary<FeudalTitle, float> deJureDrift { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FeudalTitle)
            {
                FeudalTitle target = (FeudalTitle)obj;
                return this.fief != null ? this.fief == target.fief : this.name == target.name;
            }
            return base.Equals(obj);
        }

        public FeudalTitle(TitleType type, Settlement fief, List<FeudalTitle> vassals, Hero deJure, Hero deFacto, string name, FeudalContract contract)
        {
            this.type = type;
            this.fief = fief;
            this.vassals = vassals;
            this.deJure = deJure;
            this.deFacto = deFacto;
            this.name = new TextObject(BannerKings.Helpers.Helpers.GetTitlePrefix(type) + " of " + name);
            this.shortName = new TextObject(name);
            this.contract = contract;
            dueTax = 0;
            this.claims = new Dictionary<Hero, ClaimType>();
            this.deJureDrift = new Dictionary<FeudalTitle, float>();
        }

        public void SetName(TextObject shortname) => this.shortName = shortname;

        public Dictionary<FeudalTitle, float> DeJureDrifts
        {
            get 
            {
                Dictionary<FeudalTitle, float> dic = new Dictionary<FeudalTitle, float>();
                foreach (KeyValuePair<FeudalTitle, float> pair in this.deJureDrift)
                    dic.Add(pair.Key, pair.Value);

                return dic;
            }
        }

        public void TickDrift(FeudalTitle current, float progress = 0.000793f)
        {
            this.AddDrift(current, progress);
            List<FeudalTitle> toRemove = null;
            foreach (FeudalTitle sovereign in this.deJureDrift.Keys.ToList())
                if (sovereign != current)
                {
                    this.deJureDrift[sovereign] -= progress;
                    if (this.deJureDrift[sovereign] <= 0f)
                    {
                        if (toRemove == null) toRemove = new List<FeudalTitle>();
                        toRemove.Add(sovereign);
                    }
                }

            if (toRemove != null)
                foreach (FeudalTitle title in toRemove)
                    this.deJureDrift.Remove(title);
        }

        private void AddDrift(FeudalTitle newSovereign, float progress)
        {
            if (this.deJureDrift.ContainsKey(newSovereign))
                this.deJureDrift[newSovereign] += progress;
            else this.deJureDrift.Add(newSovereign, 0f);

            if (this.deJureDrift[newSovereign] >= 1f)
            {
                this.SetSovereign(newSovereign)
            }
            else if (this.deJureDrift[newSovereign] < 0f)
                this.deJureDrift[newSovereign] = 0f;
        }

        public Dictionary<Hero, ClaimType> Claims
        {
            get
            {
                if (this.claims == null) this.claims = new Dictionary<Hero, ClaimType>();
                return this.claims;
            }
        }

        public Dictionary<Hero, CampaignTime> OngoingClaims
        {
            get
            {
                if (this.ongoingClaims == null) this.ongoingClaims = new Dictionary<Hero, CampaignTime>();
                return this.ongoingClaims;
            }
        }

        public ClaimType GetHeroClaim(Hero hero)
        {;

            if (hero == null) return ClaimType.None;
            if (this.Claims.ContainsKey(hero))
                return this.Claims[hero];
            else if (this.OngoingClaims.ContainsKey(hero))
                return ClaimType.Ongoing;
            else return ClaimType.None;
        }

        public void AddOngoingClaim(Hero hero)
        {
            if (hero == null) return;
            if (!this.OngoingClaims.ContainsKey(hero))
                this.OngoingClaims.Add(hero, CampaignTime.YearsFromNow(1));
            
        }

        public void AddClaim(Hero hero, ClaimType type, bool force = false)
        {
            if (hero == null || type == ClaimType.None) return;

            if (!force)
            {
                if (this.OngoingClaims.ContainsKey(hero))
                {
                    this.Claims.Add(hero, type);
                    this.OngoingClaims.Remove(hero);
                }
            } else
            {
                this.Claims.Add(hero, type);
                if (this.OngoingClaims.ContainsKey(hero))
                    this.OngoingClaims.Remove(hero);
            } 
        }

        public void RemoveClaim(Hero hero)
        {
            if (this.claims.ContainsKey(hero))
                this.Claims.Remove(hero);
        }

        public void CleanClaims()
        {
            Hero currentDeFacto = this.DeFacto;
            List<Hero> toRemove = new List<Hero>();
            foreach (KeyValuePair<Hero, ClaimType> pair in this.Claims)
            {
                if (pair.Key.IsDead)
                {
                    toRemove.Add(pair.Key);
                    continue;
                }
                if (pair.Value == ClaimType.DeFacto && pair.Key != currentDeFacto)
                    toRemove.Add(pair.Key);
            }
                
            foreach (Hero hero in toRemove)
                this.Claims.Remove(hero);

            toRemove.Clear();
            foreach (Hero hero in this.OngoingClaims.Keys.ToList())
                if (hero.IsDead)
                    toRemove.Add(hero);

            foreach (Hero hero in toRemove)
                this.Claims.Remove(hero);
        }

        public TextObject FullName
        {
            get
            {
                TextObject text = new TextObject("{=!}{TITLE} of {NAME}");
                text.SetTextVariable("TITLE", BannerKings.Helpers.Helpers.GetTitlePrefix(this.type, deJure.Culture));
                text.SetTextVariable("NAME", this.shortName.ToString());
                return text;
            }
        }

        public Hero DeFacto
        {
            get
            {
                if (this.fief != null)
                {
                    return this.fief.Owner;
                }
                else
                {
                    Dictionary<Hero, int> contestors = new Dictionary<Hero, int>();
                    foreach (FeudalTitle title in this.vassals)
                    {
                        Hero vassal = title.DeFacto;
                        if (contestors.ContainsKey(vassal))
                            contestors[vassal] += 1;
                        else contestors.Add(vassal, 1);
                    }

                    int deJureCount = contestors.ContainsKey(this.deJure) ? contestors[this.deJure] : 0;
                    int highestCount = contestors.Values.Max();
                    List<Hero> highestCountHeroes = contestors.Keys.ToList().FindAll(x => contestors[x] == highestCount);
                    if (highestCountHeroes.Contains(this.deJure))
                        return this.deJure;
                    else
                    {
                        Hero selected = highestCountHeroes[0];
                        if (highestCountHeroes.Count > 1)
                            foreach (Hero competitor in highestCountHeroes)
                                if (competitor != selected && competitor.Clan.Tier > selected.Clan.Tier ||
                                    competitor.Clan.Influence > selected.Clan.Influence)
                                    selected = competitor;

                        return selected;

                    }
                }
            }
        }

        public bool Active => this.deJure != null || this.deFacto != null;

        public void SetSovereign(FeudalTitle sovereign, bool notification = false)
        {
            this.sovereign = sovereign;
            if (this.vassals != null && this.vassals.Count > 0)
                foreach (FeudalTitle vassal in this.vassals)
                    vassal.SetSovereign(sovereign);
            if (notification)
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}The {TILE} has drifted into a legal part of {SOVEREIGN}.")
                    .SetTextVariable("TITLE", this.FullName)
                    .SetTextVariable("SOVEREIGN", sovereign.FullName)
                    .ToString()));
        }

        public void ChangeContract(GovernmentType government)
        {
            this.contract.ChangeGovernment(government);
            if (this.vassals != null && this.vassals.Count > 0)
                foreach (FeudalTitle vassal in this.vassals)
                    vassal.ChangeContract(government);
        }

        public void ChangeContract(SuccessionType succession)
        {
            this.contract.ChangeSuccession(succession);
            if (this.vassals != null && this.vassals.Count > 0)
                foreach (FeudalTitle vassal in this.vassals)
                    vassal.ChangeContract(succession);
        }

        public void ChangeContract(InheritanceType inheritance)
        {
            this.contract.ChangeInheritance(inheritance);
            if (this.vassals != null && this.vassals.Count > 0)
                foreach (FeudalTitle vassal in this.vassals)
                    vassal.ChangeContract(inheritance);
        }

        public void ChangeContract(GenderLaw genderLaw)
        {
            this.contract.ChangeGenderLaw(genderLaw);
            if (this.vassals != null && this.vassals.Count > 0)
                foreach (FeudalTitle vassal in this.vassals)
                    vassal.ChangeContract(genderLaw);
        }
    }

    public enum TitleType
    {
        Empire,
        Kingdom,
        Dukedom,
        County,
        Barony,
        Lordship
    }

    public enum ClaimType
    {
        Fabricated,
        Previous_Owner,
        DeFacto,
        Clan_Split,
        None,
        Ongoing
    }
}
