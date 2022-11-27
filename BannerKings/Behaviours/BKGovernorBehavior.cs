using BannerKings.UI;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours
{
    public class BKGovernorBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddDialogue(starter);
        }

        private void AddDialogue(CampaignGameStarter starter)
        {

            starter.AddPlayerLine("bk_question_governor", "hero_main_options", "bk_answer_governor",
                "{=W0wTid0n}I would like to ask you about management.",
                IsPlayerGovernor, null);

            starter.AddDialogLine("bk_answer_governor", "bk_answer_governor", "bk_governor_management_questions",
                "{=8229dt8y}Certainly. What would you like to know about?",
                 IsPlayerGovernor, null);



            starter.AddPlayerLine("bk_governor_management_workforce", "bk_governor_management_questions", "bk_governor_management_workforce_answer",
                "{=QMepdVPu}Population and Workforce.",
                IsPlayerGovernor, null);


            starter.AddPlayerLine("bk_governor_management_food", "bk_governor_management_questions", "bk_governor_management_food_answer",
                "{=YjjmPpzV}Food production.",
                IsPlayerGovernor, null);

            starter.AddPlayerLine("bk_governor_management_goods", "bk_governor_management_questions", "bk_governor_management_goods_answer",
                "{=pwzUGCBg}Goods production.",
                IsPlayerGovernor, null);


            starter.AddDialogLine("bk_governor_management_goods_answer", "bk_governor_management_goods_answer", "bk_governor_management_goods_answer_last",
                "{=r9fs17QX}In towns, most goods will be produced by local craftsmen. They supply goods such as weapons, clothing, iron, tools and others. More craftsmen will mean more goods in general. Workshops provide more specialized goods and at faster pace. The quality of goods is influenced by the local production quality, while speed of production is influenced by production efficiency.",
                IsPlayerGovernor, null);

            starter.AddDialogLine("bk_governor_management_goods_answer_last", "bk_governor_management_goods_answer_last", "hero_main_options",
                "{=kx9hK4SA}Villages are similar, though their productions are much more limited. The village's production will largely depend on it's workforce. Various productions, such as farm goods, will depend on it's acreage, meaning that having extra workers over what the land needs will be fruitless. Other productions such as ores depend exclusively on workforce numbers, and are not limited by acreage. Village productions can be expanded by village buildings.",
                IsPlayerGovernor, null);



            starter.AddDialogLine("bk_governor_management_workforce_answer", "bk_governor_management_workforce_answer", "bk_governor_management_workforce_answer_last",
                "{=kB584OYU}Population is limited by the settlement's population limit. This limit relies mostly on the local acreage - more useful land will allow more people to fed. Serfs and slaves compose the workforce and make the bulk of labor, including construction and food production. Slaves are usually better in hard labor such as mining, while serfs are more productive as farmers. State slaves contribute to state projects such as construction projects or worforce policies.",
                IsPlayerGovernor, null);

            starter.AddDialogLine("bk_governor_management_workforce_answer_last", "bk_governor_management_workforce_answer_last", "hero_main_options",
                "{=DgUr4orO}A quick way of increasing workforce is gathering prisoners and outlaws and turning them into slaves. Keep in mind, slaves are not free, and do not contribute to population growth. Serfs represent the bulk of growth. Increasing population limit and allowing them to grow by themselves is the recommended long term strategy.",
                IsPlayerGovernor, null);



            starter.AddDialogLine("bk_governor_management_food_answer", "bk_governor_management_food_answer", "bk_governor_management_food_answer_last",
                "{=L87e0h2a}Food production relies on acreage. Each acre of land requires an amount of workforce. If it is met, the acre will produce food. The output depends on acre type - farmlands are the most productive. When town's stocks are filled, the production will be sold to the market. A town only truly runs out of food when the market is empty, as people will buy it off when their own stocks are empty.",
                IsPlayerGovernor, null);

            starter.AddDialogLine("bk_governor_management_food_answer_last", "bk_governor_management_food_answer_last", "hero_main_options",
                "{=UNBcpRLu}Villages work exactly the same. They will supply town markets by selling their output and thus generating an income. Increasing workforce through population increase is the main method of increasing general food output. However, if workforce saturation is over 100%, it is recommended to focus on acreage expansion instead, as new laborers will not have land to work on.",
                IsPlayerGovernor, null);

        }

        private bool IsPlayerGovernor()
        {
            var hero = Hero.OneToOneConversationHero;
            if (hero == null || hero.CurrentSettlement == null)
            {
                return false;
            }

            var settlement = hero.CurrentSettlement;
            return settlement.Town != null && hero.GovernorOf == settlement.Town;
        }
    }
}
