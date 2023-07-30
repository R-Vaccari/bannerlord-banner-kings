using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Events.CourtEvents
{
    public class DestroyedCropsEvent : BannerKingsEvent
    {
        private Village village;
        private Estate estate;
        private bool isSlander;

        public DestroyedCropsEvent() : base("destroyed_crops")
        {
        }


        public override TextObject Name => new TextObject("{=n3j1wX9z}Destroyed Crops");

        public override BannerKingsEvent GetCopy(Hero hero)
        {
            DestroyedCropsEvent crops = new DestroyedCropsEvent();
            crops.Hero = hero;
            return crops;
        }

        public override bool IsPossible(Hero hero)
        {
            if (hero.IsClanLeader() && Hero.Clan.Villages.GetRandomElementWithPredicate(x =>
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(x.Settlement);
                return data.EstateData != null && data.EstateData.Estates.Any(x => x.Owner != hero);
            }) != null)
            {
                return true;
            }

            return false;
        }

        protected override void SetUp()
        {
            if (Hero.Clan != null)
            {
                village = Hero.Clan.Villages.GetRandomElementWithPredicate(x =>
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(x.Settlement);
                    return data.EstateData != null && data.EstateData.Estates.Any(x => x.Owner != Hero);
                });

                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
                estate = data.EstateData.Estates.GetRandomElementWithPredicate(x => x.Owner != Hero);
                if (village != null && estate != null)
                {
                    SetTexts();
                    if (Hero == Hero.MainHero)
                    {
                        ShowPlayerPrompt();
                    }
                    else
                    {
                        AiResolve();
                    }
                }
            }
        }

        protected override void SetTexts()
        {
            Description = new TextObject("{=qqxppUOY}Peasants at {VILLAGE} report that their crops were trampled on by cattle from {ESTATE}, owned by {OWNER}. They refuse any responsability, yet {HEADSMAN} swears the estate is at fault.")
                .SetTextVariable("HEADSMAN", village.Settlement.Notables.FirstOrDefault(x => x.IsHeadman).Name)
                .SetTextVariable("ESTATE", estate.Name)
                .SetTextVariable("VILLAGE", village.Name)
                .SetTextVariable("OWNER", estate.Owner.Name);
        }

        public override void ShowPlayerPrompt()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<EventResolution> Resolutions
        {
            get
            {
                yield return new EventResolution(new TextObject(),
                    new TextObject(),
                    null,
                    1,
                    0.1f,
                    null,
                    300,
                    (Hero fulfiller) =>
                    {
                        return true;
                    },
                    (Hero fulfiller) =>
                    {
                        return 1f;
                    },
                    (Hero fulfiller) =>
                    {

                    });
            }
        }
    }
}
