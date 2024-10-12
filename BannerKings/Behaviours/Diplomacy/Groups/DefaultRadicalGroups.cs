using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class DefaultRadicalGroups : DefaultTypeInitializer<DefaultRadicalGroups, RadicalGroup>
    {
        public RadicalGroup Claimant { get; } = new RadicalGroup("Claimant");
        public RadicalGroup Secession { get; } = new RadicalGroup("SecessionGroup");

        public override IEnumerable<RadicalGroup> All
        {
            get
            {
                yield return Claimant;
                yield return Secession;
                foreach (RadicalGroup group in ModAdditions)
                {
                    yield return group;
                }
            }
        }

        public override void Initialize()
        {
            Claimant.Initialize(new TextObject("{=!}Claimant"),
                new TextObject("{=0XhSiqsR}A claimant group supports replacing the realm's current ruler with a claimant. The claimant must be a valid candidate under the realm's Succession law. The stronger a candidate, the more likely others will join the Claimant group. Current ruler's legitimacy and personal relationship with individual lords are also very important factors. In case the claimant is of the same clan as the current ruler, this means that they would become the family head themselves."),
                DefaultDemands.Instance.Claimant);

            Secession.Initialize(new TextObject("{=!}Secession"),
                new TextObject("{=!}Demand secession from the current realm. Group members are reorganized in a newly founded realm lead by a ruler of their choosing. Secession apologists do not pursue any changes in their existing realm - only to leave it for a realm of their own making. Unlike independence apologists, secession group members will be bound together in a single new realm. Members will be persuaded to join according to their opinions of their current ruler and the ruler-to-be supported by the group."),
                DefaultDemands.Instance.Secession);
        }
    }
}
