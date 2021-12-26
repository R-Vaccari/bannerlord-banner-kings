
using Populations.Managers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using static Populations.Managers.TitleManager;

namespace Populations
{
    public class TitleConfig
    {

        public TitleManager TitleManager;

        public void InitManagers(HashSet<FeudalTitle> titles, Dictionary<Hero, HashSet<FeudalTitle>> titleHolders)
        {
            this.TitleManager = new TitleManager(titles, titleHolders);
        }

        public void InitManagers(TitleManager titleManager)
        {
            this.TitleManager = titleManager;
        }

        public static TitleConfig Instance
        {
            get => ConfigHolder.CONFIG;
        }
            

        internal struct ConfigHolder
        {
             public static TitleConfig CONFIG = new TitleConfig();
        }
    }
}
