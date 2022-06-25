using BannerKings.Managers.Institutions.Guilds;
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
			guild = data.EconomicData.Guild;
			guildInfo = new MBBindingList<InformationElement>();
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			GuildInfo.Clear();
			GuildInfo.Add(new InformationElement("Capital:", guild.Capital.ToString(),
				"This guild's financial resources"));
			GuildInfo.Add(new InformationElement("Influence:", guild.Influence.ToString(),
				"Soft power this guild has, allowing them to call in favors and make demands"));
		}

        [DataSourceProperty]
		public ImageIdentifierVM GuildMaster => new ImageIdentifierVM(CharacterCode.CreateFrom(guild.Leader.CharacterObject));

		[DataSourceProperty]
		public string GuildMasterName => "Guildmaster " + guild.Leader.Name;

		[DataSourceProperty]
		public MBBindingList<InformationElement> GuildInfo
		{
			get => guildInfo;
			set
			{
				if (value != guildInfo)
				{
					guildInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		public void ExecuteClose() => UIManager.Instance.CloseUI();
		
	}
}
