using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms.Succession
{
    public class RepublicElectionDecision : BKKingElectionDecision
    {
        public RepublicElectionDecision(Clan proposerClan, Clan clanToExclude = null) : base(proposerClan,
            BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(proposerClan.Kingdom),
            proposerClan.Leader,
            clanToExclude
            )
        {
            toExclude = clanToExclude;
        }

        [SaveableProperty(200)] protected Clan toExclude { get; set; }

        public override TextObject GetChooseTitle()
        {
            var textObject = new TextObject("{=iD9oLBip}Choose the next Grand-Prince of the {KINGDOM_NAME} Republic");
            textObject.SetTextVariable("KINGDOM_NAME", Kingdom.Name);
            return textObject;
        }

        public override TextObject GetSupportDescription()
        {
            var textObject = new TextObject("{=NOKaWV7r}{KINGDOM_NAME} Republic will decide who will be elected Grand-Prince. You can pick your stance regarding this decision.");
            textObject.SetTextVariable("KINGDOM_NAME", Kingdom.Name);
            return textObject;
        }

        public override TextObject GetChooseDescription()
        {
            var textObject = new TextObject("{=iD9oLBip}Choose the next Grand-Prince of the {KINGDOM_NAME} Republic");
            textObject.SetTextVariable("KINGDOM_NAME", Kingdom.Name);
            return textObject;
        }
    }
}