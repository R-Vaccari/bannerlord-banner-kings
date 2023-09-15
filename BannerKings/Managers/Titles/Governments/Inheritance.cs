using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class Inheritance : ContractAspect
    {
        public Inheritance(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, float childrenScore, float sibingScore, float spouseScore,
            float relativeScore, float authoritarian, float oligarchic, float egalitarian)
        {
            Initialize(name, description);
            ChildrenScore = childrenScore;
            SiblingScore = sibingScore;
            SpouseScore = spouseScore;
            RelativeScore = relativeScore;
            Authoritarian = authoritarian;
            Oligarchic = oligarchic;
            Egalitarian = egalitarian;
        }

        public override void PostInitialize()
        {
            Inheritance i = DefaultInheritances.Instance.GetById(this);
            Initialize(i.name, i.description, i.ChildrenScore, i.SiblingScore, i.SpouseScore, i.RelativeScore,
                i.Authoritarian,  i.Oligarchic,  i.Egalitarian);
        }

        public float ChildrenScore { get; private set; }
        public float SiblingScore { get; private set; }
        public float SpouseScore { get; private set; }
        public float RelativeScore { get; private set; }
    }
}
