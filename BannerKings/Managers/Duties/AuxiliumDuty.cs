using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Duties
{
    public class AuxiliumDuty : BannerKingsDuty
    {
        public AuxiliumDuty(CampaignTime dueTime, MobileParty leader, float completion, Settlement proximity,
            TextObject armyName) : base(dueTime, FeudalDuties.Auxilium, completion)
        {
            Party = leader;
            RunnedHours = 0;
            ArmyHours = 0;
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=c0yr2MTu}Duty of Military Aid").ToString(),
                new TextObject("{=tT53t7ny}Your suzerain, {SUZERAIN}, has summoned you to fulfill your oath of military aid. You have {DAYS} days to join {ARMY}, currently close to {SETTLEMENT}. Failing to fulfill it will tarnish your name and standing with your suzerain.")
                .SetTextVariable("SUZERAIN", leader.LeaderHero.Name)
                .SetTextVariable("DAYS", 2)
                .SetTextVariable("ARMY", armyName)
                .SetTextVariable("SETTLEMENT", proximity.Name)
                .ToString(),
                true, 
                false, 
                GameTexts.FindText("str_ok").ToString(), 
                null, null, 
                null),
                true);
        }

        [SaveableProperty(4)] public MobileParty Party { get; set; }

        [SaveableProperty(5)] private int RunnedHours { get; set; }

        [SaveableProperty(6)] private int ArmyHours { get; set; }

        public override void Finish()
        {
            var proportion = MathF.Clamp(ArmyHours / (float) RunnedHours, 0f, 1f);
            TextObject result = null;
            var suzerain = Party.Owner;
            if (suzerain == null)
            {
                suzerain = Clan.PlayerClan.Kingdom.Leader;
            }

            if (proportion < Completion)
            {
                Clan.PlayerClan.Renown -= 30f * (1f - proportion);
                var relation = MBRandom.RandomInt(-12, -5);
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, suzerain, relation, false);
                result = new TextObject("{=TkkCkCWZ}You have failed to fulfill your duty of military assistance to {SUZERAIN}. As a result, your clan's renown has suffered, and your relationship with suzerain has changed by {RELATION}.");
                result.SetTextVariable("SUZERAIN", suzerain.Name);
                result.SetTextVariable("RELATION", relation);
            }
            else
            {
                var influence = 15f;
                GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, influence);
                var relation = MBRandom.RandomInt(5, 10);
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, suzerain, relation, false);
                result = new TextObject("{=uPR7ENhu}{SUZERAIN} holds your duty of military aid fulfilled. You have gained {INFLUENCE}{INFLUENCE_ICON} and {RELATION} relation with your suzerain.");
                result.SetTextVariable("INFLUENCE", influence);
                result.SetTextVariable("SUZERAIN", suzerain.Name);
                result.SetTextVariable("RELATION", relation);
            }

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=c0yr2MTu}Duty of Military Aid").ToString(), 
                result.ToString(),
                true, 
                false,
                GameTexts.FindText("str_done").ToString(), 
                null, 
                null, 
                null), 
                true);
        }

        public override void Tick()
        {
            var remaining = DueTime.RemainingHoursFromNow;
            var army = Party.Army;
            if (army != null)
            {
                if (remaining <= 0f)
                {
                    RunnedHours++;
                    if (army.Parties.Contains(MobileParty.MainParty))
                    {
                        ArmyHours++;
                    }
                }
                else if (army.Parties.Contains(MobileParty.MainParty))
                {
                    DueTime = CampaignTime.Now;
                }
            }
        }
    }
}