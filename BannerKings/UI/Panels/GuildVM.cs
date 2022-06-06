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
			this.guild = data.EconomicData.Guild;
			this.guildInfo = new MBBindingList<InformationElement>();
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			GuildInfo.Clear();
			GuildInfo.Add(new InformationElement("Capital:", this.guild.Capital.ToString(),
				"This guild's financial resources."));
			GuildInfo.Add(new InformationElement("Influence:", this.guild.Influence.ToString(),
				"The guild's grip on the settlement, composed by it's members and their power."));
			GuildInfo.Add(new InformationElement("Income:", this.guild.Income.ToString(),
				"Daily income of the guild, based on their influence over the settlement, it's mercantilism and autonomy."));
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
	}
}
