using BannerKings.Managers.Kingdoms.Succession;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class Succession : BannerKingsObject
    {
        private Func<Hero, FeudalTitle, HashSet<Hero>> getSuccessionCandidates;
        private Func<Hero, Hero, FeudalTitle, bool, ExplainedNumber> calculateHeirScore;
        private Func<Kingdom, bool> isAdequate;
        public Succession(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, bool elected, float authoritarian, float oligarchic, float egalitarian,
            TextObject candidatesText,
            TextObject scoreText,
            Func<Hero, FeudalTitle, HashSet<Hero>> getSuccessionCandidates,
            Func<Hero, Hero, FeudalTitle, bool, ExplainedNumber> calculateHeirScore,
            Func<Kingdom, bool> isAdequate = null)
        {
            Initialize(name, description);
            Authoritarian = authoritarian;
            Oligarchic = oligarchic;
            Egalitarian = egalitarian;
            CandidatesText = candidatesText;
            ScoreText = scoreText;
            this.getSuccessionCandidates = getSuccessionCandidates;
            this.calculateHeirScore = calculateHeirScore;
            this.isAdequate = isAdequate;
        }

        public void PostInitialize()
        {
            Succession s = DefaultSuccessions.Instance.GetById(this);
            Initialize(s.name, s.description, s.ElectedSuccession, s.Authoritarian, s.Oligarchic,
                s.Egalitarian, s.CandidatesText, s.ScoreText, s.getSuccessionCandidates,
                s.calculateHeirScore, s.isAdequate);
        }

        public bool IsKingdomAdequate(Kingdom kingdom)
        {
            if (isAdequate != null) isAdequate(kingdom);
            return true;
        }

        public float Authoritarian { get; private set; }
        public float Oligarchic { get; private set; }
        public float Egalitarian { get; private set; }

        public TextObject CandidatesText { get; private set; }
        public TextObject ScoreText { get; private set; }

        public HashSet<Hero> GetSuccessionCandidates(Hero currentLeader, FeudalTitle title) 
            => getSuccessionCandidates(currentLeader, title);
        public ExplainedNumber CalculateHeirScore(Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations = false)
            => calculateHeirScore.Invoke(currentLeader, candidate, title, explanations);

        public bool ElectedSuccession { get; private set; }
    }
}
