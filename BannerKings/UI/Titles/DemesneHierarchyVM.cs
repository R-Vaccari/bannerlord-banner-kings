using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Titles;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Titles
{
    public class DemesneHierarchyVM : BannerKingsViewModel
    {
        private ImageIdentifierVM banner;
        private DecisionElement contract;
        private DecisionElement foundKingdom;
        private MBBindingList<DecisionElement> decisions;
        private readonly Kingdom kingdom;
        private string name;
        private readonly FeudalTitle title;
        private TitleElementVM tree;

        public DemesneHierarchyVM(FeudalTitle title, Kingdom kingdom) : base(null, false)
        {
            this.title = title;
            if (kingdom == null)
            {
                kingdom = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
            }
            this.kingdom = kingdom;
            decisions = new MBBindingList<DecisionElement>();
            if (title != null)
            {
                Tree = new TitleElementVM(title);
                if (kingdom != null)
                {
                    Banner = new ImageIdentifierVM(BannerCode.CreateFrom(kingdom.Banner), true);
                } 
                else
                {
                    Banner = new ImageIdentifierVM(BannerCode.CreateFrom(title.deJure.Clan.Banner), true);
                }
                Name = title.FullName.ToString();
            }
        }

        [DataSourceProperty]
        public DecisionElement FoundKingdom
        {
            get => foundKingdom;
            set
            {
                if (value != foundKingdom)
                {
                    foundKingdom = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement Contract
        {
            get => contract;
            set
            {
                if (value != contract)
                {
                    contract = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DecisionElement> Decisions
        {
            get => decisions;
            set
            {
                if (value != decisions)
                {
                    decisions = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get => name;
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Banner
        {
            get => banner;
            set
            {
                if (value != banner)
                {
                    banner = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public TitleElementVM Tree
        {
            get => tree;
            set
            {
                if (value != tree)
                {
                    tree = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Decisions.Clear();

            if (title?.contract == null)
            {
                return;
            }

            var allSetup = kingdom != null && kingdom == BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
            var contractButton = new DecisionElement().SetAsButtonOption(new TextObject("{=tpH62HBy}Contract").ToString(),
                () => BannerKingsConfig.Instance.TitleManager.ShowContract(kingdom.Leader,
                    GameTexts.FindText("str_done").ToString()),
                new TextObject("{=yRn9AcwU}Review this kingdom's contract, signed by lords that join it."));
            contractButton.Enabled = allSetup;
            var foundButton = new DecisionElement().SetAsButtonOption(new TextObject("{=nbV21qZv}Found Kingdom").ToString(),
                ShowFoundKingdom,
                new TextObject("{=yRn9AcwU}Review this kingdom's contract, signed by lords that join it."));
            foundButton.Enabled = allSetup;

            Contract = contractButton;
            FoundKingdom = foundButton;

            if (Clan.PlayerClan.Kingdom != null)
            {
               
            } 
        }

        private void ShowFoundKingdom()
        {
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(Clan.PlayerClan.Kingdom, Hero.MainHero);
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=ztoYKWVA}Founding a new Kingdom").ToString(),
                new TextObject("{=5VhaJ732}Found a new title for your kingdom. The title will legitimize your position and allow the de Jure domain of the kingdom to expand through de Jure drift of dukedoms, as well as extend your influence as a suzerain. Founding a title would increase your clan's renown by {RENOWN}. \n \nCosts: {GOLD} {GOLD_ICON}, {INFLUENCE} {INFLUENCE_ICON} \n\nCan form kingdom: {POSSIBLE} \n\nExplanation: {REASON}")
                    .SetTextVariable("POSSIBLE", GameTexts.FindText(action.Possible ? "str_yes" : "str_no"))
                    .SetTextVariable("GOLD", $"{(int)action.Gold:n0}")
                    .SetTextVariable("GOLD_ICON", "<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">")
                    .SetTextVariable("INFLUENCE", (int) action.Influence)
                    .SetTextVariable("INFLUENCE_ICON", "<img src=\"General\\Icons\\Influence@2x\" extend=\"7\">")
                    .SetTextVariable("RENOWN", action.Renown)
                    .SetTextVariable("REASON", action.Reason)
                    .ToString(),
                action.Possible, true, GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                () =>
                {
                    var duchies = (from clan in kingdom.Clans from dukedom in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan.Leader).FindAll(x => x.type == TitleType.Dukedom) select new InquiryElement(dukedom, dukedom.FullName.ToString(), null)).ToList();

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=CSRMOcCm}Founding Dukedoms").ToString(),
                        new TextObject("{=JgLZ27UL}Select up to 3 dukedoms that will compose your kingdom. The kingdom's contract will follow the first dukedom's contract. Dukedom titles from other clans in the faction may be included as well.").ToString(),
                        duchies,
                        true,
                        3,
                        GameTexts.FindText("str_done").ToString(),
                        string.Empty,
                        delegate(List<InquiryElement> list)
                        {
                            var firstDukedom = (FeudalTitle) list[0].Identifier;
                            var vassals = (from element in list where (FeudalTitle) list[0].Identifier != firstDukedom select (FeudalTitle) element.Identifier).ToList();

                            action.SetTile(firstDukedom);
                            action.SetVassals(vassals);
                            action.TakeAction(null);
                        }, null));
                }, null));
        }

        private DecisionElement CreateButton(List<InquiryElement> options, BKContractDecision decision, string law, TextObject hint)
        {
            return new DecisionElement()
                .SetAsButtonOption(law, delegate
                {
                    var description = new TextObject("{=TJJzW4VL}Select a {LAW} to be voted on. Starting an election costs {INFLUENCE} influence.");
                    description.SetTextVariable("LAW", law);
                    var cost = decision?.GetInfluenceCost(null) ?? 0;
                    description.SetTextVariable("INFLUENCE", cost);

                    if (kingdom != null && options.Count > 0 && decision != null)
                    {
                        MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(law,
                            description.ToString(),
                            options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                            delegate(List<InquiryElement> x)
                            {
                                if (Clan.PlayerClan.Influence < cost)
                                {
                                    InformationManager.DisplayMessage(new InformationMessage("Not enough influence."));
                                }
                                else if (decision.Kingdom.UnresolvedDecisions.Any(x => x is BKContractDecision))
                                {
                                    InformationManager.DisplayMessage(
                                        new InformationMessage("Ongoing contract-altering decision."));
                                }
                                else
                                {
                                    GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, -cost);
                                    decision.UpdateDecision((int)x[0].Identifier);
                                    kingdom.AddDecision(decision, true);
                                }
                            }, null, string.Empty));
                    }
                }, hint);
        }

        private List<InquiryElement> GetGenderLaws()
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

        private List<InquiryElement> GetInheritances()
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

        private List<InquiryElement> GetSuccessions()
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

        private List<InquiryElement> GetGovernments()
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
    }
}