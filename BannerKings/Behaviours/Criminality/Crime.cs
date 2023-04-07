using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Criminality
{
    public class Crime : BannerKingsObject
    {
        public Crime(string stringId) : base(stringId)
        {
        }

        public Hero Hero { get; private set; }
        public Kingdom Kingdom { get; private set; }
        public CrimeSeverity Severity { get; private set; }

        public enum CrimeSeverity
        {
            Transgression,
            Blasphemy,
            Treason
        }
    }
}
