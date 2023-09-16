using BannerKings.Extensions;
using BannerKings.Managers.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultContractAspects : DefaultTypeInitializer<DefaultContractAspects, ContractAspect>
    {
        public ContractAspect ConquestMight { get; } = new ContractRight("ConquestMight");
        public ContractAspect ConquestClaim { get; } = new ContractRight("ConquestClaim");
        public ContractAspect ConquestDistributed { get; } = new ContractRight("ConquestDistributed");

        public ContractAspect Enfoeffment { get; } = new ContractRight("Enfoeffment");


        public ContractDuty Geld { get; } = new ContractDuty("Geld");

        public override IEnumerable<ContractAspect> All
        {
            get
            {
                yield return Geld;
            }
        }

        public List<ContractAspect> GetIdealKingdomAspects(Kingdom kingdom, Government government)
        {
            List<ContractAspect> result = new List<ContractAspect>(4);
            result.Add(Geld);

            return result;
        }

        public override void Initialize()
        {
            Geld.Initialize(new TextObject("{=!}Geld"),
                new TextObject("{=!}The Geld is the traditional taxation form of the Wilunding. It is calculated on the assessment of productive land, which they divide in the so called Hides, and each of these hides is taxed a given amount of gold. While relatively simple to be calculated, the Geld completely ignores the productive value of the land, and thus can be unfairly assessed on a fief of particularly unproductive acreage or lacking in farmlands, inherently most productive acreage type. The geld may be levied up to 2 times a year, but it is not popular - each levy induces a diplomatic penalty."),
                new TextObject("{=!}Your suzerain, {SUZERAIN}, calls upon you to provide them the Geld."),
                new TextObject("{=!}"),
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
                });
        }
    }
}
