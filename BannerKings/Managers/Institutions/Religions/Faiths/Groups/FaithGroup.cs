using BannerKings.Behaviours.Diplomacy;
using BannerKings.Extensions;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
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

        public void Initialize(TextObject name, TextObject title, TextObject description)
        {
            this.name = name;
            this.description = description;
            Title = title;
        }

        [SaveableProperty(4)] public Hero Leader { get; private set; }
        public abstract bool ShouldHaveLeader { get; }
        public abstract bool IsPreacher { get; }
        public abstract bool IsTemporal { get; }
        public abstract bool IsPolitical { get; }
        public abstract TextObject Explanation { get; }
        public TextObject Title { get; private set; }

        public bool HasValidLeader() => Leader != null && Leader.IsAlive;

        public void TickLeadership(Religion religion)
        {
            if (Leader == null || Leader.IsAlive) return;

            Hero hero = null;
            if (IsPreacher)
            {
                Settlement faithSeat = religion.Faith.FaithSeat;
                PopulationData data = faithSeat.PopulationData();
                if (data.ReligionData != null && data.ReligionData.DominantReligion != null &&
                    data.ReligionData.DominantReligion.Equals(religion))
                {
                    foreach (var clergy in religion.Clergy)
                    {
                        if (clergy.Key == faithSeat)
                        {
                            hero = clergy.Value.Hero;
                            break;
                        }
                    }
                }
            }

            if (IsTemporal)
            {
                if (Leader.Clan != null && !Leader.Clan.IsEliminated && Leader.Clan.Leader != Leader)
                {
                    hero = Leader.Clan.Leader;
                }
            }

            if (hero != null) MakeHeroLeader(religion, hero);
        }

        public bool CanReligionMakeLeader(Religion religion)
        {
            float fervor = BannerKingsConfig.Instance.ReligionModel.CalculateFervor(religion).ResultNumber;
            foreach (Religion r in BannerKingsConfig.Instance.ReligionsManager.GetReligions())
            {
                if (r.Equals(religion)) continue;

                if (r.Faith.FaithGroup.Equals(religion.Faith.FaithGroup))
                {
                    float f = BannerKingsConfig.Instance.ReligionModel.CalculateFervor(religion).ResultNumber;
                    if (f >= fervor + 0.2f) return false;
                }
            }

            return true;
        }

        public List<Hero> EvaluatePossibleLeaders(Religion religion)
        {
            List<Hero> results = new List<Hero>();
            if (!CanReligionMakeLeader(religion)) return results;

            if (IsPolitical)
            {
                foreach (Kingdom k in Kingdom.All)
                {
                    KingdomDiplomacy kd = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                        .GetKingdomDiplomacy(k);
                    if (kd != null)
                    {
                        CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(k.RulingClan);
                        CouncilMember councilPosition = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Spiritual);
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
            }
            

            if (IsPreacher)
            {
                Hero result = null;
                Settlement faithSeat = religion.Faith.FaithSeat;
                foreach (var clergy in religion.Clergy)
                {
                    if (clergy.Key == faithSeat)
                    {
                        result = clergy.Value.Hero;
                        break;
                    }
                }

                results.Add(result);
            }

            return results;
        }

        public void MakeHeroLeader(Religion religion, Hero leader, Hero creator = null, bool notify = true)
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

            if (notify)
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{HERO} is now the faith leader for the followers of the {GROUP}.")
                    .SetTextVariable("HERO", leader.Name)
                    .SetTextVariable("GROUP", Name)
                    .ToString(),
                    Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
            
            Leader = leader;
            if (Title != null) religion.SetClergyName(leader, Title);
            Leader.AddPower(150f);

            if (creator != null)
            {
                creator.AddSkillXp(BKSkills.Instance.Theology, 5000);
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(creator, leader, 20);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is FaithGroup)
            {
                return StringId == ((FaithGroup)obj).StringId;  
            }
            return base.Equals(obj);
        }
    }
}