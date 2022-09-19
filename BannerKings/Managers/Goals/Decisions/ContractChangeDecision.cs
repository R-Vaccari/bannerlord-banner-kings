using BannerKings.Managers.Helpers;
using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Titles;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class ContractChangeDecision : Goal
    {
        private ContractChangeOption chosenAction;

        public ContractChangeDecision() : base("goal_contract_change", GoalUpdateType.Manual)
        {
            var name = new TextObject("{=!}Propose Contract Change");
            var description = new TextObject("{=!}Propose a contract change to your faction's titles.");

            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (!IsAvailable())
            {
                failedReasons.Add(GameTexts.FindText("str_need_to_be_a_part_of_kingdom"));
            }

            if (BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom) == null)
            {
                failedReasons.Add(new TextObject("{=!}Your faction has no contract. Found a kigdom-level title for your faction first."));
            }

            if (FactionManager.GetEnemyKingdoms(Clan.PlayerClan.Kingdom).Count() > 0)
            {
                failedReasons.Add(new TextObject("{=!}Contract changes can not be proposed during wars."));
            }

            if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.UnresolvedDecisions.Any(x => x is BKContractDecision))
            {
                failedReasons.Add(new TextObject("{=!}A contract-altering proposal is already being voted on."));
            }


            return failedReasons.IsEmpty();
        }

        internal override Hero GetFulfiller()
        {
            return Hero.MainHero;
        }

        internal override void ShowInquiry()
        {
            var elements = new List<InquiryElement>();
            var options = new List<ContractChangeOption>();
            var fulfiller = GetFulfiller();
            var kingdom = fulfiller.Clan.Kingdom;
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);

            var government = new BKGovernmentDecision(fulfiller.Clan, GovernmentType.Feudal, title);
            options.Add(new ContractChangeOption(new TextObject("{=PSrEtF5L}Government"),
                new TextObject("{=jeNEcVGi}Propose a change in government structure, altering the allowed succession forms and aspects of settlement governance. Depending on the government choice, an appropriate succession type will be enforced as well."),
                300, government, GetGovernments(kingdom, title)));

            var succession = new BKSuccessionDecision(fulfiller.Clan, SuccessionType.Hereditary_Monarchy, title);
            options.Add(new ContractChangeOption(new TextObject("{=rTUgik07}Succession"),
                new TextObject("{=ie9VdKpd}Propose a change in the realm's succession, altering how the next sovereign is chosen."),
                250, succession, GetSuccessions(kingdom, title)));

            var inheritance = new BKInheritanceDecision(fulfiller.Clan, InheritanceType.Primogeniture, title);
            options.Add(new ContractChangeOption(new TextObject("{=aELuNrRC}Inheritance"),
                new TextObject("{=PXXY56Vd}Propose a change in clan inheritances, that is, who becomes the clan leader once the leader dies."),
                200, inheritance, GetInheritances(kingdom, title)));

            var gender = new BKGenderDecision(fulfiller.Clan, GenderLaw.Agnatic, title);
            options.Add(new ContractChangeOption(new TextObject("{=LESrJQvC}Gender Law"), 
                new TextObject("{=kXUWrDRZ}Propose a change in gender laws, dictating whether males and females are viewed equally in various aspects."),
                150, gender, GetGenderLaws(kingdom, title)));

            foreach (var option in options)
            {
                elements.Add(new InquiryElement(option, option.Name.ToString(), null, true, option.Description.ToString()));
            }


            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=oBxXQmTb}Request Council Position").ToString(),
                new TextObject("{=bLxGGL9z}Choose a council position to fill. Different positions have different criteria for accepting candidates - some will be entirely blocked off, such as religious positions. Swapping with an existing lord will incur relations penalties.").ToString(),
                elements, 
                true, 
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    chosenAction = (ContractChangeOption)selectedOptions.First().Identifier;
                    ApplyGoal();
                }, 
                null, 
                string.Empty));
        }

        internal override void ApplyGoal()
        {
            chosenAction.Execute();
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }

        private List<InquiryElement> GetGenderLaws(Kingdom kingdom, FeudalTitle title)
        {
            var laws = new List<InquiryElement>();
            foreach (var type in BannerKingsConfig.Instance.TitleManager.GetGenderLawTypes())
            {
                if (kingdom != null && type != title.contract.GenderLaw)
                {
                    var decision = new BKGenderDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", type.ToString());
                    text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
                    laws.Add(new InquiryElement(type, text.ToString(), null, true,
                        Utils.Helpers.GetGenderLawDescription(type)));
                }
            }

            return laws;
        }

        private List<InquiryElement> GetInheritances(Kingdom kingdom, FeudalTitle title)
        {
            var laws = new List<InquiryElement>();
            foreach (var type in BannerKingsConfig.Instance.TitleManager.GetInheritanceTypes())
            {
                if (kingdom != null && type != title.contract.Inheritance)
                {
                    var decision = new BKInheritanceDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", type.ToString());
                    text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
                    laws.Add(new InquiryElement(type, text.ToString(), null, true,
                        Utils.Helpers.GetInheritanceDescription(type)));
                }
            }

            return laws;
        }

        private List<InquiryElement> GetSuccessions(Kingdom kingdom, FeudalTitle title)
        {
            var laws = new List<InquiryElement>();
            foreach (var type in SuccessionHelper.GetValidSuccessions(title.contract.Government))
            {
                if (kingdom != null && type != title.contract.Succession)
                {
                    var decision = new BKSuccessionDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", Utils.Helpers.GetSuccessionTypeName(type));
                    text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
                    laws.Add(new InquiryElement(type, text.ToString(), null, true,
                        Utils.Helpers.GetSuccessionTypeDescription(type)));
                }
            }

            return laws;
        }

        private List<InquiryElement> GetGovernments(Kingdom kingdom, FeudalTitle title)
        {
            var laws = new List<InquiryElement>();
            foreach (var type in BannerKingsConfig.Instance.TitleManager.GetGovernmentTypes())
            {
                if (kingdom != null && type != title.contract.Government)
                {
                    var decision = new BKGovernmentDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", type.ToString());
                    text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
                    laws.Add(new InquiryElement(type, text.ToString(), null, true,
                        Utils.Helpers.GetGovernmentDescription(type)));
                }
            }

            return laws;
        }


        internal class ContractChangeOption
        {
            internal TextObject Name { get; private set; }
            internal TextObject Description { get; private set; }
            internal float Influence { get; private set; }
            internal BKContractDecision Decision { get; private set; }
            internal List<InquiryElement> Options { get; private set; }

            internal ContractChangeOption(TextObject name, TextObject description, float influence, BKContractDecision decision, List<InquiryElement> options)
            {
                Name = name;
                Description = description;
                Influence = influence;
                Decision = decision;
                Options = options;
            }   

            internal void Execute()
            {
                var description = new TextObject("{=TJJzW4VL}Select a {LAW} to be voted on. Starting an election costs {INFLUENCE} influence.")
                    .SetTextVariable("LAW", Name)
                    .SetTextVariable("INFLUENCE", Influence);

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    Name.ToString(),
                    description.ToString(),
                    Options, 
                    true, 
                    1, 
                    GameTexts.FindText("str_done").ToString(), 
                    string.Empty,
                    delegate (List<InquiryElement> x)
                    {
                        GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, -Influence);
                        Decision.UpdateDecision((int)x[0].Identifier);
                        Clan.PlayerClan.Kingdom.AddDecision(Decision, true);
                        
                    }, null, string.Empty));
            }
        }
    }
}