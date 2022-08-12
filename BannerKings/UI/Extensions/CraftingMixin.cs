using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Localization;
using System.Linq;
using TaleWorlds.CampaignSystem.ViewModelCollection.Craft;
using TaleWorlds.Core.ViewModelCollection;
using BannerKings.Behaviours;
using TaleWorlds.Library;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("UpdateCraftingStamina")]
    internal class CraftingMixin : BaseViewModelMixin<CraftingVM>
    {
		private CraftingVM crafting;
        private float startingStamina, spentStamina;
        private string hoursSpent;

		public CraftingMixin(CraftingVM vm) : base(vm)
        {
			crafting = vm;
            startingStamina = 0f;
            spentStamina = 0f;
            CraftingAvailableHeroItemVM heroVm = crafting.AvailableCharactersForSmithing.FirstOrDefault(x => x.Hero == Hero.MainHero);
            if (heroVm != null)
                startingStamina = heroVm.CurrentStamina;
            
        }

        public override void OnRefresh()
        {

            CraftingAvailableHeroItemVM heroVm = crafting.AvailableCharactersForSmithing.FirstOrDefault(x => x.Hero == Hero.MainHero);
            if (heroVm != null && heroVm.CurrentStamina != startingStamina)
                spentStamina = startingStamina - heroVm.CurrentStamina;

            if (crafting.IsMainActionEnabled)
            {
                /*float hours;

               if (crafting.IsInSmeltingMode)
                   hours = Campaign.Current.Models.SmithingModel.GetEnergyCostForSmelting(crafting.Smelting.CurrentSelectedItem.EquipmentElement.Item,
                       crafting.CurrentCraftingHero.Hero);
               else if (crafting.IsInRefinementMode)
               {
                   Crafting.RefiningFormula reference = crafting.Refinement.CurrentSelectedAction.RefineFormula;
                   Crafting.RefiningFormula formula = new Crafting.RefiningFormula(reference.Input1, reference.Input1Count, reference.Input2,
                       reference.Input2Count, reference.Output, reference.OutputCount, reference.Output2, reference.Output2Count);
                   hours = Campaign.Current.Models.SmithingModel.GetEnergyCostForRefining(ref formula, crafting.CurrentCraftingHero.Hero);
               }
               else
               {
                   CraftingState craftingState;
                   if ((craftingState = (GameStateManager.Current.ActiveState as CraftingState)) != null)
                   {
                       ItemObject currentCraftedItemObject = craftingState.CraftingLogic.GetCurrentCraftedItemObject(true, overrideData);
                       hours = Campaign.Current.Models.SmithingModel.GetEnergyCostForSmithing(currentCraftedItemObject, crafterHero);
                   }
               }*/

                HoursSpentText = new TextObject("{=!}Hours spent for all actions: {HOURS} hours.")
                        .SetTextVariable("HOURS", GetSpentHours().ToString("0.0"))
                        .ToString();
            }
        }

        private float GetSpentHours() => spentStamina / 5f;

        [DataSourceMethod]
        public void CloseWithWait()
        {
            crafting.ExecuteCancel();
            if (spentStamina != 0f) Campaign.Current.GetCampaignBehavior<BKSettlementActions>().StartCraftingMenu(GetSpentHours());
        }

        [DataSourceProperty]
        public string HoursSpentText
        {
            get => hoursSpent;
            set
            {
                if (value != hoursSpent)
                {
                    hoursSpent = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "HoursSpentText");
                }
            }
        }
    }
}
