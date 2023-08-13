using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Southern
{
    public class Zabiha : Offering
    {
        public Zabiha() : base(MBObjectManager.Instance.GetObject<ItemObject>(x => x.StringId == "cow"), 20)
        {
        }

        public override TextObject GetName()
        {
            return new TextObject("{=iNbOvrB8}Zabiha");
        }

        public override void Execute(Hero executor)
        {
            base.Execute(executor);
        }

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=TmB613jp}Will you commit to the slaughter, as the Code and the Patriarch have taught our kin?"));
        }
    }
}