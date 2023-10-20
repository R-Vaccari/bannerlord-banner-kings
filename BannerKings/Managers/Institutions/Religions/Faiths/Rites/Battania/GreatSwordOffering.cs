using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites.Battania
{
    public class GreatSwordOffering : WeaponOffering
    {
        public GreatSwordOffering() : base(WeaponClass.TwoHandedSword, ItemObject.ItemTiers.Tier5)
        {
        }

        public override void Complete(Hero actionTaker)
        {
            base.Complete(actionTaker);
            MBInformationManager.AddQuickInformation(new TextObject("{=1kTYb7pF}The {ITEM} was sacrificed in a sacred lynn!")
                    .SetTextVariable("ITEM", base.selectedItem.GetModifiedItemName()),
                0, 
                actionTaker.CharacterObject,
                "event:/ui/notification/relation");
        }

        public override TextObject GetName() => new TextObject("{=GtRd2Arh}Lann-Tairgseadh");
        public override TextObject GetDescription() => new TextObject("{=ZS2v37LF}As a gift for our Fian ancestors, a great blade must be offered for them to use it in the afterlife. They ought to be thrown deep into the sacred lynns, the most sacred passageways between the world of men and the underworld.");

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=hpz9UVs4}{?PLAYER.GENDER}My lady{?}My lord{\\?}, a blade sacrifice would certainly please the spirits, aye. A high quality is expected, so it may be worthy of your ancestors. Will you offer the {BLADE}?")
                .SetTextVariable("BLADE", selectedItem.GetModifiedItemName()));
        }
    }
}
