using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Battania
{
    public class LanceOffering : WeaponOffering
    {
        public LanceOffering() : base(WeaponClass.TwoHandedPolearm, ItemObject.ItemTiers.Tier5)
        {
        }

        public override void Complete(Hero actionTaker)
        {
            base.Complete(actionTaker);
            MBInformationManager.AddQuickInformation(new TextObject("{=0dLoO9An}The {ITEM} was buried in a hallowed mound for the gods.")
                    .SetTextVariable("ITEM", base.selectedItem.GetModifiedItemName()),
                0, 
                actionTaker.CharacterObject,
                "event:/ui/notification/relation");
        }

        public override TextObject GetName() => new TextObject("{=vrORZVsM}Lance Offering");
        public override TextObject GetDescription() => new TextObject("{=xWMAda9w}An offering to Horsa, the prophet Horse-God, to honor him for giving us the spirit to carve our kingdom.");

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=FE5xFScd}{?PLAYER.GENDER}My lady{?}My lord{\\?}, a high quality is expected, so it may be worthy of your ancestors. Horsa came to this land and planted his spear to claim it. Now we give back to him, faithfully. Will you offer the {BLADE}?")
                .SetTextVariable("BLADE", selectedItem.GetModifiedItemName()));
        }
    }
}
