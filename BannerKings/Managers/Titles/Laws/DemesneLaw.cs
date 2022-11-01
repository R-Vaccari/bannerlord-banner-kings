using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Laws
{
    public class DemesneLaw : BannerKingsObject
    {
       
        public DemesneLaw(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, TextObject effects, DemesneLawTypes type,
            float authoritarian, float egalitarian, float oligarchic, int influenceCost, int index, CultureObject culture = null)
        {
            Initialize(name, description);
            Effects = effects;
            LawType = type;
            AuthoritarianWeight = authoritarian;
            EgalitarianWeight = egalitarian; 
            OligarchicWeight = oligarchic;
            InfluenceCost = influenceCost;
            Culture = culture;
            Index = index;
        }

        public DemesneLaw GetCopy()
        {
            var law = new DemesneLaw(StringId);
            law.Initialize(Name, Description, Effects, LawType,
                AuthoritarianWeight, EgalitarianWeight, OligarchicWeight,
                InfluenceCost, Index, Culture);
            law.SetIssueDate(CampaignTime.Now);

            return law;
        }

        public int Index { get; private set; }

        public void SetIssueDate(CampaignTime date) => IssueDate = date;
        public CampaignTime IssueDate { get; private set; }

        public bool AvailableForVoting => IssueDate.ElapsedYearsUntilNow >= 1f;

        public float AuthoritarianWeight { get; private set; }
        public float EgalitarianWeight { get; private set; }
        public float OligarchicWeight { get; private set; }
        public TextObject Effects { get; private set; }
        public CultureObject Culture { get; private set; }
        public int InfluenceCost { get; private set; }
        public DemesneLawTypes LawType { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is DemesneLaw)
            {
                return (obj as DemesneLaw).StringId == StringId;
            }

            return base.Equals(obj);
        }
    }

    public enum DemesneLawTypes
    {
        Slavery,
        Drafting,
        SerfDuties,
        CraftsmenDuties,
        SlaveDuties,
        NobleDuties,
        EstateTenure
    }
}
