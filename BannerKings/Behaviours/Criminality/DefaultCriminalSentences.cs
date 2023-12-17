using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Criminality
{
    public class DefaultCriminalSentences : DefaultTypeInitializer<DefaultCriminalSentences, CriminalSentence>
    {
        public CriminalSentence Fine { get; } = new CriminalSentence("fine");
        public CriminalSentence CeaseCaravan { get; } = new CriminalSentence("cease_caravan");
        public CriminalSentence CeaseWorkshop { get; } = new CriminalSentence("cease_worshop");
        public CriminalSentence RevokeTitle { get; } = new CriminalSentence("revoke_title");
        public CriminalSentence Gelding { get; } = new CriminalSentence("gelding");
        public CriminalSentence Beheading { get; } = new CriminalSentence("beheading");

        public override IEnumerable<CriminalSentence> All
        {
            get
            {
                yield return Fine;
                yield return RevokeTitle;
                yield return Beheading;
            }
        }

        public override void Initialize()
        {
            Fine.Initialize(new TextObject("{=ma7sx96C}Monetary Fine"),
                new TextObject(),
                (Crime crime) =>
                {
                    Hero hero = crime.Hero;
                    if (hero.Occupation == Occupation.Bandit)
                    {
                        return false;
                    }

                    int cost = MBRandom.RoundRandomized(BannerKingsConfig.Instance.CrimeModel.GetMonetaryFine(crime).ResultNumber);
                    if (hero.Clan != null)
                    {
                        return hero.Gold >= cost || hero.Clan.Gold >= cost;
                    }

                    return hero.Gold >= cost;
                },
                (Crime crime, Hero executor) =>
                {
                    return false;
                },
                (Crime crime, Hero executor, CriminalSentence sentence) =>
                {
                    Hero hero = crime.Hero;
                    int cost = MBRandom.RoundRandomized(BannerKingsConfig.Instance.CrimeModel.GetMonetaryFine(crime).ResultNumber);
                    if (hero.Gold >= cost)
                    {
                        hero.ChangeHeroGold(-cost);
                    }
                    else if (hero.Clan != null)
                    {
                        hero.Clan.Leader.ChangeHeroGold(-cost);
                    }

                    executor.ChangeHeroGold(cost);
                });

            Beheading.Initialize(new TextObject("{=VsAvikWv}Beheading"),
               new TextObject(),
               (Crime crime) =>
               {
                   Hero hero = crime.Hero;
                   if (hero.Occupation == Occupation.Bandit)
                   {
                       return true;
                   }

                   return true;
               },
               (Crime crime, Hero executor) =>
               {
                   Hero hero = crime.Hero;
                   if (hero.Occupation == Occupation.Bandit)
                   {
                       return false;
                   }
                   
                   if (hero.MapFaction == crime.Kingdom.MapFaction)
                   {
                       return crime.Severity == Crime.CrimeSeverity.Treason;
                   }

                   return crime.Severity != Crime.CrimeSeverity.Treason;
               },
               (Crime crime, Hero executor, CriminalSentence sentence) =>
               {
                   Hero hero = crime.Hero;
                   if (sentence.IsSentenceTyranical(crime, executor))
                   {
                       foreach (Hero friend in Hero.AllAliveHeroes)
                       {
                           if (friend.IsFriend(hero))
                           {
                               bool affectRelatives;
                               int relationChangeForExecutingHero = TaleWorlds.CampaignSystem.Campaign.Current.Models.ExecutionRelationModel
                                   .GetRelationChangeForExecutingHero(hero, friend, out affectRelatives);
                               if (relationChangeForExecutingHero != 0)
                               {
                                   ChangeRelationAction.ApplyRelationChangeBetweenHeroes(executor,
                                       friend,
                                       relationChangeForExecutingHero,
                                       true);
                               }
                           }
                       }
                   }

                   KillCharacterAction.ApplyByMurder(hero, executor, false);
                   uint color = Utils.TextHelper.COLOR_LIGHT_YELLOW;
                   if (hero.Clan == Clan.PlayerClan || Hero.MainHero.IsFriend(hero))
                   {
                       color = Utils.TextHelper.COLOR_LIGHT_RED;
                   }

                   InformationManager.DisplayMessage(new InformationMessage(
                       new TextObject("{=WH4jmWgv}{HERO} has been beheaded as sentence to the {CRIME} crime.")
                       .SetTextVariable("HERO", hero.Name)
                       .SetTextVariable("CRIME", crime.Name)
                       .ToString(),
                       Color.FromUint(color)));
               });

            RevokeTitle.Initialize(new TextObject("{=wbOepT0V}Revoke Title"),
               new TextObject(),
               (Crime crime) =>
               {
                   Hero hero = crime.Hero;
                   return hero.IsLord && BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero).Count > 0;
               },
               (Crime crime, Hero executor) =>
               {
                   return crime.Severity != Crime.CrimeSeverity.Treason;
               },
               (Crime crime, Hero executor, CriminalSentence sentence) =>
               {
                   Hero hero = crime.Hero;
                   var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero);
                   int count = MathF.Max(1, (int)(titles.Count * 0.2f));
                   if (executor == Hero.MainHero)
                   {
                       List<InquiryElement> inquiryElements = new List<InquiryElement>();
                       foreach (var title in titles)
                       {
                           inquiryElements.Add(new InquiryElement(
                               title,
                               title.FullName.ToString(),
                               null));
                       }

                       MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                           new TextObject("{=wbOepT0V}Revoke Title").ToString(),
                           new TextObject("{=5pOghsyn}Sentence {HERO} to lose his title properties to pay for the {CRIME} crime. 20% of their titles may be ceased, at a minimum of 1.")
                           .SetTextVariable("HERO", hero.Name)
                           .SetTextVariable("CRIME", crime.Name)
                           .ToString(),
                           inquiryElements,
                           true,
                           1,
                           count,
                           GameTexts.FindText("").ToString(),
                           GameTexts.FindText("").ToString(),
                           (List<InquiryElement> List) =>
                           {
                               foreach (var element in List)
                               {
                                   FeudalTitle result = (FeudalTitle)element.Identifier;
                                   BannerKingsConfig.Instance.TitleManager.InheritTitle(hero, executor, result);
                               }
                           },
                           null),
                           true);
                   }
                   else
                   {
                       var results = titles.Take(count);
                       foreach (var result in results)
                       {
                           BannerKingsConfig.Instance.TitleManager.InheritTitle(hero, executor, result);
                       }
                   }
               });
        }
    }
}
