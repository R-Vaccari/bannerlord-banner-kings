using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class War
    {
        public War(IFaction attacker, IFaction defender, CasusBelli casusBelli, Kingdom sovereign = null)
        {
            Attacker = attacker;
            Defender = defender;
            CasusBelli = casusBelli;
            Sovereign = sovereign;
        }

        public IFaction Attacker { get; }
        public IFaction Defender { get; }
        public CasusBelli CasusBelli { get; }
        public Kingdom Sovereign { get; }

        public bool IsInternalWar() => Attacker.IsClan && Defender.IsClan && Sovereign != null;
        public bool IsMatchingWar(IFaction faction1, IFaction faction2) => faction1 == Attacker && faction2 == Defender ||
            faction2 == Attacker && faction1 == Defender;

        public void EndWar()
        {

        }
    }
}
