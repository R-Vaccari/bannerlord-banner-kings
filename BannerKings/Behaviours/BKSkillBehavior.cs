using System;
using System.Collections.Generic;
using System.Reflection;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    internal class BKSkillBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, OnDailyTickParty);
            CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener(this, OnComesOfAge);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnDailyTickParty(MobileParty party)
        {
            if (party.HasPerk(BKPerks.Instance.TheologyReligiousTeachings))
            {
                foreach (var element in party.MemberRoster.GetTroopRoster())
                {
                    if (element.Character.IsHero)
                    {
                        var hero = element.Character.HeroObject;
                        var skillValue = hero.GetSkillValue(BKSkills.Instance.Theology);
                        if (skillValue < int.MaxValue)
                        {
                            hero.AddSkillXp(BKSkills.Instance.Theology, 2f);
                        }
                    }
                }
            }
        }

        private void OnComesOfAge(Hero hero)
        {
            if (hero.Father != null && hero.Father.GetPerkValue(BKPerks.Instance.TheologyReligiousTeachings))
            {
                hero.HeroDeveloper.AddAttribute(BKAttributes.Instance.Wisdom, 1, false);
            }

            if (hero.Mother != null && hero.Mother.GetPerkValue(BKPerks.Instance.TheologyReligiousTeachings))
            {
                hero.HeroDeveloper.AddAttribute(BKAttributes.Instance.Wisdom, 1, false);
            }
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

                if(!attrsDic.ContainsKey(BKAttributes.Instance.Wisdom))
                {
                    attrsDic.Add(BKAttributes.Instance.Wisdom, 2);
                }

                var charSkills = (CharacterSkills) skills.GetValue(hero);
                var skillsDic = (Dictionary<SkillObject, int>) charSkills.GetType().BaseType
                    .GetField("_attributes", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(charSkills);

                if (charSkills.HasProperty(BKSkills.Instance.Scholarship))
                {
                    continue;
                }

                if (!skillsDic.ContainsKey(BKSkills.Instance.Scholarship))
                    skillsDic.Add(BKSkills.Instance.Scholarship, 0);
                
                if (!skillsDic.ContainsKey(BKSkills.Instance.Theology))
                    skillsDic.Add(BKSkills.Instance.Theology, 0);

                if (!skillsDic.ContainsKey(BKSkills.Instance.Lordship))
                    skillsDic.Add(BKSkills.Instance.Lordship, 0);
            }
        }
    }
}