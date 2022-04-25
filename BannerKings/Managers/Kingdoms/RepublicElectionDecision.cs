using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using System.Linq;
using TaleWorlds.Localization;
using TaleWorlds.Library;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms
{
    public class RepublicElectionDecision : BKKingElectionDecision
	{
		[SaveableProperty(200)]
		protected Clan toExclude { get; set; }
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
			this.toExclude = clanToExclude;
        }

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
			Dictionary<Clan, float> dictionary = new Dictionary<Clan, float>();
			foreach (Clan clan in base.Kingdom.Clans)
			{
				if (!clan.IsUnderMercenaryService && !clan.IsEliminated && clan != this.toExclude)
				{
					Hero leader = clan.Leader;
					float age = leader.Age;
					float strength = Campaign.Current.Models.DiplomacyModel.GetClanStrength(clan) / 5f;
					float renown = clan.Renown / 5f;
					dictionary.Add(clan, (age + strength + renown + MBRandom.RandomFloat - MBRandom.RandomFloat) / 3f);
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

        public override float CalculateMeritOfOutcome(DecisionOutcome candidateOutcome)
        {
			float merit = 0f;
			foreach (Clan clan in base.Kingdom.Clans)
				merit += CalculateMeritInternal(clan, candidateOutcome);
			return merit;
		}

		private float CalculateMeritInternal(Clan clan, DecisionOutcome candidateOutcome)
        {
			float merit = 0f;
			Hero leader = clan.Leader;
			if (leader != Hero.MainHero)
			{
				if (clan == base.Kingdom.RulingClan)
					merit -= 5f;

				merit += leader.Age / 10f;
				merit += (float)clan.Tier / 2f;
				merit += MBRandom.RandomFloat;
				merit -= MBRandom.RandomFloat * 2f;
				merit += leader.GetSkillValue(DefaultSkills.Charm) * 0.01f;
				merit += leader.GetSkillValue(DefaultSkills.Steward) * 0.01f;

				if (leader.GetTraitLevel(DefaultTraits.Authoritarian) > 0)
					merit -= 1f;
			}
			return MathF.Clamp(merit, -3f, 8f);
		}
    }
}
