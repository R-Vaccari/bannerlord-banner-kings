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
            MBInformationManager.AddQuickInformation(new TextObject("{=5sWFJZV6}{COUNT} {OFFERING} was ritually offered by {HERO}.")
                    .SetTextVariable("HERO", actionTaker.Name),
                0, 
                actionTaker.CharacterObject,
                "event:/ui/notification/relation");
        }

        public override TextObject GetName() => new TextObject("{=!}Lann-Tairgseadh");
        public override TextObject GetDescription() => new TextObject("{=!}As a gift for our Fian ancestors, a great blade must be offered for them to use it in the afterlife. They ought to be thrown deep into the sacred lynns, the most sacred passageways between the world of men and the underworld.");

        public override void SetDialogue()
        {
            throw new NotImplementedException();
        }
    }
}
