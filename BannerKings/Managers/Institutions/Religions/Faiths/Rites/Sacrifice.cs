using System.Collections.Generic;
using System.Drawing;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public class Sacrifice : ContextualRite
    {
        private Hero input;

        public override void Execute(Hero executor)
        {
            TextObject reason;
            if (!MeetsCondition(executor, out reason))
            {
                return;
            }

            var options = new List<InquiryElement>();
            foreach (var element in executor.PartyBelongedTo.PrisonRoster.GetTroopRoster())
            {
                if (element.Character.IsHero)
                {
                    var hero = element.Character.HeroObject;
                    TextObject description;
                    bool available = MeetsCondition(hero, out description);
                    options.Add(new InquiryElement(hero, 
                        hero.Name.ToString(),
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(element.Character)),
                        available,
                        description.ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GetName().ToString(),
                    GetDescription().ToString(),
                    options, 
                    false, 
                    1,
                    1, 
                    GameTexts.FindText("str_done").ToString(), string.Empty,
                    delegate(List<InquiryElement> x)
                    {
                        input = (Hero?) x[0].Identifier;
                        SetDialogue();
                    }, null, string.Empty));
        }

        public override void Complete(Hero actionTaker)
        {
            if (input == null)
            {
                return;
            }

            var inputReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(input);
            var actionTakerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(actionTaker);
            if (inputReligion != null && inputReligion == actionTakerReligion)
            {
                return;
            }

            var piety = GetPietyReward();
            KillCharacterAction.ApplyByExecution(input, actionTaker, false);
            MBInformationManager.AddQuickInformation(new TextObject("{=d8ecHZ0P}{SACRIFICE} was ritually sacrificed by {HERO}.")
                    .SetTextVariable("HERO", actionTaker.Name)
                    .SetTextVariable("SACRIFICE", input.Name),
                0, actionTaker.CharacterObject, "event:/ui/notification/relation");

            BannerKingsConfig.Instance.ReligionsManager.AddPiety(actionTaker, piety, actionTaker.Clan == Clan.PlayerClan);
            actionTaker.AddSkillXp(BKSkills.Instance.Theology, piety * 1.2f);

            if (actionTaker.GetPerkValue(BKPerks.Instance.TheologyRitesOfPassage))
            {
                actionTaker.Clan.AddRenown(5f);
            }

            input = null;
        }

        public override bool MeetsCondition(Hero hero, out TextObject reason)
        {
            reason = new TextObject("{=oo3xtFfT}This rite is available to be performed.");
            var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            bool baseResult = hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                             data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval(hero));

            if (!baseResult)
            {
                reason = new TextObject("{=NZyz0ChH}Not enough time ({YEARS} years) have passed since the last rite of this type was performed.")
                    .SetTextVariable("YEARS", GetTimeInterval(hero).ToString("0.0"));
            }

            bool prisoners = hero.PartyBelongedTo != null && hero.PartyBelongedTo.PrisonRoster.TotalHeroes > 0;
            if (!prisoners)
            {
                reason = new TextObject("{=EqMjsAzB}You need lord prisoners to be sacrificed.");
            }

            return baseResult && prisoners;
        }

        public override TextObject GetDescription()
        {
            return new TextObject("{=87WtQvgP}Sacrifice a worthy enemy hero to prove your devotion.");
        }

        public override TextObject GetName()
        {
            return new TextObject("{=0aY6zmgK}Human Sacrifice");
        }

        public override RiteType GetRiteType()
        {
            return RiteType.SACRIFICE;
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

        public override float GetPietyReward()
        {
            var renown = 100f;
            if (input.Clan != null)
            {
                renown = input.Clan.Renown / 10f;
            }

            return renown;
        }

        public override void SetDialogue()
        {
            //MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM", new TextObject("{=eTqH0eDT}The fate of {HERO} was sealed once they dared draw sword on us. Let us rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
            //   .SetTextVariable("HERO", input.Name), false);
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=uSMsxDP1}The fate of {HERO} was sealed once they dared draw sword on us. Affirm the rite and we shall rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
                    .SetTextVariable("HERO", input.Name));
        }

        public override TextObject GetRequirementsText(Hero hero)
        {
            return new TextObject("{=qBDbqpf3}May be performed every {YEARS} years\nRequires a lord from an enemy faction")
                .SetTextVariable("YEARS", GetTimeInterval(hero));
        }
    }
}