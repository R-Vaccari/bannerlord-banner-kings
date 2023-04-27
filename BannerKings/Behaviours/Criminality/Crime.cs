using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Criminality
{
    public class Crime : BannerKingsObject
    {
        public Crime(string stringId) : base(stringId)
        {
        }

        public Crime GetCopy(Hero criminal, Kingdom kingdom, CrimeSeverity severity)
        {
            Crime c = new Crime(StringId);
            c.Initialize(Name, Description);
            c.Hero = criminal;
            c.Kingdom = kingdom;
            c.Severity = severity;
            c.Date = CampaignTime.Now;
            return c;
        }

        [SaveableProperty(10)] public Hero Hero { get; private set; }
        [SaveableProperty(11)] public Kingdom Kingdom { get; private set; }
        [SaveableProperty(12)] public CrimeSeverity Severity { get; private set; }
        [SaveableProperty(13)] public CampaignTime Date { get; private set; }

        public enum CrimeSeverity
        {
            Transgression,
            Blasphemy,
            Treason
        }

        public override bool Equals(object obj)
        {
            if (obj is Crime)
            {
                Crime c = (Crime)obj;
                return c.StringId == StringId && Hero == c.Hero && Kingdom == c.Kingdom;
            }
            return base.Equals(obj);
        }
    }
}
