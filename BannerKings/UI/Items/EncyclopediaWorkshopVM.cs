using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class EncyclopediaWorkshopVM : HeroVM
    {
        private string workshopTypeId, name;
        private BasicTooltipViewModel hint;
        private Workshop workshop;

        public EncyclopediaWorkshopVM(Workshop workshop) : base(workshop.Owner, true)
        {
            this.workshop = workshop;
            WorkshopTypeId = UIHelper.GetWorkshopIconText(workshop.WorkshopType.StringId);
            NameText = workshop.Name.ToString();
            Hint = new BasicTooltipViewModel(() => UIHelper.GetEncyclopediaWorkshopTooltip(workshop));
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
           
        }

        [DataSourceProperty]
        public string NameText
        {
            get => name;
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChangedWithValue(value, "NameText");
                }
            }
        }

        [DataSourceProperty]
        public BasicTooltipViewModel Hint
        {
            get => hint;
            set
            {
                if (value != hint)
                {
                    hint = value;
                    OnPropertyChangedWithValue(value, "Hint");
                }
            }
        }

        [DataSourceProperty]
        public string WorkshopTypeId
        {
            get => workshopTypeId;   
            set
            {
                if (value != workshopTypeId)
                {
                    workshopTypeId = value;
                    OnPropertyChangedWithValue(value, "WorkshopTypeId");
                }
            }
        }
    }
}
