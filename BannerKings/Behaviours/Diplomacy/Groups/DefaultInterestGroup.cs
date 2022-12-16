
using System.Collections.Generic;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class DefaultInterestGroup : DefaultTypeInitializer<DefaultInterestGroup, InterestGroup>
    {
        public InterestGroup Traditionalists { get; } = new InterestGroup("traditionalists");
        public InterestGroup Oligarchists { get; } = new InterestGroup("oligarchists");
        public InterestGroup Zealots { get; } = new InterestGroup("zealots");
        public InterestGroup Gentry { get; } = new InterestGroup("gentry");
        public InterestGroup Guilds { get; } = new InterestGroup("guilds");
        public override IEnumerable<InterestGroup> All => throw new System.NotImplementedException();

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }
    }
}
