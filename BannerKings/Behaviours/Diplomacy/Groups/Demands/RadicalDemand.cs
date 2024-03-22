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

        public abstract void EndRebellion(Kingdom rebels, Kingdom original, bool success);

        protected void FinishRadicalDemand()
        {
            if (Group != null)
            {
                Group.Members.Clear();
                Group.SetLeader(null);

                if (Group.KingdomDiplomacy.Kingdom == Clan.PlayerClan.MapFaction)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=sKFZDhiA}The radical {GROUP} group has been dissolved.")
                        .SetTextVariable("GROUP", Group.Name)
                        .ToString(),
                        Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                }
            }
        }
    }
}
