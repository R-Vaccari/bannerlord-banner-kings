using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using TaleWorlds.TwoDimension;

namespace BannerKings.Managers.Kingdoms.Succession
{
    public class BKKingElectionDecision : KingSelectionKingdomDecision
    {
        [SaveableProperty(100)] protected FeudalTitle Title { get; set; }
        [SaveableProperty(101)] protected Hero PreviousRuler { get; set; }

        public BKKingElectionDecision(Clan proposerClan, FeudalTitle title, Hero previousRuler, Clan clanToExclude = null) 
            : base(proposerClan, clanToExclude)
        {
            Title = title;
            PreviousRuler = previousRuler;
        }

        public override bool IsAllowed() => Title != null && PreviousRuler != null && 
            DetermineInitialCandidates().Count() >= 3 && base.IsAllowed();

        public override IEnumerable<DecisionOutcome> DetermineInitialCandidates()
        {
            foreach (var candidate in BannerKingsConfig.Instance.TitleModel.CalculateSuccessionLine(Title, PreviousRuler.Clan, PreviousRuler, 3))
            {
                yield return new KingSelectionDecisionOutcome(candidate.Key);
            }
        }

        public override void ApplyChosenOutcome(DecisionOutcome chosenOutcome)
        {
            base.ApplyChosenOutcome(chosenOutcome);
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Kingdom);
            if (title != null)
            {
                var deJure = title.deJure;
                var king = ((KingSelectionDecisionOutcome)chosenOutcome).King;
                if (deJure != king)
                {
                    BannerKingsConfig.Instance.TitleManager.InheritTitle(deJure, king, title);
                }
            }
        }

        public override float DetermineSupport(Clan clan, DecisionOutcome possibleOutcome)
        {
            Hero candidate = ((KingSelectionDecisionOutcome)possibleOutcome).King;
            var result = new ExplainedNumber(BannerKingsConfig.Instance.TitleModel.GetSuccessionHeirScore(PreviousRuler, candidate, Title).ResultNumber * 10f);

            if (clan != candidate.Clan)
            {
                result.Add(clan.Leader.GetRelation(candidate) * 10f);
            }
            else
            {
                result.Add(1000f);
                return result.ResultNumber;
            }

            if (candidate.Culture != Kingdom.Culture)
            {
                result.AddFactor(-0.2f);
            }

            var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
            if (religion != null)
            {
                var candidateReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(candidate);
                if (candidateReligion != religion)
                {
                    var stance = FaithStance.Untolerated;
                    if (candidateReligion != null)
                    {
                        stance = religion.GetStance(candidateReligion.Faith);
                    }

                    switch (stance)
                    {
                        case FaithStance.Untolerated:
                            result.AddFactor(-0.15f);
                            break;
                        case FaithStance.Hostile:
                            result.AddFactor(-0.4f);
                            break;
                    }
                }
            }

            float oligarchic = 100f - (Mathf.Abs(candidate.GetTraitLevel(DefaultTraits.Oligarchic) -
                clan.Leader.GetTraitLevel(DefaultTraits.Oligarchic)) * 12f);
            float egalitarian = 100f - (MathF.Abs(candidate.GetTraitLevel(DefaultTraits.Egalitarian) -
                clan.Leader.GetTraitLevel(DefaultTraits.Egalitarian)) * 12f);
            float authoritarian = 100f - (MathF.Abs(candidate.GetTraitLevel(DefaultTraits.Authoritarian) -
                clan.Leader.GetTraitLevel(DefaultTraits.Authoritarian)) * 12f);
            result.Add(oligarchic);
            result.Add(egalitarian);
            result.Add(authoritarian);

            result.AddFactor(clan.Leader.GetRelation(candidate) * 0.02f);
            return result.ResultNumber;
        }

        public override float CalculateMeritOfOutcome(DecisionOutcome candidateOutcome)
        {
            float num = 1f;
            foreach (Clan clan in base.Kingdom.Clans)
            {
                if (clan.Leader != Hero.MainHero)
                {
                    num += DetermineSupport(clan, candidateOutcome);
                }
            }

            return num;
        }
    }
}