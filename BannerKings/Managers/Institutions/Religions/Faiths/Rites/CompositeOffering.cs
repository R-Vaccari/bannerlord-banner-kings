using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class CompositeOffering : ContextualRite
    {
        protected readonly Dictionary<ItemObject, int> inputs;

        public CompositeOffering(Dictionary<ItemObject, int> inputs)
        {
            this.inputs = inputs;
        }

        public override void Complete(Hero actionTaker)
        {
            foreach (var pair in inputs)
            {
                actionTaker.PartyBelongedTo.ItemRoster.AddToCounts(pair.Key, -pair.Value);
            }
            
            MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has completed the {RITE} ritual.")
                    .SetTextVariable("HERO", actionTaker.Name)
                    .SetTextVariable("RITE", GetName()),
                0, 
                actionTaker.CharacterObject, 
                Utils.Helpers.GetRelationDecisionSound());

            var piety = GetPietyReward();
            BannerKingsConfig.Instance.ReligionsManager.AddPiety(actionTaker, piety, true);
            actionTaker.AddSkillXp(BKSkills.Instance.Theology, piety * 1.2f);

            if (actionTaker.GetPerkValue(BKPerks.Instance.TheologyRitesOfPassage))
            {
                actionTaker.Clan.AddRenown(5f);
            }
        }

        public override void Execute(Hero executor)
        {
            SetDialogue();
        }

        public override bool MeetsCondition(Hero hero, out TextObject reason)
        {
            reason = new TextObject("{=oo3xtFfT}This rite is available to be performed.");
            var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            bool baseResult = hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                             data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval(hero));
            bool hasItems = false;
            if (baseResult)
            {
                var roster = hero.PartyBelongedTo.ItemRoster;
                foreach (var pair in inputs)
                {
                    int count = roster.GetItemNumber(pair.Key);
                    if (count >= pair.Value)
                    {
                        hasItems = true;
                    }
                    else hasItems = false;
                }
            }
            else
            {
                reason = new TextObject("{=G5vYrCrV}Not enough time ({YEARS} years) has passed since the last rite of this type was performed.")
                    .SetTextVariable("YEARS", GetTimeInterval(hero).ToString("0.0"));
            }

            if (!hasItems)
            {
                reason = new TextObject("{=!}This rite requires several items, check its description for details.");
            }

            return baseResult && hasItems;
        }

        public override RiteType GetRiteType()
        {
            return RiteType.OFFERING;
        }

        public override float GetTimeInterval(Hero hero)
        {
            var result = 2f;
            if (hero.GetPerkValue(BKPerks.Instance.TheologyRitesOfPassage))
            {
                result -= 0.25f;
            }

            return result;
        }
    }
}