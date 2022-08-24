using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public class Offering : Rite
    {
        private readonly ItemObject input;
        private readonly int inputCount;

        public Offering(ItemObject input, int inputCount)
        {
            this.input = input;
            this.inputCount = inputCount;
        }

        public override void Complete(Hero actionTaker)
        {
            actionTaker.PartyBelongedTo.ItemRoster.AddToCounts(input, -inputCount);
            MBInformationManager.AddQuickInformation(new TextObject("{=QwUb3USzp}{OFFERING} was ritually offered by {HERO}.")
                    .SetTextVariable("HERO", actionTaker.Name)
                    .SetTextVariable("OFFERING", input.Name),
                0, actionTaker.CharacterObject, "event:/ui/notification/relation");

            var piety = GetPietyReward();
            BannerKingsConfig.Instance.ReligionsManager.AddPiety(actionTaker, piety, true);
            actionTaker.AddSkillXp(BKSkills.Instance.Theology, piety * 1.2f);
        }

        public override void Execute(Hero executor)
        {
            if (!MeetsCondition(executor))
            {
            }
        }

        public override TextObject GetDescription()
        {
            return new TextObject("Make an offering of {ITEM}. {COUNT} must be offered for the rite to be fulfilled.")
                .SetTextVariable("ITEM", input.Name)
                .SetTextVariable("COUNT", inputCount);
        }

        public override bool MeetsCondition(Hero hero)
        {
            var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            var baseResult = hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                             data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval());
            var hasItems = false;
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

        public override float GetTimeInterval()
        {
            return 2f;
        }

        public override void SetDialogue()
        {
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject(
                        "{=LCPwBvbfM}The fate of {HERO} was sealed once they dared draw sword on us. Affirm the rite and we shall rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
                    .SetTextVariable("HERO", input.Name));
        }
    }
}