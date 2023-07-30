using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Criminality
{
    public class DefaultCrimes : DefaultTypeInitializer<DefaultCrimes, Crime>
    {
        public Crime Banditry { get; } = new Crime("banditry");
        public Crime Deviancy { get; } = new Crime("Deviancy");
        public Crime Adultery { get; } = new Crime("Adultery");
        public Crime DiplomaticTransgression { get; } = new Crime("DiplomaticTransgression");
        public Crime Abduction { get; } = new Crime("Abduction");
        public Crime Homicide { get; } = new Crime("Homicide");
        public override IEnumerable<Crime> All
        {
            get
            {
                yield return Banditry;
            }
        }

        public override void Initialize()
        {
            Banditry.Initialize(new TextObject("{=PHtRTUEP}Banditry"),
                new TextObject("{=YGVxxoVL}On {DATE}, {HERO} is accused of generalized banditry, including unlawful thievery, pillaging and murdering. Banditry is only associated with commoners, as the crimes of nobility are held to different standards. Banditry calls for the death sentence."));

            Deviancy.Initialize(new TextObject("{=s6ey3Mhd}Deviancy"),
                new TextObject("{=WyZ4C8eF}On {DATE}, {HERO} is accused of sexual deviancy according to the beliefs of their faith. Applicable punishments depend on how much the faith dislikes this behavior."));

            Adultery.Initialize(new TextObject("{=Po5UKApM}Adultery"),
                new TextObject("{=XaPdcJ5V}On {DATE}, {HERO} is accused of adultery according to the beliefs of their faith. Every faith describes how two or more persons may be arranged as spouses in marriage, and thus sets the boundaries for adultery. Applicable punishments depend on how much the faith dislikes this behavior."));

            DiplomaticTransgression.Initialize(new TextObject("{=ADhXDNxC}Diplomatic Transgression"),
                new TextObject("{=jQ5f5KMa}On {DATE}, {HERO} is accused of infringing their autonomy within diplomatic matters, such as unilaterally starting a war with another realm. Actions such as raiding another realm's villages during peace breach the peace agreement and bring your entire realm to war, be it their will or not, qualifying you as a criminal."));

            Abduction.Initialize(new TextObject("{=mPiJsBBs}Abduction"),
                new TextObject("{=SSbd6EBN}On {DATE}, {HERO} is accused of abducting {TARGET}."));

            Homicide.Initialize(new TextObject("{=2D39bHCB}Homicide"),
                new TextObject("{=02v3EJRN}On {DATE}, {HERO} is accused of murdering {TARGET}."));
        }
    }
}
