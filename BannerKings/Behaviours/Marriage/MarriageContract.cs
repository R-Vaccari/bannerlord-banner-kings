using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Marriage
{
    public class MarriageContract
    {

        public Hero Proposer { get; private set; }
        public Hero Proposed { get; private set; }
        public Clan FinalClan { get; private set; }
        public int Dowry { get; private set; }
    }
}
