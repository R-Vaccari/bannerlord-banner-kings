using StoryMode;
using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours
{
    public class BKCampaignStartBehavior : CampaignBehaviorBase
    {
        private bool hasSeenInquiry;
        private StartOption option;
        private Religion religion;
        private CampaignTime startTime = CampaignTime.Never;

        public void SetStartOption(StartOption option) => this.option = option;
        public void SetReligion(Religion religion) => this.religion = religion;

        public bool HasDebuff(StartOption option)
        {
            if (this.option != null && this.option.Equals(option) && startTime.ElapsedYearsUntilNow < 5)
            {
                return true;
            }

            return false;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoaded);

            if (Game.Current.GameType is CampaignStoryMode)
            {
                StoryModeEvents.OnStoryModeTutorialEndedEvent.AddNonSerializedListener(this,
                () => 
                {
                    OnCharacterCreationOver();
                    GiveClansResources();
                    GiveTownsResources();
                });
            }
            else
            {
                 CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, 
                () => 
                {
                    OnCharacterCreationOver();
                    GiveClansResources();
                    GiveTownsResources();
                });
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData)
            {
                option = null;
            }

            dataStore.SyncData("bannerkings-campaignstart-option", ref option);
            dataStore.SyncData("bannerkings-campaignstart-time", ref startTime);
            dataStore.SyncData("bannerkings-campaignstart-inquiry", ref hasSeenInquiry);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            DefaultStartOptions.Instance.Initialize();
        }

        private void CheckShowStartOptions()
        {
            if (option != null)
            {
                return;
            }

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=BrXXDSaU}Campaign Start").ToString(),
                new TextObject("{=S5VQtaEg}It seems you have started Banner Kings in a saved game. Would you like to choose a custom start? This will reset your party and inventory - do not choose unless you have just started your campaign.").ToString(),
                true,
                true,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                () => OnCharacterCreationOver(),
                null));
        }

        private void RunReligion()
        {
            if (religion != null)
            {
                BannerKingsConfig.Instance.ReligionsManager.AddToReligion(Hero.MainHero, religion);
                BannerKingsConfig.Instance.ReligionsManager.AddPiety(Hero.MainHero,
                    BannerKingsConfig.Instance.ReligionsManager.GetStartingPiety(religion, Hero.MainHero));
            }
        }

        public void RunStartOption()
        {
            startTime = CampaignTime.Now;
            if (option == null) return;

            var mainHero = Hero.MainHero;
            mainHero.ChangeHeroGold(option.Gold - mainHero.Gold);

            AddFood(MobileParty.MainParty, option.Food);

            if (option.Lifestyle != null)
            {
                BannerKingsConfig.Instance.EducationManager.SetStartOptionLifestyle(Hero.MainHero, option.Lifestyle);
            }

            if (option.IsCriminal)
            {
                var list = new List<InquiryElement>();
                foreach (var kingdom in Kingdom.All)
                {
                    list.Add(new InquiryElement(kingdom, kingdom.Name.ToString(),
                        new ImageIdentifier(kingdom.Banner)));
                }

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                            new TextObject("{=XbjMOmXU}Kingdom Selection").ToString(),
                            new TextObject("{=ZLTgWVVS}Choose where you want to be recognized as a criminal.").ToString(),
                            list,
                            false,
                            1,
                            1,
                            GameTexts.FindText("str_accept").ToString(),
                            string.Empty,
                            delegate (List<InquiryElement> list)
                            {
                                var kingdom = (Kingdom)list[0].Identifier;
                                ChangeCrimeRatingAction.Apply(kingdom, option.Criminal);
                            },
                            null),
                            true,
                            false);
            }

            if (option.Action != null)
            {
                option.Action?.Invoke();
            }

            GainKingdomInfluenceAction.ApplyForDefault(mainHero, option.Influence);
            if (!hasSeenInquiry)
            {
                ShowInquiry();
            }
        }

        private void AddFood(MobileParty party, int limit)
        {
            foreach (ItemRosterElement itemRosterElement in party.ItemRoster)
            {
                if (!itemRosterElement.EquipmentElement.IsQuestItem)
                {
                    party.ItemRoster.Remove(itemRosterElement);
                }
            }

            while (party.Food < limit)
            {
                foreach (var itemObject in Items.All)
                {
                    if (!itemObject.IsFood || !(party.Food < limit))
                    {
                        continue;
                    }

                    var num2 = MBRandom.RoundRandomized(party.Party.NumberOfAllMembers *
                                                        (1f / itemObject.Value) * 16 * MBRandom.RandomFloat *
                                                        MBRandom.RandomFloat * MBRandom.RandomFloat *
                                                        MBRandom.RandomFloat);
                    if (num2 > 0)
                    {
                        party.ItemRoster.AddToCounts(itemObject, MBMath.ClampInt(num2, 1, limit - (int)party.Food));
                    }
                }
            }
        }

        private void OnGameLoaded()
        {
            if (!hasSeenInquiry)
            {
                CheckShowStartOptions();
            }
        }

        private void GiveTownsResources()
        {
            foreach (Town town in Town.AllFiefs)
            {
                town.ChangeGold((int)(town.Prosperity * 10f));
            }
        }

        private void GiveClansResources()
        {
            foreach (Clan clan in Clan.NonBanditFactions)
            {
                if (clan == Clan.PlayerClan) continue;

                int gold = (int)(clan.Tier * 25000f);
                float influence = clan.Tier * 200f;
                clan.Leader.ChangeHeroGold(gold);
                GainKingdomInfluenceAction.ApplyForDefault(clan.Leader, influence);
            }
        }

        public void OnCharacterCreationOver()
        {
            hasSeenInquiry = true;
            BannerKingsConfig.Instance.EducationManager.CorrectPlayerEducation();
            Concept.SetConceptTextLinks();

            var elements = new List<InquiryElement>();
            LearningElement start = new LearningElement(
                new TextObject("{=tScOpxMZ}Custom Start ({CURRENT})")
                .SetTextVariable("CURRENT", option != null ? option.Name : new TextObject("{=rj6769F8}None selected")),
                TextObject.Empty,
                new TextObject(),
                () => UIManager.Instance.ShowWindow("campaignStart"));
            elements.Add(new InquiryElement(start, start.Name.ToString(), null));

            int count = DefaultReligions.Instance.All.Count();
            TextObject relHint = TextObject.Empty;
            bool relAllowed = true;
            if (count == 0)
            {
                relAllowed = false;
                relHint = new TextObject("{=4W1sGEzQ}You do not have any religions installed. Install the separate mod, 'Banner Kings: Cultures Expanded' for the official, lore-based BK religions. Alternatively, install another third party mod that adds BK religions.");
            }

            LearningElement religion = new LearningElement(
                new TextObject("{=Y1giFY9O}Religion ({CURRENT})")
                .SetTextVariable("CURRENT", this.religion != null ? this.religion.Faith.GetFaithName() : new TextObject("{=rj6769F8}None selected")),
                TextObject.Empty,
                relHint,
                () => UIManager.Instance.ShowWindow("religionStart"));
            elements.Add(new InquiryElement(religion, 
                religion.Name.ToString(), 
                null, 
                relAllowed, 
                religion.Hint.ToString()));

            LearningElement importantConcepts = new LearningElement(
                new TextObject("{=f3GuDtEU}Important Concepts"),
                TextObject.Empty,
                new TextObject("{=ju5qvoMH}The main topics a newcomer to the mod should become familiarized with, concerning essential gameplay systems and balance changes."),
                () => ShowInquiry2());
            elements.Add(new InquiryElement(importantConcepts, importantConcepts.Name.ToString(), null, true, importantConcepts.Hint.ToString()));

            LearningElement concepts = new LearningElement(
                new TextObject("{=j9h1yOpp}Learning Concepts"),
                TextObject.Empty,
                new TextObject("{=cMzxQoUS}Learn about the various topics of the mod."),
                () => ShowInquiry());
            elements.Add(new InquiryElement(concepts, concepts.Name.ToString(), null, true, concepts.Hint.ToString()));     

            LearningElement finish = new LearningElement(
                new TextObject("{=ktbC3W5M}Finish"),
                TextObject.Empty,
                new TextObject("{=SgJXJKeu}Start the game with the chosen start and religion."),
                () =>
                {
                    RunReligion();
                    RunStartOption();
                });
            elements.Add(new InquiryElement(finish, finish.Name.ToString(), null, true, finish.Hint.ToString()));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=WaBMVVH9}Banner Kings").ToString(),
                new TextObject("{=pZ8F1HZR}Welcome to the Banner Kings mod. Before starting your campaign, you should pick a custom start and a religion for your character first. You can also read on topics of the mod. All these topics are available on the Concepts page of Encyclopedia.{newline}{newline}Support is only provided through Discord. Please check out the mod-links over there if you consider contributing. Have fun!").ToString(),
                elements,
                false,
                1,
                1,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    LearningElement result = (LearningElement)list[0].Identifier;
                    result.Action();
                },
                null), 
                true,
                false);
        }

        private void ShowInquiry()
        {
            var elements = new List<InquiryElement>();
            foreach (Concept concept in Concept.All)
            {
                if (concept.StringId.StartsWith("str_bk"))
                {
                    LearningElement element = new LearningElement(concept.Title, concept.Description, TextObject.Empty, null, concept);
                    elements.Add(new InquiryElement(element, element.Name.ToString(), null, true, element.Hint.ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=PbR13RE6}Banner Kings - Learning Concepts").ToString(),
                new TextObject("{=ZnFCNKON}To access these concepts again, access Concepts in Encyclopedia. All names and descriptions here are exactly what is present over there.").ToString(),
                elements,
                true,
                1,
                1,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    var result = (LearningElement)list[0].Identifier;
                    ShowInnerInquiry(result);
                },
                delegate (List<InquiryElement> list)
                {
                    OnCharacterCreationOver();
                }), true);
        }

        private void ShowInquiry2()
        {
            var elements = new List<InquiryElement>();
            List<Concept> concepts = new List<Concept>(10);
            foreach (Concept concept in Concept.All)
            {
                if (concept.StringId == "str_bk_religions" || concept.StringId == "str_bk_join_religion" || 
                    concept.StringId == "str_bk_titles" || concept.StringId == "str_bk_knights" ||
                    concept.StringId == "str_bk_peerage" || concept.StringId == "str_bk_gentry" ||
                    concept.StringId == "str_bk_spouse" || concept.StringId == "str_bk_education" ||
                    concept.StringId == "str_bk_party_supplies")
                {
                    concepts.Add(concept);
                }
            }

            foreach (Concept concept in concepts)
            {
                LearningElement element = new LearningElement(concept.Title, concept.Description, TextObject.Empty, null, concept);
                elements.Add(new InquiryElement(element, element.Name.ToString(), null, true, element.Hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=DWiRFSfS}Banner Kings - Important Concepts").ToString(),
                new TextObject("{=ZnFCNKON}To access these concepts again, access Concepts in Encyclopedia. All names and descriptions here are exactly what is present over there.").ToString(),
                elements,
                true,
                1,
                1,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    var result = (LearningElement)list[0].Identifier;
                    ShowInnerInquiry2(result);
                },
                delegate (List<InquiryElement> list)
                {
                    OnCharacterCreationOver();
                }), true);
        }

        private void ShowInnerInquiry(LearningElement element)
        {
            /*TaleWorlds.CampaignSystem.Campaign.Current.EncyclopediaManager.GoToLink(element.Concept.EncyclopediaLink);
            ShowInquiry();*/
            InformationManager.ShowInquiry(new InquiryData(element.Name.ToString(),
                element.Description.ToString(),
                true,
                false,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                () => ShowInquiry(),
                null,
                string.Empty));
        }

        private void ShowInnerInquiry2(LearningElement element)
        {
            /*TaleWorlds.CampaignSystem.Campaign.Current.EncyclopediaManager.GoToLink(element.Concept.EncyclopediaLink);
            ShowInquiry();*/
            InformationManager.ShowInquiry(new InquiryData(element.Name.ToString(),
                element.Description.ToString(),
                true,
                false,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                () => ShowInquiry2(),
                null,
                string.Empty));
        }

        private class LearningElement
        {
            public LearningElement(TextObject name, TextObject description, TextObject hint, Action action = null, Concept concept = null)
            {
                Name = name;
                Description = description;
                Hint = hint;
                Action = action;
                Concept = concept;
            }

            public Concept Concept { get; }
            public TextObject Name { get; }
            public TextObject Description { get; }
            public TextObject Hint { get; }
            public Action Action { get; }
        }
    }
}
