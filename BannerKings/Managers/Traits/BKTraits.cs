using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Traits
{
    public class BKTraits : DefaultTypeInitializer<BKTraits, TraitObject>
    {
        public TraitObject PresetScholar { get; } = new TraitObject("PresetScholar");
        public TraitObject PresetDruid { get; } = new TraitObject("PresetDruid");

        public TraitObject Just { get; private set; }
        public TraitObject Humble { get; private set; }
        public TraitObject Diligent { get; private set; }
        public TraitObject Ambitious { get; private set; }
        public TraitObject Zealous { get; private set; }
        public TraitObject Seductive { get; private set; }

        public TraitObject AptitudeViolence { get; private set; }
        public TraitObject AptitudeErudition { get; private set; }
        public TraitObject AptitudeSocializing { get; private set; }

        public TraitObject Musician { get; } = new TraitObject("Musician");

        public TraitObject Castrated { get; } = new TraitObject("Castrated");

        public TraitObject CongenitalAttractive { get; private set; }
        public TraitObject CongenitalIntelligent { get; private set; }
        public TraitObject CongenitalRobust { get; private set; }

        public TraitObject Stress { get; private set; }

        public override IEnumerable<TraitObject> All => throw new NotImplementedException();

        public IEnumerable<TraitObject> PresetTraits
        {
            get
            {
                yield return PresetScholar;
            }
        }

        public IEnumerable<TraitObject> CongenitalTraits
        {
            get
            {
                yield return CongenitalAttractive;
                yield return CongenitalIntelligent;
                yield return CongenitalRobust;
            }
        }

        public IEnumerable<TraitObject> PoliticalTraits
        {
            get
            {
                yield return DefaultTraits.Oligarchic;
                yield return DefaultTraits.Authoritarian;
                yield return DefaultTraits.Egalitarian;
            }
        }

        public IEnumerable<TraitObject> PersonalityTraits
        {
            get
            {
                yield return DefaultTraits.Honor;
                yield return DefaultTraits.Calculating;
                yield return DefaultTraits.Mercy;
                yield return DefaultTraits.Valor;
                yield return DefaultTraits.Generosity;
                yield return Just;
                yield return Humble;
                yield return Diligent;
                yield return Ambitious;
                yield return Zealous;
                yield return Seductive;
            }
        }

        public IEnumerable<TraitObject> AptitudeTraits
        {
            get
            {
                yield return AptitudeViolence;
                yield return AptitudeErudition;
                yield return AptitudeSocializing;
            }
        }

        public IEnumerable<TraitObject> LifestyleTraits 
        { 
            get
            {
                yield return Musician;
            } 
        }

        public IEnumerable<TraitObject> MedicalTraits
        {
            get
            {
                yield return Castrated;
            }
        }

        public override void Initialize()
        {
            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("CongenitalAttractive"));
            CongenitalAttractive.Initialize(new TextObject("{=!}Attractiveness"),
                new TextObject("{=!}Attractiveness is how objectively likable one's physical appearance is. It is a trait inherited from one's parents. Attractive persons are naturally more Charmful and tend to have more children with their spouses.\nEffects:\nAdded Charm points\nIncreased fertility\nCharacters are more sexually attracted to you"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("CongenitalIntelligent"));
            CongenitalIntelligent.Initialize(new TextObject("{=!}Intellect"),
                new TextObject("{=!}A person's ability to understand topics in depth. It is a trait inherited from one's parents. Persons with good intellect are considered more Intelligent and learn topics quicker.\nEffects:\nAdded Intelligence points\nFaster skill learning"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("CongenitalRobust"));
            CongenitalRobust.Initialize(new TextObject("{=!}Robustness"),
                new TextObject("{=!}Robustness describes one's physical resilience. It is a trait inherited from one's parents. Robust persons are said to have more Endurance and be harder to kill on the battlefield.\nEffects:\nAdded Endurance points\nIncreased character health"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Seductive"));
            Seductive.Initialize(new TextObject("{=x384LrUe}Seductiveness"),
                new TextObject("{=!}Seductiveness is one's ability to sexually charm someone else. Seductive persons have an easier time coupling with their spouses and are more liked as marriage prospects.\nEffects:\nIncreased fertility\nCharacters are more sexually attracted to you"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Just"));
            Just.Initialize(new TextObject("{=1q03XzNp}Justice"),
                new TextObject("{=!}Justice is the appropriate punishment and reward for a given deed. Just rulers are often respected by their vassals for delivering appropriate sentences, but also adequately rewarding loyalty.\nEffects:\nIncreased settlement stability\nDecreased relation loss from political disagreements"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Humble"));
            Humble.Initialize(new TextObject("{=g2YTeU2e}Humility"),
                new TextObject("{=!}Humility describes a person's ability to restrain their ego. Humble persons are often more generally likable by their peers, by those who outrank them and those beneath them.\nEffects:\nIncreased settlement loyalty\nImproved relations gain"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Diligent"));
            Diligent.Initialize(new TextObject("{=18oVwc0j}Diligence"),
                new TextObject("{=!}Diligence is one's willingness to work tirelessly towards their goal. Diligent persons are constantly working on a project, or otherwise feel like withering away. Though this means they accomplish more, it is also intense on their psyche.\nEffects:\nFaster progress in Education topics\nHigher stress gain"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Ambitious"));
            Ambitious.Initialize(new TextObject("{=vZizAC8R}Ambition"),
                new TextObject("{=!}Ambition is one's desire to ascend in their social status. An ambitious vassal is always looking for ways to further their position, often at the expense of others."),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("Zealous"));
            Zealous.Initialize(new TextObject("{=HLML3UUw}Zealotry"),
                new TextObject("{=e18wpkZB}Zealotry is one's fervor for their faith. Zealots will favor their faith's teachings over the opinions of others, and justify their actions through faith. Zealous persons are seen are more pious within their faith.\nEffects:\nIncreased piety gain\nStronger disagreements over faith"),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("AptitudeViolence"));
            AptitudeViolence.Initialize(new TextObject("{=6Az4EDih}Violence"),
                new TextObject("{=yVBYM231}Aptitude for violence describes how inclined one is towards hurting others. Violent persons are more inclined to learn combat skills."),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("AptitudeErudition"));
            AptitudeErudition.Initialize(new TextObject("{=b7tgia5e}Erudition"),
                new TextObject("{=3GMTHrkh}Aptitude for erudition describes how inclined one is towards intellectual tasks. Erudite persons are more inclined to learn intellectual skills such as Medicine, Scholarship, and others."),
                true,
                -2,
                2);

            CongenitalAttractive = Game.Current.ObjectManager.RegisterPresumedObject(new TraitObject("AptitudeSocializing"));
            AptitudeSocializing.Initialize(new TextObject("{=o2Zvo6fr}Socializing"),
                new TextObject("{=1ABqAjFQ}Aptitude for socializing describes how apt one is in expressing themselves and resolving conflicts. Social persons are more inclined to learn Leadership, Charm and Trade skills."),
                true,
                -2,
                2);
        }
    }
}
