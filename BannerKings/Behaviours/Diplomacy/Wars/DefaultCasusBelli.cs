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
        public CasusBelli ImperialReconquest { get; } = new CasusBelli("imperial_reconquest");
        public CasusBelli CulturalLiberation { get; } = new CasusBelli("cultural_liberation");
        public CasusBelli GreatRaid { get; } = new CasusBelli("great_raid");
        public CasusBelli Invasion { get; } = new CasusBelli("invasion");
        public override IEnumerable<CasusBelli> All => throw new NotImplementedException();

        public override void Initialize()
        {
            CulturalLiberation.Initialize(new TextObject("{=!}Cultural Liberation"),
                new TextObject("{=!}Liberate a fief of your people from the rule of foreigners. Any town or castle that is mostly composed by our culture is reason enough for us to rule it rather than foreigners."),
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

                },
                new TextObject("{=!}The {ATTACKER} marches to war! {FIEF} is being liberated from the oppresion of {DEFENDER}!"));

            Invasion.Initialize(new TextObject("{=!}Invasion"),
                new TextObject("{=!}Invade a foreign kingdom as is the way of our ancestors. Seize their property and stablish our own rule."),
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
                    var id = faction1.StringId;
                    return (id == "vlandia" || id == "aserai") && faction2.Culture != faction1.Culture;
                },
                (Kingdom kingdom) =>
                {
                    return true;
                },
                new Dictionary<TraitObject, float>()
                {

                },
                new TextObject("{=!}The {ATTACKER} is launching a large scale invasion on the {DEFENDER}!"));

            GreatRaid.Initialize(new TextObject("{=!}Great Raid"),
                new TextObject("{=!}Pillage and steal from our enemies as our ancestors did. Ruling their lands may be unviable, but it will not stop us from taking what we are owed by the rule of the strongest."),
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
                    var id = faction1.StringId;
                    return (id == "battania" || id == "khuzait" || id == "sturgia") && faction2.Culture != faction1.Culture;
                },
                (Kingdom kingdom) =>
                {
                    return true;
                },
                new Dictionary<TraitObject, float>()
                {

                },
                new TextObject("{=!}The {ATTACKER} ride out for a great raid! {DEFENDER} towns and villages will be razed to the ground."));

            ImperialSuperiority.Initialize(new TextObject("{=!}Imperial Superiority"),
                new TextObject("{=!}Subjugate barbarians with our Imperial might as the original Empire once did. Strength is the language that they understand."),
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
                        if (faction1.Culture.StringId == "empire" && faction2.Culture != faction1.Culture)
                        {
                            return true;
                        }

                        var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction1 as Kingdom);
                        if (title != null && title.contract.Government == Managers.Titles.GovernmentType.Imperial)
                        {
                            if (faction2.IsKingdomFaction)
                            {
                                var enemyTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction1 as Kingdom);
                                if (enemyTitle != null && enemyTitle.contract.Government != Managers.Titles.GovernmentType.Imperial &&
                                faction2.Culture != faction1.Culture)
                                {
                                    return true;
                                }
                            }
                            else if (faction2.Culture != faction1.Culture) 
                            {
                                return true;
                            }
                        }
                    }

                    return false;
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

                },
                new TextObject("{=!}The {ATTACKER} is subjugating the barbarians of {DEFENDER}!"));

            ImperialReconquest.Initialize(new TextObject("{=!}Imperial Reconquest"),
                new TextObject("{=!}Subjugate pretenders of the Empire. As rightful heirs of the Calradian Empire, any other kingdom that claims to be so ought to be subjugated and annexed by any means necessary."),
                1.5f,
                0.1f,
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
                    return faction1.Culture.StringId == "empire" && faction2.Culture == faction1.Culture;
                },
                (Kingdom kingdom) =>
                {
                    return kingdom.Culture.StringId == "empire";
                },
                new Dictionary<TraitObject, float>()
                {

                },
                new TextObject("{=!}The {ATTACKER} is marching on {DEFENDER}! They claim to be the rightful heir of the Empire."));
        }
    }
}
