using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class Festival : RecurrentRite
    {
        protected Settlement festivalPlace;

        public override void Execute(Hero executor)
        {
            if (!MeetsCondition(executor))
            {
                return;
            }

            var options = new List<InquiryElement>();
            foreach (var fief in executor.Clan.Fiefs)
            {
                options.Add(new InquiryElement(fief.Settlement, fief.Name.ToString(), null));
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GetName().ToString(),
                    GetDescription().ToString(),
                    options,
                    false,
                    1,
                    GameTexts.FindText("str_done").ToString(),
                    string.Empty,
                    delegate (List<InquiryElement> x)
                    {
                        festivalPlace = (Settlement?)x[0].Identifier;
                    },
                    null,
                    string.Empty));
        }

        public override float GetPietyReward()
        {
            return 100;
        }

        public override bool MeetsCondition(Hero hero) => hero.Clan != null && hero.IsClanLeader() &&
            hero.Clan.Fiefs.Count > 0;
    }
}
