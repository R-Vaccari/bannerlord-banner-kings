using BannerKings.Behaviours.Mercenary;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Mercenary
{
    internal class MercenaryPrivilegeVM : BannerKingsViewModel
    {
        public MercenaryPrivilegeVM(MercenaryPrivilege privilege) : base(null, true)
        {
            Privilege = privilege;
        }

        private MercenaryPrivilege Privilege { get; set; }

        [DataSourceProperty] public string Name => Privilege.Name.ToString();

        [DataSourceProperty]
        public string Description => new TextObject("{=!}Level {NUMBER}")
            .SetTextVariable("NUMBER", GetNumeral(Privilege.Level)).ToString();

        [DataSourceProperty] public HintViewModel Hint => new(Privilege.Description);

        private string GetNumeral(int level)
        {
            if (level == 4)
            {
                return "IV";
            }
            else if (level == 5)
            {
                return "V";
            }
            else if (level == 3)
            {
                return "III";
            }
            else if (level == 2)
            {
                return "II";
            }
            else if (level == 1)
            {
                return "I";
            }

            return "";
        }
    }
}
