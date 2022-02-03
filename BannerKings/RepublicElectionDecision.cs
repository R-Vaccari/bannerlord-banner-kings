using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using System.Linq;
using TaleWorlds.Localization;

namespace BannerKings
{
    public class RepublicElectionDecision : KingSelectionKingdomDecision
    {

		public override TextObject GetChooseTitle()
		{
			TextObject textObject = new TextObject("{=!}Choose the next Grand-Prince of the {KINGDOM_NAME} Republic", null);
			textObject.SetTextVariable("KINGDOM_NAME", base.Kingdom.Name);
			return textObject;
		}

		public override TextObject GetSupportDescription()
		{
			TextObject textObject = new TextObject("{=!}{KINGDOM_NAME} Republic will decide who will be elected Grand-Prince. You can pick your stance regarding this decision.", null);
			textObject.SetTextVariable("KINGDOM_NAME", base.Kingdom.Name);
			return textObject;
		}

		public override TextObject GetChooseDescription()
		{
			TextObject textObject = new TextObject("{=!}Choose the next Grand-Prince of the {KINGDOM_NAME} Republic", null);
			textObject.SetTextVariable("KINGDOM_NAME", base.Kingdom.Name);
			return textObject;
		}

		public RepublicElectionDecision(Clan proposerClan, Clan clanToExclude = null) : base(proposerClan, clanToExclude)
        {

        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
			Dictionary<Clan, float> dictionary = new Dictionary<Clan, float>();
			foreach (Clan clan in base.Kingdom.Clans)
			{
				if (!clan.IsUnderMercenaryService && !clan.IsEliminated)
				{
					Hero leader = clan.Leader;
					float age = leader.Age;
					float strength = Campaign.Current.Models.DiplomacyModel.GetClanStrength(clan) / 5f;
					float renown = clan.Renown / 5f;
					dictionary.Add(clan, (age + strength + renown) / 3f);
				}
			}
			IEnumerable<KeyValuePair<Clan, float>> enumerable = (from t in dictionary
																 orderby t.Value descending
																 select t).Take(3);
			foreach (KeyValuePair<Clan, float> keyValuePair in enumerable)
				yield return new KingSelectionKingdomDecision.KingSelectionDecisionOutcome(keyValuePair.Key.Leader);
			
			IEnumerator<KeyValuePair<Clan, float>> enumerator2 = null;
			yield break;
		}
    }
}
