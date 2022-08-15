using BannerKings.Managers.Education.Lifestyles;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using Helpers;

namespace BannerKings.Managers.CampaignStart
{
    public class DefaultStartOptions : DefaultTypeInitializer<DefaultStartOptions, StartOption>
    {
        private StartOption adventurer, indebtedLord, mercenary, outlaw, caravaneer;

        public StartOption Adventurer => adventurer;
        public StartOption IndebtedLord => indebtedLord;
        public StartOption Mercenary => mercenary;
        public StartOption Outlaw => outlaw;
        public StartOption Caravaneer => caravaneer;

        public override IEnumerable<StartOption> All
        {
            get
            {
                yield return Adventurer;
                yield return IndebtedLord;
                yield return Mercenary;
                yield return Outlaw;
                yield return Caravaneer;
            }
        }

        public override void Initialize()
        {

            MBReadOnlyList<ItemObject> items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>(); 
            ItemObject mule = items.FirstOrDefault(x => x.StringId == "mule");
            ItemObject sumpter = items.FirstOrDefault(x => x.StringId == "sumpter_horse");


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
                new TextObject("{=!}Start as a lord in a kingdom, with a Lordship title. Significant gold and influence onus that will take time to recuperate. Influence gain is halved for 5 years. Gain 25 lordship skill."), 
                -25000, 25, 10, 50, -50f,
                () =>
                {
                    Settlement settlement = SettlementHelper.FindNearestSettlement(x => x.OwnerClan != null && x.OwnerClan.Kingdom != null, null);
                    Kingdom kingdom = settlement.OwnerClan.Kingdom;
                    if (kingdom == null) kingdom = Kingdom.All.GetRandomElement();
                    ChangeKingdomAction.ApplyByJoinToKingdom(Clan.PlayerClan, kingdom, false);
                    BannerKingsConfig.Instance.TitleManager.GiveLordshipOnKingdomJoin(kingdom, Clan.PlayerClan, true);
                    MobileParty.MainParty.ItemRoster.AddToCounts(sumpter, 2);
                }, 
                0f,
                null,
                null);

            mercenary = new StartOption("start_mercenary");
            mercenary.Initialize(new TextObject("{=!}Mercenary"),
                new TextObject("{=!}You serve as a free mercenary company, roaming around the continent in search of employment. After a long period of joblessness, you find your company in the verge of collapse, with little morale, food and finances."),
                new TextObject("{=!}Start with a mercenary band, in desperate need for plundering gold and food. Party morale reduced for 5 years. Mercenary lifestyle is kickstarted as part of your education."),
                250, 0, 22, 30, 0f,
                () =>
                {
                    MBReadOnlyList<CharacterObject> characters = Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>();
                    TroopRoster roster = MobileParty.MainParty.MemberRoster;
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_1"), 4);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_2"), 6);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_4"), 4);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_3"), 3);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_5"), 5);

                    MobileParty.MainParty.ItemRoster.AddToCounts(sumpter, 8);
                }, 
                50f,
                null,
                DefaultLifestyles.Instance.Mercenary);

            outlaw = new StartOption("start_outlaw");
            outlaw.Initialize(new TextObject("{=!}Outlaw"),
                new TextObject("{=!}Lacking in morals, you assemble a party of like-minded brigands, making a living out stealing and plundering. Your efforts, however, have not gone unnoticed by the local authorities."),
                new TextObject("{=!}Start with a outlaw band, in desperate need for plundering gold and food. Criminal rating does not reduce for 5 years. Outlaw lifestyle is kickstarted as part of your education."),
                50, 2, 15, 50, 0f,
                () =>
                {
                    MBReadOnlyList<PartyTemplateObject> templates = Game.Current.ObjectManager.GetObjectTypeList<PartyTemplateObject>();
                    string templateName = "looters_template";
                    
                    CultureObject culture = Hero.MainHero.Culture;
                    if (culture.StringId == "sturgia") templateName = "sea_raiders_template";
                    else if (culture.StringId == "battania") templateName = "forest_bandits_template";
                    else if (culture.StringId == "aserai") templateName = "desert_bandits_template";
                    else if (culture.StringId == "khuzait") templateName = "steppe_bandits_template";
                    else if (culture.StringId == "vlandia") templateName = "mountain_bandits_template";

                    PartyTemplateObject template = templates.First(x => x.StringId == templateName);
                    TroopRoster roster = MobileParty.MainParty.MemberRoster;
                    
                    for (int i = 0; i < 15; i++)
                    {
                        if (i >= 10 && template.Stacks.Count >= 2) roster.AddToCounts(template.Stacks[1].Character, 1);
                        else roster.AddToCounts(template.Stacks[0].Character, 1);
                    }

                    MobileParty.MainParty.ItemRoster.AddToCounts(sumpter, 6);
                },
                100f,
                null,
                DefaultLifestyles.Instance.Outlaw);

            caravaneer = new StartOption("start_caravaneer");
            caravaneer.Initialize(new TextObject("{=!}Robbed Caravaneer"),
                new TextObject("{=!}Your caravan has been recently harassed by criminals - most of your belongings are lost, and certainly all your denars. A few goods, mules and wounded soldiers remain."),
                new TextObject("{=!}Start with a wounded caravan, some food, mules and goods. Party speed is reduced by 5% for 5 years. Caravaneer lifestyle is kickstarted as part of your education."),
                0, 6, 12, 50, 0f,
                () =>
                {
                    MBReadOnlyList<PartyTemplateObject> templates = Game.Current.ObjectManager.GetObjectTypeList<PartyTemplateObject>();
                    PartyTemplateObject template = templates.FirstOrDefault(x => x.StringId == "caravan_template_" + Hero.MainHero.Culture.StringId);
                    MobileParty party = MobileParty.MainParty;

                    if (template != null)
                    {
                        TroopRoster roster = party.MemberRoster;
                        for (int i = 0; i < 12; i++)
                        {
                            bool wounded = MBRandom.RandomFloat < 0.6f;
                            if (i >= 7 && template.Stacks.Count >= 2) roster.AddToCounts(template.Stacks[1].Character, 1, false, wounded ? 1 : 0);
                            else roster.AddToCounts(template.Stacks[0].Character, 1, false, wounded ? 1 : 0);
                        }
                    }

                    party.ItemRoster.AddToCounts(sumpter, 3);
                    party.ItemRoster.AddToCounts(mule, 2);

                    int goodsValue = 0;
                    foreach (ItemObject itemObject in TaleWorlds.CampaignSystem.Items.AllTradeGoods)
                        if (!itemObject.IsFood && goodsValue < 1200)
                        {
                            int num2 = (int)(1f * (10f / 13f) / itemObject.Value * MBRandom.RandomFloat);
                            if (num2 > 0) party.ItemRoster.AddToCounts(itemObject, num2);
                            goodsValue += (int)(num2 * itemObject.Value);
                        }
                },
                0f,
                null,
                DefaultLifestyles.Instance.Caravaneer);
        }
    }
}
