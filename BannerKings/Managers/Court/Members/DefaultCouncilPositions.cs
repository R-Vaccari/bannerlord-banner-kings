using System.Collections.Generic;

namespace BannerKings.Managers.Court.Members
{
    public class DefaultCouncilPositions : DefaultTypeInitializer<DefaultCouncilPositions, CouncilPosition>
    {
        public CouncilPosition Marshal { get; set; } = new Marshal();
        public CouncilPosition Steward { get; set; } = new Steward();
        public CouncilPosition Chancellor { get; set; } = new Chancellor();
        public CouncilPosition Spiritual { get; set; } = new Spiritual();
        public CouncilPosition Spymaster { get; set; } = new Spymaster();
        public CouncilPosition Philosopher { get; set; } = new Philosopher();
        public CouncilPosition Castellan { get; set; } = new Castellan();
        public CouncilPosition Elder { get; set; } = new Elder();

        public override IEnumerable<CouncilPosition> All
        {
            get
            {
                yield return Marshal;
                yield return Steward;
                yield return Chancellor;
                yield return Spymaster;
                yield return Spiritual;
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
