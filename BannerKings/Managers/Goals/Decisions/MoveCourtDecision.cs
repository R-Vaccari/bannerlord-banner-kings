using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class MoveCourtDecision : Goal
    {
        private Town town;

        public MoveCourtDecision(Hero fulfiller = null) : base("goal_move_court", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Personal;

        public override Goal GetCopy(Hero fulfiller)
        {
            MoveCourtDecision copy = new MoveCourtDecision(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable()
        {
            Hero hero = GetFulfiller();
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan);
            return council != null && council.Peerage != null && council.Peerage.CanHaveCouncil &&
                hero.Clan.Fiefs.Count > 0;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            Hero hero = GetFulfiller();
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan);

            List<Town> list = new List<Town>();
            list.AddRange(hero.Clan.Fiefs);

            if (council.Location != null)
            {
                list.Remove(council.Location);
                if (list.Count == 0)
                {
                    failedReasons.Add(new TextObject("{=Or0X6Y4J}Your court is already located in the only available location."));
                }
            }

            return failedReasons.IsEmpty();
        }

        public override void ShowInquiry()
        {
            var options = new List<InquiryElement>();
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);

            foreach (var fief in Clan.PlayerClan.Fiefs)
            {
                int gold = (int)BannerKingsConfig.Instance.CouncilModel.CalculateRelocateCourtPrice(Clan.PlayerClan, fief).ResultNumber;
                if (fief != council.Location)
                {
                    options.Add(new InquiryElement(fief,
                        new TextObject("{=NpzmdWTX}{TOWN} - {GOLD}{GOLD_ICON}")
                        .SetTextVariable("TOWN", fief.Name)
                        .SetTextVariable("GOLD", gold)
                        .ToString(),
                        null,
                        Hero.MainHero.Gold >= gold,
                        String.Empty));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=v094GOtN}Relocate Court").ToString(),
                new TextObject("{=q6td7iJ3}Move your court to a different castle or town. Moving your court costs according to your income and the court's amenities.").ToString(),
                options, 
                true, 
                1, 
                1,
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    town = (Town)selectedOptions.First().Identifier;
                    ApplyGoal();
                }, 
                null, 
                string.Empty));
        }

        public override void ApplyGoal()
        {
            Clan clan = GetFulfiller().Clan;
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            int gold = (int)BannerKingsConfig.Instance.CouncilModel.CalculateRelocateCourtPrice(clan, town).ResultNumber;
            council.SetCourtLocation(town);
            clan.Leader.ChangeHeroGold(-gold);
        }

        public override void DoAiDecision()
        {
        }
    }
}