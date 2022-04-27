using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers
{
    public class ReligionsManager
    {

        
        private List<FaithGroup> FaithGroups { get; set; }
        private List<Faith> Faiths { get; set; }
        private Dictionary<Religion, List<Hero>> Religions { get; set; }


        public void InitializeReligions()
        {
            DefaultDivinities.Instance.Initialize();
            DefaultFaiths.Instance.Initialize();
            CultureObject aserai = BannerKings.Helpers.Helpers.GetCulture("aserai");
            CultureObject khuzait = BannerKings.Helpers.Helpers.GetCulture("khuzait");
            CultureObject imperial = BannerKings.Helpers.Helpers.GetCulture("imperial");

            Religion aseraiReligion = new Religion(Settlement.All.First(x => x.StringId == "town_A1"), DefaultFaiths.Instance.AseraCode, new DescentralizedLeadership(),
                new List<CultureObject>() { aserai, khuzait, imperial });

            this.Religions.Add(aseraiReligion, new List<Hero>());
        }
    }
}
