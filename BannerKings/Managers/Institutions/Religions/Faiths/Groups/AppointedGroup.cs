using BannerKings.Behaviours.Diplomacy;
using BannerKings.Managers.Court;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Groups
{
    public class AppointedGroup : FaithGroup
    {
        public AppointedGroup(CouncilMember member, string id) : base(id)
        {
            CouncilMember = member;
        }

        public CouncilMember CouncilMember { get; set; }
        public override bool ShouldHaveLeader => true;
        public override bool IsPreacher => false;
        public override bool IsTemporal => true;
        public override bool IsPolitical => true;

        public override TextObject Explanation => new TextObject("{=!}The representative of the {GROUPS} must be a council member fulfilling the {ROLE} council role. Valid councils are those of realms that follow a religion that is part of this faith group. Additionally, only councils of rulers are valid.")
            .SetTextVariable("GROUPS", Name)
            .SetTextVariable("ROLE", CouncilMember.Name);

        public override void EvaluateMakeNewLeader(Religion religion)
        {
        }

        public override List<Hero> EvaluatePossibleLeaders(Religion religion)
        {
            List<Hero> results = new List<Hero>();
            foreach (Kingdom k in Kingdom.All)
            {
                KingdomDiplomacy kd = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                    .GetKingdomDiplomacy(k);
                if (kd != null) 
                {
                    CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(k.RulingClan);
                    CouncilMember councilPosition = council.GetCouncilPosition(CouncilMember);
                    if (councilPosition != null && councilPosition.Member != null)
                    {
                        Religion heroReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(councilPosition.Member);
                        if (heroReligion.Faith.FaithGroup.Equals(religion.Faith.FaithGroup))
                        {
                            results.Add(councilPosition.Member);
                        }
                    }   
                }
            }

            return results;
        }
    }
}
