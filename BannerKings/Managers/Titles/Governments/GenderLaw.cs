using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class GenderLaw : ContractAspect
    {
        public GenderLaw(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, float authoritarian,
            float oligarchic, float egalitarian, float malePreference, float femalePreference,
            bool maleSupressed, bool femaleSupressed)
        {
            Initialize(name, description);
            Authoritarian = authoritarian;
            Oligarchic = oligarchic;
            Egalitarian = egalitarian;
            MalePreference = malePreference;
            FemalePreference = femalePreference;
            MaleSupressed = maleSupressed;
            FemaleSupressed = femaleSupressed;
        }

        public override void PostInitialize()
        {
            GenderLaw i = DefaultGenderLaws.Instance.GetById(this);
            Initialize(i.name, i.description, i.Authoritarian, i.Oligarchic, i.Egalitarian, i.MalePreference,
                i.FemalePreference, i.MaleSupressed, i.FemaleSupressed);
        }

        public float MalePreference { get; private set; }
        public float FemalePreference { get; private set; }
        public bool MaleSupressed { get; private set; }
        public bool FemaleSupressed { get; private set; }
    }
}
