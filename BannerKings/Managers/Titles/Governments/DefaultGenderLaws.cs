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

        public GenderLaw GetKingdomIdealGenderLaw(Kingdom kingdom, Government government)
        {
            string id = kingdom.StringId;
            if (id == "empire_s" || id == "khuzait")
            {
                return Cognatic;
            }

            return Agnatic;
        }

        public override void Initialize()
        {
            Agnatic.Initialize(new TextObject(),
                new TextObject(),
                0f,
                0f,
                0f,
                1f,
                0.1f,
                false,
                true);

            Cognatic.Initialize(new TextObject(),
                new TextObject(),
                0f,
                0f,
                0f,
                1f,
                1f,
                false,
                false);

            Enatic.Initialize(new TextObject(),
                new TextObject(),
                0f,
                0f,
                0f,
                1f,
                0.1f,
                true,
                false);
        }
    }
}
