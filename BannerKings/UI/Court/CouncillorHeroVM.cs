using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Localization;

namespace BannerKings.UI.Court
{
    public class CouncillorHeroVM : HeroVM
    {
        private TextObject positionName;
        public CouncillorHeroVM(Hero hero, TextObject positionName) : base(hero)
        {
            this.positionName = positionName;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            if (Hero != null) NameText = new TextObject("{=!}{POSITION} {HERO}")
                    .SetTextVariable("POSITION", positionName)
                    .SetTextVariable("HERO", Hero.FirstName)
                    .ToString();
        }
    }
}
