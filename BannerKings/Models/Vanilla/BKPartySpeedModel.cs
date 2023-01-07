using BannerKings.Behaviours;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartySpeedModel : DefaultPartySpeedCalculatingModel
    {
        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            var baseResult = base.CalculateFinalSpeed(mobileParty, finalSpeed);
            if (mobileParty.LeaderHero != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.LeaderHero);
                if (data.HasPerk(BKPerks.Instance.FianHighlander))
                {
                    baseResult.AddFactor(0.05f, BKPerks.Instance.FianHighlander.Name);
                }

                var faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(mobileParty.CurrentNavigationFace);
                if (faceTerrainType == TaleWorlds.Core.TerrainType.Desert && data.HasPerk(BKPerks.Instance.JawwalDuneRider))
                {
                    baseResult.AddFactor(0.8f, BKPerks.Instance.JawwalDuneRider.Name);
                }

                if (data.HasPerk(BKPerks.Instance.CaravaneerStrider))
                {
                    baseResult.AddFactor(0.03f, BKPerks.Instance.CaravaneerStrider.Name);
                }

                if (Campaign.Current.IsNight && data.HasPerk(BKPerks.Instance.OutlawNightPredator))
                {
                    baseResult.AddFactor(0.06f, BKPerks.Instance.OutlawNightPredator.Name);
                }

                if (data.Lifestyle != null)
                {
                    if (data.Lifestyle.Equals(DefaultLifestyles.Instance.Outlaw))
                    {
                        var count = 0;
                        foreach (var element in mobileParty.MemberRoster.GetTroopRoster())
                        {
                            if (element.Character.IsHero || element.Character.Occupation == Occupation.Bandit)
                            {
                                count += element.Number;
                            }
                        }

                        baseResult.AddFactor(count / mobileParty.MemberRoster.TotalManCount * 0.1f, data.Lifestyle.Name);
                    }   
                    else if (data.Lifestyle.Equals(DefaultLifestyles.Instance.Varyag))
                    {
                        var count = 0;
                        foreach (var element in mobileParty.MemberRoster.GetTroopRoster())
                        {
                            if (element.Character.IsHero || element.Character.Occupation == Occupation.Bandit)
                            {
                                count += element.Number;
                            }
                        }

                        baseResult.AddFactor(count / mobileParty.MemberRoster.TotalManCount * 0.08f, data.Lifestyle.Name);
                    }
                } 
            }

            if (mobileParty.IsCaravan && mobileParty.Owner != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.Owner);
                if (Campaign.Current.IsDay && data.HasPerk(BKPerks.Instance.CaravaneerDealer))
                {
                    baseResult.AddFactor(0.05f, BKPerks.Instance.FianHighlander.Name);
                }
            }

            if (mobileParty.LeaderHero == Hero.MainHero && Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>()
                    .HasDebuff(DefaultStartOptions.Instance.Caravaneer))
            {
                baseResult.AddFactor(-0.05f, DefaultStartOptions.Instance.Caravaneer.Name);
            }

            if (BannerKingsSettings.Instance.SlowerParties > 0f)
            {
                baseResult.AddFactor(-BannerKingsSettings.Instance.SlowerParties, new TaleWorlds.Localization.TextObject("{=OohdenyR}Slower Parties setting"));
            }

            return baseResult;
        }
    }
}