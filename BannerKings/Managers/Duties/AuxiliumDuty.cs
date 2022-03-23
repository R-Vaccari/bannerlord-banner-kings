using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Duties
{
    public class AuxiliumDuty : BannerKingsDuty
    {
        [SaveableProperty(4)]
        public MobileParty Party { get; private set; }

        [SaveableProperty(5)]
        private int RunnedHours { get; set; }

        [SaveableProperty(6)]
        private int ArmyHours { get; set; }

        public AuxiliumDuty(CampaignTime dueTime, MobileParty leader, float completion) : base(dueTime, TitleManager.FeudalDuties.Auxilium, completion)
        {
            this.Party = leader;
            RunnedHours = 0;
            ArmyHours = 0;
        }

        public override void Finish()
        {
            float proportion = (float)ArmyHours / (float)RunnedHours;
            string result = null;
            if (proportion < base.Completion)
            {
                Clan.PlayerClan.Renown -= 50f * (1f - proportion);
                ChangeRelationAction.ApplyPlayerRelation(this.Party.LeaderHero, (int)((float)MBRandom.RandomInt(-5, -12) * (1f - proportion)), false, false);
                result = string.Format("You have failed to fulfill your duty if military assistance to {0}. As a result, your clan's reputation has suffered, and your liege is unsatisfied.", this.Party.LeaderHero.Name);
            } else
            {
                float influence = 15f;
                GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, influence);
                ChangeRelationAction.ApplyPlayerRelation(this.Party.LeaderHero, MBRandom.RandomInt(5, 10), false, false);
                result = string.Format("{0} holds your duty of military aid fulfilled. You have gained {1} influence, and your liege has more positive view on you.", this.Party.LeaderHero.Name, influence);
            }

            InformationManager.ShowInquiry(new InquiryData("Duty of Military Aid", result,
                    true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
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
