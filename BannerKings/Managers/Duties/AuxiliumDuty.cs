using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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

        public AuxiliumDuty(CampaignTime dueTime, MobileParty leader, float completion, Settlement proximity, TextObject armyName) : base(dueTime, FeudalDuties.Auxilium, completion)
        {
            this.Party = leader;
            RunnedHours = 0;
            ArmyHours = 0;
            InformationManager.ShowInquiry(new InquiryData("Duty Calls",
                    string.Format("Your suzerain, {0}, has summoned you to fulfill your oath of military aid. You have {1} days to join {2}, currently close to {3}.",
                    leader.LeaderHero.Name.ToString(), 2, armyName, proximity.Name),
                    true, false, "Understood", null, null, null), false);
        }

        public override void Finish()
        {
            float proportion = MathF.Clamp((float)ArmyHours / (float)RunnedHours, 0f, 1f);
            TextObject result = null;
            Hero suzerain = this.Party.Owner;
            if (suzerain == null)
                suzerain = Clan.PlayerClan.Kingdom.Leader;
            if (proportion < base.Completion)
            {
                Clan.PlayerClan.Renown -= 30f * (1f - proportion);
                int relation = MBRandom.RandomInt(-12, -5);
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, suzerain, relation, false);
                result = new TextObject("{=!}You have failed to fulfill your duty of military assistance to {SUZERAIN}. As a result, your clan's renown has suffered, and your relationship with suzerain has changed by {RELATION}.");
                result.SetTextVariable("SUZERAIN", suzerain.Name);
                result.SetTextVariable("RELATION", relation);
            } else
            {
                float influence = 15f;
                GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, influence);
                int relation = MBRandom.RandomInt(5, 10);
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, suzerain, relation, false);
                result = new TextObject("{=!}{SUZERAIN} holds your duty of military aid fulfilled. You have gained {INFLUENCE} and {RELATION} relatin with your suzerain.");
                result.SetTextVariable("INFLUENCE", influence);
                result.SetTextVariable("SUZERAIN", suzerain.Name);
                result.SetTextVariable("RELATION", relation);
            }

            InformationManager.ShowInquiry(new InquiryData("Duty of Military Aid", result.ToString(),
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
