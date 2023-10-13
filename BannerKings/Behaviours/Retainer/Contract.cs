using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace BannerKings.Behaviours.Retainer
{
    public class Contract
    {
        public Contract(Hero contractor, bool isFreelancer, CharacterObject template = null)
        {
            Contractor = contractor;
            IsFreelancer = isFreelancer;
            Template = template;
            Leaves = 2;
        }

        public Hero Contractor { get; private set; }
        public bool IsFreelancer { get; private set; }
        private ItemRoster LastItems { get; set; }
        public CharacterObject Template { get; private set; }
        public int Leaves { get; private set; }
        public Equipment Equipment
        {
            get
            {
                if (Template != null) return Template.FirstBattleEquipment;
                return null;
            }
        }

        public int HiringCost
        {
            get
            {
                if (!IsFreelancer)
                {
                    return TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(Hero.MainHero.CharacterObject,
                        Contractor);
                }
                else
                {
                    return 
                        MBRandom.RoundRandomized(BannerKingsConfig.Instance.CompanionModel
                        .GetHiringPrice(Hero.MainHero, false) * 0.12f);
                }
            }
        }

        public int Wage
        {
            get
            {
                if (!IsFreelancer)
                {
                    return TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyWageModel.GetCharacterWage(Hero.MainHero.CharacterObject);
                }
                else
                {
                    return BannerKingsConfig.Instance.CompanionModel.GetHeroWage(Hero.MainHero);
                }
            }
        }

        public void Promote(CharacterObject template)
        {
            foreach (ItemRosterElement element in LastItems)
            {

            }
        }
    }
}
