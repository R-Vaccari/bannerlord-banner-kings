using System;
using System.Collections.Generic;
using System.Reflection;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace BannerKings.Behaviours;

internal class BKSkillBehavior : CampaignBehaviorBase
{
    public override void RegisterEvents()
    {
        CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
    }

    public override void SyncData(IDataStore dataStore)
    {
    }

    private void OnGameLoaded(CampaignGameStarter starter)
    {
        Type heroType = null;
        FieldInfo attrs = null;
        FieldInfo skills = null;


        foreach (var hero in Hero.AllAliveHeroes)
        {
            if (heroType == null)
            {
                heroType = hero.GetType();
                attrs = heroType.GetField("_characterAttributes", BindingFlags.Instance | BindingFlags.NonPublic);
                skills = heroType.GetField("_heroSkills", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            var charAttrs = (CharacterAttributes) attrs.GetValue(hero);
            if (charAttrs.HasProperty(BKAttributes.Instance.Wisdom))
            {
                continue;
            }

            var attrsDic = (Dictionary<CharacterAttribute, int>) charAttrs.GetType().BaseType
                .GetField("_attributes", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(charAttrs);
            attrsDic.Add(BKAttributes.Instance.Wisdom, 2);

            var charSkills = (CharacterSkills) skills.GetValue(hero);
            var skillsDic = (Dictionary<SkillObject, int>) charSkills.GetType().BaseType
                .GetField("_attributes", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(charSkills);

            if (charSkills.HasProperty(BKSkills.Instance.Scholarship))
            {
                continue;
            }

            skillsDic.Add(BKSkills.Instance.Scholarship, 0);
            skillsDic.Add(BKSkills.Instance.Theology, 0);
            skillsDic.Add(BKSkills.Instance.Lordship, 0);
        }
    }
}