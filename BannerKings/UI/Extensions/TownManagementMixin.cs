using BannerKings.Managers.Items;
using BannerKings.Managers.Populations;
using BannerKings.UI.TownManagement;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HarmonyLib;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshTownManagementStats")]
    public class TownManagementMixin : BaseViewModelMixin<TownManagementVM>
    {
        private readonly TownManagementVM townManagement;
        private MBBindingList<MaterialItemVM> materials;
        private bool missingPolicy, missingMaterials, missingGovernor;
        private PopulationData data;

        public TownManagementMixin(TownManagementVM vm) : base(vm)
        {
            townManagement = vm;
            materials = new MBBindingList<MaterialItemVM>();
            data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
        }

        [DataSourceProperty] public string ArmorText => new TextObject("{=h40bm0cG}Craft").ToString();
        [DataSourceProperty] public string MissingPolicyText => new TextObject("{=jg72kgKe}Missing construction policy!\nEdit your workforce policy under Demesne Management, Demesne tab.").ToString();
        [DataSourceProperty] public string MissingMaterialsText => new TextObject("{=mcyKmOiC}Missing materials!\nBring materials into the Stash or let caravans bring them naturally.").ToString();
        [DataSourceProperty] public string MissingGovernorText => new TextObject("{=CO7bKu1R}Missing Governor!\nA governor will buy necessary materials for projects without your intervention.").ToString();

        public override void OnRefresh()
        {
            Materials.Clear();
            MissingPolicy = !BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(data.Settlement, "workforce",
                (int)WorkforcePolicy.Construction);
            Dictionary<ItemObject, int> demands = new Dictionary<ItemObject, int>();

            bool materialsMissing = false;
            if (data.Settlement.Town.CurrentBuilding != null)
            {
                foreach (var requirement in BannerKingsConfig.Instance.ConstructionModel.GetMaterialRequirements(data.Settlement.Town.CurrentBuilding))
                {
                    demands.Add(requirement.Item1, requirement.Item2);
                    if (BannerKingsConfig.Instance.ConstructionModel.GetMaterialSupply(requirement.Item1, data.Settlement.Town) < requirement.Item2)
                    {
                        materialsMissing = true;
                    }
                }
            }

            MissingMaterials = materialsMissing;

            ItemObject clay = Campaign.Current.ObjectManager.GetObject<ItemObject>("clay");
            var settlement = data.Settlement;
            MissingGovernor = settlement.Town.Governor == null;

            Materials.Add(new MaterialItemVM(DefaultItems.HardWood, settlement, -demands.GetValueSafe(DefaultItems.HardWood)));
            Materials.Add(new MaterialItemVM(clay, settlement, -demands.GetValueSafe(clay)));
            Materials.Add(new MaterialItemVM(DefaultItems.IronOre, settlement, -demands.GetValueSafe(DefaultItems.IronOre)));
            Materials.Add(new MaterialItemVM(DefaultItems.Tools, settlement, -demands.GetValueSafe(DefaultItems.Tools)));
            Materials.Add(new MaterialItemVM(BKItems.Instance.Limestone, settlement, -demands.GetValueSafe(BKItems.Instance.Limestone)));
            Materials.Add(new MaterialItemVM(BKItems.Instance.Marble, settlement, -demands.GetValueSafe(BKItems.Instance.Marble)));
        }

        [DataSourceProperty]
        public MBBindingList<MaterialItemVM> Materials
        {
            get => materials;
            set
            {
                if (value != materials)
                {
                    materials = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "Materials");
                }
            }
        }

        [DataSourceProperty]
        public bool MissingGovernor
        {
            get => missingGovernor;
            set
            {
                if (value != missingGovernor)
                {
                    missingGovernor = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool MissingPolicy
        {
            get => missingPolicy;
            set
            {
                if (value != missingPolicy)
                {
                    missingPolicy = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool MissingMaterials
        {
            get => missingMaterials;
            set
            {
                if (value != missingMaterials)
                {
                    missingMaterials = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}