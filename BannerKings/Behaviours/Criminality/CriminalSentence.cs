using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Criminality
{
    public class CriminalSentence : BannerKingsObject
    {
        private Func<Crime, bool> isAdequateForHero;
        private Action<Crime, Hero> executeSentence;
        private Func<Crime, Hero, bool> isSentenceTyranical;

        public CriminalSentence(string stringId) : base(stringId)
        {
            SentenceDate = CampaignTime.Never;
        }

        public void Initialize(TextObject name, TextObject description, Func<Crime, bool> isAdequateForHero,
            Func<Crime, Hero, bool> isSentenceTyranical, Action<Crime, Hero> executeSentence)
        {
            Initialize(name, description);
            this.isAdequateForHero = isAdequateForHero;
            this.isSentenceTyranical = isSentenceTyranical;
            this.executeSentence = executeSentence;
        }

        public CampaignTime SentenceDate { get; private set; }

        public bool IsSentenceTyranical(Crime crime, Hero executor) => isSentenceTyranical(crime, executor);

        public bool IsAdequateForHero(Crime crime) => isAdequateForHero(crime);

        public void ExecuteSentence(Crime crime, Hero executor)
        {
            SentenceDate = CampaignTime.Now;
            executeSentence(crime, executor);
        }
    }
}
