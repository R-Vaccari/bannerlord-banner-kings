using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Titles.Laws;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class DemesneLawChangeDecision : Goal
    {
        private LawChangeOption chosenAction;

        public DemesneLawChangeDecision(Hero fulfiller = null) : base("goal_contract_law_change", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Kingdom;

        public override Goal GetCopy(Hero fulfiller)
        {
            DemesneLawChangeDecision copy = new DemesneLawChangeDecision(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (!IsAvailable())
            {
                failedReasons.Add(GameTexts.FindText("str_need_to_be_a_part_of_kingdom"));
                return false;
            }

            if (BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom) == null)
            {
                failedReasons.Add(new TextObject("{=akMdyYw0}Your faction has no contract. Found a kigdom-level title for your faction first."));
            }

            if (FactionManager.GetEnemyKingdoms(Clan.PlayerClan.Kingdom).Count() > 0)
            {
                failedReasons.Add(new TextObject("{=qCfmQGiD}Contract changes can not be proposed during wars."));
            }

            if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.UnresolvedDecisions.Any(x => x is BKContractChangeDecision || x is DemesneLawChangeDecision))
            {
                failedReasons.Add(new TextObject("{=nsQZHLQf}A contract-altering proposal is already being voted on."));
            }


            return failedReasons.IsEmpty();
        }

        public override void ShowInquiry()
        {
            var elements = new List<InquiryElement>();
            var options = new List<LawChangeOption>();
            var fulfiller = GetFulfiller();
            var kingdom = fulfiller.Clan.Kingdom;
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);

            foreach (var law in title.Contract.DemesneLaws)
            {
                var decision = new BKDemesneLawDecision(fulfiller.Clan, title, law);
                var list = new List<InquiryElement>();

                foreach (var option in DefaultDemesneLaws.Instance.GetLawsByType(law.LawType))
                {
                    if (option.Equals(law))
                    {
                        continue;
                    }

                    list.Add(new InquiryElement(option, option.Name.ToString(), null,
                        Clan.PlayerClan.Influence >= option.InfluenceCost, option.Description.ToString()));
                }

                options.Add(new LawChangeOption(GameTexts.FindText("str_bk_demesne_law", law.LawType.ToString()),
                    new TextObject(),
                    law.AvailableForVoting,
                    decision, 
                    list));
            }

            foreach (var option in options)
            {
                elements.Add(new InquiryElement(option, option.Name.ToString(), null, option.Enabled, option.Description.ToString()));
            }


            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=1z57WDgP}Demesne Law Change").ToString(),
                new TextObject("{=Uq1b1dmT}Propose a change to your faction's demesne laws. These laws describe a multitude of aspects of your faction, such as slavery, duties of lower classes and tenure of estates. Laws that have been enacted for less than 1 year may not be changed.").ToString(),
                elements, 
                true, 
                1,
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    chosenAction = (LawChangeOption)selectedOptions.First().Identifier;
                    ApplyGoal();
                }, 
                null, 
                string.Empty));
        }

        public override void ApplyGoal()
        {
            chosenAction.Execute();
        }

        public override void DoAiDecision()
        {
        }

        internal class LawChangeOption
        {
            internal TextObject Name { get; private set; }
            internal TextObject Description { get; private set; }
            internal bool Enabled { get; private set; }
            internal BKDemesneLawDecision Decision { get; private set; }
            internal List<InquiryElement> Options { get; private set; }

            internal LawChangeOption(TextObject name, TextObject description, bool enabled, BKDemesneLawDecision decision, List<InquiryElement> options)
            {
                Name = name;
                Description = description;
                Decision = decision;
                Options = options;
                Enabled = enabled;
            }   

            internal void Execute()
            {
                var description = new TextObject("{=n0kVmshx}Select a demesne {LAW} to be voted on. All the Peers of the realm will vote on it.")
                    .SetTextVariable("LAW", Name);

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    Name.ToString(),
                    description.ToString(),
                    Options, 
                    true, 
                    1,
                    1, 
                    GameTexts.FindText("str_done").ToString(), 
                    string.Empty,
                    delegate (List<InquiryElement> x)
                    {
                        Decision.UpdateDecision((DemesneLaw)x[0].Identifier);
                        Clan.PlayerClan.Kingdom.AddDecision(Decision, false);
                        
                    }, null, string.Empty));
            }
        }
    }
}