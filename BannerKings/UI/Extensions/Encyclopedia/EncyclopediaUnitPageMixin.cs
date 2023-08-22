using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Library;
using TaleWorlds.Core.ViewModelCollection.Selector;
using BannerKings.Managers.Innovations.Eras;
using BannerKings.Managers.Innovations;
using TaleWorlds.CampaignSystem;
using System.Linq;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Localization;
using System.Runtime.InteropServices.ComTypes;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace BannerKings.UI.Extensions.Encyclopedia
{
    [ViewModelMixin("RefreshValues")]
    internal class EncyclopediaUnitPageMixin : BaseViewModelMixin<EncyclopediaUnitPageVM>
    {
        private SelectorVM<SelectorItemVM> eraSelection;
        private InnovationData data;

        public EncyclopediaUnitPageMixin(EncyclopediaUnitPageVM vm) : base(vm)
        {
            data = BannerKingsConfig.Instance.InnovationsManager
                .GetInnovationData((vm.Obj as CharacterObject).Culture);

            if (data != null)
            {
                EraSelection = new SelectorVM<SelectorItemVM>(0, OnSelected);
                int i = 0;
                int selected = 0;
                foreach (var era in DefaultEras.Instance.All)
                {
                    ;
                    EraSelection.AddItem(new SelectorItemVM(era.Name.ToString(),
                        era.Description));

                    if (era.Equals(data.Era)) selected = i;
                    i++;
                }

                EraSelection.SelectedIndex = selected;
            }
        }

        private void OnSelected(SelectorVM<SelectorItemVM> s)
        {
            int index = s.SelectedIndex;
            SetTroopEra(DefaultEras.Instance.All.ElementAt(index));
        }

        private void SetTroopEra(Era era)
        {
            BKTroopAdvancement adv = era.GetTroopAdvancement(ViewModel.Obj as CharacterObject);
            if (adv != null && adv.UpgradeEquipment != null)
            {
                SetEquipment(adv.UpgradeEquipment.AllEquipments);
            }
            else if (era.Equals(DefaultEras.Instance.FirstEra))
            {
                SetEquipment((ViewModel.Obj as CharacterObject).AllEquipments);
            }
        }

        private void SetEquipment(List<Equipment> equipments)
        {
            ViewModel.EquipmentSetSelector.ItemList.Clear();
            foreach (var equipment in equipments)
            {
                if (!equipment.IsCivilian)
                {
                    ViewModel.EquipmentSetSelector.AddItem(new EncyclopediaUnitEquipmentSetSelectorItemVM(equipment, ""));
                }
            }

            ViewModel.EquipmentSetSelector.SelectedIndex = 0;
            ViewModel.EquipmentSetText = new TextObject("{=vggt7exj}Set {CURINDEX}/{COUNT}", null)
                .SetTextVariable("COUNT", ViewModel.EquipmentSetSelector.ItemList.Count)
                .SetTextVariable("CURINDEX", 1)
                .ToString();

            ViewModel.UnitCharacter.SetEquipment(ViewModel.EquipmentSetSelector.ItemList.First().EquipmentSet);
            ViewModel.CurrentSelectedEquipmentSet = ViewModel.EquipmentSetSelector.SelectedItem;
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> EraSelection
        {
            get => eraSelection;
            set
            {
                if (value != eraSelection)
                {
                    eraSelection = value;
                    ViewModel?.OnPropertyChangedWithValue(value, "EraSelection");
                }
            }
        }
    }
}