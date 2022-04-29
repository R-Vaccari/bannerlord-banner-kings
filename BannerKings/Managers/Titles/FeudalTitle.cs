﻿using System.Collections.Generic;
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

        [SaveableProperty(13)]
        private Dictionary<FeudalTitle, float> deJureDrift { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FeudalTitle)
            {
                FeudalTitle target = (FeudalTitle)obj;
                return fief != null ? fief == target.fief : name == target.name;
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
            shortName = new TextObject(name);
            this.contract = contract;
            dueTax = 0;
            claims = new Dictionary<Hero, ClaimType>();
            deJureDrift = new Dictionary<FeudalTitle, float>();
        }

        public void SetName(TextObject shortname) => shortName = shortname;

        public Dictionary<FeudalTitle, float> DeJureDrifts
        {
            get 
            {
                if (deJureDrift == null) deJureDrift = new Dictionary<FeudalTitle, float>();
                Dictionary<FeudalTitle, float> dic = new Dictionary<FeudalTitle, float>();
                foreach (KeyValuePair<FeudalTitle, float> pair in deJureDrift)
                    dic.Add(pair.Key, pair.Value);

                return dic;
            }
        }

        public void TickDrift(FeudalTitle current, float progress = 0.000793f)
        {
            AddDrift(current, progress);
            List<FeudalTitle> toRemove = null;
            foreach (FeudalTitle sovereign in deJureDrift.Keys.ToList())
                if (sovereign != current)
                {
                    deJureDrift[sovereign] -= progress;
                    if (deJureDrift[sovereign] <= 0f)
                    {
                        if (toRemove == null) toRemove = new List<FeudalTitle>();
                        toRemove.Add(sovereign);
                    }
                }

            if (toRemove != null)
                foreach (FeudalTitle title in toRemove)
                    deJureDrift.Remove(title);
        }

        private void AddDrift(FeudalTitle newSovereign, float progress)
        {
            if (DeJureDrifts.ContainsKey(newSovereign))
                deJureDrift[newSovereign] += progress;
            else deJureDrift.Add(newSovereign, 0f);

            if (deJureDrift[newSovereign] >= 1f)
                DriftTitle(newSovereign);
            
            else if (deJureDrift[newSovereign] < 0f)
                deJureDrift[newSovereign] = 0f;
        }

        public Dictionary<Hero, ClaimType> Claims
        {
            get
            {
                if (claims == null) claims = new Dictionary<Hero, ClaimType>();
                return claims;
            }
        }

        public Dictionary<Hero, CampaignTime> OngoingClaims
        {
            get
            {
                if (ongoingClaims == null) ongoingClaims = new Dictionary<Hero, CampaignTime>();
                return ongoingClaims;
            }
        }

        public ClaimType GetHeroClaim(Hero hero)
        {
            if (hero == null) return ClaimType.None;
            if (Claims.ContainsKey(hero))
                return Claims[hero];
            if (OngoingClaims.ContainsKey(hero))
                return ClaimType.Ongoing;
            return ClaimType.None;
        }

        public void AddOngoingClaim(Hero hero)
        {
            if (hero == null) return;
            if (!OngoingClaims.ContainsKey(hero))
                OngoingClaims.Add(hero, CampaignTime.YearsFromNow(1));
        }

        public void AddClaim(Hero hero, ClaimType type, bool force = false)
        {
            if (hero == null || type == ClaimType.None) return;
            Claims.Add(hero, type);
            if (OngoingClaims.ContainsKey(hero))
                OngoingClaims.Remove(hero);
            if (hero == Hero.MainHero)
                InformationManager.AddQuickInformation(new TextObject("{=!}You now have a claim on the {TITLE}")
                            .SetTextVariable("TITLE", FullName));
        }

        public void RemoveClaim(Hero hero)
        {
            if (claims.ContainsKey(hero))
                Claims.Remove(hero);
        }

        public void CleanClaims()
        {
            Hero currentDeFacto = DeFacto;
            List<Hero> toRemove = new List<Hero>();
            foreach (KeyValuePair<Hero, ClaimType> pair in Claims)
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
                Claims.Remove(hero);

            toRemove.Clear();
            foreach (Hero hero in OngoingClaims.Keys.ToList())
                if (hero.IsDead)
                    toRemove.Add(hero);

            foreach (Hero hero in toRemove)
                Claims.Remove(hero);
        }

        public TextObject FullName
        {
            get
            {
                TextObject text = new TextObject("{=!}{TITLE} of {NAME}");
                text.SetTextVariable("TITLE", BannerKings.Helpers.Helpers.GetTitlePrefix(type, deJure.Culture));
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

                Dictionary<Hero, int> contestors = new Dictionary<Hero, int>();
                foreach (FeudalTitle title in vassals)
                {
                    Hero vassal = title.DeFacto;
                    if (contestors.ContainsKey(vassal))
                        contestors[vassal] += 1;
                    else contestors.Add(vassal, 1);
                }

                int deJureCount = contestors.ContainsKey(deJure) ? contestors[deJure] : 0;
                int highestCount = contestors.Values.Max();
                List<Hero> highestCountHeroes = contestors.Keys.ToList().FindAll(x => contestors[x] == highestCount);
                if (highestCountHeroes.Contains(deJure))
                    return deJure;
                Hero selected = highestCountHeroes[0];
                if (highestCountHeroes.Count > 1)
                    foreach (Hero competitor in highestCountHeroes)
                        if (competitor != selected && competitor.Clan.Tier > selected.Clan.Tier ||
                            competitor.Clan.Influence > selected.Clan.Influence)
                            selected = competitor;

                return selected;
            }
        }

        public bool Active => deJure != null || deFacto != null;

        public bool IsSovereignLevel => (int)type <= 1;

        public void DriftTitle(FeudalTitle newSovereign)
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

                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{TITLE} has drifted into a legal part of {SOVEREIGN}")
                    .SetTextVariable("TITLE", FullName)
                    .SetTextVariable("SOVEREIGN", newSovereign.FullName)
                    .ToString()));
            }
        }

        public void SetSovereign(FeudalTitle sovereign)
        {
            this.sovereign = sovereign;
            if (vassals != null && vassals.Count > 0)
                foreach (FeudalTitle vassal in vassals)
                    vassal.SetSovereign(sovereign);
        }

        public void ChangeContract(GovernmentType government)
        {
            contract.ChangeGovernment(government);
            if (vassals != null && vassals.Count > 0)
                foreach (FeudalTitle vassal in vassals)
                    vassal.ChangeContract(government);
        }

        public void ChangeContract(SuccessionType succession)
        {
            contract.ChangeSuccession(succession);
            if (vassals != null && vassals.Count > 0)
                foreach (FeudalTitle vassal in vassals)
                    vassal.ChangeContract(succession);
        }

        public void ChangeContract(InheritanceType inheritance)
        {
            contract.ChangeInheritance(inheritance);
            if (vassals != null && vassals.Count > 0)
                foreach (FeudalTitle vassal in vassals)
                    vassal.ChangeContract(inheritance);
        }

        public void ChangeContract(GenderLaw genderLaw)
        {
            contract.ChangeGenderLaw(genderLaw);
            if (vassals != null && vassals.Count > 0)
                foreach (FeudalTitle vassal in vassals)
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
