using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Kingdoms
{
    public class RepublicElectionDecision : BKKingElectionDecision
    {
        public RepublicElectionDecision(Clan proposerClan, Clan clanToExclude = null) : base(proposerClan, clanToExclude)
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

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            var dictionary = new Dictionary<Clan, float>();
            foreach (var clan in Kingdom.Clans)
            {
                if (!clan.IsUnderMercenaryService && !clan.IsEliminated && clan != toExclude)
                {
                    var leader = clan.Leader;
                    var age = leader.Age;
                    var strength = Campaign.Current.Models.DiplomacyModel.GetClanStrength(clan) / 5f;
                    var renown = clan.Renown / 5f;
                    dictionary.Add(clan, (age + strength + renown + MBRandom.RandomFloat - MBRandom.RandomFloat) / 3f);
                }
            }

            var enumerable = (from t in dictionary
                orderby t.Value descending
                select t).Take(3);
            foreach (var keyValuePair in enumerable)
            {
                yield return new KingSelectionDecisionOutcome(keyValuePair.Key.Leader);
            }
        }

        public override float CalculateMeritOfOutcome(DecisionOutcome candidateOutcome)
        {
            var merit = 0f;
            foreach (var clan in Kingdom.Clans)
            {
                merit += CalculateMeritInternal(clan, candidateOutcome);
            }

            return merit;
        }

        private float CalculateMeritInternal(Clan clan, DecisionOutcome candidateOutcome)
        {
            var merit = 0f;
            var leader = clan.Leader;
            if (leader != Hero.MainHero)
            {
                if (clan == Kingdom.RulingClan)
                {
                    merit -= 5f;
                }

                merit += leader.Age / 10f;
                merit += clan.Tier / 2f;
                merit += MBRandom.RandomFloat;
                merit -= MBRandom.RandomFloat * 2f;
                merit += leader.GetSkillValue(DefaultSkills.Charm) * 0.01f;
                merit += leader.GetSkillValue(DefaultSkills.Steward) * 0.01f;

                if (leader.GetTraitLevel(DefaultTraits.Authoritarian) > 0)
                {
                    merit -= 1f;
                }
            }

            return MathF.Clamp(merit, -3f, 8f);
        }
    }
}