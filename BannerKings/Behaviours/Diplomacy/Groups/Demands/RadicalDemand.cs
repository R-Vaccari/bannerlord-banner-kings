using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups.Demands
{
    public abstract class RadicalDemand : Demand
    {
        protected RadicalDemand(string stringId) : base(stringId)
        {
        }

        protected void ReintegrateMembers(Kingdom rebels, Kingdom original)
        {
            List<Clan> clans = new List<Clan>(rebels.Clans);
            foreach (Clan c in clans)
            {
                ChangeKingdomAction.ApplyByJoinToKingdom(c, original);
            }
        }

        public new void ShowPlayerPrompt()
        {
            SetTexts();
            InformationManager.ShowInquiry(new InquiryData(Name.ToString(),
                PlayerPromptText.ToString(),
                true,
                false,
                new TextObject("{=j90Aa0xG}Resolve").ToString(),
                "",
                () => ShowPlayerDemandAnswers(),
                null,
                Utils.Helpers.GetKingdomDecisionSound()),
                true,
                true);
        }

        public abstract void EndRebellion(Kingdom rebels, Kingdom original, bool success);

        protected void FinishRadicalDemand()
        {
            if (Group != null)
            {
                if (Group.Leader == Hero.MainHero && Group.KingdomDiplomacy.Kingdom == Clan.PlayerClan.MapFaction)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=sKFZDhiA}The radical {GROUP} group has been dissolved.")
                        .SetTextVariable("GROUP", Group.Name)
                        .ToString(),
                        Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                }

                Group.Members.Clear();
                Group.SetLeader(null);
            }
        }

        public new void Tick()
        {
            if (!Active) Finish();
        }
    }
}
