using BannerKings.Extensions;
using BannerKings.Managers.Populations;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultContractAspects : DefaultTypeInitializer<DefaultContractAspects, ContractAspect>
    {
        public ContractRight ConquestMight { get; } = new ContractRight("ConquestMight");
        public ContractRight ConquestClaim { get; } = new ContractRight("ConquestClaim");
        public ContractRight ConquestDistributed { get; } = new ContractRight("ConquestDistributed");
        public ContractRight Enfeoffment { get; } = new ContractRight("Enfeoffment");

        public ContractDuty Geld { get; } = new ContractDuty("Geld");
        public ContractDuty FeudalTax { get; } = new ContractDuty("FeudalTax");
        public ContractDuty TribalTax { get; } = new ContractDuty("TribalTax");
        public ContractDuty ImperialTax { get; } = new ContractDuty("ImperialTax");

        public override IEnumerable<ContractAspect> All
        {
            get
            {
                yield return Geld;
                yield return FeudalTax;
                yield return TribalTax;
                yield return ImperialTax;
                yield return ConquestClaim;
                yield return ConquestDistributed;
                yield return ConquestMight;
                yield return Enfeoffment;
            }
        }

        public List<ContractAspect> GetIdealKingdomAspects(string id, Government government)
        {
            List<ContractAspect> result = new List<ContractAspect>(4);
            if (id == BannerKingsConfig.VlandiaCulture)
            {
                result.Add(Geld);
            }
            else
            {
                if (government == DefaultGovernments.Instance.Feudal)
                {
                    result.Add(FeudalTax);
                }
                else if (government == DefaultGovernments.Instance.Imperial || government == DefaultGovernments.Instance.Republic)
                {
                    result.Add(ImperialTax);
                }
                else
                {
                    result.Add(TribalTax);
                }
            }

            if (government == DefaultGovernments.Instance.Feudal)
            {
                result.Add(ConquestClaim);
            }
            else if (government == DefaultGovernments.Instance.Imperial || government == DefaultGovernments.Instance.Republic)
            {
                result.Add(ConquestDistributed);
            }
            else
            {
                result.Add(ConquestMight);
            }

            return result;
        }

        public override void Initialize()
        {
            ConquestDistributed.Initialize(new TextObject("{=RYmV2PEY}Distributed Conquest"),
                new TextObject("{=!}An Imperial invention, made to counter the 'barbaric' idea of conquest through might, which incites the creation of despots and dictators. It is up to the state to lawfully distribute dominions and thus diffuse power."),
                new TextObject("{=!}Ability to directly petition a fief as a fiefless peer, and your suzerain has 2 or more fiefs{newline}Ownership votes favour clans with less properties"),
                true,
                150,
                ContractAspect.AspectTypes.Conquest,
                (ContractRight right, Hero suzerain, Hero vassal) =>
                {
                    Town town = suzerain.Clan.Fiefs.First(x =>
                    {
                        Town capital = BannerKingsConfig.Instance.CourtManager.GetCouncil(suzerain.Clan).Location;
                        return x != capital;
                    });
                    ChangeOwnerOfSettlementAction.ApplyByKingDecision(vassal, town.Settlement);
                },
                (ContractRight right, Hero suzerain, Hero vassal) =>
                {
                    return suzerain.Clan.Fiefs.Count > 1 && vassal.Clan.Fiefs.Count == 0;
                });

            ConquestClaim.Initialize(new TextObject("{=kyB8tkgY}Conquest by Claim"),
                new TextObject("{=!}A 'sophisticated' view on ownership, based on legal frameworks. One that holds a lawful claim to a domain should therefore be awarded its control, regardless of other factors."),
                new TextObject("{=!}Ability to directly petition a fief as holder of its title{newline}Title claimants of the fief are stronger candidates on ownership votes"),
                true,
                150,
                ContractAspect.AspectTypes.Conquest,
                (ContractRight right, Hero suzerain, Hero vassal) =>
                {
                    Town town = suzerain.Clan.Fiefs.First(x =>
                    {
                        bool claim = false;
                        FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(x.Settlement);
                        if (title.deJure == vassal) claim = true;

                        return claim;
                    });

                    ChangeOwnerOfSettlementAction.ApplyByKingDecision(vassal, town.Settlement);
                },
                (ContractRight right, Hero suzerain, Hero vassal) =>
                {
                    return suzerain.Clan.Fiefs.Any(x =>
                    {
                        bool claim = false;
                        FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(x.Settlement);
                        if (title.deJure == vassal) claim = true;

                        return claim;
                    });
                });

            ConquestMight.Initialize(new TextObject("{=kyB8tkgY}Conquest by Might"),
                new TextObject("{=!}A long tradition within Calradia. To conquer through the sword makes the bounty rightfully yours."),
                new TextObject("{=!}Ability to directly petition a fief you conquered{newline}Settlement conquerors are stronger candidates on ownership votes"),
                true,
                150,
                ContractAspect.AspectTypes.Conquest,
                (ContractRight right, Hero suzerain, Hero vassal) =>
                {
                    Town town = suzerain.Clan.Fiefs.First(x => x.LastCapturedBy == vassal.Clan);
                    ChangeOwnerOfSettlementAction.ApplyByKingDecision(vassal, town.Settlement);
                },
                (ContractRight right, Hero suzerain, Hero vassal) =>
                {
                    return suzerain.Clan.Fiefs.Any(x => x.LastCapturedBy == vassal.Clan);
                });


            Geld.Initialize(new TextObject("{=ROqmFxKG}Geld"),
                new TextObject("{=ju3k6y7Y}The Geld is the traditional taxation form of the Wilunding. It is calculated on the assessment of productive land, which they divide in the so called Hides, and each of these hides is taxed a given amount of gold. While relatively simple to be calculated, the Geld completely ignores the productive value of the land, and thus can be unfairly assessed on a fief of particularly unproductive acreage or lacking in farmlands, inherently most productive acreage type. The geld may be levied up to 2 times a year, but it is not popular - each levy induces a diplomatic penalty."),
                new TextObject("{=!}Your suzerain, {SUZERAIN}, calls upon you to provide them the Geld. They are entitled to {RESULT} in payment."),
                new TextObject("{=tHsiBHVr}{VASSAL} will pay you {RESULTS}{GOLD_ICON}"),
                2,
                25,
                ContractAspect.AspectTypes.Taxes,
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    GiveGoldAction.ApplyBetweenCharacters(vassal,
                        suzerain,
                        duty.CalculateDuty(suzerain, vassal));
                },
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    return vassal.Gold >= duty.CalculateDuty(suzerain, vassal);
                },
                (Hero suzerain, Hero vassal) =>
                {
                    float acres = 0f;
                    foreach (var fief in vassal.Clan.Settlements)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(fief);
                        if (fief.IsVillage && fief.Village.GetActualOwner() != vassal) continue;

                        acres += data.LandData.Acreage;
                    }

                    return MBRandom.RoundRandomized(acres / 5f);
                },
                (Kingdom kingdom) => kingdom.Culture.StringId == "vlandia");

            FeudalTax.Initialize(new TextObject("{=zEjkMpcb}Feudal Tax"),
                new TextObject("{=1eKZHKW7}The feudal tax is the standard taxation form of feudal realms. This tax is calculated on each fief's revenue times a given amount of days. Whenever a fief is legally held by the taxed lord, its tax contribution is reduced. The tax may be levied once a year, but it is not popular - each levy induces a diplomatic penalty."),
                new TextObject("{=ARCne9A7}Your suzerain, {SUZERAIN}, calls upon you to provide them the feudal tax. They are entitled to {RESULT} in payment."),
                new TextObject("{=tHsiBHVr}{VASSAL} will pay you {RESULTS}{GOLD_ICON}"),
                2,
                25,
                ContractAspect.AspectTypes.Taxes,
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    GiveGoldAction.ApplyBetweenCharacters(vassal,
                        suzerain,
                        duty.CalculateDuty(suzerain, vassal));
                },
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    return vassal.Gold >= duty.CalculateDuty(suzerain, vassal);
                },
                (Hero suzerain, Hero vassal) =>
                {
                    float acres = 0f;
                    foreach (var fief in vassal.Clan.Settlements)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(fief);
                        if (fief.IsVillage && fief.Village.GetActualOwner() != vassal) continue;

                        acres += data.LandData.Acreage;
                    }

                    return MBRandom.RoundRandomized(acres / 5f);
                },
                (Kingdom kingdom) => 
                {
                    bool result = false;
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null && title.Contract.Government == DefaultGovernments.Instance.Feudal)
                        result = true;

                    return result;
                });

            TribalTax.Initialize(new TextObject("{=LW28FkEb}Tribal Tax"),
                new TextObject("{=ZMw1OXMF}The tribal tax is a simple taxation form that charges a given amount of gold per each settlement. The tribal tax may be levied once a year, but it is not popular - each levy induces a diplomatic penalty."),
                new TextObject("{=uYdiJpNj}Your suzerain, {SUZERAIN}, calls upon you to provide them the tribal tax. They are entitled to {RESULT} in payment."),
                new TextObject("{=tHsiBHVr}{VASSAL} will pay you {RESULTS}{GOLD_ICON}"),
                1,
                20,
                ContractAspect.AspectTypes.Taxes,
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    GiveGoldAction.ApplyBetweenCharacters(vassal,
                        suzerain,
                        duty.CalculateDuty(suzerain, vassal));
                },
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    return vassal.Gold >= duty.CalculateDuty(suzerain, vassal);
                },
                (Hero suzerain, Hero vassal) =>
                {
                    float result = 0f;
                    foreach (var fief in vassal.Clan.Settlements)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(fief);
                        if (fief.IsVillage && fief.Village.GetActualOwner() != vassal) continue;

                        if (fief.IsTown) result += 10000f;
                        else if (fief.IsCastle) result += 5000f;
                        else result += 1500f;
                    }

                    return MBRandom.RoundRandomized(result);
                },
                (Kingdom kingdom) =>
                {
                    bool result = false;
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null && title.Contract.Government == DefaultGovernments.Instance.Tribal)
                        result = true;

                    return result;
                });

            ImperialTax.Initialize(new TextObject("{=vuiuFsMz}Imperial Tax"),
                new TextObject("{=Ena0wdVR}The imperial tax is calculated on the assessment of productive land, taxed differently based on their quality, namely if they are farmland, pastureland and woodland. The tax may be levied up to 2 times a year, but it is not popular - each levy induces a diplomatic penalty."),
                new TextObject("{=Zt95Yeto}Your suzerain, {SUZERAIN}, calls upon you to provide them the Imperial tax. They are entitled to {RESULT} in payment."),
                new TextObject("{=tHsiBHVr}{VASSAL} will pay you {RESULTS}{GOLD_ICON}"),
                2,
                10,
                ContractAspect.AspectTypes.Taxes,
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    GiveGoldAction.ApplyBetweenCharacters(vassal,
                        suzerain,
                        duty.CalculateDuty(suzerain, vassal));
                },
                (ContractDuty duty, Hero suzerain, Hero vassal) =>
                {
                    return vassal.Gold >= duty.CalculateDuty(suzerain, vassal);
                },
                (Hero suzerain, Hero vassal) =>
                {
                    float result = 0f;
                    foreach (var fief in vassal.Clan.Settlements)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(fief);
                        if (fief.IsVillage && fief.Village.GetActualOwner() != vassal) continue;

                        result += data.LandData.Farmland / 2f;
                        result += data.LandData.Pastureland / 4f;
                        result += data.LandData.Woodland / 10f;
                    }

                    return MBRandom.RoundRandomized(result);
                },
                (Kingdom kingdom) =>
                {
                    bool result = false;
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null && (title.Contract.Government == DefaultGovernments.Instance.Imperial ||
                    title.Contract.Government == DefaultGovernments.Instance.Republic))
                        result = true;

                    return result;
                });
        }
    }
}
