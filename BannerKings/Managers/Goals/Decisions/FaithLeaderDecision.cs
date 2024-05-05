using BannerKings.Managers.Institutions.Religions;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class FaithLeaderDecision : Goal
    {
        private Hero faithLeader;
        private Religion religion;

        public FaithLeaderDecision(Hero fulfiller = null) : base("goal_faith_leader_decision", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Religious;

        public override Goal GetCopy(Hero fulfiller)
        {
            FaithLeaderDecision copy = new FaithLeaderDecision(fulfiller);
            return copy;
        }

        public override bool IsAvailable() => GetFulfiller().Clan.Kingdom != null && 
            BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(GetFulfiller()) != null;

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (!IsAvailable())
            {
                return false;
            }

            Hero fulfiller = GetFulfiller();
            if (fulfiller.Clan.Kingdom.Leader != fulfiller)
            {
                failedReasons.Add(new TextObject("{=!}You must be a ruler in order to install a faith leader."));
            }

            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(fulfiller);
            if (!religion.Faith.FaithGroup.ShouldHaveLeader)
            {
                failedReasons.Add(new TextObject("{=!}The faith group {GROUP} does not accept a leader.")
                    .SetTextVariable("GROUP", religion.Faith.FaithGroup.Name));
            }

            if (!religion.Faith.FaithGroup.CanReligionMakeLeader(religion))
            {
                failedReasons.Add(new TextObject("{=!}Your religion cannot make a leader for the {GROUP}, another faith of the group has significantly more fervor than yours.")
                    .SetTextVariable("GROUP", religion.Faith.FaithGroup.Name));
            }

            if (religion.Faith.FaithGroup.Leader != null && religion.Faith.FaithGroup.Leader.IsAlive)
            {
                failedReasons.Add(new TextObject("{=!}The faith group {GROUP} already has a leader. A new Faith Leader can be created once there are none occupying this position.")
                    .SetTextVariable("GROUP", religion.Faith.FaithGroup.Name));
            }

            return failedReasons.Count == 0;
        }

        public override void ShowInquiry()
        {
            var leaders = new List<InquiryElement>();
            religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            foreach (Hero hero in religion.Faith.FaithGroup.EvaluatePossibleLeaders(religion))
            {
                float cost = BannerKingsConfig.Instance.ReligionModel.CreateFaithLeaderCost(religion, Hero.MainHero, hero).ResultNumber;
                float piety = BannerKingsConfig.Instance.ReligionsManager.GetPiety(Hero.MainHero);
                TextObject explanation = new TextObject("{=!}{HERO} is a candidate to become faith leader for the {GROUP} from within the {FAITH}. Their opinion of you is {OPINION}.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("FAITH", religion.Faith.GetName())
                    .SetTextVariable("OPINION", hero.GetRelationWithPlayer());

                bool faction = hero.MapFaction == Clan.PlayerClan.MapFaction;
                if (!faction) explanation = new TextObject("{=!}{HERO} is a candidate to become faith leader for the {GROUP} from within the {FAITH}. They are not part of your realm, and therefore not endorsed by you.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("FAITH", religion.Faith.GetName())
                    .SetTextVariable("OPINION", hero.GetRelationWithPlayer());

                bool hasPiety = piety >= cost;
                if (!hasPiety) explanation = new TextObject("{=!}{HERO} is a candidate to become faith leader for the {GROUP} from within the {FAITH}. You do not have enough piety to endorse them.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("FAITH", religion.Faith.GetName())
                    .SetTextVariable("OPINION", hero.GetRelationWithPlayer());

                leaders.Add(new InquiryElement(hero, 
                    new TextObject("{=!}{HERO} - {PIETY}{PIETY_ICON}")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("PIETY", (int)cost)
                    .SetTextVariable("PIETY_ICON", Utils.TextHelper.PIETY_ICON)
                    .ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, true)),
                    hasPiety && faction,
                    explanation.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Faith Leader").ToString(),
                new TextObject("{=!}As a ruler, you are able to endorse a new leader for the {GROUP}. The candidate must be part of your realm and of the {FAITH} faith. This leader will represent all faiths within the faith group.{newline}{newline}Holding positive opinion from your faith leader can yield different advantages. Valid candidates according to the traditions of the faith group are as follows: {EXPLANATION}")
                .SetTextVariable("GROUP", religion.Faith.FaithGroup.Name) 
                .SetTextVariable("EXPLANATION", religion.Faith.FaithGroup.Explanation)
                .SetTextVariable("FAITH", religion.Faith.GetName())
                .ToString(),
                leaders,
                true,
                1,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                delegate (List<InquiryElement> list)
                {
                    faithLeader = (Hero)list[0].Identifier;
                    ApplyGoal();
                },
                null));
        }

        public override void ApplyGoal()
        {
            religion.Faith.FaithGroup.MakeHeroLeader(religion, faithLeader, GetFulfiller()); 
        }

        public override void DoAiDecision()
        {
            if (!IsAvailable()) return;

            if (!IsFulfilled(out List<TextObject> reasons)) return;

            List<ValueTuple<Hero, float>> options = new List<(Hero, float)>();
            religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(GetFulfiller());
            foreach (Hero hero in religion.Faith.FaithGroup.EvaluatePossibleLeaders(religion))
            {
                if (hero.MapFaction != GetFulfiller().MapFaction) continue;

                float score = 1f;
                score += GetFulfiller().GetRelation(hero) / 100f;
                options.Add((hero, score));
            }

            Hero result = MBRandom.ChooseWeighted(options);
            if (result != null)
            {
                faithLeader = result;
                ApplyGoal();
            }
        }
    }
}