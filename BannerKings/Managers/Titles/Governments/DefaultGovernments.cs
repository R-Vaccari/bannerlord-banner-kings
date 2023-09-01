using System;
using System.Collections.Generic;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultGovernments : DefaultTypeInitializer<DefaultGovernments, Government>
    {
        public Government Republic { get; } = new Government("Republic");
        public Government Imperial { get; } = new Government("Imperial");
        public Government Feudal { get; } = new Government("Feudal");
        public Government Tribal { get; } = new Government("Tribal");

        public override IEnumerable<Government> All => throw new NotImplementedException();

        public override void Initialize()
        {
            Republic.Initialize(new TextObject(),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<TaleWorlds.CampaignSystem.PolicyObject>()
                {

                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.Republic
                });

            Imperial.Initialize(new TextObject(),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<TaleWorlds.CampaignSystem.PolicyObject>()
                {

                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.Imperial
                });

            Tribal.Initialize(new TextObject(),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<TaleWorlds.CampaignSystem.PolicyObject>()
                {

                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.TribalElective
                });

            Feudal.Initialize(new TextObject(),
                new TextObject(),
                new TextObject(),
                0.5f,
                new List<TaleWorlds.CampaignSystem.PolicyObject>()
                {

                },
                new List<Succession>()
                {
                    DefaultSuccessions.Instance.FeudalElective
                });
        }
    }
}
