using BannerKings.Managers.Skills;
using System.Collections.Generic;
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
        public Sacrifice()
        {
            
        }

        public override void Execute(Hero executor)
        {
            if (!MeetsCondition(executor)) return;
            List<InquiryElement> options = new List<InquiryElement>();
            foreach (TroopRosterElement element in executor.PartyBelongedTo.PrisonRoster.GetTroopRoster())
                if (element.Character.IsHero)
                {
                    Hero hero = element.Character.HeroObject;
                    options.Add(new InquiryElement(hero, hero.Name.ToString(), new ImageIdentifier(CampaignUIHelper.GetCharacterCode(element.Character))));
                }

            InformationManager.ShowMultiSelectionInquiry(
                        new MultiSelectionInquiryData(
                            GetName().ToString(),
                            GetDescription().ToString(),
                            options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                            delegate (List<InquiryElement> x)
                            {
                                input = (Hero?)x[0].Identifier;
                                Complete(executor);
                            }, null, string.Empty));
        }

        protected override void Complete(Hero actionTaker)
        {
            if (input == null) return;
            //Religion inputReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(input);
            float piety = GetPietyReward();
            KillCharacterAction.ApplyByMurder(input, actionTaker, false);
            BannerKingsConfig.Instance.ReligionsManager.AddPiety(actionTaker, piety, true);
            actionTaker.AddSkillXp(BKSkills.Instance.Theology, piety * 2f);

            /*if (inputReligion == null || inputReligion != BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(actionTaker))
            {
                int relationChangeForExecutingHero = Campaign.Current.Models.ExecutionRelationModel.GetRelationChangeForExecutingHero(victim, clan.Leader, out affectRelatives);
                if (relationChangeForExecutingHero != 0)
                    ChangeRelationAction.ApplyPlayerRelation(clan.Leader, relationChangeForExecutingHero, affectRelatives, true);
                
            }*/
        }

        public new bool MeetsCondition(Hero hero) => base.MeetsCondition(hero) &&
            hero.IsPartyLeader && hero.PartyBelongedTo.PrisonRoster.TotalHeroes > 0;

        public override TextObject GetDescription() => new TextObject("{=!}Sacrifice {HERO} to prove your devotion.");

        public override TextObject GetName() => new TextObject("{=!}Human Sacrifice");

        public override RiteType GetRiteType() => RiteType.SACRIFICE;

        public override float GetTimeInterval() => 2f;

        public override float GetPietyReward()
        {
            float renown = 150f;
            if (input.Clan != null) renown = input.Clan.Renown;
            return renown;
        }

       
    }
}
