using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class DefaultMercenaryPrivileges : DefaultTypeInitializer<DefaultMercenaryPrivileges, MercenaryPrivilege>
    {
        public override IEnumerable<MercenaryPrivilege> All
        {
            get
            {
                yield return IncreasedPay;
                yield return EstateGrant;
                yield return WorkshopGrant;
                yield return CustomTroop3;
                yield return CustomTroop5;
                yield return BaronyGrant;
                yield return FullPeerage;
            }
        }

        public MercenaryPrivilege IncreasedPay { get; set; } = new MercenaryPrivilege("privilege_increased_pay");
        public MercenaryPrivilege EstateGrant { get; set; } = new MercenaryPrivilege("privilege_estate_grant");
        public MercenaryPrivilege WorkshopGrant { get; set; } = new MercenaryPrivilege("privilege_workshop_grant");
        public MercenaryPrivilege CustomTroop3 { get; set; } = new MercenaryPrivilege("privilege_levy_troop");
        public MercenaryPrivilege CustomTroop5 { get; set; } = new MercenaryPrivilege("privilege_professional_troop");
        public MercenaryPrivilege BaronyGrant { get; set; } = new MercenaryPrivilege("privilege_barony_grant");
        public MercenaryPrivilege FullPeerage { get; set; } = new MercenaryPrivilege("privilege_full_peerage");

        public override void Initialize()
        {
            IncreasedPay.Initialize(new TextObject("{=!}Increased Pay"),
                new TextObject("{=!}Increase the revenue of your mercenary contract by 5% per level."),
                new TextObject("{=!}Max level: {LEVEL}")
                .SetTextVariable("LEVEL", 5),
                100f,
                5,
                delegate(MercenaryCareer career)
                {
                    return true;
                });

            WorkshopGrant.Initialize(new TextObject("{=!}Workshop Grant"),
                new TextObject("{=!}Acquire a workshop property. The property will be situated in one of the towns of your contractor."),
                new TextObject("{=!}Your contractor is not currently capable of granting a workshop, due to lacking themselves or funds to subsidize the acquisition of one. Alternatively, you've reached your workshop limit.\nMax level: {LEVEL}")
                .SetTextVariable("LEVEL", 2),
                500f,
                2,
                delegate (MercenaryCareer career)
                {
                    return true;
                });

            EstateGrant.Initialize(new TextObject("{=!}Estate Grant"),
                new TextObject("{=!}Acquire an estate property. The property will be situated in one of the villages of your contractor."),
                new TextObject("{=!}Your contractor is currently unable to provide a vacant estate.\nMax level: {LEVEL}")
                .SetTextVariable("LEVEL", 2),
                650f,
                2,
                delegate (MercenaryCareer career)
                {
                    return true;
                });

            CustomTroop3.Initialize(new TextObject("{=!}Mercenary Levy"),
                new TextObject("{=!}Stablish a mercenary levy (tier III) troop for your company. A custrom troop is designable and will only be available for your clan, in towns of the kingdom associated with the career they were designed."),
                new TextObject("{=!}Max level: {LEVEL}")
                .SetTextVariable("LEVEL", 1),
                500f,
                1,
                delegate (MercenaryCareer career)
                {
                    return true;
                });

            CustomTroop5.Initialize(new TextObject("{=!}Mercenary Professional"),
                new TextObject("{=!}Stablish a mercenary professional (tier V) troop for your company. A custrom troop is designable and will only be available for your clan, in towns of the kingdom associated with the career they were designed."),
                new TextObject("{=!}Max level: {LEVEL}").SetTextVariable("LEVEL", 1),
                100f,
                1,
                delegate (MercenaryCareer career)
                {
                    return true;
                });

            BaronyGrant.Initialize(new TextObject("Barony Grant"),
                new TextObject("{=!}Become landed in the fashion of a lord. Request a castle alongside it's barony-level title. The settlement ownership will not undo your mercenary contract."),
                new TextObject("{=!}You must not own any settlements or titles. Your contractor may not have an extra castle and it's title available.\nMax level: {LEVEL}").SetTextVariable("LEVEL", 1),
                100f,
                1,
                delegate (MercenaryCareer career)
                {
                    return true;
                });

            FullPeerage.Initialize(new TextObject("Full Peerage"),
                new TextObject("{=!}Join your contractor's realm as a full Peer. Your mercenary life will be left behind and you will be considered among the elite of nobility."),
                new TextObject().SetTextVariable("LEVEL", 1),
                100f,
                1,
                delegate (MercenaryCareer career)
                {
                    return true;
                });
        }
    }
}
