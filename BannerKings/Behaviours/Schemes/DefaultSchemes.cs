using BannerKings.Managers.Court;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Schemes
{
    public class DefaultSchemes : DefaultTypeInitializer<DefaultSchemes, Scheme>
    {
        public Scheme Assassination = new Scheme("Assassination");
        public Scheme Sway = new Scheme("Sway");
        public Scheme ForgeClaim = new Scheme("ForgeClaim");
        public Scheme FabricateCrime = new Scheme("FabricateCrime");

        public override IEnumerable<Scheme> All => throw new NotImplementedException();

        public override void Initialize()
        {
            Assassination.Initialize(new TextObject("{=!}Assassination"),
                new TextObject(),
                null,
                DefaultSkills.Roguery,
                Scheme.SchemeType.Criminal,
                (CouncilData council) =>
                {
                    return true;
                },
                (CouncilData council) =>
                {
                    List<Hero> targets = new List<Hero>();
                    return targets;
                },
                (CouncilData council, Hero target) =>
                {
                    return true;
                });
        }
    }
}
