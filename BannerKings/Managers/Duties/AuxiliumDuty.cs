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
            InformationManager.ShowInquiry(new InquiryData("Duty Calls",
                string.Format(
                    "Your suzerain, {0}, has summoned you to fulfill your oath of military aid. You have {1} days to join {2}, currently close to {3}.",
                    leader.LeaderHero.Name, 2, armyName, proximity.Name),
                true, false, "Understood", null, null, null));
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
                result = new TextObject("{=8EAqqJ9A}{SUZERAIN} holds your duty of military aid fulfilled. You have gained {INFLUENCE} and {RELATION} relatin with your suzerain.");
                result.SetTextVariable("INFLUENCE", influence);
                result.SetTextVariable("SUZERAIN", suzerain.Name);
                result.SetTextVariable("RELATION", relation);
            }

            InformationManager.ShowInquiry(new InquiryData("Duty of Military Aid", result.ToString(),
                true, false, GameTexts.FindText("str_done").ToString(), null, null, null), true);
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