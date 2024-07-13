using BannerKings.Managers.Titles;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class MergeArmyGoal : Goal
    {
        private Army selectedArmy;

        public MergeArmyGoal(Hero fulfiller = null) : base("goal_merge_army", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Kingdom;

        public override Goal GetCopy(Hero fulfiller)
        {
            MergeArmyGoal copy = new MergeArmyGoal(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable() => Clan.PlayerClan.Kingdom != null;

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            Hero fulfiller = GetFulfiller();

            if (fulfiller.PartyBelongedTo == null)
            {
                failedReasons.Add(new TextObject("{=QHfkhG0b}Not in a party."));
            }
            else
            {
                Army army = fulfiller.PartyBelongedTo.Army;
                if (army == null || army.LeaderParty != fulfiller.PartyBelongedTo)
                {
                    failedReasons.Add(new TextObject("{=!}Not currently leading an army."));
                }
            }
            
            return failedReasons.IsEmpty();
        }

        private List<Army> GetArmiesToMerge() 
        {
            List<Army> results = new List<Army>();
            Kingdom kingdom = GetFulfiller().Clan.Kingdom;
            if (kingdom != null)
            {
                foreach (Army army in kingdom.Armies)
                {
                    if (army.LeaderParty.LeaderHero == GetFulfiller()) continue;
                    results.Add(army);
                }
            }

            return results;
        }

        public override void ShowInquiry()
        {
            List<InquiryElement> elements = new List<InquiryElement>();
            foreach (Army army in GetArmiesToMerge())
            {
                int cost = GetInfluenceCost(army);
                ValueTuple<bool, TextObject> available = IsAvailable(army);
                elements.Add(new InquiryElement(army,
                    new TextObject("{=!}{ARMY} - {COUNT} strong, {INFLUENCE}{INFLUENCE_ICON}")
                    .SetTextVariable("ARMY", army.Name)
                    .SetTextVariable("COUNT", army.TotalManCount)
                    .SetTextVariable("INFLUENCE", cost)
                    .ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(army.LeaderParty.LeaderHero.CharacterObject, false)),
                    available.Item1,
                    available.Item2.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Merge Armies").ToString(),
                new TextObject("{=!}Merge another existing army into your own.").ToString(),
                elements,
                true,
                1,
                elements.Count,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_reject").ToString(),
                delegate (List<InquiryElement> list)
                {
                    foreach (InquiryElement element in list)
                    {
                        selectedArmy = (Army)element.Identifier;
                        ApplyGoal();
                    }
                },
                null));
        }

        private ValueTuple<bool, TextObject> IsAvailable(Army toMerge)
        {
            bool available = true;
            TextObject reason = new TextObject("{=!}Armies may be merged.");
            int cost = GetInfluenceCost(toMerge);
            if (GetFulfiller().Clan.Influence < cost)
            {
                available = false; 
                reason = new TextObject("{=!}You do not have enough influence ({INFLUENCE}{INFLUENCE_ICON}).")
                    .SetTextVariable("INFLUENCE", cost)
                    .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON);
            }

            if (toMerge.LeaderParty.MapEvent != null)
            {
                available = false;
                reason = new TextObject("{=!}The {ARMY} is currently occupied.")
                    .SetTextVariable("ARMY", toMerge.Name);
            }

            FeudalTitle fulfillerTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(GetFulfiller());
            FeudalTitle armyTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(toMerge.LeaderParty.LeaderHero);
            if ((fulfillerTitle == null && armyTitle != null) || 
                (armyTitle != null &&  armyTitle != null && fulfillerTitle.TitleType > armyTitle.TitleType))
            {
                available = false;
                reason = new TextObject("{=!}{HERO} holds a higher position than you as holder of the {TITLE}.")
                     .SetTextVariable("TITLE", armyTitle.FullName)
                    .SetTextVariable("HERO", toMerge.LeaderParty.LeaderHero.Name);
            }

            return new ValueTuple<bool, TextObject>(available, reason);
        }

        private int GetInfluenceCost(Army toMerge)
        {
            float result = 0;
            foreach (MobileParty party in toMerge.Parties)
            {
                result += BannerKingsConfig.Instance.ArmyManagementModel.CalculatePartyInfluenceCost(GetFulfiller().PartyBelongedTo,
                    party) * 0.5f;
            }
            
            return (int)result;
        }

        public override void ApplyGoal()
        {
            var hero = GetFulfiller();
            int cost = GetInfluenceCost(selectedArmy);
            if (hero.Clan.Influence >= cost)
            {
                List<MobileParty> parties = new List<MobileParty>(selectedArmy.Parties);
                DisbandArmyAction.ApplyByUnknownReason(selectedArmy);
                foreach (MobileParty party in parties)
                {
                    SetPartyAiAction.GetActionForEscortingParty(party, hero.PartyBelongedTo);
                }
                ChangeClanInfluenceAction.Apply(hero.Clan, -cost);
            }
        }

        public override void DoAiDecision()
        {
            if (!IsFulfilled(out var reasons)) return;

            foreach (Army army in GetArmiesToMerge())
            {
                if (IsAvailable(army).Item1)
                {
                    selectedArmy = army;
                    ApplyGoal();
                }
            }
        }
    }
}