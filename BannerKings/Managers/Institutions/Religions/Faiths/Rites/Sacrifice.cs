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
                            options, false, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                            delegate (List<InquiryElement> x)
                            {
                                input = (Hero?)x[0].Identifier;
                                SetDialogue();
                            }, null, string.Empty));
        }

        public override void Complete(Hero actionTaker)
        {
            if (input == null) return;
            Religion inputReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(input);
            Religion actionTakerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(actionTaker);
            if (inputReligion != null && inputReligion == actionTakerReligion) return;
            float piety = GetPietyReward();
            KillCharacterAction.ApplyByExecution(input, actionTaker, false);
            InformationManager.AddQuickInformation(new TextObject("{=!}{SACRIFICE} was ritually sacrificed by {HERO}.")
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
            FaithfulData data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            return hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval()) && hero.IsPartyLeader && hero.PartyBelongedTo.PrisonRoster.TotalHeroes > 0; ;
        }
        
        public override TextObject GetDescription() => new TextObject("{=!}Sacrifice {HERO} to prove your devotion.");
        public override TextObject GetName() => new TextObject("{=!}Human Sacrifice");
        public override RiteType GetRiteType() => RiteType.SACRIFICE;
        public override float GetTimeInterval() => 2f;

        public override float GetPietyReward()
        {
            float renown = 100f;
            if (input.Clan != null) renown = input.Clan.Renown / 10f;
            return renown;
        }

        public override void SetDialogue()
        {
            //MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM", new TextObject("{=!}The fate of {HERO} was sealed once they dared draw sword on us. Let us rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
            //   .SetTextVariable("HERO", input.Name), false);
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM", new TextObject("{=!}The fate of {HERO} was sealed once they dared draw sword on us. Affirm the rite and we shall rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
               .SetTextVariable("HERO", input.Name), false);
        }
    }
}
