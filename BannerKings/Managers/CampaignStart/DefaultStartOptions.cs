using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.CampaignStart
{
    public class DefaultStartOptions : DefaultTypeInitializer<DefaultStartOptions, StartOption>
    {
        public StartOption Adventurer { get; private set; }

        public StartOption IndebtedLord { get; private set; }

        public StartOption Mercenary { get; private set; }

        public StartOption Outlaw { get; private set; }

        public StartOption Caravaneer { get; private set; }

        public StartOption Gladiator { get; private set; }

        public override IEnumerable<StartOption> All
        {
            get
            {
                yield return Adventurer;
                yield return IndebtedLord;
                yield return Mercenary;
                yield return Outlaw;
                yield return Caravaneer;
                yield return Gladiator;
            }
        }

        public override void Initialize()
        {
            Adventurer = new StartOption("start_adventurer");
            Adventurer.Initialize(new TextObject("{=0VSP2ghD}Adventurer"),
                new TextObject("{=wTcCEBLB}A free spirit, you are roaming the continent without constraints, or a clear objective. The world is for the taking, will you take your share?"),
                new TextObject("{=2fcEecjz}Vanilla start. No troops, goods or any benefits."),
                1000, 0, 0, 50, 0f,
                null);

            IndebtedLord = new StartOption("start_lord");
            IndebtedLord.Initialize(new TextObject("{=2VoYzCNj}Indebted Lord"),
                new TextObject("{=P1rwCkZJ}After a series of inherited problems and bad decisions, you find yourself in debt. Thankfuly, you are a landed lord, with income from your Lordship. A food supply and a small retinue accompany you, though their loyalty will be tested by the lack of denars..."),
                new TextObject("{=qGzc0y7c}Start as a lord in a kingdom, with a Lordship title. No settlement income or influence for 5 years. The village you own can be managed by you, and you will receive it's income after 5 years. Gain Scholarship skill."),
                0, 25, 10, 50, -50f,
                () =>
                {
                    var items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
                    var sumpter = items.FirstOrDefault(x => x.StringId == "sumpter_horse");

                    var templates = Game.Current.ObjectManager.GetObjectTypeList<PartyTemplateObject>();
                    var template = templates.First(x =>
                        x.StringId == "kingdom_hero_party_" + Hero.MainHero.Culture.StringId + "_template");

                    var roster = MobileParty.MainParty.MemberRoster;
                    for (var i = 0; i < 10; i++)
                    {
                        if (i >= 6)
                        {
                            roster.AddToCounts(template.Stacks[2].Character, 1);
                        }
                        else
                        {
                            roster.AddToCounts(template.Stacks[1].Character, 1);
                        }
                    }

                    var settlement =
                        SettlementHelper.FindNearestSettlement(x => x.OwnerClan is {Kingdom: { }});
                    var kingdom = settlement.OwnerClan.Kingdom;
                    if (kingdom == null)
                    {
                        kingdom = Kingdom.All.GetRandomElement();
                    }

                    ChangeKingdomAction.ApplyByJoinToKingdom(Clan.PlayerClan, kingdom, false);
                    BannerKingsConfig.Instance.TitleManager.GiveLordshipOnKingdomJoin(kingdom, Clan.PlayerClan, true);
                    MobileParty.MainParty.ItemRoster.AddToCounts(sumpter, 2);
                    Hero.MainHero.AddSkillXp(BKSkills.Instance.Scholarship, 2000);
                });

            Mercenary = new StartOption("start_mercenary");
            Mercenary.Initialize(new TextObject("{=kLHXZnLY}Mercenary"),
                new TextObject("{=wcWg3KPt}You serve as a free mercenary company, roaming around the continent in search of employment. After a long period of joblessness, you find your company in the verge of collapse, with little morale, food and finances."),
                new TextObject("{=G2yNVZ9R}Start with a mercenary band, in desperate need for plundering gold and food. Party morale reduced for 5 years. Mercenary lifestyle is kickstarted as part of your education."),
                250, 0, 22, 30, 0f,
                () =>
                {
                    var items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
                    var sumpter = items.FirstOrDefault(x => x.StringId == "sumpter_horse");

                    var characters = Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>();
                    var roster = MobileParty.MainParty.MemberRoster;
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

            Outlaw = new StartOption("start_outlaw");
            Outlaw.Initialize(new TextObject("{=GTYYnH9E}Outlaw"),
                new TextObject("{=YLxp50Ln}Lacking in morals, you assemble a party of like-minded brigands, making a living out stealing and plundering. Your efforts, however, have not gone unnoticed by the local authorities."),
                new TextObject("{=rSAjO3Qg}Start with a outlaw band, in desperate need for plundering gold and food. Criminal rating does not reduce for 5 years. Outlaw lifestyle is kickstarted as part of your education."),
                50, 2, 15, 50, 0f,
                () =>
                {
                    var items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
                    var sumpter = items.FirstOrDefault(x => x.StringId == "vlandia_horse");
                    var templates = Game.Current.ObjectManager.GetObjectTypeList<PartyTemplateObject>();
                    var templateName = "looters_template";

                    var culture = Hero.MainHero.Culture;
                    templateName = culture.StringId switch
                    {
                        "sturgia" => "sea_raiders_template",
                        "battania" => "forest_bandits_template",
                        "aserai" => "desert_bandits_template",
                        "khuzait" => "steppe_bandits_template",
                        "vlandia" => "mountain_bandits_template",
                        _ => templateName
                    };

                    var template = templates.First(x => x.StringId == templateName);
                    var roster = MobileParty.MainParty.MemberRoster;

                    for (var i = 0; i < 15; i++)
                    {
                        if (i >= 10 && template.Stacks.Count >= 2)
                        {
                            roster.AddToCounts(template.Stacks[1].Character, 1);
                        }
                        else
                        {
                            roster.AddToCounts(template.Stacks[0].Character, 1);
                        }
                    }

                    MobileParty.MainParty.ItemRoster.Add(new ItemRosterElement(sumpter, 6));
                },
                100f,
                null,
                DefaultLifestyles.Instance.Outlaw);

            Caravaneer = new StartOption("start_caravaneer");
            Caravaneer.Initialize(new TextObject("{=2FW79uHM}Robbed Caravaneer"),
                new TextObject("{=L1QFOwvp}Your caravan has been recently harassed by criminals - most of your belongings are lost, and certainly all your denars. A few goods, mules and wounded soldiers remain."),
                new TextObject("{=dSJsWjyR}Start with a wounded caravan, some food, mules and goods. Party speed is reduced by 5% for 5 years. Caravaneer lifestyle is kickstarted as part of your education."),
                0, 6, 12, 50, 0f,
                () =>
                {
                    var templates = Game.Current.ObjectManager.GetObjectTypeList<PartyTemplateObject>();
                    var template = templates.FirstOrDefault(x =>
                        x.StringId == "caravan_template_" + Hero.MainHero.Culture.StringId);
                    var party = MobileParty.MainParty;

                    var items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
                    var mule = items.FirstOrDefault(x => x.StringId == "mule");
                    var sumpter = items.FirstOrDefault(x => x.StringId == "sumpter_horse");

                    if (template != null)
                    {
                        var roster = party.MemberRoster;
                        for (var i = 0; i < 12; i++)
                        {
                            var wounded = MBRandom.RandomFloat < 0.6f;
                            if (i >= 7 && template.Stacks.Count >= 2)
                            {
                                roster.AddToCounts(template.Stacks[1].Character, 1, false, wounded ? 1 : 0);
                            }
                            else
                            {
                                roster.AddToCounts(template.Stacks[0].Character, 1, false, wounded ? 1 : 0);
                            }
                        }
                    }

                    party.ItemRoster.AddToCounts(sumpter, 3);
                    party.ItemRoster.AddToCounts(mule, 2);

                    var goodsValue = 0;
                    foreach (var itemObject in TaleWorlds.CampaignSystem.Extensions.Items.AllTradeGoods)
                    {
                        if (itemObject.IsFood || goodsValue >= 1200)
                        {
                            continue;
                        }

                        var num2 = (int) (1f * (10f / 13f) / itemObject.Value * MBRandom.RandomFloat);
                        if (num2 > 0)
                        {
                            party.ItemRoster.AddToCounts(itemObject, num2);
                        }

                        goodsValue += num2 * itemObject.Value;
                    }
                },
                0f,
                null,
                DefaultLifestyles.Instance.Caravaneer);

            Gladiator = new StartOption("start_gladiator");
            Gladiator.Initialize(new TextObject("{=wTyw0yfR}Gladiator"),
                new TextObject("{=ScHHoM2v}You are an promising athlete, roaming the world looking for a good fight, gold and glory."),
                new TextObject("{=uUjoaben}Start with a couple mercenaries and food. Party size is reduced by 40% for 5 years. Gladiator lifestyle is kickstarted as part of your education."),
                0, 6, 8, 50, 0f,
                () =>
                {
                    var items = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();

                    var characters = Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>();
                    var roster = MobileParty.MainParty.MemberRoster;
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_1"), 4);
                    roster.AddToCounts(characters.First(x => x.StringId == "mercenary_2"), 4);

                    MobileParty.MainParty.ItemRoster.Add(new ItemRosterElement(items.First(x => x.StringId == "oil"), 2));
                },
                0f,
                null,
                DefaultLifestyles.Instance.Gladiator);
        }
    }
}