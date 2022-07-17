using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    class BKSkillBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameLoaded));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnGameLoaded(CampaignGameStarter starter)
        {
            Type heroType = null;
            FieldInfo attrs = null;
            FieldInfo skills = null;


            foreach (Hero hero in Hero.AllAliveHeroes)
            {
                if (heroType == null)
                {
                    heroType = hero.GetType();
                    attrs = heroType.GetField("_characterAttributes", BindingFlags.Instance | BindingFlags.NonPublic);
                    skills = heroType.GetField("_heroSkills", BindingFlags.Instance | BindingFlags.NonPublic);
                }

                CharacterAttributes charAttrs = (CharacterAttributes)attrs.GetValue(hero);
                if (charAttrs.HasProperty(BKAttributes.Instance.Wisdom)) continue;

                Dictionary<CharacterAttribute, int> attrsDic = (Dictionary<CharacterAttribute, int>)charAttrs.GetType().BaseType
                    .GetField("_attributes", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(charAttrs);
                attrsDic.Add(BKAttributes.Instance.Wisdom, 2);

                CharacterSkills charSkills = (CharacterSkills)skills.GetValue(hero);
                Dictionary<SkillObject, int> skillsDic = (Dictionary<SkillObject, int>)charSkills.GetType().BaseType
                    .GetField("_attributes", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(charSkills);

                if (charSkills.HasProperty(BKSkills.Instance.Scholarship)) continue;
                skillsDic.Add(BKSkills.Instance.Scholarship, 0);
                skillsDic.Add(BKSkills.Instance.Theology, 0);
                skillsDic.Add(BKSkills.Instance.Lordship, 0);
            }
        }
    }
}
