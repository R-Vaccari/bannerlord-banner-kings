using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Localization;

namespace BannerKings.UI.Court
{
    public class CustomNameHeroVM : HeroVM
    {
        private readonly bool fullName;
        private readonly TextObject positionName;

        public CustomNameHeroVM(Hero hero, TextObject positionName, bool fullName = false) : base(hero)
        {
            this.positionName = positionName;
            this.fullName = fullName;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            if (Hero != null)
            {
                NameText = new TextObject("{=YhfiFgUn6}{POSITION_NAME} {HERO}")
                    .SetTextVariable("POSITION_NAME", positionName)
                    .SetTextVariable("HERO", fullName ? Hero.Name : Hero.FirstName)
                    .ToString();
            }
        }
    }
}