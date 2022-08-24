using BannerKings.Behaviours;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Duties
{
    public class RansomDuty : BannerKingsDuty
    {
        public RansomDuty(CampaignTime dueTime, Hero owner, float completion) : base(dueTime, FeudalDuties.Ransom,
            completion)
        {
            DebtOwner = owner;
        }

        [SaveableProperty(4)] public Hero DebtOwner { get; set; }

        public override void Finish()
        {
            string result = null;
            if (Hero.MainHero.Gold >= Completion)
            {
                GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, DebtOwner, (int) Completion);
                result = new TextObject(
                        "{SUZERAIN} holds your oath of ransom aid fulfilled. You have payed {RANSOM} gold and your liege is satisfied.")
                    .ToString();
            }
            else
            {
                result = new TextObject(
                        "You have failed to fulfill your duty of ransom assistance to {SUZERAIN}. As a result, your clan's reputation has suffered, and your liege is unsatisfied.")
                    .ToString();
            }

            BKRansomBehavior.playerRansomDuty = null;
            InformationManager.ShowInquiry(new InquiryData("Duty of Ransom Aid", result,
                true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
        }

        public override void Tick()
        {
            GameTexts.SetVariable("SUZERAIN", DebtOwner.Name);
            GameTexts.SetVariable("RANSOM", Completion);
            var remaining = DueTime.RemainingDaysFromNow;
            if (remaining > 0)
            {
                Finish();
            }

            GameTexts.SetVariable("REMAINING", Completion);
            InformationManager.ShowInquiry(new InquiryData("Duty of Ransom Aid",
                new TextObject(
                        "{=!} Your suzerain, {SUZERAIN}, has requested that you fulfill your contract obligations and pay him {RANSOM} gold in order to compensate their ransom. You have {REMAINING} days left to pay it.")
                    .ToString(),
                true, true, "Pay Immediatly", "Withhold For a Day", delegate { Finish(); }, null));
        }
    }
}