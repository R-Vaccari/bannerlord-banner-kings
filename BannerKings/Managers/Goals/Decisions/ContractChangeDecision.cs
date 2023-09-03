using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Titles;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class ContractChangeDecision : Goal
    {
        private ContractChangeOption chosenAction;

        public ContractChangeDecision() : base("goal_contract_change", GoalCategory.Kingdom, GoalUpdateType.Manual)
        {
            var name = new TextObject("{=fKXLiUti}Propose Contract Change");
            var description = new TextObject("{=Ba2hpnco}Propose a contract change to your faction's titles.");

            Initialize(name, description);
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

            if (Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Kingdom.UnresolvedDecisions.Any(x => x is BKContractDecision || x is DemesneLawChangeDecision))
            {
                failedReasons.Add(new TextObject("{=nsQZHLQf}A contract-altering proposal is already being voted on."));
            }

            return failedReasons.IsEmpty();
        }

        public override void ShowInquiry()
        {
            var elements = new List<InquiryElement>();
            var options = new List<ContractChangeOption>();
            var fulfiller = GetFulfiller();
            var kingdom = fulfiller.Clan.Kingdom;
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);

            /*var government = new BKGovernmentDecision(fulfiller.Clan, GovernmentType.Feudal, title);
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
                150, gender, GetGenderLaws(kingdom, title)));*/

            foreach (var option in options)
            {
                elements.Add(new InquiryElement(option, option.Name.ToString(), null, true, option.Description.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=mXhTPXZq}Contract Change").ToString(),
                new TextObject("{=B5e1fznG}Propose a change to you faction's contract. These changes may be the form of governance, succession, clan inheritance or gender laws.").ToString(),
                elements, 
                true, 
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    if (selectedOptions.Count > 0)
                    {
                        chosenAction = (ContractChangeOption)selectedOptions.First().Identifier;
                        ApplyGoal();
                    }
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
            throw new NotImplementedException();
        }

        /*private List<InquiryElement> GetGenderLaws(Kingdom kingdom, FeudalTitle title)
        {
            var laws = new List<InquiryElement>();
            foreach (var type in BannerKingsConfig.Instance.TitleManager.GetGenderLawTypes())
            {
                if (kingdom != null && type != title.Contract.GenderLaw)
                {
                    var decision = new BKGenderDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", type.ToString());
                    text.SetTextVariable("SUPPORT", new KingdomElection(decision).GetLikelihoodForOutcome(0).ToString("0.00"));
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
                if (kingdom != null && type != title.Contract.Inheritance)
                {
                    var decision = new BKInheritanceDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", type.ToString());
                    text.SetTextVariable("SUPPORT", new KingdomElection(decision).GetLikelihoodForOutcome(0).ToString("0.00"));
                    laws.Add(new InquiryElement(type, text.ToString(), null, true,
                        Utils.Helpers.GetInheritanceDescription(type)));
                }
            }

            return laws;
        }

        private List<InquiryElement> GetSuccessions(Kingdom kingdom, FeudalTitle title)
        {
            var laws = new List<InquiryElement>();
            foreach (var type in SuccessionHelper.GetValidSuccessions(title.Contract.Government))
            {
                if (kingdom != null && type != title.Contract.Succession)
                {
                    var decision = new BKSuccessionDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", Utils.Helpers.GetSuccessionTypeName(type));
                    text.SetTextVariable("SUPPORT", new KingdomElection(decision).GetLikelihoodForOutcome(0).ToString("0.00"));
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
                if (kingdom != null && type != title.Contract.Government)
                {
                    var decision = new BKGovernmentDecision(Clan.PlayerClan, type, title);
                    var text = new TextObject("{=F7iMS7Tz}{LAW} - ({SUPPORT}% support)");
                    text.SetTextVariable("LAW", type.ToString());
                    text.SetTextVariable("SUPPORT", new KingdomElection(decision).GetLikelihoodForOutcome(0).ToString("0.00"));
                    laws.Add(new InquiryElement(type, text.ToString(), null, true,
                        Utils.Helpers.GetGovernmentDescription(type)));
                }
            }

            return laws;
        }*/

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
                        if (x.Count > 0)
                        {
                            GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, -Influence);
                            Decision.UpdateDecision((int)x[0].Identifier);
                            Clan.PlayerClan.Kingdom.AddDecision(Decision, true);
                        }
                    }, null, string.Empty));
            }
        }
    }
}