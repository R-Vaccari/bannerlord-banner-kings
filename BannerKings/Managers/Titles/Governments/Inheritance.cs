using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class Inheritance : BannerKingsObject
    {
        public Inheritance(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, float childrenScore, float sibingScore, float spouseScore,
            float relativeScore)
        {
            Initialize(name, description);
            ChildrenScore = childrenScore;
            SiblingScore = sibingScore;
            SpouseScore = spouseScore;
            RelativeScore = relativeScore;
        }

        public void PostInitialize()
        {
            Inheritance i = DefaultInheritances.Instance.GetById(this);
            Initialize(i.name, i.description, i.ChildrenScore, i.SiblingScore, i.SpouseScore, i.RelativeScore);
        }

        public float ChildrenScore { get; private set; }
        public float SiblingScore { get; private set; }
        public float SpouseScore { get; private set; }
        public float RelativeScore { get; private set; }
    }
}
