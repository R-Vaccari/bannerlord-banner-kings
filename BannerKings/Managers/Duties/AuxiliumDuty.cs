using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Managers.Duties
{
    public class AuxiliumDuty : BannerKingsDuty
    {
        public MobileParty Party { get; private set; }
        private int RunnedHours { get; set; }
        private int ArmyHours { get; set; }

        public AuxiliumDuty(CampaignTime dueTime, MobileParty leader) : base(dueTime, TitleManager.FeudalDuties.Auxilium)
        {
            this.Party = leader;
            RunnedHours = 0;
            ArmyHours = 0;
        }

        public override void Finish()
        {
            float proportion = (float)ArmyHours / (float)RunnedHours;
            string result = null;
            if (proportion < 0.8f)
            {
                Clan.PlayerClan.Renown -= 50f;
                result = string.Format("You have failed to fulfill your duty if military assistance to {0}. As a result, your clan's reputation has suffered.", this.Party.LeaderHero.Name);
            } else
            {
                result = string.Format("{0} holds your duty of military aid fulfilled.", this.Party.LeaderHero.Name);
            }

            InformationManager.ShowInquiry(new InquiryData("Duty of Military Aid", result,
                    true, false, "Understood", null, null, null), false);
        }

        public override void Tick()
        {
            float remaining = base.DueTime.RemainingHoursFromNow;
            Army army = Party.Army;
            if (army != null) 
            {
                if (remaining <= 0f)
                {
                    RunnedHours++;
                    if (army.Parties.Contains(MobileParty.MainParty))
                        ArmyHours++;
                }
                else if (army.Parties.Contains(MobileParty.MainParty))
                    base.DueTime = CampaignTime.Now;
            }
        }
    }
}
