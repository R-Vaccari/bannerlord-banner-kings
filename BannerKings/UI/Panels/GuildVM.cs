using BannerKings.Managers.Institutions;
using BannerKings.Populations;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.UI.Panels
{
    public class GuildVM : BannerKingsViewModel
    {
		private Guild guild;
		private MBBindingList<InformationElement> guildInfo;

		public GuildVM(PopulationData data) : base(data, true)
        {
			this.guild = data.EconomicData.Guild;
			this.guildInfo = new MBBindingList<InformationElement>();
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			GuildInfo.Clear();
			GuildInfo.Add(new InformationElement("Capital:", this.guild.Capital.ToString(),
				"This guild's financial resources"));
			GuildInfo.Add(new InformationElement("Influence:", this.guild.Influence.ToString(),
				"Soft power this guild has, allowing them to call in favors and make demands"));
		}

        [DataSourceProperty]
		public ImageIdentifierVM GuildMaster => new ImageIdentifierVM(CharacterCode.CreateFrom(this.guild.Leader.CharacterObject));

		[DataSourceProperty]
		public string GuildMasterName => "Guildmaster " + this.guild.Leader.Name;

		[DataSourceProperty]
		public MBBindingList<InformationElement> GuildInfo
		{
			get => guildInfo;
			set
			{
				if (value != guildInfo)
				{
					guildInfo = value;
					base.OnPropertyChangedWithValue(value, "GuildInfo");
				}
			}
		}

		public void ExecuteClose() => UIManager.Instance.CloseUI();
		
	}
}
