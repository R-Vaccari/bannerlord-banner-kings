using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Diplomacy
{
    public class War
    {
        public IFaction Attacker { get; }
        public IFaction Defender { get; }

        public Kingdom Sovereign { get; }

        public bool IsInternalWar() => Attacker.IsClan && Defender.IsClan && Sovereign != null;
    }
}
