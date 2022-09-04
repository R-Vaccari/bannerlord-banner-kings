using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers;
using BannerKings.Managers.CampaignStart;
using BannerKings.UI;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKCampaignStartBehavior : CampaignBehaviorBase
    {
        private bool hasSeenInquiry;
        private StartOption option;
        private CampaignTime startTime = CampaignTime.Never;

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
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnGameCreated);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener(this, OnCharacterCreationOver);
            CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener(this, OnGameLoaded);
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
            BannerKingsConfig.Instance.InitManagers();
        }

        public void SetStartOption(StartOption option)
        {
            this.option = option;
            startTime = CampaignTime.Now;

            var mainHero = Hero.MainHero;
            mainHero.ChangeHeroGold(option.Gold - mainHero.Gold);

            AddFood(MobileParty.MainParty, option.Food);

            if (option.Lifestyle != null)
            {
                BannerKingsConfig.Instance.EducationManager.SetStartOptionLifestyle(Hero.MainHero, option.Lifestyle);
            }

            if (option.IsCriminal)
            {
                var settlement = SettlementHelper.FindNearestSettlement(x => x.OwnerClan is {Kingdom: { }});
                ChangeCrimeRatingAction.Apply(settlement.OwnerClan.Kingdom, option.Criminal);
            }

            if (option.Action != null)
            {
                option.Action?.Invoke();
            }

            GainKingdomInfluenceAction.ApplyForDefault(mainHero, option.Influence);

            InitializeAllData();
            ShowInquiry();
        }

        private void AddFood(MobileParty party, int limit)
        {
            party.ItemRoster.Clear();
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
                        party.ItemRoster.AddToCounts(itemObject, MBMath.ClampInt(num2, 1, limit - (int) party.Food));
                    }
                }
            }
        }

        private void OnGameCreated(CampaignGameStarter starter)
        {
            InitializeAllData();
        }

        private void OnGameLoaded()
        {
            if (!hasSeenInquiry)
            {
                ShowInquiry();
            }

            PostInitialize();
        }

        private void OnCharacterCreationOver()
        {
            UIManager.Instance.ShowWindow("campaignStart");
        }

        private void InitializeAllData()
        {
            if (hasSeenInquiry)
            {
                return;
            }

            BannerKingsConfig.Instance.InitManagers();
            foreach (var settlement in Settlement.All.Where(settlement => settlement.IsVillage || settlement.IsTown || settlement.IsCastle))
            {
                PopulationManager.InitializeSettlementPops(settlement);
            }

            foreach (var clan in Clan.All.Where(clan => !clan.IsEliminated && !clan.IsBanditFaction))
            {
                BannerKingsConfig.Instance.CourtManager.CreateCouncil(clan);
            }

            foreach (var hero in Hero.AllAliveHeroes)
            {
                BannerKingsConfig.Instance.EducationManager.InitHeroEducation(hero);
            }

            BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
        }

        private void PostInitialize()
        {
            BannerKingsConfig.Instance.EducationManager.PostInitialize();
            BannerKingsConfig.Instance.InnovationsManager.PostInitialize();
            BannerKingsConfig.Instance.ReligionsManager.PostInitialize();
            BannerKingsConfig.Instance.GoalManager.PostInitialize();
        }

        private void ShowInquiry()
        {
            hasSeenInquiry = true;
            var elements = new List<InquiryElement>();
            var religions = new LearningElement(new TextObject("{=LUF7xieE}Religions"),
                new TextObject("{=!}In Banner Kings, religions are attached to cultures and possibly factions. All heroes (lords, notables) and settlements of that group will follow the adequate faith. All religions are defined by a few core aspects, their doctrines, rites, divinities and clergy.\nDoctrines are features of the faith - they may introduce buffs and debuffs, or slightly change the faith's behavior.\nClergy are the representatives of the faith. They function as notables in settlements. Each faith has a different hierarchy of clergy, with different titles and spawning at different types of settlements. The clergy are how heroes may interact with the faith\nThrough them you may perform rites, which will award you with piety, as well as receive blessings from the divinities."),
                new TextObject("{=ow1a4qxs}Learn about the novel faiths in the continent."));
            elements.Add(new InquiryElement(religions, religions.Name.ToString(), null, true, religions.Hint.ToString()));

            var settlements = new LearningElement(new TextObject("{=hwQxchW0}Settlement Management"),
                new TextObject("{=!}In Banner Kings, settlement management is several layers more in depth than vanilla. Settlements have many new interconnected factors such as Mercantilism, Militarism, Stability, Autonomy, and others, making settlement management way more intricate than choosing a new town project every few weeks or so. They also have several new policies (multi choice options) and decisions (true/false options) that it's owner may manage.\nYou can check all this information in Demesne Management tab in your settlements - all of the fields have tooltips with further explanations."),
                new TextObject("{=kDkATiAT}Learn about the extensive changes to settlements."));
            elements.Add(new InquiryElement(settlements, settlements.Name.ToString(), null, true,
                settlements.Hint.ToString()));

            var skills = new LearningElement(new TextObject("{=zdgoYNq8}Game Balance - Skills"),
                new TextObject("{=ALDs4Tpn}Banner Kings introduces a new Attribute - Wisdom - as well as 3 new skills: Scholarship, Lordship and Theology. These new skills provide new ways to acquire experience and level up. On top of that, skill modifiers are changed. For starters, the minimum experience gain is now 5% rather than 0%. This means that the vanilla game would block you from learning skills past a certain past - this is not the case anymore. You can always keep learning, albeit very slowly."),
                new TextObject("{=8PsrPfnu}Learn about the new skills and experience gainning changes."));
            elements.Add(new InquiryElement(skills, skills.Name.ToString(), null, true, skills.Hint.ToString()));

            var softLimits = new LearningElement(new TextObject("{=0Oz1EwY8}Game Balance - Soft Limits"),
                new TextObject("{=v779BscB}Among the many new stats tracking heroes and settlements are the Demesne Limit and Vassal Limit features. These act as soft limits respectively for fief ownership and vassal clans / knights under you. The limits set the point of diminishing returns. Going over it too much will eventually lead to negative returns. Effectively, Demesne Limit makes you incapable to hold an indeterminate amount of fiefs - you will only be able to efficiently manage a few - while vassal limit makes oversized kingdoms increasingly unviable to manage. The vanilla game sets no limit to how much land or people you control - those limits severely restrict these abilities."),
                new TextObject("{=bV8R0OtS}Learn about the new soft limits that significantly prevent snowballing or overgrowing."));
            elements.Add(new InquiryElement(softLimits, softLimits.Name.ToString(), null, true,
                softLimits.Hint.ToString()));

            var aiFinances = new LearningElement(new TextObject("{=Y7CD0UET}Game Balance - AI Finances & War"),
                new TextObject("{=0wyLYy4O}In Banner Kings, AI lords now have much more dynamic financial lives. AI clans will buy caravans and workshops in similar manner to player. The same costs, incomes and restrictions apply.\nOver time, this means AI clans will build several sources of income, allowing them to grow bigger and stronger, or have reserves then losing their settlements.\nAnother major change is that AI will now save money during peace time. Lords will roam the world with small parties - enough to not get consistently captured by bandits - saving their reserves for war time. As a consequence of that, settlements train their volunteers - when war time comes, armies will be immediatly drafted and made mostly of quality troops instead of recruits.\nLastly, notables will now financially aid the clans they are supporters of. This creates a flow of currency from notables to lords, making them a more active part of wartimes."),
                new TextObject("{=xE67VpmQ}Learn about the expansions to AI financial decisions that make the game more dynamic."));
            elements.Add(new InquiryElement(aiFinances, aiFinances.Name.ToString(), null, true,
                aiFinances.Hint.ToString()));

            var food = new LearningElement(new TextObject("{=xrP4hW4m}Game Balance - Settlement Food"),
                new TextObject("{=fgcGuVJa}Food is extensively reworked. Food in settlements is now produced and consumed in the hundreds by the day - every single person in the population eats, in a similar rate to your soldiers. Food in settlements is now dictated by settlement acreage and it's workforce. The settlement needs acres ready to work and people - serfs and slaves - to work on them. Settlements will no longer starve with markets full of food - the population will buy off the market stocks when production does not meed demand, meaning true starvating will only start when markets are completely out of food. Food stocks are much higher, based on population. Excess food in stocks will rot. If the limit is reached, the population sells food items to the market instead of the food simply disappearing. Food can be manually dumped into the settlement by stocking the Stack with food - it will only be consumed when the normal reserves are very low."),
                new TextObject("{=xE67VpmQ}Learn about the expansions to AI financial decisions that make the game more dynamic."));
            elements.Add(new InquiryElement(food, food.Name.ToString(), null, true, food.Hint.ToString()));

            var economy = new LearningElement(new TextObject("{=2oJQ4Snn}Economy"),
                new TextObject("{=audvSv7u}In Banner Kings, the economy landscape is quite different.\nDemand is generated by population in settlements, and therefore much more dynamic than before as population classes fluctuate, and each class has different demands.\nTrade goods have modifiers such as 'fine', 'masterwork' or 'crude', what opens new possibilities in terms of profit.\nManufactured items are on average quite more expensive, making workshops and caravans significantly more profitable.\nWorkshops owners now pay taxes to the settlement owner.\nSettlement market gold no longer resets on a daily basis - settlements can accumulate large amounts of gold, or go bankrupt, hence need some time to recover."),
                new TextObject("{=WwRKELwd}Learn about the various improvements over the economy system."));
            elements.Add(new InquiryElement(economy, economy.Name.ToString(), null, true, economy.Hint.ToString()));

            var economyVillages = new LearningElement(new TextObject("{=2oJQ4Snn}Economy - Villages"),
                new TextObject("{=6fErfVWf}Villages produce income by selling their production outputs. Villages will now produce based on population workforce, as well as available acreage, if fitting. For example, agricultural productions are limited by the amount of acres available and their production capacity - even if you have extra workers, the fields can only produce so much.\nAs a result, villages can produce much more income than before. This is extra relevant for Knighthood, which you can read more about in the articles below."),
                new TextObject("{=WwRKELwd}Learn about the various improvements over the economy system."));
            elements.Add(new InquiryElement(economyVillages, economyVillages.Name.ToString(), null, true,
                economyVillages.Hint.ToString()));


            var education = new LearningElement(new TextObject("{=Dmdy2KUu}Educations"),
                new TextObject("{=!}In Banner Kings, Educations are now a big part of heroes' personal lives. Heroes may learn languages, read books and adopt lifestyles. Languages may be taught to you by someone fluent in it.\nBooks are sold by the few book sellers in the continent. Each book is written in a different language - you can not read any book at will.\nLifestyles present a new way to specialize in a specific way of living. Lifestyles are composed by 2 skills, and advance slowly through time. At some point, you need to reach a skill threshold to keep advancing. Advancement is rewarded with unique, powerful perks each lifestyle contains. In addition to these, lifestyles have passive effects - both a negative and positive one, which respectively punish and reward you for playing as the lifestyle is intended to be played."),
                new TextObject("{=jtu8D8Ws}Learn about the novel education system and it's aspects."));
            elements.Add(new InquiryElement(education, education.Name.ToString(), null, true, education.Hint.ToString()));

            var titles = new LearningElement(new TextObject("{=2qXtnwSn}Titles"),
                new TextObject("{=QBywqQOA}In Banner Kings, legal titles exist in what is called Demesne (domain) Hierarchy. Titles may be landed or not. Landed titles are directly attached to a fief - lordships are village titles, baronies are castle titles, counties are town titles. Above these are unlanded titles, such as dukedoms and kingdoms. The hierarchy stablishes suzerain-vassal relationships between the title holders. Holding a title has several impacts, mainly affecting your Vassal Limit and Demesne Limit, that limit your number of vassal clans & knights, and your limit of fiefs, respectively. Every faction at game start has a kingdom-level title that represents them. This title is passed on to successive leaders of the faction, in different forms, depending on the title's Succession law."),
                new TextObject("{=awGcr7kt}Learn about the novel titles and their usages."));
            elements.Add(new InquiryElement(titles, titles.Name.ToString(), null, true, titles.Hint.ToString()));

            var titleLaws = new LearningElement(new TextObject("{=2qXtnwSn}Titles - Laws"),
                new TextObject("{=2qXtnwSn}Titles are also composed of laws, duties and rights, that together form the contract. All titles in the same hierarchy have the same contract. All titles have Succession, Inheritance, Government and Gender laws. Succession describes how the faction leadership is inherited, while Inheritance describes the clans' leadership inheritance. Government has various passive effects on settlements as well as dictate what kingdom policies are possible, and sometimes restricts the other laws. Gender law describes the preference for a certain gender in terms of granting knighthood, faction succession and clan inheritance."),
                new TextObject("{=U0i1ViDT}Learn more about the various laws that compose titles."));
            elements.Add(new InquiryElement(titleLaws, titleLaws.Name.ToString(), null, true, titleLaws.Hint.ToString()));

            var titleDutiesRights = new LearningElement(new TextObject("{=2qXtnwSn}Titles - Duties & Rights"),
                new TextObject("{=2qXtnwSn}Titles are also composed of laws, duties and rights, that together form the contract. All titles in the same hierarchy have the same contract. Duties and Rights are fulfilled between vassal and suzerain. Your suzerain is the title holder of the title directly above your highest title. Say you have a dukedom title - in the hierarchy, the dukedom is under a kingdom. The kingdom title holder will be your suzerain, and you their vassal. Vassals fulfill Duties such as paying taxes, or participating in armies (the player is coerced to participate, with renown penalties). Suzerains fulfill rights such as granting titles or financial aids in certain circunstances."),
                new TextObject("{=ozmTcXCx}Learn about the duties and rights between vassals and suzerains."));
            elements.Add(new InquiryElement(titleDutiesRights, titleDutiesRights.Name.ToString(), null, true,
                titleDutiesRights.Hint.ToString()));

            var knighthood = new LearningElement(new TextObject("{=3zJ0YuXS}Knighthood & Dynamic Clans"),
                new TextObject("{=19HT7FGR}In Banner Kings, companions are no longer able to raise clan parties. Instead, they can be knighted, a process that involves spending influence and denars. Once knighted, they can raise a party. It also requires a lordship title, who the knight will receive and use it's income to pay their own party - you only pay for those led by your family members. Knights can eventualy grow as lords and found their own clan. Knights are used by the AI, meaning many new lords are travelling on the map, and eventually many new clans sprout. The player may try to stop their own knights from creating clans."),
                new TextObject("{=W3vTvLzq}Learn about the new lords on the map, knights."));
            elements.Add(new InquiryElement(knighthood, knighthood.Name.ToString(), null, true,
                knighthood.Hint.ToString()));

            var councils = new LearningElement(new TextObject("{=RzNg7v1H}Councils"),
                new TextObject("{=!}In Banner Kings, clans have a council. Councils are composed by 5 roles - marshal, steward, chancellor, spymaster and religious advisor.\nThese roles may be performed by notables or vassals, and each of them requires a specific skillset. Therefore, different candidates will have different competences at each role.\nCouncils may also be Royal Councils. These are the councils of the faction leading clans. These councils have extra positions and characteristics. The new positions are dynamic and depend on various factors such as kingdom culture and government form. Lastly, Royal Councils' 5 main roles may only be filled by lords, the king's vassals, and not by lowly notables."),
                new TextObject("{=rvZCvLgV}Learn about the novel clan and royal councils."));
            elements.Add(new InquiryElement(councils, councils.Name.ToString(), null, true, councils.Hint.ToString()));

            var innovations = new LearningElement(new TextObject("{=Sj0dhRwh}Innovations"),
                new TextObject("{=!}In Banner Kings, innovations in technology and society happen over time. These may alter several aspects - mainly in settlements - such as agricultural output or general production efficiency.\nEach culture has a set of innovations that advances independently from others. They also have a Fascination - an innovation that develops faster.\nInnovations are developed with research points. These are generated by settlements, mainly settlements' noble population. The more settlements a culture has, the faster it advances."),
                new TextObject("{=sV6intBt}Learn about the novel cultural innovations."));
            elements.Add(new InquiryElement(innovations, innovations.Name.ToString(), null, true,
                innovations.Hint.ToString()));

            var smithing = new LearningElement(new TextObject("{=9cjjh1VG}Smithing"),
                new TextObject("{=!}In Banner Kings, smithing is significantly extended with the possibility of crafting bardings, shields, ammunitions and armor pieces. Unlike weapon crafting, these have a botching chance, based on the piece's difficulty compared to your smithing skill. When botched, you get xp, but no item.\nIn addition, smithing is made harder. Everything costs more stamina to build. Every point of stamina spent correlates to time spent in the map - spending all your stamina will make you wait several hours in the settlement, working on your smithing. These hours are also paid hours, because the smithy is not yours. Essentialy, Banner Kings aims to fix smithing as a massive exploit in the game, while extending it's uses."),
                new TextObject("{=J9mNrDgM}Learn about the various changes that fix smithing as an exploit and expand it's uses."));
            elements.Add(new InquiryElement(smithing, smithing.Name.ToString(), null, true, smithing.Hint.ToString()));


            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=WaBMVVH9}Banner Kings").ToString(),
                new TextObject("{=CpymySe7}Welcome to the Banner Kings mod. BK is a comprehensive mod that alters and expands various of Bannerlord's non combat systems. Below are some topics you can learn more about the impacts of the mod. You can later revisit these topics in the Concepts part of Bannerlord's encyclopedia. Visit the mod page for Discord and donation links - support is only provided through Discord. Have fun!").ToString(),
                elements,
                true,
                1,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                delegate(List<InquiryElement> list)
                {
                    var result = (LearningElement) list[0].Identifier;
                    ShowInnerInquiry(result);
                },
                delegate { BannerKingsConfig.Instance.ReligionsManager.ShowPopup(); }), true);
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

        private class LearningElement
        {
            public LearningElement(TextObject name, TextObject description, TextObject hint)
            {
                Name = name;
                Description = description;
                Hint = hint;
            }

            public TextObject Name { get; }

            public TextObject Description { get; }

            public TextObject Hint { get; }
        }
    }
}