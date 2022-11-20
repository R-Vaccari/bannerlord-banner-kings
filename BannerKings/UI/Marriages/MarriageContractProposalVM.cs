using BannerKings.Behaviours.Marriage;
using BannerKings.UI.Items;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Marriages
{
    public class MarriageContractProposalVM : BannerKingsViewModel
    {
        private MBBindingList<Hero> proposerCandidates, proposedCandidates;
        private HeroVM proposedHero, proposerHero;
        private bool proposedSelected, proposerSelected, arrangedMarriage, invertedClan, canInvertClan,
            canChangeArrangedMarriage, canCreateAlliance, alliance, feast;
        private DecisionElement invertedClanToggle, arrangedMarriageToggle, allianceToggle, feastToggle;
        private HintViewModel influenceCostHint, dowryValueHint, willAcceptHint,
            proposerSpouseHint, proposedSpouseHint;
        private string dowryValueText, influenceCostText, finalClanText, willAcceptText,
            proposerSpouseText, proposedSpouseText;

        public MarriageContractProposalVM(Hero clanLeader) : base(null, true)
        {
            ClanLeaderProposedTo = clanLeader;
            ProposerCandidates = new MBBindingList<Hero>();
            ProposedCandidates = new MBBindingList<Hero>();

            InvertedClanToggle = new DecisionElement()
                .SetAsBooleanOption(new TextObject("{=!}Invert Clan").ToString(),
                false,
                delegate (bool value)
                {
                    if (CanInvertClan)
                    {
                        InvertedClan = value;
                        RefreshValues();
                    }
                },
                new TextObject("{=!}Invert the expected result for final clan. The clan whose member leaves it is owed the Dowry by the other family. Therefore, if your family member is leaving your clan, you are owed the Dowry. Spouses are less likely to leave their clans if they are it's leader or it's primary expected inheritor."));

            ArrangedMarriageToggle = new DecisionElement()
                .SetAsBooleanOption(new TextObject("{=!}Arranged Marriage").ToString(),
                false,
                delegate (bool value)
                {
                    if (CanChangeArrangedMarriage)
                    {
                        ArrangedMarriage = value;
                        RefreshValues();
                    }
                }, 
                new TextObject("{=!}Arrange the marriage without consulting the spouses. While their personal relations are still considered, the to-be-spouses have no power to dictate the marriage result. If you are one of the spouses, this means no Courting phase - the marriage is sealed off right away."));


            AllianceToggle = new DecisionElement()
                .SetAsBooleanOption(new TextObject("{=!}Alliance Pact").ToString(),
                false,
                delegate (bool value)
                {
                    alliance = value;
                    RefreshValues();
                },
                new TextObject("{=!}Join both houses in alliance. By doing so, both houses are bound to not enter in conflict with each other when it comes to internal matters of the kingdom. Instead, if they are the leading houses of separate kingdoms, it would prevent a war between both realms. Subject houses may still fight each other if their sovereigns declare war."));

            FeastToggle = new DecisionElement()
                .SetAsBooleanOption(new TextObject("{=!}Feast Celebration").ToString(),
                false,
                delegate (bool value)
                {
                    feast = value;
                    RefreshValues();
                },
                new TextObject("{=!}Arrange a feast to celebrate the marriage. A selection of families within the realm will be invited, and as a host you ought to provide them a quality celebration. Doing so will allow you to improve your standing with them, as well as bring your family renown."));


            RefreshValues();
        }

        public Hero ClanLeaderProposedTo { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            ProposerCandidates.Clear();
            ProposedCandidates.Clear();

            ProposerSelected = ProposerHero != null;
            ProposedSelected = ProposedHero != null;

            CanInvertClan = ProposedHero != null && ProposerHero != null && !ProposerHero.Hero.IsHumanPlayerCharacter;
            InvertedClanToggle.Enabled = CanInvertClan;

            foreach (var hero in Clan.PlayerClan.Heroes)
            {
                if (!hero.IsChild && hero.Spouse == null )
                {
                    if (ProposedHero != null && ProposedHero.Hero.IsFemale == hero.IsFemale)
                    {
                        continue;
                    }

                    ProposerCandidates.Add(hero);
                }
            }

            foreach (var hero in ClanLeaderProposedTo.Clan.Heroes)
            {
                if (!hero.IsChild && hero.Spouse == null)
                {
                    if (ProposerHero != null && ProposerHero.Hero.IsFemale == hero.IsFemale)
                    {
                        continue;
                    }

                    ProposedCandidates.Add(hero);
                }
            }

            ProposerSpouseValueText = "0";
            ProposedSpouseValueText = "0";
            ProposerSpouseHint = new HintViewModel();
            ProposedSpouseHint = new HintViewModel();

            CanChangeArrangedMarriage = true;

            if (ProposerHero != null)
            {
                var score = BannerKingsConfig.Instance.MarriageModel.GetSpouseScore(ProposerHero.Hero, true);
                ProposerSpouseValueText = score.ResultNumber.ToString("0");
                ProposerSpouseHint.HintText = new TextObject("{=!}Spouse value.\n\n{HINT}")
                    .SetTextVariable("HINT", score.GetExplanations());

                if (ProposerHero.Hero != Hero.MainHero)
                {
                    ArrangedMarriageToggle.OptionValueAsBoolean = true;
                    CanChangeArrangedMarriage = false;
                }
            }

            ArrangedMarriageToggle.Enabled = CanChangeArrangedMarriage;

            if (ProposedHero != null)
            {
                var score = BannerKingsConfig.Instance.MarriageModel.GetSpouseScore(ProposedHero.Hero, true);
                ProposedSpouseValueText = score.ResultNumber.ToString("0");
                ProposedSpouseHint.HintText = new TextObject("{=!}Spouse value.\n\n{HINT}")
                    .SetTextVariable("HINT", score.GetExplanations());
            }


            CanCreateAlliance = false;

            DowryValueText = "0";
            InfluenceCostText = "0";
            InfluenceCostHint = new HintViewModel();
            DowryValueHint = new HintViewModel();
            FinalClanText = new TextObject("{=!}Unclear").ToString();
            WillAcceptText = new TextObject("{=!}Unclear").ToString();
            WillAcceptHint = new HintViewModel();
            if (ProposerHero != null && ProposedHero != null)
            {

                if (ProposerHero.Hero.IsCommonBorn() || ProposedHero.Hero.IsCommonBorn())
                {
                    CanCreateAlliance = false;
                }

               var willAccept = BannerKingsConfig.Instance.MarriageModel.IsMarriageAdequate(ProposerHero.Hero,
                    ProposedHero.Hero, true);
                WillAcceptText = GameTexts.FindText(willAccept.ResultNumber >= 1f ? "str_yes" : "str_no").ToString();
                WillAcceptHint.HintText = new TextObject("{=!}Whether or not their clan will accept this proposal. This is mainly dicatated by the different of value between the spouses, but also influence by their relationship with each other and the clan's trust towards you, as well as other kingdom-related matters, such as matching cultures and faiths.\n\n{REASON}")
                    .SetTextVariable("REASON", willAccept.GetExplanations());

                Clan finalClan = GetFinalClan();
                FinalClanText = finalClan.Name.ToString();

                var influence = BannerKingsConfig.Instance.MarriageModel.GetInfluenceCost(ProposedHero.Hero, true);
                InfluenceCostText = ((int)influence.ResultNumber).ToString();
                InfluenceCostHint.HintText = new TextObject("{=!}The influence cost associated with this marriage. Influence is associated with the social standing of the other clan. A clan of high standing, such as those leading kingdoms, are valuable marriage targets both within and outside their factions. Thus, the more important a clan is, the more influence is associated with marrying them.\n\n{HINT}")
                    .SetTextVariable("HINT", influence.GetExplanations());

                var dowry = BannerKingsConfig.Instance.MarriageModel.GetDowryValue(GetDowryHero(), ArrangedMarriage, true);
                DowryValueText = ((int)dowry.ResultNumber).ToString();
                DowryValueHint.HintText = new TextObject("{=!}The dowry is the financial security provided by the clan that takes in a new family member. It serves to show good will and genuine interest in the marriage by requiring a significant investment in it. Dowries are calculated based on the spouse's value as a family member - their position in the original clan and their usefulness. If a member of your clan is leaving the family to join another, you are owed the dowry.\n\n{HINT}")
                    .SetTextVariable("HINT", dowry.GetExplanations());
            }

            AllianceToggle.Enabled = CanCreateAlliance;
            if (!CanCreateAlliance)
            {
                AllianceToggle.OptionValueAsBoolean = false;
            }
        }

        private Hero GetDowryHero()
        {
            Hero dowryHero = ProposedHero.Hero;
            if (GetFinalClan() != Clan.PlayerClan)
            {
                dowryHero = ProposerHero.Hero;
            }

            return dowryHero;
        }

        private Clan GetFinalClan()
        {
            Clan finalClan = BannerKingsConfig.Instance.MarriageModel.GetClanAfterMarriage(ProposerHero.Hero,
                                ProposedHero.Hero);

            if (InvertedClan)
            {
                if (finalClan == Clan.PlayerClan)
                {
                    finalClan = ProposedHero.Hero.Clan;
                }
                else
                {
                    finalClan = Clan.PlayerClan;
                }
            }

            return finalClan;
        }

        public void SelectProposer()
        {
            var list = new List<InquiryElement>();
            foreach (var hero in ProposerCandidates)
            {
                list.Add(new InquiryElement(hero,
                    hero.Name.ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, true))));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Select Spouse").ToString(),
                new TextObject("{=!}Select the member of your family to be married. If you pick yourself, the final clan cannot be inverted (resulting in you leaving your clan). If you pick another clan member, the marriage will have to be arranged, and take immediate effect, unless a feast is organized, in which case the marriage is postponed to the end of the feast.").ToString(),
                list,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    ProposerHero = new HeroVM((Hero)list[0].Identifier);
                    InvertedClan = false;
                    RefreshValues();
                },
                null));
        }

        public void SelectProposed()
        {
            var list = new List<InquiryElement>();
            foreach (var hero in ProposedCandidates)
            {
                list.Add(new InquiryElement(hero,
                    hero.Name.ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, true))));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Select Spouse").ToString(),
                new TextObject("{=!}Select the spouse from their clan. Each spouse has a different score, which represents their importance to their family. The marriage will be acceptable or not according to the difference in the score of your proposed spouse and theirs.").ToString(),
                list,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    ProposedHero = new HeroVM((Hero)list[0].Identifier);
                    InvertedClan = false;
                    RefreshValues();
                },
                null));
        }

        private void MakeContract()
        {
            Campaign.Current.GetCampaignBehavior<BKMarriageBehavior>().SetProposedMarriage(
                new MarriageContract(ProposerHero.Hero,
                ProposedHero.Hero,
                GetFinalClan(),
                (int)BannerKingsConfig.Instance.MarriageModel.GetDowryValue(GetDowryHero(), ArrangedMarriage, true).ResultNumber,
                (int)BannerKingsConfig.Instance.MarriageModel.GetInfluenceCost(ProposedHero.Hero, true).ResultNumber,
                ArrangedMarriage,
                alliance,
                feast
                ));

            ExecuteClose();
        }

        [DataSourceProperty]
        public string DowryHeaderText => new TextObject("{=!}Dowry").ToString();

        [DataSourceProperty]
        public string ClanHeaderText => new TextObject("{=!}Final Clan").ToString();

        [DataSourceProperty]
        public string InfluenceHeaderText => GameTexts.FindText("str_influence").ToString();

        [DataSourceProperty]
        public string ConfirmText => GameTexts.FindText("str_accept").ToString();

        [DataSourceProperty]
        public string WillAcceptHeaderText => new TextObject("{=!}Acceptable").ToString();

        [DataSourceProperty]
        public string SpouseHeaderText => new TextObject("{=!}Spouse Score").ToString();


        [DataSourceProperty]
        public string ProposerSpouseValueText
        {
            get => proposerSpouseText;
            set
            {
                if (value != proposerSpouseText)
                {
                    proposerSpouseText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string ProposedSpouseValueText
        {
            get => proposedSpouseText;
            set
            {
                if (value != proposedSpouseText)
                {
                    proposedSpouseText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public string FinalClanText
        {
            get => finalClanText;
            set
            {
                if (value != finalClanText)
                {
                    finalClanText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public string InfluenceCostText
        {
            get => influenceCostText;
            set
            {
                if (value != influenceCostText)
                {
                    influenceCostText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string DowryValueText
        {
            get => dowryValueText;
            set
            {
                if (value != dowryValueText)
                {
                    dowryValueText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string WillAcceptText
        {
            get => willAcceptText;
            set
            {
                if (value != willAcceptText)
                {
                    willAcceptText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ProposerSpouseHint
        {
            get => proposerSpouseHint;
            set
            {
                if (value != proposerSpouseHint)
                {
                    proposerSpouseHint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ProposedSpouseHint
        {
            get => proposedSpouseHint;
            set
            {
                if (value != proposedSpouseHint)
                {
                    proposedSpouseHint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel WillAcceptHint
        {
            get => willAcceptHint;
            set
            {
                if (value != willAcceptHint)
                {
                    willAcceptHint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel DowryValueHint
        {
            get => dowryValueHint;
            set
            {
                if (value != dowryValueHint)
                {
                    dowryValueHint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel InfluenceCostHint
        {
            get => influenceCostHint;
            set
            {
                if (value != influenceCostHint)
                {
                    influenceCostHint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement InvertedClanToggle
        {
            get => invertedClanToggle;
            set
            {
                if (value != invertedClanToggle)
                {
                    invertedClanToggle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement ArrangedMarriageToggle
        {
            get => arrangedMarriageToggle;
            set
            {
                if (value != arrangedMarriageToggle)
                {
                    arrangedMarriageToggle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement AllianceToggle
        {
            get => allianceToggle;
            set
            {
                if (value != allianceToggle)
                {
                    allianceToggle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement FeastToggle
        {
            get => feastToggle;
            set
            {
                if (value != feastToggle)
                {
                    feastToggle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<Hero> ProposerCandidates 
        { 
            get => proposerCandidates;
            set
            {
                if (value != proposerCandidates)
                {
                    proposerCandidates = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<Hero> ProposedCandidates
        {
            get => proposedCandidates;
            set
            {
                if (value != proposedCandidates)
                {
                    proposedCandidates = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HeroVM ProposerHero
        {
            get => proposerHero;
            set
            {
                if (value != proposerHero)
                {
                    proposerHero = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HeroVM ProposedHero
        {
            get => proposedHero;
            set
            {
                if (value != proposedHero)
                {
                    proposedHero = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool ArrangedMarriage
        {
            get => arrangedMarriage;
            set
            {
                if (value != arrangedMarriage)
                {
                    arrangedMarriage = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public bool InvertedClan
        {
            get => invertedClan;
            set
            {
                if (value != invertedClan)
                {
                    invertedClan = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CanCreateAlliance
        {
            get => canCreateAlliance;
            set
            {
                if (value != canCreateAlliance)
                {
                    canCreateAlliance = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CanChangeArrangedMarriage
        {
            get => canChangeArrangedMarriage;
            set
            {
                if (value != canChangeArrangedMarriage)
                {
                    canChangeArrangedMarriage = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CanInvertClan
        {
            get => canInvertClan;
            set
            {
                if (value != canInvertClan)
                {
                    canInvertClan = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool ProposedSelected
        {
            get => proposedSelected;
            set
            {
                if (value != proposedSelected)
                {
                    proposedSelected = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool ProposerSelected
        {
            get => proposerSelected;
            set
            {
                if (value != proposerSelected)
                {
                    proposerSelected = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }  
    }
}
