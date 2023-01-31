using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public class Offering : ContextualRite
    {
        protected readonly ItemObject input;
        protected readonly int inputCount;

        public Offering(ItemObject input, int inputCount)
        {
            this.input = input;
            this.inputCount = inputCount;
        }

        public override void Complete(Hero actionTaker)
        {
            actionTaker.PartyBelongedTo.ItemRoster.AddToCounts(input, -inputCount);
            MBInformationManager.AddQuickInformation(new TextObject("{=5sWFJZV6}{COUNT} {OFFERING} was ritually offered by {HERO}.")
                    .SetTextVariable("HERO", actionTaker.Name)
                    .SetTextVariable("COUNT", inputCount)
                    .SetTextVariable("OFFERING", input.Name),
                0, actionTaker.CharacterObject, "event:/ui/notification/relation");

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

        public override TextObject GetDescription()
        {
            return new TextObject("{=rK6v9sBw}Make an offering of {COUNT} {ITEM}, as the faith prescribes.")
                .SetTextVariable("ITEM", input.Name)
                .SetTextVariable("COUNT", inputCount);
        }

        public override bool MeetsCondition(Hero hero)
        {
            var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            bool baseResult = hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                             data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval(hero));
            bool hasItems = false;
            if (baseResult)
            {
                var roster = hero.PartyBelongedTo.ItemRoster;
                foreach (var element in roster)
                {
                    var item = element.EquipmentElement.Item;
                    if (item != null && item.StringId == input.StringId && element.Amount >= inputCount)
                    {
                        hasItems = true;
                        break;
                    }
                }
            }

            return baseResult && hasItems;
        }

        public override TextObject GetName()
        {
            return new TextObject("{ITEM} Offering").SetTextVariable("ITEM", input.Name);
        }

        public override float GetPietyReward()
        {
            return MathF.Sqrt(input.Value * inputCount) * 3f;
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

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=8OwEJjZb}Will you relinquish {COUNT} {ITEM} to prove your faith?")
                    .SetTextVariable("COUNT", inputCount)
                    .SetTextVariable("ITEM", input.Name));
        }

        public override TextObject GetRequirementsText(Hero hero)
        {
            return new TextObject("{=6Yj8erp7}May be performed every {YEARS} years\nRequires {COUNT} {ITEM}")
                .SetTextVariable("YEARS", GetTimeInterval(hero))
                .SetTextVariable("COUNT", inputCount)
                .SetTextVariable("ITEM", input.Name);
        }
    }
}