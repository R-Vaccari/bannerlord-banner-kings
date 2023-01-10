using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class DefaultCasusBelli : DefaultTypeInitializer<DefaultCasusBelli, CasusBelli>
    {
        public CasusBelli ImperialSuperiority { get; } = new CasusBelli("imperial_superiority");
        public CasusBelli CulturalLiberation { get; } = new CasusBelli("cultural_liberation");
        public override IEnumerable<CasusBelli> All => throw new NotImplementedException();

        public override void Initialize()
        {
            CulturalLiberation.Initialize(new TextObject("{=!}Cultural Liberation"),
                new TextObject(),
                1.5f,
                0.2f,
                0.8f,
                5000f,
                (War war) =>
                {
                    return war.CasusBelli.Fief.MapFaction == war.Attacker;
                },
                (War war) =>
                {
                    var targetFaction = war.CasusBelli.Fief.MapFaction;
                    return targetFaction != war.Defender && targetFaction != war.Attacker;
                },
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    var settlement = casusBelli.Fief;
                    return settlement != null && settlement.Culture == faction1.Culture && settlement.Culture != faction2.Culture;
                },
                (Kingdom kingdom) =>
                {
                    return true;
                },
                new Dictionary<TraitObject, float>()
                {

                });

            ImperialSuperiority.Initialize(new TextObject("{=!}Imperial Superiority"),
                new TextObject(),
                1.5f,
                0.2f,
                0.8f,
                5000f,
                (War war) =>
                {
                    return war.CasusBelli.Fief.MapFaction == war.Attacker;
                },
                (War war) =>
                {
                    var targetFaction = war.CasusBelli.Fief.MapFaction;
                    return targetFaction != war.Defender && targetFaction != war.Attacker;
                },
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    if (faction1.IsKingdomFaction)
                    {
                        var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction1 as Kingdom);
                        if (title != null && title.contract.Government == Managers.Titles.GovernmentType.Imperial)
                        {

                        }
                    }
                    var settlement = casusBelli.Fief;
                    return settlement != null && settlement.Culture == faction1.Culture && settlement.Culture != faction2.Culture;
                },
                (Kingdom kingdom) =>
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null && title.contract.Government == Managers.Titles.GovernmentType.Imperial)
                    {
                        return true;
                    }

                    return false;
                },
                new Dictionary<TraitObject, float>()
                {

                });
        }
    }
}
