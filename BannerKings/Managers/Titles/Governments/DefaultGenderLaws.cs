using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultGenderLaws : DefaultTypeInitializer<DefaultGenderLaws, GenderLaw>
    {
        public GenderLaw Agnatic { get; } = new GenderLaw("Agnatic");
        public GenderLaw Cognatic { get; } = new GenderLaw("Cognatic");
        public GenderLaw Enatic { get; } = new GenderLaw("Enatic");

        public override IEnumerable<GenderLaw> All
        {
            get
            {
                yield return Agnatic;
                yield return Cognatic;
                yield return Enatic;
            }
        }

        public GenderLaw GetKingdomIdealGenderLaw(string id, Government government)
        {
            if (id == "empire_s" || id == "khuzait")
            {
                return Cognatic;
            }

            return Agnatic;
        }

        public override void Initialize()
        {
            Agnatic.Initialize(new TextObject("{=!}Agnatic"),
                new TextObject("{=!}Agnatic gender law gives precedence to male inheritors over females. While females are not blocked from inheritance, it its likely they will only inherit after the male options are exhausted, unless a female can exceed quite significantly in competence the best existing male option. Females are suppressed from important positions such as knighthood."),
                0f,
                0f,
                0f,
                1f,
                0.1f,
                false,
                true);

            Cognatic.Initialize(new TextObject("{=!}Cognatic"),
                new TextObject("{=!}Cognatic gender law gives no precedence to any of either genders, allowing both to fulfill important positions, and clan inheritances to be based solely on their competence criteria. For example, with Cognatic Primogeniture, the eldest child should inherit the clan, regardless of their gender."),
                0f,
                0f,
                0f,
                1f,
                1f,
                false,
                false);

            Enatic.Initialize(new TextObject("{=!}Enatic"),
                new TextObject("{=!}Enatic gender law gives precedence to female inheritors over males. While males are not blocked from inheritance, it its likely they will only inherit after the female options are exhausted, unless a male can exceed quite significantly in competence the best existing female option. Males are suppressed from important positions such as knighthood."),
                0f,
                0f,
                0f,
                0.1f,
                1f,
                true,
                false);
        }
    }
}
