using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Institutions.Religions.Leaderships;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Religion : MBObjectBase
    {
        [SaveableField(4)] private Dictionary<Settlement, Clergyman> clergy; 
        [field: SaveableField(3)] public Faith Faith { get; private set; }
        public HeadOfFaith HeadOfFaith { get; private set; }

        public Religion(string id) : base(id)
        {
            clergy = new Dictionary<Settlement, Clergyman>();
        }

        public void Initialize(Faith faith, 
            List<CultureObject> favoredCultures)
        {
            Faith = faith;
            FavoredCultures = favoredCultures;
        }

        public List<CultureObject> FavoredCultures { get; private set; }
        public MBReadOnlyList<Rite> Rites => new MBReadOnlyList<Rite>(Faith.Rites);
        public CultureObject MainCulture => FavoredCultures[0];

        public bool HasDoctrine(Doctrine doctrine) => Faith.Doctrines.Contains(doctrine);

        public FaithStance GetStance(Faith otherFaith)
        {
            if (HasDoctrine(DefaultDoctrines.Instance.Tolerant)) return FaithStance.Tolerated;
            return Faith.GetStance(otherFaith);
        }

        public void ChangeClergymanRank(Clergyman clergyman, int newRank)
        {
            var firstName = clergyman.Hero.FirstName;
            var fullName = new TextObject("{=6MHqUBXt}{RELIGIOUS_TITLE} {NAME}")
                .SetTextVariable("RELIGIOUS_TITLE", Faith.GetRankTitle(newRank))
                .SetTextVariable("NAME", firstName);
            clergyman.Hero.SetName(fullName, firstName);
            clergyman.Rank = newRank;
        }

        public void RemoveClergyman(Clergyman clergyman)
        {
            Settlement settlement = null;
            foreach (var pair in clergy)
            {
                if (pair.Key != null && pair.Value != null && pair.Value == clergyman)
                {
                    settlement = pair.Key;
                }
            }

            if (settlement != null)
            {
                clergy.Remove(settlement);
                List<Hero> notables = (List<Hero>)AccessTools.Field(settlement.GetType(), "_notablesCache").GetValue(settlement);
                if (notables.Contains(clergyman.Hero))
                {
                    notables.Remove(clergyman.Hero);
                }
            }
        }

        public MBReadOnlyDictionary<Settlement, Clergyman> Clergy => clergy.GetReadOnlyDictionary();
        public ExplainedNumber Fervor => BannerKingsConfig.Instance.ReligionModel.CalculateFervor(this);

        internal void PostInitialize(Faith faith)
        {
            StringId = faith.GetId();
            if (clergy == null) clergy = new Dictionary<Settlement, Clergyman>();
            Religion rel = DefaultReligions.Instance.GetById(this);
            FavoredCultures = rel.FavoredCultures;
            Faith = faith;
        }

        public void AddClergyman(Settlement settlement, Hero hero)
        {
            var clergyman = new Clergyman(hero, Faith.GetIdealRank(settlement));
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
            if (clergy.ContainsKey(settlement))
            {
                var hero = clergy[settlement];
                if (hero == null || hero.Hero.IsDead)
                {
                    hero = GenerateClergyman(settlement);
                    clergy[settlement] = hero;
                    return hero;
                }

                return hero;
            } else
            {
                var hero = GenerateClergyman(settlement);
                return hero;
            }
        }
        
        public Clergyman GenerateClergyman(Settlement settlement)
        {
            var rank = Faith.GetIdealRank(settlement);
            if (rank <= 0)
            {
                return null;
            }

            var character = Faith.GetPreset(rank);
            var title = Faith.GetRankTitle(rank);
            Hero preacher = settlement.HeroesWithoutParty.FirstOrDefault(x => x.IsPreacher && x.Name.ToString().Contains(title.ToString()));
            if (preacher != null)
            {
                var clergyman = new Clergyman(preacher, rank);
                if (!clergy.ContainsKey(settlement))
                {
                    clergy.Add(settlement, clergyman);
                }
                else
                {
                    clergy[settlement] = clergyman;
                }
                return clergyman;
            }

            if (character != null)
            {
                var hero = GenerateClergymanHero(character, settlement, rank);
                EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                var clergyman = new Clergyman(hero, rank);
                if (!clergy.ContainsKey(settlement))
                {
                    clergy.Add(settlement, clergyman);
                } else
                {
                    clergy[settlement] = clergyman;
                }
                return clergyman;
            }

            throw new BannerKingsException(string.Format("No preset found for faith with id [{0}] at clergy rank [{1}]",
                Faith.GetId(), rank));
        }

        private Hero GenerateClergymanHero(CharacterObject preset, Settlement settlement, int rank)
        {
            var hero = HeroCreator.CreateSpecialHero(preset, settlement);
            var firstName = hero.FirstName;
            var fullName = new TextObject("{=6MHqUBXt}{RELIGIOUS_TITLE} {NAME}")
                .SetTextVariable("RELIGIOUS_TITLE", Faith.GetRankTitle(rank))
                .SetTextVariable("NAME", firstName);
            hero.SetName(fullName, firstName);
            return hero;
        }

        public override bool Equals(object obj)
        {
            if (obj is Religion rel)
            {
                return Faith.GetId() == rel.Faith.GetId();
            }

            return base.Equals(obj);
        }
    }
}