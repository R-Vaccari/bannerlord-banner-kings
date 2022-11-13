using BannerKings.UI.Items;
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
        private bool proposedSelected, proposerSelected, arrangedMarriage, invertedClan, canInvertClan;
        private DecisionElement invertedClanToggle, arrangedMarriageToggle, allianceToggle, feastToggle;
        private HintViewModel influenceCostHint;
        private string dowryValueText, influenceCostText;

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
                    }
                },
                new TextObject("{=!}Invert the expected result for final clan. The clan whose member leaves it is owed the Dowry by the other family. Therefore, if your family member is leaving your clan, you are owed the Dowry. Spouses are less likely to leave their clans if they are it's leader or it's primary expected inheritor."));

            ArrangedMarriageToggle = new DecisionElement()
                .SetAsBooleanOption(new TextObject("{=!}Arranged Marriage").ToString(),
                false,
                delegate (bool value)
                {
                    ArrangedMarriage = value;
                }, 
                new TextObject("{=!}Arrange the marriage without consulting the spouses. While their personal relations are still considered, the to-be-spouses have no power to dictate the marriage result. If you are one of the spouses, this means no Courting phase - the marriage is sealed off right away."));


            AllianceToggle = new DecisionElement()
                .SetAsBooleanOption(new TextObject("{=!}Alliance Pact").ToString(),
                false,
                delegate (bool value)
                {
                    ArrangedMarriage = value;
                },
                new TextObject("{=!}Arrange the marriage without consulting the spouses. While their personal relations are still considered, the to-be-spouses have no power to dictate the marriage result. If you are one of the spouses, this means no Courting phase - the marriage is sealed off right away."));

            FeastToggle = new DecisionElement()
                .SetAsBooleanOption(new TextObject("{=!}Feast Celebration").ToString(),
                false,
                delegate (bool value)
                {
                    ArrangedMarriage = value;
                },
                new TextObject("{=!}Arrange the marriage without consulting the spouses. While their personal relations are still considered, the to-be-spouses have no power to dictate the marriage result. If you are one of the spouses, this means no Courting phase - the marriage is sealed off right away."));


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

            Clan finalClan = null;


            DowryValueText = "0";
            InfluenceCostText = "0";
            InfluenceCostHint = new HintViewModel();
            if (ProposerHero != null && ProposedHero != null)
            {
                var influence = BannerKingsConfig.Instance.MarriageModel.GetInfluenceCost(ProposerHero.Hero,
                    ProposedHero.Hero, finalClan, true);

                InfluenceCostText = ((int)influence.ResultNumber).ToString();
                InfluenceCostHint.HintText = new TextObject("{=!}{HINT}")
                    .SetTextVariable("HINT", influence.GetExplanations());
            }
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
                new TextObject("{=!}").ToString(),
                null,
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
                new TextObject("{=!}").ToString(),
                null,
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
