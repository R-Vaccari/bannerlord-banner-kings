using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class DefaultRadicalGroups : DefaultTypeInitializer<DefaultRadicalGroups, RadicalGroup>
    {
        public RadicalGroup Claimant { get; } = new RadicalGroup("Claimant");

        public override IEnumerable<RadicalGroup> All
        {
            get
            {
                yield return Claimant;
                foreach (RadicalGroup group in ModAdditions)
                {
                    yield return group;
                }
            }
        }

        public override void Initialize()
        {
            Claimant.Initialize(new TextObject("Claimant"),
                new TextObject("{=0XhSiqsR}A claimant group supports replacing the realm's current ruler with a claimant. The claimant must be a valid candidate under the realm's Succession law. The stronger a candidate, the more likely others will join the Claimant group. Current ruler's legitimacy and personal relationship with individual lords are also very important factors. In case the claimant is of the same clan as the current ruler, this means that they would become the family head themselves."),
                DefaultDemands.Instance.Claimant);
        }
    }
}
