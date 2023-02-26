using System.Collections.Generic;

namespace BannerKings.Managers.Court.Members
{
    public class DefaultCouncilPositions : DefaultTypeInitializer<DefaultCouncilPositions, CouncilMember>
    {
        public CouncilMember Marshal { get; set; } = new Marshal();
        public CouncilMember Steward { get; set; } = new Steward();
        public CouncilMember Chancellor { get; set; } = new Chancellor();
        public CouncilMember Spiritual { get; set; } = new Spiritual();
        public CouncilMember Spymaster { get; set; } = new Spymaster();
        public CouncilMember Philosopher { get; set; } = new Philosopher();
        public CouncilMember Castellan { get; set; } = new Castellan();
        public CouncilMember Elder { get; set; } = new Elder();
        public CouncilMember Spouse { get; set; } = new Spouse();

        public override IEnumerable<CouncilMember> All
        {
            get
            {
                yield return Marshal;
                yield return Steward;
                yield return Chancellor;
                yield return Spymaster;
                yield return Spiritual;
                yield return Spouse;
                yield return Philosopher;
                yield return Castellan;
                yield return Elder;
            }
        }

        public override void Initialize()
        {
        }
    }
}
