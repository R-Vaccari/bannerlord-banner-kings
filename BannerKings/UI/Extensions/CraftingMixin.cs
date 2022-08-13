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
using TaleWorlds.Core;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("UpdateCraftingStamina")]
    public class CraftingMixin : BaseViewModelMixin<CraftingVM>
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
            armorCrafting = new ArmorCraftingVM(this);
        }

        public override void OnRefresh()
        {

            if (crafting.IsInCraftingMode || crafting.IsInRefinementMode || crafting.IsInSmeltingMode)
                IsInArmorMode = false;

            UpdateMainAction();

            CraftingAvailableHeroItemVM heroVm = crafting.AvailableCharactersForSmithing.FirstOrDefault(x => x.Hero == Hero.MainHero);
            if (heroVm != null && heroVm.CurrentStamina != startingStamina)
                spentStamina = startingStamina - heroVm.CurrentStamina;

            if (crafting.IsMainActionEnabled)
            {

                HoursSpentText = new TextObject("{=!}Hours spent for all actions: {HOURS} hours.")
                        .SetTextVariable("HOURS", GetSpentHours().ToString("0.0"))
                        .ToString();

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
            }
        }

        public void UpdateMainAction()
        {
            if (IsInArmorMode)
            {
                UpdateMaterials();

                if (!HasEnergy())
                {
                    crafting.IsMainActionEnabled = false;
                    if (crafting.MainActionHint != null)
                        crafting.MainActionHint = new BasicTooltipViewModel(() => new TextObject("{=PRE5RKpp}You must rest and spend time before you can do this action.", null).ToString());
                }
                else if (!HasMaterials())
                {
                    crafting.IsMainActionEnabled = false;
                    if (crafting.MainActionHint != null)
                        crafting.MainActionHint = new BasicTooltipViewModel(() => new TextObject("{=gduqxfck}You don't have all required materials!", null).ToString());

                }
                else crafting.IsMainActionEnabled = true;
            }
        }

        public Hero Hero => crafting.CurrentCraftingHero.Hero;

        private float GetSpentHours() => spentStamina / 6f;

        public void UpdateMaterials()
        {
            int[] materials = CurrentMaterials;
            for (int l = 0; l < 9; l++) crafting.PlayerCurrentMaterials[l].ResourceChangeAmount = -materials[l];
        }

        public bool HasEnergy() => crafting.CurrentCraftingHero.CurrentStamina >= CurrentEnergy;
        public bool HasMaterials()
        {
            bool ingots = crafting.PlayerCurrentMaterials.Any((CraftingResourceItemVM m) => m.ResourceChangeAmount + m.ResourceAmount < 0);
            bool extraMaterials = false;
            if (ingots)
            {
                int[] materials = CurrentMaterials;
                MBReadOnlyList<ItemObject> items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
                if (materials[9] > 0) extraMaterials = PartyBase.MainParty.ItemRoster.GetItemNumber(items.First(x => x.StringId == "leather")) > materials[9];
                if (materials[10] > 0) extraMaterials = PartyBase.MainParty.ItemRoster.GetItemNumber(items.First(x => x.StringId == "linen")) > materials[10];
            }

            return ingots && extraMaterials;
        }
        

        private int[] CurrentMaterials => BannerKingsConfig.Instance.SmithingModel.GetSmeltingOutputForArmor(armorCrafting.CurrentItem.Item);
        private int CurrentEnergy => BannerKingsConfig.Instance.SmithingModel.CalculateArmorStamina(armorCrafting.CurrentItem.Item);

        [DataSourceMethod]
        public void ExecuteSwitchToArmor()
        {
            crafting.IsInSmeltingMode = false;
            crafting.IsInCraftingMode = false;
            crafting.IsInRefinementMode = false;
            IsInArmorMode = true;
            armorCrafting.RefreshValues();

            CraftingVM.OnItemRefreshedDelegate onItemRefreshed = crafting.OnItemRefreshed;
            if (onItemRefreshed != null)
                onItemRefreshed(false);
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
