using System.Collections.Generic;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public class Sacrifice : Rite
    {
        private Hero input;

        public override void Execute(Hero executor)
        {
            if (!MeetsCondition(executor))
            {
                return;
            }

            var options = new List<InquiryElement>();
            foreach (var element in executor.PartyBelongedTo.PrisonRoster.GetTroopRoster())
            {
                if (element.Character.IsHero)
                {
                    var hero = element.Character.HeroObject;
                    options.Add(new InquiryElement(hero, hero.Name.ToString(),
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(element.Character))));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GetName().ToString(),
                    GetDescription().ToString(),
                    options, false, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
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
            MBInformationManager.AddQuickInformation(new TextObject("{=1mtxQhBo3}{SACRIFICE} was ritually sacrificed by {HERO}.")
                    .SetTextVariable("HERO", actionTaker.Name)
                    .SetTextVariable("SACRIFICE", input.Name),
                0, actionTaker.CharacterObject, "event:/ui/notification/relation");

            BannerKingsConfig.Instance.ReligionsManager.AddPiety(actionTaker, piety, actionTaker.Clan == Clan.PlayerClan);
            actionTaker.AddSkillXp(BKSkills.Instance.Theology, piety * 1.2f);

            /*foreach (Clan clan in Clan.All)
            {
                Religion clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                if (clan != actionTaker.Clan && (clanReligion == null || !clanReligion.Doctrines.Contains("sacrifice")))
                {
                    bool affectRelatives;
                    int relationChangeForExecutingHero = Campaign.Current.Models.ExecutionRelationModel.GetRelationChangeForExecutingHero(input, actionTaker, out affectRelatives);
                    if (relationChangeForExecutingHero != 0)
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(actionTaker, clan.Leader, relationChangeForExecutingHero, true);
                }
            }*/
        }

        public override bool MeetsCondition(Hero hero)
        {
            var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            return hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                   data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval()) && hero.IsPartyLeader &&
                   hero.PartyBelongedTo.PrisonRoster.TotalHeroes > 0;
            ;
        }

        public override TextObject GetDescription()
        {
            return new TextObject("{=cN87MvnZk}Sacrifice {HERO} to prove your devotion.");
        }

        public override TextObject GetName()
        {
            return new TextObject("{=9RwT7jANc}Human Sacrifice");
        }

        public override RiteType GetRiteType()
        {
            return RiteType.SACRIFICE;
        }

        public override float GetTimeInterval()
        {
            return 2f;
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
            //MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM", new TextObject("{=VBKx9TVQb}The fate of {HERO} was sealed once they dared draw sword on us. Let us rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
            //   .SetTextVariable("HERO", input.Name), false);
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject(
                        "{=8tokmbziD}The fate of {HERO} was sealed once they dared draw sword on us. Affirm the rite and we shall rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
                    .SetTextVariable("HERO", input.Name));
        }
    }
}