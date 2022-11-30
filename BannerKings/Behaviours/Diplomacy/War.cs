using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Diplomacy
{
    public class War
    {
        public IFaction Attacker { get; }
        public IFaction Defender { get; }

        public Kingdom Sovereign { get; }

        public bool IsInternalWar() => Attacker.IsClan && Defender.IsClan && Sovereign != null;
        public bool IsMatchingWar(IFaction faction1, IFaction faction2) => (faction1 == Attacker && faction2 == Defender) ||
            (faction2 == Attacker && faction1 == Defender);

        public void EndWar()
        {

        }
    }
}
