using BannerKings.Managers.CampaignStart;
using BannerKings.UI;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKCampaignStartBehavior : CampaignBehaviorBase
    {

        private StartOption option;
        public CampaignTime StartTime { get; private set; } = CampaignTime.Never;

        public override void RegisterEvents()
        {
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, new Action(OnCharacterCreationOver));
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData) option = null;

            dataStore.SyncData("bannerkings-campaignstart-option", ref option);
            dataStore.SyncData("bannerkings-campaignstart-time", ref option);
        }

        public void SetStartOption(StartOption option)
        {
            this.option = option;
            option.Action?.Invoke();
            StartTime = CampaignTime.Now;

            Hero mainHero = Hero.MainHero;
            mainHero.ChangeHeroGold(option.Gold - mainHero.Gold);
            GainKingdomInfluenceAction.ApplyForDefault(mainHero, option.Influence);
            AddFood(MobileParty.MainParty, option.Food);
            if (option.Lifestyle != null)
                BannerKingsConfig.Instance.EducationManager.SetStartOptionLifestyle(Hero.MainHero, option.Lifestyle);

            if (option.IsCriminal)
            {
                Settlement settlement = SettlementHelper.FindNearestSettlement(x => x.OwnerClan != null && x.OwnerClan.Kingdom != null, null);
                ChangeCrimeRatingAction.Apply(settlement.OwnerClan.Kingdom, option.Criminal);
            }

            ShowInquiry();
        }

        private void AddFood(MobileParty party, int limit)
        {
            party.ItemRoster.Clear();
            while (party.Food < limit)
            {
                foreach (ItemObject itemObject in Items.All)
                    if (itemObject.IsFood && party.Food < limit)
                    {
                        int num2 = MBRandom.RoundRandomized(party.Party.NumberOfAllMembers *
                            (1f / itemObject.Value) * 16 * MBRandom.RandomFloat *
                            MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
                        if (num2 > 0) party.ItemRoster.AddToCounts(itemObject, MBMath.ClampInt(num2, 1, limit - (int)party.Food));
                    }
            }
        }

        private void OnCharacterCreationOver() => UIManager.Instance.ShowWindow("campaignStart");
        

        private void ShowInquiry()
        {
            List<InquiryElement> elements = new List<InquiryElement>();
            LearningElement religions = new LearningElement(new TextObject("{=!}Religions"), 
                new TextObject("{=!}"), 
                new TextObject("{=!}Learn about the novel faiths in the continent."));
            elements.Add(new InquiryElement(religions, religions.Name.ToString(), null, true, religions.Hint.ToString()));

            LearningElement settlements = new LearningElement(new TextObject("{=!}Settlement Management"),
                new TextObject("{=!}In Banner Kings, settlement management is several layers more in depth than vanilla. Settlemetns have many new interconnected factors such as Mercantilism, Militarism, Stability, Autonomy, and others, making settlement management way more intricate than choosing a new town project every few weeks or so. They also have several new policies (multi choice options) and decisions (true/false options) that it's owner may manage. You can check all this information in Demesne Management tab in your settlements - all of the fields have tooltips with further explanations."),
                new TextObject("{=!}Learn about the extensive changes to settlements."));
            elements.Add(new InquiryElement(settlements, settlements.Name.ToString(), null, true, settlements.Hint.ToString()));

            LearningElement skills = new LearningElement(new TextObject("{=!}Game Balance - Skills"),
                new TextObject("{=!}Banner Kings introduces a new Attribute - Wisdom - as well as 3 new skills: Scholarship, Lordship and Theology. On top of that, skill modifiers are changed. For starters, the minimum experience gain is now 5% rather than 0%. This means that the vanilla game would block you from learning skills past a certain past - this is not the case anymore. You can always keep learning, albeit very slowly. Learning limits are also changed - attribute points provided mere 5 additional learning limit to skills, despite being the hardest point to acquire. Instead they now provide 15."),
                new TextObject("{=!}Learn about the new skills and experience gainning changes."));
            elements.Add(new InquiryElement(skills, skills.Name.ToString(), null, true, skills.Hint.ToString()));

            LearningElement softLimits = new LearningElement(new TextObject("{=!}Game Balance - Soft Limits"),
                new TextObject("{=!}Among the many new stats tracking heroes and settlements are the Demesne Limit and Vassal Limit features. These act as soft limits respectively for fief ownership and vassal clans / knights under you. The limits set the point of diminishing returns. Going over it too much will eventually lead to negative returns. Effectively, Demesne Limit makes you incapable to hold an indeterminate amount of fiefs - you will only be able to efficiently manage a few - while vassal limit makes oversized kingdoms increasingly unviable to manage. The vanilla game sets no limit to how much land or people you control - those limits severely restrict these abilities."),
                new TextObject("{=!}Learn about the new soft limits that significantly prevent snowballing or overgrowing."));
            elements.Add(new InquiryElement(softLimits, softLimits.Name.ToString(), null, true, softLimits.Hint.ToString()));

            LearningElement aiFinances = new LearningElement(new TextObject("{=!}Game Balance - AI Finances & War"),
                new TextObject("{=!}In Banner Kings, AI lords now have much more dynamic financial lives. AI clans will buy caravans and workshops in similar manner to player. The same costs, incomes and restrictions apply. Over time, this means AI clans will build several sources of income, allowing them to grow bigger and stronger, or have reserves then losing their settlements. Another major change is that AI will now save money during peace time. Lords will roam the world with small parties - enough to not get consistently captured by bandits - saving their reserves for war time. As a consequence of that, settlements train their volunteers - when war time comes, armies will be immediatly drafted and made mostly of quality troops instead of recruits."),
                new TextObject("{=!}Learn about the expansions to AI financial decisions that make the game more dynamic."));
            elements.Add(new InquiryElement(aiFinances, aiFinances.Name.ToString(), null, true, aiFinances.Hint.ToString()));

            LearningElement economy = new LearningElement(new TextObject("{=!}Economy"),
                new TextObject("{=!}"),
                new TextObject("{=!}Learn about the various improvements over the economy system."));
            elements.Add(new InquiryElement(economy, economy.Name.ToString(), null, true, economy.Hint.ToString()));

            LearningElement education = new LearningElement(new TextObject("{=!}Educations"),
                new TextObject("{=!}"),
                new TextObject("{=!}Learn about the novel education system and it's aspects."));
            elements.Add(new InquiryElement(education, education.Name.ToString(), null, true, education.Hint.ToString()));

            LearningElement titles = new LearningElement(new TextObject("{=!}Titles"),
                new TextObject("{=!}In Banner Kings, legal titles exist in what is called Demesne (domain) Hierarchy. Titles may be landed or not. Landed titles are directly attached to a fief - lordships are village titles, baronies are castle titles, counties are town titles. Above these are unlanded titles, such as dukedoms and kingdoms. The hierarchy stablishes suzerain-vassal relationships between the title holders. Holding a title has several impacts, mainly affecting your Vassal Limit and Demesne Limit, that limit your number of vassal clans & knights, and your limit of fiefs, respectively. Every faction at game start has a kingdom-level title that represents them. This title is passed on to successive leaders of the faction, in different forms, depending on the title's Succession law."),
                new TextObject("{=!}Learn about the novel titles and their usages."));
            elements.Add(new InquiryElement(titles, titles.Name.ToString(), null, true, titles.Hint.ToString()));

            LearningElement titleLaws = new LearningElement(new TextObject("{=!}Titles - Laws"),
                new TextObject("{=!}Titles are also composed of laws, duties and rights, that together form the contract. All titles in the same hierarchy have the same contract. All titles have Succession, Inheritance, Government and Gender laws. Succession describes how the faction leadership is inherited, while Inheritance describes the clans' leadership inheritance. Government has various passive effects on settlements as well as dictate what kingdom policies are possible, and sometimes restricts the other laws. Gender law describes the preference for a certain gender in terms of granting knighthood, faction succession and clan inheritance."),
                new TextObject("{=!}Learn more about the various laws that compose titles."));
            elements.Add(new InquiryElement(titleLaws, titleLaws.Name.ToString(), null, true, titleLaws.Hint.ToString()));

            LearningElement titleDutiesRights = new LearningElement(new TextObject("{=!}Titles - Duties & Rights"),
                new TextObject("{=!}Titles are also composed of laws, duties and rights, that together form the contract. All titles in the same hierarchy have the same contract. Duties and Rights are fulfilled between vassal and suzerain. Your suzerain is the title holder of the title directly above your highest title. Say you have a dukedom title - in the hierarchy, the dukedom is under a kingdom. The kingdom title holder will be your suzerain, and you their vassal. Vassals fulfill Duties such as paying taxes, or participating in armies (the player is coerced to participate, with renown penalties). Suzerains fulfill rights such as granting titles or financial aids in certain circunstances."),
                new TextObject("{=!}Learn about the duties and rights between vassals and suzerains."));
            elements.Add(new InquiryElement(titleDutiesRights, titleDutiesRights.Name.ToString(), null, true, titleDutiesRights.Hint.ToString()));

            LearningElement knighthood = new LearningElement(new TextObject("{=!}Knighthood & Dynamic Clans"),
                new TextObject("{=!}In Banner Kings, companions are no longer able to raise clan parties. Instead, they can be knighted, a process that involves spending influence and denars. Once knighted, they can raise a party. It also requires a lordship title, who the knight will receive and use it's income to pay their own party - you only pay for those led by your family members. Knights can eventualy grow as lords and found their own clan. Knights are used by the AI, meaning many new lords are travelling on the map, and eventually many new clans sprout. The player may try to stop their own knights from creating clans."),
                new TextObject("{=!}Learn about the new lords on the map, knights."));
            elements.Add(new InquiryElement(knighthood, knighthood.Name.ToString(), null, true, knighthood.Hint.ToString()));

            LearningElement councils = new LearningElement(new TextObject("{=!}Councils"),
                new TextObject("{=!}"),
                new TextObject("{=!}Learn about the novel clan and royal councils."));
            elements.Add(new InquiryElement(councils, councils.Name.ToString(), null, true, councils.Hint.ToString()));

            LearningElement innovations = new LearningElement(new TextObject("{=!}Innovations"),
                new TextObject("{=!}"),
                new TextObject("{=!}Learn about the novel cultural innovations."));
            elements.Add(new InquiryElement(innovations, innovations.Name.ToString(), null, true, innovations.Hint.ToString()));

            LearningElement smithing = new LearningElement(new TextObject("{=!}Smithing"),
                new TextObject("{=!}"),
                new TextObject("{=!}Learn about the various changes that fix smithing as an exploit and expand it's uses."));
            elements.Add(new InquiryElement(smithing, smithing.Name.ToString(), null, true, smithing.Hint.ToString()));


            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(new TextObject("{=!}Banner Kings").ToString(),
                new TextObject("{=!}Welcome to the Banner Kings mod. BK is a comprehensive mod that alters and expands various of Bannerlord's non combat systems. Below are some topics you can learn more about the impacts of the mod. You can later revisit these topics in the Concepts part of Bannerlord's encyclopedia. Visit the mod page for Discord and donation links - support is only provided through Discord. Have fun!").ToString(),
                elements,
                true,
                1,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    LearningElement result = (LearningElement)list[0].Identifier;
                    ShowInnerInquiry(result);
                },
                delegate (List<InquiryElement> list)
                {
                    BannerKingsConfig.Instance.ReligionsManager.ShowPopup();
                }), true);
        }

        private void ShowInnerInquiry(LearningElement element)
        {
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

        class LearningElement
        {
            private TextObject name, description, hint;
            public LearningElement(TextObject name, TextObject description, TextObject hint)
            {
                this.name = name;
                this.description = description;
                this.hint = hint;
            }

            public TextObject Name => name;
            public TextObject Description => description;
            public TextObject Hint => hint;
        }
    }
}
