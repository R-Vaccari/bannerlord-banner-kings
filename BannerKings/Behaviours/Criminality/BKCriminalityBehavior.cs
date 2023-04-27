using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Criminality
{
    public class BKCriminalityBehavior : BannerKingsBehavior
    {
        private Dictionary<Hero, List<Crime>> crimes = new Dictionary<Hero, List<Crime>>();

        public override void RegisterEvents()
        {
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-crimes", ref crimes);
        }

        public void FinishCrime(Crime crime)
        {
            crimes[crime.Hero].Remove(crime);
            if (crimes[crime.Hero].IsEmpty())
            {
                crimes.Remove(crime.Hero);
            }
        }

        public Dictionary<Hero, List<Crime>> GetCriminals(Hero jailor)
        {
            Dictionary<Hero, List<Crime>> dic = new Dictionary<Hero, List<Crime>>();
            if (jailor == null || jailor.Clan == null || jailor.Clan.Kingdom == null)
            {
                return dic;
            }

            foreach (Settlement settlement in jailor.Clan.Settlements)
            {
                foreach (var prisoner in settlement.Party.PrisonerHeroes)
                {
                    Hero hero = prisoner.HeroObject;
                    if (crimes.ContainsKey(hero))
                    {
                        foreach (var crime in crimes[hero])
                        {
                            if (!dic.ContainsKey(hero))
                            {
                                dic.Add(hero, new List<Crime>());
                            }

                            if (crime.Kingdom == jailor.Clan.Kingdom)
                            {
                                dic[hero].Add(crime);
                            }
                        }
                    }
                }
            }

            return dic;
        }

        private void AddCrime(Hero hero, Crime crime)
        {
            if (!crimes.ContainsKey(hero))
            {
                crimes.Add(hero, new List<Crime>());
            }

            if (!crimes[hero].Contains(crime))
            {
                crimes[hero].Add(crime);
                if (Hero.MainHero.MapFaction == crime.Kingdom.MapFaction)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=!}{HERO} has been found guilty of the {CRIME} crime.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("CRIME", crime.Name)
                        .ToString(),
                        Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                }
            }
        }

        private void OnHeroPrisonerTaken(PartyBase party, Hero prisoner)
        {
            if (!party.MapFaction.IsKingdomFaction || prisoner.Occupation != Occupation.Bandit)
            {
                return;
            }

            Kingdom kingdom = (Kingdom)party.MapFaction;
            AddCrime(prisoner, DefaultCrimes.Instance.Banditry.GetCopy(prisoner, kingdom, Crime.CrimeSeverity.Transgression));
        }
    }
}
