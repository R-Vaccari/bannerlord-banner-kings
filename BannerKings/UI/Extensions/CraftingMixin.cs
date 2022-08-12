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
using BannerKings.UI.Crafting;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("UpdateCraftingStamina")]
    internal class CraftingMixin : BaseViewModelMixin<CraftingVM>
    {
		private CraftingVM crafting;
        private ArmorCraftingVM armorCrafting;
        private float startingStamina, spentStamina;
        private string hoursSpent;
        private HintViewModel craftingArmorHint;
        private bool isInArmorMode;


        public CraftingMixin(CraftingVM vm) : base(vm)
        {
			crafting = vm;
            startingStamina = 0f;
            spentStamina = 0f;
            craftingArmorHint = new HintViewModel();
            isInArmorMode = false;
            CraftingAvailableHeroItemVM heroVm = crafting.AvailableCharactersForSmithing.FirstOrDefault(x => x.Hero == Hero.MainHero);
            if (heroVm != null)
                startingStamina = heroVm.CurrentStamina;
            armorCrafting = new ArmorCraftingVM();
        }

        public override void OnRefresh()
        {

            if (crafting.IsInCraftingMode || crafting.IsInRefinementMode || crafting.IsInSmeltingMode)
                IsInArmorMode = false;

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

                TextObject botchText = new TextObject();

                HoursSpentText = new TextObject("{=!}Hours spent for all actions: {HOURS} hours.\n{BOTCH}")
                        .SetTextVariable("HOURS", GetSpentHours().ToString("0.0"))
                        .SetTextVariable("BOTCH", botchText)
                        .ToString();
            }
        }

        private float GetSpentHours() => spentStamina / 6f;

        [DataSourceMethod]
        public void ExecuteSwitchToArmor()
        {
            crafting.IsInSmeltingMode = false;
            crafting.IsInCraftingMode = false;
            crafting.IsInRefinementMode = false;
            IsInArmorMode = true;
            armorCrafting.RefreshValues();

            // int[] smithingCostsForWeaponDesign = Campaign.Current.Models.SmithingModel.GetSmithingCostsForWeaponDesign();
            // for (int l = 0; l < 9; l++)
            //    crafting.PlayerCurrentMaterials[l].ResourceChangeAmount = smithingCostsForWeaponDesign[l];

        }

        [DataSourceMethod]
        public void CloseWithWait()
        {
            crafting.ExecuteCancel();
            if (spentStamina != 0f) Campaign.Current.GetCampaignBehavior<BKSettlementActions>().StartCraftingMenu(GetSpentHours());
        }

        [DataSourceProperty]
        public string ArmorText => new TextObject("{=!}Forge Armor").ToString();
   

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

        [DataSourceProperty]
        public ArmorCraftingVM ArmorCrafting
        {
            get => armorCrafting;
            set
            {
                if (value != armorCrafting)
                {
                    armorCrafting = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "ArmorCrafting");
                }
            }
        }

        [DataSourceProperty]
        public bool IsInArmorMode
        {
            get => isInArmorMode;     
            set
            {
                if (value != isInArmorMode)
                {
                     isInArmorMode = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "IsInArmorMode");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel CraftingArmorHint
        {
            get => craftingArmorHint;
            set
            {
                if (value != craftingArmorHint)
                {
                    craftingArmorHint = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "CraftingArmorHint");
                }
            }
        }
    }
}
