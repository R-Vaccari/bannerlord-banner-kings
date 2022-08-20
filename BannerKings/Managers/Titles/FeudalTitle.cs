using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Titles
{
    public class FeudalTitle
    {
        public FeudalTitle(TitleType type, Settlement fief, List<FeudalTitle> vassals, Hero deJure, Hero deFacto,
            string name, FeudalContract contract)
        {
            this.type = type;
            this.fief = fief;
            this.vassals = vassals;
            this.deJure = deJure;
            this.deFacto = deFacto;
            this.name = new TextObject(Utils.Helpers.GetTitlePrefix(type, contract.Government) + " of " + name);
            shortName = new TextObject(name);
            this.contract = contract;
            dueTax = 0;
            claims = new Dictionary<Hero, ClaimType>();
            deJureDrift = new Dictionary<FeudalTitle, float>();
        }

        [SaveableProperty(1)] public TitleType type { get; }

        [SaveableProperty(2)] public Settlement fief { get; }

        [SaveableProperty(3)] public List<FeudalTitle> vassals { get; }

        [SaveableProperty(4)] public Hero deJure { get; internal set; }

        [SaveableProperty(5)] public Hero deFacto { get; internal set; }

        [SaveableProperty(6)] private TextObject name { get; }

        [SaveableProperty(7)] public TextObject shortName { get; private set; }

        [SaveableProperty(8)] public float dueTax { get; set; }

        [SaveableProperty(9)] public FeudalTitle sovereign { get; private set; }

        [SaveableProperty(10)] public FeudalContract contract { get; }

        [SaveableProperty(11)] private Dictionary<Hero, ClaimType> claims { get; set; }

        [SaveableProperty(12)] private Dictionary<Hero, CampaignTime> ongoingClaims { get; set; }

        [SaveableProperty(13)] private Dictionary<FeudalTitle, float> deJureDrift { get; set; }

        public Dictionary<FeudalTitle, float> DeJureDrifts
        {
            get
            {
                if (deJureDrift == null)
                {
                    deJureDrift = new Dictionary<FeudalTitle, float>();
                }

                var dic = new Dictionary<FeudalTitle, float>();
                foreach (var pair in deJureDrift)
                {
                    dic.Add(pair.Key, pair.Value);
                }

                return dic;
            }
        }

        public Dictionary<Hero, ClaimType> Claims
        {
            get
            {
                if (claims == null)
                {
                    claims = new Dictionary<Hero, ClaimType>();
                }

                return claims;
            }
        }

        public Dictionary<Hero, CampaignTime> OngoingClaims
        {
            get
            {
                if (ongoingClaims == null)
                {
                    ongoingClaims = new Dictionary<Hero, CampaignTime>();
                }

                return ongoingClaims;
            }
        }

        public TextObject FullName
        {
            get
            {
                var text = new TextObject("{=!}{TITLE} of {NAME}");
                var type = GovernmentType.Feudal;
                if (contract != null)
                {
                    type = contract.Government;
                }

                text.SetTextVariable("TITLE", Utils.Helpers.GetTitlePrefix(this.type, contract.Government, deJure.Culture));
                text.SetTextVariable("NAME", shortName.ToString());
                return text;
            }
        }

        public Hero DeFacto
        {
            get
            {
                if (fief != null)
                {
                    return fief.Owner;
                }

                var contestors = new Dictionary<Hero, int>();
                foreach (var title in vassals)
                {
                    var vassal = title.DeFacto;
                    if (contestors.ContainsKey(vassal))
                    {
                        contestors[vassal] += 1;
                    }
                    else
                    {
                        contestors.Add(vassal, 1);
                    }
                }

                var deJureCount = contestors.ContainsKey(deJure) ? contestors[deJure] : 0;
                var highestCount = contestors.Values.Max();
                var highestCountHeroes = contestors.Keys.ToList().FindAll(x => contestors[x] == highestCount);
                if (highestCountHeroes.Contains(deJure))
                {
                    return deJure;
                }

                var selected = highestCountHeroes[0];
                if (highestCountHeroes.Count > 1)
                {
                    foreach (var competitor in highestCountHeroes)
                    {
                        if ((competitor != selected && competitor.Clan.Tier > selected.Clan.Tier) ||
                            competitor.Clan.Influence > selected.Clan.Influence)
                        {
                            selected = competitor;
                        }
                    }
                }

                return selected;
            }
        }

        public bool Active => deJure != null || deFacto != null;

        public bool IsSovereignLevel => (int) type <= 1;

        public override bool Equals(object obj)
        {
            if (obj is FeudalTitle)
            {
                var target = (FeudalTitle) obj;
                return fief != null ? fief == target.fief : name == target.name;
            }

            return base.Equals(obj);
        }

        public void SetName(TextObject shortname)
        {
            shortName = shortname;
        }

        public void TickClaims()
        {
            var toAdd = new Dictionary<Hero, ClaimType>();
            foreach (var pair in OngoingClaims)
            {
                if (pair.Value.RemainingDaysFromNow <= 0f)
                {
                    toAdd.Add(pair.Key, ClaimType.Fabricated);
                }
                else
                {
                    pair.Key.AddSkillXp(BKSkills.Instance.Lordship, 3.5f);
                }
            }

            foreach (var pair in toAdd)
            {
                AddClaim(pair.Key, pair.Value);
            }
        }

        public void TickDrift(FeudalTitle current, float progress = 0.000793f)
        {
            AddDrift(current, progress);
            List<FeudalTitle> toRemove = null;
            foreach (var sovereign in deJureDrift.Keys.ToList())
            {
                if (sovereign != current)
                {
                    deJureDrift[sovereign] -= progress;
                    if (deJureDrift[sovereign] <= 0f)
                    {
                        if (toRemove == null)
                        {
                            toRemove = new List<FeudalTitle>();
                        }

                        toRemove.Add(sovereign);
                    }
                }
            }

            if (toRemove != null)
            {
                foreach (var title in toRemove)
                {
                    deJureDrift.Remove(title);
                }
            }
        }

        private void AddDrift(FeudalTitle newSovereign, float progress)
        {
            if (DeJureDrifts.ContainsKey(newSovereign))
            {
                deJureDrift[newSovereign] += progress;
            }
            else
            {
                deJureDrift.Add(newSovereign, 0f);
            }

            if (deJureDrift[newSovereign] >= 1f)
            {
                DriftTitle(newSovereign);
            }

            else if (deJureDrift[newSovereign] < 0f)
            {
                deJureDrift[newSovereign] = 0f;
            }
        }

        public ClaimType GetHeroClaim(Hero hero)
        {
            if (hero == null)
            {
                return ClaimType.None;
            }

            if (Claims.ContainsKey(hero))
            {
                return Claims[hero];
            }

            if (OngoingClaims.ContainsKey(hero))
            {
                return ClaimType.Ongoing;
            }

            return ClaimType.None;
        }

        public void AddOngoingClaim(Hero hero)
        {
            if (hero == null)
            {
                return;
            }

            if (!OngoingClaims.ContainsKey(hero))
            {
                OngoingClaims.Add(hero, CampaignTime.YearsFromNow(1));
            }
        }

        public void AddClaim(Hero hero, ClaimType type, bool force = false)
        {
            if (hero == null || type == ClaimType.None)
            {
                return;
            }

            if (!Claims.ContainsKey(hero))
            {
                Claims.Add(hero, type);
            }

            if (OngoingClaims.ContainsKey(hero))
            {
                OngoingClaims.Remove(hero);
            }

            if (hero == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=!}You now have a claim on the {TITLE}")
                    .SetTextVariable("TITLE", FullName));
            }
        }

        public void RemoveClaim(Hero hero)
        {
            if (claims.ContainsKey(hero))
            {
                Claims.Remove(hero);
            }
        }

        public void CleanClaims()
        {
            var currentDeFacto = DeFacto;
            var toRemove = new List<Hero>();
            foreach (var pair in Claims)
            {
                if (pair.Key.IsDead)
                {
                    toRemove.Add(pair.Key);
                    continue;
                }

                if (pair.Value == ClaimType.DeFacto && pair.Key != currentDeFacto)
                {
                    toRemove.Add(pair.Key);
                }
            }

            foreach (var hero in toRemove)
            {
                Claims.Remove(hero);
            }

            toRemove.Clear();
            foreach (var hero in OngoingClaims.Keys.ToList())
            {
                if (hero.IsDead)
                {
                    toRemove.Add(hero);
                }
            }

            foreach (var hero in toRemove)
            {
                Claims.Remove(hero);
            }
        }

        public void DriftTitle(FeudalTitle newSovereign, bool notify = true)
        {
            if (type == TitleType.Dukedom && newSovereign.IsSovereignLevel)
            {
                sovereign.vassals.Remove(this);
                SetSovereign(newSovereign);
                newSovereign.vassals.Add(this);

                ChangeContract(newSovereign.contract.Government);
                ChangeContract(newSovereign.contract.Succession);
                ChangeContract(newSovereign.contract.Inheritance);
                ChangeContract(newSovereign.contract.GenderLaw);

                if (notify)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=!}{TITLE} has drifted into a legal part of {SOVEREIGN}")
                            .SetTextVariable("TITLE", FullName)
                            .SetTextVariable("SOVEREIGN", newSovereign.FullName)
                            .ToString()));
                }
            }
        }

        public void SetSovereign(FeudalTitle sovereign)
        {
            this.sovereign = sovereign;
            if (vassals != null && vassals.Count > 0)
            {
                foreach (var vassal in vassals)
                {
                    vassal.SetSovereign(sovereign);
                }
            }
        }

        public void ChangeContract(GovernmentType government)
        {
            contract.ChangeGovernment(government);
            if (vassals != null && vassals.Count > 0)
            {
                foreach (var vassal in vassals)
                {
                    vassal.ChangeContract(government);
                }
            }
        }

        public void ChangeContract(SuccessionType succession)
        {
            contract.ChangeSuccession(succession);
            if (vassals != null && vassals.Count > 0)
            {
                foreach (var vassal in vassals)
                {
                    vassal.ChangeContract(succession);
                }
            }
        }

        public void ChangeContract(InheritanceType inheritance)
        {
            contract.ChangeInheritance(inheritance);
            if (vassals != null && vassals.Count > 0)
            {
                foreach (var vassal in vassals)
                {
                    vassal.ChangeContract(inheritance);
                }
            }
        }

        public void ChangeContract(GenderLaw genderLaw)
        {
            contract.ChangeGenderLaw(genderLaw);
            if (vassals != null && vassals.Count > 0)
            {
                foreach (var vassal in vassals)
                {
                    vassal.ChangeContract(genderLaw);
                }
            }
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