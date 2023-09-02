using System.Collections.Generic;
using System.Linq;
using BannerKings.Extensions;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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
            TextObject name, FeudalContract contract, string stringId = null, TextObject fullName = null)
        {
            TitleType = type;
            Fief = fief;
            Vassals = vassals;
            this.deJure = deJure;
            this.deFacto = deFacto;
            if (fullName != null)
            {
                FullName = fullName;
                CustomName = true;
            }
            else
            {
                FullName = new TextObject("{=wMius2i9}{TITLE} of {NAME}")
                                .SetTextVariable("TITLE", Utils.Helpers.GetTitlePrefix(type, deJure.Culture))
                                .SetTextVariable("NAME", name);
            }

            shortName = name;
            Contract = contract;
            DueTax = 0;
            claims = new Dictionary<Hero, ClaimType>();
            deJureDrift = new Dictionary<FeudalTitle, float>();
            StringId = stringId;
        }

        [SaveableProperty(1)] public TitleType TitleType { get; private set; }
        [SaveableProperty(2)] public Settlement Fief { get; private set; }
        [SaveableProperty(3)] public List<FeudalTitle> Vassals { get; private set; }
        [SaveableProperty(4)] public Hero deJure { get; internal set; }
        [SaveableProperty(5)] public Hero deFacto { get; internal set; }
        [SaveableProperty(6)] public TextObject FullName { get; private set; }
        [SaveableProperty(7)] public TextObject shortName { get; private set; }
        [SaveableProperty(8)] public float DueTax { get; set; }
        [SaveableProperty(9)] public FeudalTitle Sovereign { get; private set; }
        [SaveableProperty(10)] public FeudalContract Contract { get; private set; }
        [SaveableProperty(11)] private Dictionary<Hero, ClaimType> claims { get; set; }
        [SaveableProperty(12)] private Dictionary<Hero, CampaignTime> ongoingClaims { get; set; }
        [SaveableProperty(13)] private Dictionary<FeudalTitle, float> deJureDrift { get; set; }
        [SaveableProperty(14)] public string StringId { get; private set; }
        [SaveableProperty(15)] public bool CustomName { get; private set; }

        public Dictionary<FeudalTitle, float> DeJureDrifts
        {
            get
            {
                deJureDrift ??= new Dictionary<FeudalTitle, float>();
                return deJureDrift.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }

        public Dictionary<Hero, ClaimType> Claims => claims ??= new Dictionary<Hero, ClaimType>();

        public Dictionary<Hero, CampaignTime> OngoingClaims => ongoingClaims ??= new Dictionary<Hero, CampaignTime>();

        public bool HeroHasValidClaim(Hero hero)
        {
            if (Claims.ContainsKey(hero))
            {
                return Claims[hero] != ClaimType.None && Claims[hero] != ClaimType.Ongoing;
            }

            return false;
        }

        public Hero DeFacto
        {
            get
            {
                if (Fief != null)
                {
                    if (Fief.IsVillage)
                    {
                        return Fief.Village.GetActualOwner();
                    }
                    return Fief.Owner;
                }

                var contestors = new Dictionary<Hero, int>();
                foreach (var vassal in Vassals.Select(title => title.DeFacto))
                {
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
                if (highestCountHeroes.Count <= 1)
                {
                    return selected;
                }

                foreach (var competitor in highestCountHeroes.Where(competitor => (competitor != selected && competitor.Clan.Tier > selected.Clan.Tier) || competitor.Clan.Influence > selected.Clan.Influence))
                {
                    selected = competitor;
                }

                return selected;
            }
        }

        public bool Active => deJure != null || deFacto != null;

        public bool IsSovereignLevel => (int) TitleType <= 1;

        public void PostInitialize()
        {
            Contract.PostInitialize(deJure.Clan.Kingdom);
            if (!CustomName)
            {
                FullName = new TextObject("{=wMius2i9}{TITLE} of {NAME}")
                                .SetTextVariable("TITLE", Utils.Helpers.GetTitlePrefix(TitleType, deJure.Culture))
                                .SetTextVariable("NAME", shortName);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is FeudalTitle target)
            {
                return Fief != null ? Fief == target.Fief : FullName == target.FullName;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void SetName(TextObject shortName)
        {
            this.shortName = shortName;
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
            foreach (var sovereign in deJureDrift.Keys.ToList().Where(sovereign => sovereign != current))
            {
                deJureDrift[sovereign] -= progress;
                if (!(deJureDrift[sovereign] <= 0f))
                {
                    continue;
                }

                toRemove ??= new List<FeudalTitle>();
                toRemove.Add(sovereign);
            }

            if (toRemove == null)
            {
                return;
            }

            foreach (var title in toRemove)
            {
                deJureDrift.Remove(title);
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

            switch (deJureDrift[newSovereign])
            {
                case >= 1f:
                    DriftTitle(newSovereign);
                    break;
                case < 0f:
                    deJureDrift[newSovereign] = 0f;
                    break;
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
                var claimTime = CampaignTime.YearsFromNow(1);

                var lordshipClaimant = BKPerks.Instance.LordshipClaimant;
                if (hero.GetPerkValue(lordshipClaimant))
                {
                    claimTime = CampaignTime.DaysFromNow(CampaignTime.DaysInYear * 0.7f);
                }
                
                OngoingClaims.Add(hero, claimTime);
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
                MBInformationManager.AddQuickInformation(new TextObject("{=bdsY7u3H}You now have a claim on the {TITLE}")
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
            toRemove.AddRange(OngoingClaims.Keys.ToList().Where(hero => hero.IsDead));

            foreach (var hero in toRemove)
            {
                Claims.Remove(hero);
            }
        }

        public void DriftTitle(FeudalTitle newSovereign, bool notify = true)
        {
            if (TitleType > TitleType.Dukedom)
            {
                return;
            }

            if (Sovereign != null && Sovereign.Vassals.Contains(this))
            {
                Sovereign.Vassals.Remove(this);
            }
            SetSovereign(newSovereign);
            newSovereign.Vassals.Add(this);

            //ChangeContract(newSovereign.Contract.Government);
           // ChangeContract(newSovereign.Contract.Succession);
            ChangeContract(newSovereign.Contract.Inheritance);
            ChangeContract(newSovereign.Contract.GenderLaw);

            if (notify)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=5ryn9tJc}{TITLE} has drifted into a legal part of {SOVEREIGN}")
                        .SetTextVariable("TITLE", FullName)
                        .SetTextVariable("SOVEREIGN", newSovereign.FullName)
                        .ToString()));
            }
        }

        public void SetSovereign(FeudalTitle sovereign)
        {
            this.Sovereign = sovereign;
            if (Vassals is {Count: > 0})
            {
                foreach (var vassal in Vassals)
                {
                    vassal.SetSovereign(sovereign);
                }
            }
        }

        public void SetLaws(List<DemesneLaw> laws)
        {
            Contract.SetLaws(laws);
            if (Vassals is { Count: > 0 })
            {
                foreach (var vassal in Vassals)
                {
                    vassal.SetLaws(laws);
                }
            }
        }

        public void EnactLaw(DemesneLaw law, Hero enactor = null)
        {
            if (enactor != null)
            {
                law = law.GetCopy();
                law.SetIssueDate(CampaignTime.Now);
                GainKingdomInfluenceAction.ApplyForDefault(enactor, -law.InfluenceCost);
                if (enactor.MapFaction == Hero.MainHero.MapFaction)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=fvbLMV0a}The {LAW} has been enacted in the {TITLE}.")
                        .SetTextVariable("LAW", law.Name)
                        .SetTextVariable("TITLE", FullName),
                        0,
                        null,
                        "event:/ui/notification/relation");
                }
            }

            Contract.EnactLaw(law);
            if (Vassals is { Count: > 0 })
            {
                foreach (var vassal in Vassals)
                {
                    vassal.EnactLaw(law);
                }
            }
        }

        public void ChangeContract(Government government)
        {
            Contract.ChangeGovernment(government);
            if (Vassals is {Count: > 0})
            {
                foreach (var vassal in Vassals)
                {
                    vassal.ChangeContract(government);
                }
            }
        }

        public void ChangeContract(Succession succession)
        {
            Contract.ChangeSuccession(succession);
            if (Vassals is {Count: > 0})
            {
                foreach (var vassal in Vassals)
                {
                    vassal.ChangeContract(succession);
                }
            }
        }

        public void ChangeContract(Inheritance inheritance)
        {
            Contract.ChangeInheritance(inheritance);
            if (Vassals is {Count: > 0})
            {
                foreach (var vassal in Vassals)
                {
                    vassal.ChangeContract(inheritance);
                }
            }
        }

        public void ChangeContract(GenderLaw genderLaw)
        {
            Contract.ChangeGenderLaw(genderLaw);
            if (Vassals is {Count: > 0})
            {
                foreach (var vassal in Vassals)
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