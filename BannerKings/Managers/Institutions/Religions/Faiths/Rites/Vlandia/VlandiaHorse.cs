using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Vlandia
{
    public class VlandiaHorse : Offering
    {
        public VlandiaHorse() : base(MBObjectManager.Instance.GetObject<ItemObject>(x => x.StringId == "t2_vlandia_horse"), 10)
        {
        }

        public override TextObject GetName() => new TextObject("{=QZonQk7x}Warhorse Sacrifice");

        public override void Execute(Hero executor)
        {
            base.Execute(executor);
        }

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=5ioijfGh}Will you offer these fine steeds to Wilund, the Smith?"));
        }
    }
}
