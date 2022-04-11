using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
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
        }

        public void SetName(TextObject shortname) => this.shortName = shortname;

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

        public void SetSovereign(FeudalTitle sovereign)
        {
            this.sovereign = sovereign;
            if (this.vassals != null && this.vassals.Count > 0)
                foreach (FeudalTitle vassal in this.vassals)
                    vassal.SetSovereign(sovereign);
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
}
