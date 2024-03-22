using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Traits;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
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
        public CasusBelli FiefClaim { get; } = new CasusBelli("fief_claim");
        public CasusBelli SuppressThreat { get; } = new CasusBelli("SuppressThreat");
        public CasusBelli Rebellion { get; } = new CasusBelli("Rebellion");
        public override IEnumerable<CasusBelli> All
        {
            get
            {
                yield return ImperialReconquest;
                yield return ImperialSuperiority;
                yield return CulturalLiberation;
                yield return GreatRaid;
                yield return Invasion;
                yield return FiefClaim;
                yield return SuppressThreat;
                yield return Rebellion;
                foreach (var item in ModAdditions)
                    yield return item;
            }
        }

        public override void Initialize()
        {
            Rebellion.Initialize(new TextObject("{=kcjyuGpA}Rebellion"),
                new TextObject("{=t8HQqf4z}A rebellion war is fought by former radical groups over a realm, after their demand was rejected by their ruler. Rebels seek to enforce their demand by force.{newline}{newline}Objective: Survive as a rebellion for over 2 years with at least 1 fief."),
                new TextObject("{=EOpunWCA}Survive for 2 years"),
                2f,
                0.1f,
                1f,
                5000f,
                (War war) =>
                {
                    return war.StartDate.ElapsedYearsUntilNow >= 1f && war.Attacker.Fiefs.Count >= 2;
                },
                (War war) => false,
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) => false,
                (Kingdom kingdom) => false,
                new Dictionary<TraitObject, float>()
                {
                    { BKTraits.Instance.Ambitious, 0.1f },
                    { DefaultTraits.Valor, 0.2f }
                },
                new TextObject("{=v7CeGL18}The {ATTACKER} marches to war! They claim {DEFENDER} are an existential threat."));

            SuppressThreat.Initialize(new TextObject("{=iN3RbEku}Suppress Threat"),
                new TextObject("{=WsSwHEk4}Liberate a fief of your people from the rule of foreigners. Any town or castle that is mostly composed by our culture is reason enough for us to rule it rather than foreigners.\n\nObjective: Capture the selected target."),
                new TextObject("{=kyB8tkgY}Conquer a fief"),
                1.3f,
                0.5f,
                1f,
                2500f,
                (War war) =>
                {
                    StanceLink attackerLink = war.Attacker.GetStanceWith(war.Defender);
                    List<Settlement> attackerConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(war.Attacker,
                       attackerLink, (Settlement x) => x.Town != null);

                    return attackerConquests.FindAll(x => x.Culture == war.Defender.Culture && x.MapFaction == war.Attacker).Count >= 1;
                },
                (War war) => false,
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    if (faction2.Fiefs.Count > 0 && faction1.Fiefs.Count > 0)
                    {
                        War possibleWar = new War(faction1, faction2, null);
                        bool strength = faction2.TotalStrength >= (faction1.TotalStrength * 0.8f);
                        float distance = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapDistanceModel.GetDistance(possibleWar.DefenderFront.Settlement,
                                 possibleWar.AttackerFront.Settlement);
                        float factor = distance / TaleWorlds.CampaignSystem.Campaign.AverageDistanceBetweenTwoFortifications;
                        return strength && factor <= 2f;
                    } return false;
                },
                (Kingdom kingdom) => true,
                new Dictionary<TraitObject, float>()
                {
                    { BKTraits.Instance.Ambitious, 0.1f },
                    { DefaultTraits.Valor, 0.2f }
                },
                new TextObject("{=v7CeGL18}The {ATTACKER} marches to war! They claim {DEFENDER} are an existential threat."));

            CulturalLiberation.Initialize(new TextObject("{=kyB8tkgY}Cultural Liberation"),
                new TextObject("{=WsSwHEk4}Liberate a fief of your people from the rule of foreigners. Any town or castle that is mostly composed by our culture is reason enough for us to rule it rather than foreigners.\n\nObjective: Capture the selected target."),
                new TextObject("{=kyB8tkgY}Conquer the target fief"),
                1.3f,
                0.5f,
                1f,
                10000f,
                (War war) =>
                {
                    return war.CasusBelli.Fief != null && (war.CasusBelli.Fief.MapFaction == war.Attacker ||
                    war.CasusBelli.Fief.OwnerClan.Kingdom == war.Attacker);
                },
                (War war) =>
                {
                    if (war.CasusBelli.Fief == null) 
                    {
                        return true;
                    }
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
                    { BKTraits.Instance.Just, 0.2f },
                    { DefaultTraits.Mercy, 0.2f },
                    { DefaultTraits.Generosity, 0.1f },
                    { DefaultTraits.Egalitarian, 0.1f },
                    { DefaultTraits.Valor, -0.1f },
                    { BKTraits.Instance.Ambitious, -0.1f }
                },
                new TextObject("{=zcmfRF08}The {ATTACKER} marches to war! {FIEF} is being liberated from the oppresion of {DEFENDER}!"));

            FiefClaim.Initialize(new TextObject("{=kyB8tkgY}Claim Fief"),
                new TextObject("{=kyB8tkgY}Conquer a fief claimed by your realm. The benefactor of the conquest will always be the claimant, regardless of other ownership procedures.\n\nObjective: Capture the selected target."),
                new TextObject("{=kyB8tkgY}Conquer the target fief"),
                1.2f,
                0.7f,
                1f,
                7500f,
                (War war) =>
                {
                    return war.CasusBelli.Fief != null && (war.CasusBelli.Fief.MapFaction == war.Attacker ||
                    war.CasusBelli.Fief.OwnerClan.Kingdom == war.Attacker);
                },
                (War war) =>
                {
                    var targetFaction = war.CasusBelli.Fief.MapFaction;
                    return targetFaction != war.Defender && targetFaction != war.Attacker;
                },
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    var settlement = casusBelli.Fief;
                    var title = casusBelli.Title;
                    ClaimType claim = title.GetHeroClaim(casusBelli.Claimant);
                    return settlement != null && claim != ClaimType.None && claim != ClaimType.Ongoing;
                },
                (Kingdom kingdom) =>
                {
                    return true;
                },
                new Dictionary<TraitObject, float>()
                {

                },
                new TextObject("{=zcmfRF08}The {ATTACKER} marches to war! {FIEF} is being liberated from the oppresion of {DEFENDER}!"));

            Invasion.Initialize(new TextObject("{=yz3sGus6}Invasion"),
                new TextObject("{=6yqojVsh}Invade a foreign kingdom as is the way of our ancestors. Seize their property and stablish our own rule.\n\nObjective: Capture 2 or more fiefs of the enemy's culture."),
                new TextObject("{=kyB8tkgY}Conquer Fiefs"),
                1.5f,
                0.2f,
                0.8f,
                8000f,
                (War war) =>
                {
                    StanceLink attackerLink = war.Attacker.GetStanceWith(war.Defender);
                    List<Settlement> attackerConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(war.Attacker,
                       attackerLink, (Settlement x) => x.Town != null);

                    return attackerConquests.FindAll(x => x.Culture == war.Defender.Culture && x.MapFaction == war.Attacker).Count >= 2;
                },
                (War war) =>
                {
                    var targetFaction = war.CasusBelli.Fief.MapFaction;
                    return targetFaction != war.Defender && targetFaction != war.Attacker;
                },
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    var id = faction1.StringId;
                    bool hasFiefs = faction2.Fiefs.ToList().FindAll(x => x.Culture == faction2.Culture).Count() >= 2;
                    return (id == "vlandia" || id == "aserai") && faction2.Culture != faction1.Culture && hasFiefs;
                },
                (Kingdom kingdom) =>
                {
                    return true;
                },
                new Dictionary<TraitObject, float>()
                {
                    { BKTraits.Instance.Just, -0.2f },
                    { DefaultTraits.Mercy, -0.1f },
                    { DefaultTraits.Egalitarian, -0.1f },
                    { DefaultTraits.Authoritarian, 0.1f },
                    { DefaultTraits.Oligarchic, 0.1f },
                    { DefaultTraits.Valor, 0.1f },
                    { BKTraits.Instance.Ambitious, 0.1f }
                },
                new TextObject("{=YSBDqEEu}The {ATTACKER} is launching a large scale invasion on the {DEFENDER}!"));

            GreatRaid.Initialize(new TextObject("{=JasbMH20}Great Raid"),
                new TextObject("{=GjgPaAne}Pillage and steal from our enemies as our ancestors did. Ruling their lands may be unviable, but it will not stop us from taking what we are owed by the rule of the strongest.\n\nObjective: Raid 8 or more villages of the enemy's culture."),
                new TextObject("{=mkkNJgAA}Mass Raiding"),
                0.5f,
                3f,
                0.8f,
                8000f,
                (War war) =>
                {
                    StanceLink attackerLink = war.Attacker.GetStanceWith(war.Defender);
                    List<Settlement> attackerConquests = DiplomacyHelper.GetRaidsInWar(war.Attacker,
                       attackerLink, null);

                    return attackerConquests.FindAll(x => x.Culture == war.Defender.Culture).Count >= 8;
                },
                (War war) =>
                {
                    var targetFaction = war.CasusBelli.Fief.MapFaction;
                    return targetFaction != war.Defender && targetFaction != war.Attacker;
                },
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    var id = faction1.StringId;
                    bool hasFiefs = faction2.Settlements.ToList().FindAll(x => x.IsVillage && x.Culture == faction2.Culture).Count() >= 12;
                    return (id == "battania" || id == "khuzait" || id == "sturgia") && faction2.Culture != faction1.Culture && hasFiefs;
                },
                (Kingdom kingdom) =>
                {
                    return true;
                },
                new Dictionary<TraitObject, float>()
                {
                    { BKTraits.Instance.Just, -0.2f },
                    { DefaultTraits.Mercy, -0.2f },
                    { DefaultTraits.Egalitarian, -0.1f },
                    { DefaultTraits.Valor, 0.2f },
                    { BKTraits.Instance.Ambitious, 0.1f }
                },
                new TextObject("{=RyUztoR1}The {ATTACKER} ride out for a great raid! {DEFENDER} towns and villages will be razed to the ground."));

            ImperialSuperiority.Initialize(new TextObject("{=t5fJoQWO}Imperial Superiority"),
                new TextObject("{=EFqkRSpY}Subjugate barbarians with our Imperial might as the original Empire once did. Strength is the language that they understand.\n\nObjective: Capture 1 or more fiefs of the enemy's culture."),
                new TextObject("{=kyB8tkgY}Conquer 2 fiefs"),
                1f,
                0.4f,
                1.8f,
                5000f,
                (War war) =>
                {
                    StanceLink attackerLink = war.Attacker.GetStanceWith(war.Defender);
                    List<Settlement> attackerConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(war.Attacker,
                       attackerLink, (Settlement x) => x.Town != null);

                    return attackerConquests.FindAll(x => x.Culture == war.Defender.Culture && x.MapFaction == war.Attacker).Count >= 2;
                },
                (War war) =>
                {
                    var targetFaction = war.CasusBelli.Fief.MapFaction;
                    return targetFaction != war.Defender && targetFaction != war.Attacker;
                },
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    bool adequateKingdom = false;
                    if (faction1.IsKingdomFaction)
                    {
                        if (faction1.Culture.StringId == "empire" && faction2.Culture != faction1.Culture)
                        {
                            adequateKingdom = true;
                        }

                        var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction1 as Kingdom);
                        if (title != null && title.Contract.Government == DefaultGovernments.Instance.Imperial)
                        {
                            if (faction2.IsKingdomFaction)
                            {
                                var enemyTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction1 as Kingdom);
                                if (enemyTitle != null && enemyTitle.Contract.Government != DefaultGovernments.Instance.Imperial &&
                                faction2.Culture != faction1.Culture)
                                {
                                    adequateKingdom = true;
                                }
                            }
                            else if (faction2.Culture != faction1.Culture) 
                            {
                                adequateKingdom =  true;
                            }
                        }
                    }

                   bool hasFiefs = faction2.Fiefs.ToList().FindAll(x => x.Culture == faction2.Culture).Count() >= 2;
                   return adequateKingdom && hasFiefs;
                },
                (Kingdom kingdom) =>
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null && title.Contract.Government == DefaultGovernments.Instance.Imperial)
                    {
                        return true;
                    }

                    return false;
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Egalitarian, -0.2f },
                    { DefaultTraits.Authoritarian, 0.3f },
                    { DefaultTraits.Oligarchic, 0.1f },
                    { DefaultTraits.Valor, 0.1f },
                    { BKTraits.Instance.Diligent, 0.1f },
                    { BKTraits.Instance.Ambitious, 0.1f },
                    { DefaultTraits.Mercy, -0.15f }
                },
                new TextObject("{=arKYyCGd}The {ATTACKER} is subjugating the barbarians of {DEFENDER}!"));

            ImperialReconquest.Initialize(new TextObject("{=Tpgeed0V}Imperial Reconquest"),
                new TextObject("{=PXJD1at9}Subjugate pretenders of the Empire. As rightful heirs of the Calradian Empire, any other kingdom that claims to be so ought to be subjugated and annexed by any means necessary.\n\nObjective: Capture 1 or more fiefs of the enemy's culture."),
                new TextObject("{=kyB8tkgY}Conquer Fiefs"),
                1.5f,
                0.1f,
                0.8f,
                8000f,
                (War war) =>
                {
                    StanceLink attackerLink = war.Attacker.GetStanceWith(war.Defender);
                    List<Settlement> attackerConquests = DiplomacyHelper.GetSuccessfullSiegesInWarForFaction(war.Attacker,
                       attackerLink, (Settlement x) => x.Town != null);

                    return attackerConquests.FindAll(x => x.Culture == war.Defender.Culture && x.MapFaction == war.Attacker).Count >= 1;
                },
                (War war) =>
                {
                    var targetFaction = war.CasusBelli.Fief.MapFaction;
                    return targetFaction != war.Defender && targetFaction != war.Attacker;
                },
                (IFaction faction1, IFaction faction2, CasusBelli casusBelli) =>
                {
                    bool hasFiefs = faction2.Fiefs.ToList().FindAll(x => x.Culture == faction2.Culture).Count() >= 1;
                    return faction1.Culture.StringId == "empire" && faction2.Culture == faction1.Culture && hasFiefs;
                },
                (Kingdom kingdom) =>
                {
                    return kingdom.Culture.StringId == "empire";
                },
                new Dictionary<TraitObject, float>()
                {
                    { DefaultTraits.Egalitarian, 0.1f },
                    { DefaultTraits.Authoritarian, 0.3f },
                    { DefaultTraits.Oligarchic, 0.1f },
                    { DefaultTraits.Valor, 0.1f },
                    { BKTraits.Instance.Diligent, 0.1f },
                    { BKTraits.Instance.Ambitious, 0.2f },
                    { BKTraits.Instance.Humble, -0.1f },
                    { DefaultTraits.Generosity, -0.1f },
                    { DefaultTraits.Mercy, -0.1f }
                },
                new TextObject("{=J6KMLMX1}The {ATTACKER} is marching on {DEFENDER}! They claim to be the rightful heir of the Empire."));
        }
    }
}
