using BannerKings.Managers.Education.Lifestyles;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Linq;

namespace BannerKings.Managers.CampaignStart
{
    public class DefaultStartOptions : DefaultTypeInitializer<DefaultStartOptions, StartOption>
    {
        private StartOption adventurer, indebtedLord, mercenary;

        public StartOption Adventurer => adventurer;
        public StartOption IndebtedLord => indebtedLord;
        public StartOption Mercenary => mercenary;

        public override IEnumerable<StartOption> All
        {
            get
            {
                yield return Adventurer;
                yield return IndebtedLord;
                yield return Mercenary;
            }
        }

        public override void Initialize()
        {

            MBReadOnlyList<CharacterObject> characters = Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>();


            adventurer = new StartOption("start_adventurer");
            adventurer.Initialize(new TextObject("{=!}Adventurer"),
                new TextObject("{=!}A free spirit, you are roaming the continent without constraints, or a clear objective. The world is for the taking, will you take your share?"),
                new TextObject("{=!}Vanilla start. No troops, goods or any benefits."),
                1000, 0, 0, 50, 0f,
                null, 
                0f,
                null,
                null);

            indebtedLord = new StartOption("start_lord");
            indebtedLord.
                Initialize(new TextObject("{=!}Indebted Lord"),
                new TextObject("{=!}After a series of inherited problems and bad decisions, you find yourself in debt. Thankfuly, you are a landed lord, with income from your Lordship. A food supply and a small retinue accompany you, though their loyalty will be tested by the lack of denars..."),
                new TextObject("{=!}Start as a lord in a kingdom, with a Lordship title. Significant gold and influence onus that will take time to recuperate. Gain 25 lordship skill."), 
                -25000, 25, 10, 50, -100f,
                () =>
                {

                }, 
                0f,
                null,
                null);

            mercenary = new StartOption("start_mercenary");
            mercenary.Initialize(new TextObject("{=!}Mercenary"),
                new TextObject("{=!}You serve as a free mercenary company, roaming around the continent in search of employment. After a long period of joblessness, you find your company in the verge of collapse, with little morale, food and finances."),
                new TextObject("{=!}Start with a mercenary band, in desperate need for plundering gold and food. Mercenary lifestyle is kickstarted as part of your education."),
                250, 0, 22, 30, 0f,
                () =>
                {
                    TroopRoster roster = MobileParty.MainParty.MemberRoster;
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_1"), 4);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_2"), 6);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_4"), 4);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_3"), 3);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_5"), 5);
                }, 
                0.5f,
                null,
                DefaultLifestyles.Instance.Mercenary);
        }
    }
}
