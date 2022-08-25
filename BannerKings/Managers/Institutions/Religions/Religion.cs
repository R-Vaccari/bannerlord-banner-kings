using System;
using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Institutions.Religions.Leaderships;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Religion : LandedInstitution
    {
        [SaveableField(1)] private readonly Dictionary<Settlement, Clergyman> clergy;

        [SaveableField(5)] private readonly List<string> doctrineIds;

        [SaveableField(4)] private readonly List<CultureObject> favoredCultures;

        public Religion(Settlement settlement, Faith faith, ReligiousLeadership leadership, List<CultureObject> favoredCultures, List<string> doctrineIds) : base(settlement)
        {
            clergy = new Dictionary<Settlement, Clergyman>();
            Leadership = leadership;
            Faith = faith;
            this.favoredCultures = favoredCultures;
            this.doctrineIds = doctrineIds;
            leadership.Initialize(this);
        }

        public MBReadOnlyList<Rite> Rites
        {
            get
            {
                var list = new List<Rite>();
                list.AddRange(Faith.Rites);
                if (doctrineIds.Contains("sacrifice"))
                {
                    list.Add(new Sacrifice());
                }

                return list.GetReadOnlyList();
            }
        }

        public CultureObject MainCulture => favoredCultures[0];
        [field: SaveableField(2)] public ReligiousLeadership Leadership { get; }

        public Divinity MainGod => Faith.MainGod;
        [field: SaveableField(3)] public Faith Faith { get; private set; }

        public MBReadOnlyList<string> Doctrines => doctrineIds.GetReadOnlyList();
        public MBReadOnlyDictionary<Settlement, Clergyman> Clergy => clergy.GetReadOnlyDictionary();

        public MBReadOnlyList<CultureObject> FavoredCultures => favoredCultures.GetReadOnlyList();

        private ExplainedNumber fervorCache;
        public ExplainedNumber Fervor => BannerKingsConfig.Instance.ReligionModel.CalculateFervor(this);
  
       
            

        internal void PostInitialize(Faith faith)
        {
            Faith = faith;
        }

        public void AddClergyman(Settlement settlement, Clergyman clergyman)
        {
            if (clergy.ContainsKey(settlement))
            {
                clergy[settlement] = clergyman;
            }
            else
            {
                clergy.Add(settlement, clergyman);
            }
        }

        public Clergyman GetClergyman(Settlement settlement)
        {
            return clergy.ContainsKey(settlement) ? clergy[settlement] : null;
        }

        public Clergyman GenerateClergyman(Settlement settlement)
        {
            var rank = Faith.GetIdealRank(settlement, settlement == this.settlement);
            if (rank <= 0)
            {
                return null;
            }

            var character = Faith.GetPreset(rank);
            if (character != null)
            {
                var hero = GenerateClergymanHero(character, settlement, rank);
                EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                var clergyman = new Clergyman(hero, rank);
                clergy.Add(settlement, clergyman);
                return clergyman;
            }

            throw new BannerKingsException(string.Format("No preset found for faith with id [{0}] at clergy rank [{1}]",
                Faith.GetId(), rank));
        }

        public Hero GenerateClergymanHero(CharacterObject preset, Settlement settlement, int rank)
        {
            var hero = HeroCreator.CreateSpecialHero(preset, settlement);
            var firstName = hero.FirstName;
            var fullName = new TextObject("{=6MHqUBXt}{RELIGIOUS_TITLE} {NAME}")
                .SetTextVariable("RELIGIOUS_TITLE", Faith.GetRankTitle(rank))
                .SetTextVariable("NAME", firstName);
            hero.SetName(fullName, firstName);
            return hero;
        }

        public bool IsFavoredCulture(CultureObject culture)
        {
            return favoredCultures.Contains(culture);
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj is Religion rel)
            {
                return Faith.GetId() == rel.Faith.GetId();
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}