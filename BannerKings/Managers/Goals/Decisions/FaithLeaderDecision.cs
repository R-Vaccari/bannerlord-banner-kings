using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Behaviours.Feasts.Feast;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class FaithLeaderDecision : Goal
    {
        private Hero leader;
        public FaithLeaderDecision(Hero fulfiller = null, FeastType type = FeastType.Normal) : base("goal_organize_feast_decision", GoalCategory.Kingdom, GoalUpdateType.Manual, fulfiller)
        {
            var name = new TextObject("{=!}Create Faith Leader");
            var description = new TextObject("{=!}As a ruler, endorse a new leader for your faith group. The possible faith leaders vary according to how the faith group works. A Faith Leader is important to push the faith's fervor, as well as sanctioning holy wars.\n");

            Initialize(name, description);
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
   
            if (religion.Faith.FaithGroup.Leader != null && religion.Faith.FaithGroup.Leader.IsAlive)
            {
                failedReasons.Add(new TextObject("{=!}The faith group {GROUP} already has a leader. A new Head of Faith can be created once there are none occupying this position.")
                    .SetTextVariable("GROUP", religion.Faith.FaithGroup.Name));
            }


            return failedReasons.Count == 0;
        }

        public override void ShowInquiry()
        {
            var leaders = new List<InquiryElement>();
            Religion playerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            foreach (Hero hero in playerReligion.Faith.FaithGroup.EvaluatePossibleLeaders(playerReligion))
            {
                float cost = BannerKingsConfig.Instance.ReligionModel.CreateFaithLeaderCost(playerReligion, Hero.MainHero, hero).ResultNumber;
                float piety = BannerKingsConfig.Instance.ReligionsManager.GetPiety(Hero.MainHero);
                TextObject explanation = new TextObject("{=!}{HERO} is a candidate to become faith leader for the {GROUP} from within the {FAITH}. Their opinion of you is {OPINION}.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("FAITH", playerReligion.Faith.GetName())
                    .SetTextVariable("OPINION", hero.GetRelationWithPlayer());

                bool faction = hero.MapFaction == Clan.PlayerClan.MapFaction;
                if (!faction) explanation = new TextObject("{=!}{HERO} is a candidate to become faith leader for the {GROUP} from within the {FAITH}. They are not part of your realm, and therefore not endorsed by you.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("FAITH", playerReligion.Faith.GetName())
                    .SetTextVariable("OPINION", hero.GetRelationWithPlayer());

                bool hasPiety = piety >= cost;
                if (!hasPiety) explanation = new TextObject("{=!}{HERO} is a candidate to become faith leader for the {GROUP} from within the {FAITH}. You do not have enough piety to endorse them.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("FAITH", playerReligion.Faith.GetName())
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
                .SetTextVariable("GROUP", playerReligion.Faith.FaithGroup.Name) 
                .SetTextVariable("EXPLANATION", playerReligion.Faith.FaithGroup.Explanation)
                .SetTextVariable("FAITH", playerReligion.Faith.GetName())
                .ToString(),
                leaders,
                true,
                1,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                delegate (List<InquiryElement> list)
                {
                    leader = (Hero)list[0].Identifier;
                    ApplyGoal();
                },
                null));
        }

        public override void ApplyGoal()
        {
            Religion religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(GetFulfiller());
            religion.Faith.FaithGroup.MakeHeroLeader(religion, leader, GetFulfiller()); 
        }

        public override void DoAiDecision()
        {
            return;
        }
    }
}