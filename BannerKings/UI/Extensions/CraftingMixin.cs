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
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("UpdateCraftingStamina")]
    public class CraftingMixin : BaseViewModelMixin<CraftingVM>
    {
		private CraftingVM crafting;
        private ArmorCraftingVM armorCrafting;
        private MBBindingList<CraftingResourceItemVM> extraMaterials;
        private float startingStamina, spentStamina;
        private string hoursSpent;
        private HintViewModel craftingArmorHint;
        private bool isInArmorMode;


        public CraftingMixin(CraftingVM vm) : base(vm)
        {
			crafting = vm;
            extraMaterials = new MBBindingList<CraftingResourceItemVM>();
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

        [DataSourceMethod]
        public void ExecuteMainActionBK()
        {
            if (!IsInArmorMode) crafting.ExecuteMainAction();
            else
            {
                SpendMaterials();
                ItemObject item = armorCrafting.CurrentItem.Item;
                EquipmentElement element = new EquipmentElement(item);

                TextObject qualityText = new TextObject();

                if ((item.HasWeaponComponent && item.WeaponComponent.ItemModifierGroup != null) ||
                    (item.HasArmorComponent && item.ArmorComponent.ItemModifierGroup != null))
                {
                    int quality = BannerKingsConfig.Instance.SmithingModel.GetModifierForCraftedItem(item);
                    ItemModifierGroup modifierGroup;
                    if (item.HasWeaponComponent) modifierGroup = item.WeaponComponent.ItemModifierGroup;
                    else modifierGroup = item.ArmorComponent.ItemModifierGroup;

                    ItemModifier modifier = modifierGroup.GetRandomModifierWithTarget(quality);
                    if (modifier != null)
                    {
                        qualityText = new TextObject("{=!} with {QUALITY} quality").SetTextVariable("QUALITY", modifier.Name);
                        element.SetModifier(modifier);
                    }
                }

                InformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has crafted {ITEM}{QUALITY}.")
                    .SetTextVariable("HERO", crafting.CurrentCraftingHero.Hero.Name)
                    .SetTextVariable("ITEM", item.Name)
                    .SetTextVariable("QUALITY", qualityText), 
                    0, null, "event:/ui/notification/relation");
                PartyBase.MainParty.ItemRoster.AddToCounts(element, 1);

                crafting.CurrentCraftingHero.Hero.AddSkillXp(DefaultSkills.Crafting,
                    BannerKingsConfig.Instance.SmithingModel.GetSkillXpForSmithingInFreeBuildMode(item));
                Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>()
                    .SetHeroCraftingStamina(crafting.CurrentCraftingHero.Hero, CurrentEnergy);

                OnRefresh();
            }  
        }

        private void SpendMaterials()
        {
            MBReadOnlyList<ItemObject> items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
            int[] materials = BannerKingsConfig.Instance.SmithingModel.GetCraftingInputForArmor(armorCrafting.CurrentItem.Item);
            for (int l = 0; l < 11; l++)
            {
                if (materials[l] == 0) continue;

                ItemObject item;
                if (l < 9) item = BannerKingsConfig.Instance.SmithingModel.GetCraftingMaterialItem((CraftingMaterials)l);
                else item = items.First(x => x.StringId == (l == 9 ? "leather" : "linen"));

                MobileParty.MainParty.ItemRoster.AddToCounts(item, -materials[l]);
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
            bool ingots = !crafting.PlayerCurrentMaterials.Any((CraftingResourceItemVM m) => m.ResourceChangeAmount + m.ResourceAmount < 0);
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
        

        private int[] CurrentMaterials => BannerKingsConfig.Instance.SmithingModel.GetCraftingInputForArmor(armorCrafting.CurrentItem.Item);
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
        public string ArmorText => new TextObject("{=!}Craft").ToString();
   

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
        public MBBindingList<CraftingResourceItemVM> CurrentExtraMaterials
        {
            get
            {
                return this._playerCurrentMaterials;
            }
            set
            {
                if (value != this._playerCurrentMaterials)
                {
                    this._playerCurrentMaterials = value;
                    base.OnPropertyChangedWithValue(value, "CurrentExtraMaterials");
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
