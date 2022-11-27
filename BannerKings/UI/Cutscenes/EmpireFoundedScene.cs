using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.UI.Cutscenes
{
    internal class EmpireFoundedScene : KingdomCreatedSceneNotificationItem
    {
        private FeudalTitle title;
        public EmpireFoundedScene(Kingdom newKingdom, FeudalTitle title) : base(newKingdom)
        {
            this.title = title;
        }

        public override IEnumerable<SceneNotificationData.SceneNotificationCharacter> GetSceneNotificationCharacters()
        {
            List<SceneNotificationData.SceneNotificationCharacter> list = new List<SceneNotificationData.SceneNotificationCharacter>();
            Hero leader = NewKingdom.Leader;
            Equipment overridenEquipment = leader.BattleEquipment.Clone(false);
            CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref overridenEquipment, true, false);
            list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(leader, overridenEquipment, false, default(BodyProperties), uint.MaxValue, uint.MaxValue, false));
            
            List<Hero> heroes = new List<Hero>();
            foreach (Clan clan in NewKingdom.Clans)
            {
                if (clan.IsUnderMercenaryService || clan.Leader == leader)
                {
                    continue;
                }

                FeudalTitle highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                if (highest != null && highest.type <= TitleType.Dukedom)
                {
                    heroes.Add(clan.Leader);
                }
            }
            
            foreach (Hero hero in heroes.Take(5))
            {
                Equipment overridenEquipment2 = hero.CivilianEquipment.Clone(false);
                CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref overridenEquipment2, true, false);
                list.Add(CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(hero, overridenEquipment2, false, default(BodyProperties), uint.MaxValue, uint.MaxValue, false));
            }
            return list;
        }

        public override TextObject TitleText
        {
            get
            {

                GameTexts.SetVariable("KINGDOM_NAME", title.FullName);
                GameTexts.SetVariable("DAY_OF_YEAR", CampaignSceneNotificationHelper.GetFormalDayAndSeasonText(CampaignTime.Now));
                GameTexts.SetVariable("YEAR", CampaignTime.Now.GetYear);
                GameTexts.SetVariable("LEADER_NAME", NewKingdom.Leader.Name);
                return new TextObject("{=JXi5502K}On {DAY_OF_YEAR}, {YEAR}, {LEADER_NAME} has reformed the {KINGDOM_NAME}. Their name is now immortalized in history!");
            }
        }

    }
}
