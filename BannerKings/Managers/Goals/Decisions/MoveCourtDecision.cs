using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class MoveCourtDecision : Goal
    {
        private Town town;

        public MoveCourtDecision() : base("goal_move_court", GoalCategory.Personal, GoalUpdateType.Manual)
        {
            var name = new TextObject("{=!}Relocate Court");
            var description = new TextObject("{=!}Relocate your House's court to a different castle or town.");

            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            Hero hero = GetFulfiller();
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan);
            return council != null && council.Peerage != null && council.Peerage.CanHaveCouncil &&
                hero.Clan.Fiefs.Count > 0;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
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
                    failedReasons.Add(new TextObject("{=!}Your court is already located in the only available location."));
                }
            }

            return failedReasons.IsEmpty();
        }

        internal override void ShowInquiry()
        {
            var options = new List<InquiryElement>();
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);

            foreach (var fief in Clan.PlayerClan.Fiefs)
            {
                int gold = (int)BannerKingsConfig.Instance.CouncilModel.CalculateRelocateCourtPrice(Clan.PlayerClan, fief).ResultNumber;
                if (fief != council.Location)
                {
                    options.Add(new InquiryElement(fief,
                        new TextObject("{=!}{TOWN} - {GOLD}{GOLD_ICON}")
                        .SetTextVariable("TOWN", fief.Name)
                        .SetTextVariable("GOLD", gold)
                        .ToString(),
                        null,
                        Hero.MainHero.Gold >= gold,
                        String.Empty));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Relocate Court").ToString(),
                new TextObject("{=!}Move your court to a different castle or town. Moving your court costs according to your income and the court's amenities.").ToString(),
                options, 
                true, 
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

        internal override void ApplyGoal()
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