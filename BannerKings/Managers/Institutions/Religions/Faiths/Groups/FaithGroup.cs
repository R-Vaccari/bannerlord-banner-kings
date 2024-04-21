using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Groups
{
    public abstract class FaithGroup : BannerKingsObject
    {
        public FaithGroup(string id) : base(id)
        {

        }

        public void Initialize(TextObject name, TextObject description)
        {
            this.name = name;
            this.description = description;
        }

        [SaveableProperty(4)] public Hero Leader { get; private set; }
        public abstract bool ShouldHaveLeader{ get; }
        public abstract bool IsPreacher { get; }
        public abstract bool IsTemporal { get; }
        public abstract bool IsPolitical { get; }
        public abstract TextObject Explanation { get; }
        public abstract void EvaluateMakeNewLeader(Religion religion);
        public abstract List<Hero> EvaluatePossibleLeaders(Religion religion);

        public void MakeHeroLeader(Religion religion, Hero leader, Hero creator = null)
        {
            if (creator != null)
            {
                float cost = BannerKingsConfig.Instance.ReligionModel.CreateFaithLeaderCost(religion, creator, leader).ResultNumber;
                float piety = BannerKingsConfig.Instance.ReligionsManager.GetPiety(creator);
                if (cost <= piety)
                {
                    BannerKingsConfig.Instance.ReligionsManager.AddPiety(creator, -piety, true);
                }
                else return;
            }

            Religion playerRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            if (playerRel != null)
            {

            }

            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} is now the faith leader for the followers of the {GROUP}.")
                .SetTextVariable("HERO", leader.Name)
                .SetTextVariable("GROUP", Name)
                .ToString(),
                Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
            Leader = leader;
            if (creator != null)
            {
                creator.AddSkillXp(BKSkills.Instance.Theology, 5000);
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(creator, leader, 20);
            }
        }
    }
}