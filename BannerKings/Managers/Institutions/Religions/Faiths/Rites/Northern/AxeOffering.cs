using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Northern
{
    public class AxeOffering : WeaponOffering
    {
        public AxeOffering() : base(WeaponClass.TwoHandedAxe, ItemObject.ItemTiers.Tier5)
        {
        }

        public override void Complete(Hero actionTaker)
        {
            base.Complete(actionTaker);
            MBInformationManager.AddQuickInformation(new TextObject("{=!}The {ITEM} was offered to Pérkos, Thunder Wielder.")
                    .SetTextVariable("ITEM", base.selectedItem.GetModifiedItemName()),
                0,
                actionTaker.CharacterObject,
                "event:/ui/notification/relation");
        }

        public override TextObject GetDescription() => new TextObject("{=!}Other than his Thunder, the almighty God of the holy canopy is known to use his axe to defend the realm of mankind. A votive offering must be offered in his name to show our good faith.");

        public override TextObject GetName() => new TextObject("{=!}Axe of Pérkos");

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=!}{?PLAYER.GENDER}My lady{?}My lord{\\?}, the spirits teach us a blade is a virtuous offering for the Gods. The best its craftsmanship is, the more it pleases the Gods, as only the best man can craft is worthy of them."));
        }
    }
}
