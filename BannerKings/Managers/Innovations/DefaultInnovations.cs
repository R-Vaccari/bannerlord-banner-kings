using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Innovations
{
    public class DefaultInnovations : DefaultTypeInitializer<DefaultInnovations, Innovation>
    {

        private Innovation heavyPlough, threeFieldsSystem, sewers;
        public Innovation HeavyPlough => heavyPlough;
        public Innovation ThreeFieldsSystem => threeFieldsSystem;

        public override IEnumerable<Innovation> All
        {
            get
            {
                yield return HeavyPlough;
                yield return ThreeFieldsSystem;
            }
        }

        public override void Initialize()
        {
            MBReadOnlyList<CultureObject> cultures = Game.Current.ObjectManager.GetObjectTypeList<CultureObject>();

            heavyPlough = new Innovation("innovation_heavy_plough");
            heavyPlough.Initialize(new TextObject(),
                new TextObject(),
                new TextObject(),
                1000f,
                null,
                null);

            threeFieldsSystem = new Innovation("innovation_three_field_system");
            threeFieldsSystem.Initialize(new TextObject(),
                new TextObject(),
                new TextObject(),
                1000f,
                null,
                null);
        }
    }
}
