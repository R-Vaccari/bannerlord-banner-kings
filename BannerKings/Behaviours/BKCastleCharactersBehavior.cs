using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Behaviours
{
    public class BKCastleCharactersBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(this.DailyTickSettlement));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
    
        }

        // Learn with it, don't copy it ;)
        private void DailyTickSettlement(Settlement settlement)
        {
            if (!settlement.IsCastle) return;

            int desiredAmont = 0;
            List<Occupation> list = new List<Occupation> { Occupation.RuralNotable, Occupation.Headman };
            foreach (Occupation occ in list)
                desiredAmont += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occ);

            float randomFloat = MBRandom.RandomFloat;
            int count = settlement.Notables.Count;
            float num2 = settlement.Notables.Any<Hero>() ? ((float)(desiredAmont - settlement.Notables.Count) / (float)desiredAmont) : 1f;
            num2 *= MathF.Pow(num2, 0.36f);
            if (randomFloat <= num2)
            {
                List<Occupation> list2 = new List<Occupation>();
                foreach (Occupation occupation2 in list)
                {
                    int num3 = 0;
                    using (List<Hero>.Enumerator enumerator2 = settlement.Notables.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                            if (enumerator2.Current.CharacterObject.Occupation == occupation2)
                                num3++;
                    }
                    int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation2);
                    if (num3 < targetNotableCountForSettlement)
                        list2.Add(occupation2);
                    
                }
                if (list2.Count > 0)
                    EnterSettlementAction.ApplyForCharacterOnly(HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement<Occupation>(), settlement), settlement);
                
            }

			UpdateVolunteers(settlement);
        }

        private void UpdateVolunteers(Settlement settlement)
        {
			foreach (Hero hero in settlement.Notables)
			{
				if (hero.CanHaveRecruits)
				{
					bool flag = false;
					CharacterObject basicVolunteer = Campaign.Current.Models.VolunteerProductionModel.GetBasicVolunteer(hero);
					for (int i = 0; i < 6; i++)
					{
						if (MBRandom.RandomFloat < Campaign.Current.Models.VolunteerProductionModel.GetDailyVolunteerProductionProbability(hero, i, settlement))
						{
							CharacterObject characterObject = hero.VolunteerTypes[i];
							if (characterObject == null)
							{
								hero.VolunteerTypes[i] = basicVolunteer;
								flag = true;
							}
							else
							{
								float num = 40000f / (MathF.Max(50f, hero.Power) * MathF.Max(50f, hero.Power));
								int tier = characterObject.Tier;
								if (MBRandom.RandomInt((int)MathF.Max(2f, (float)tier * num)) == 0 && characterObject.UpgradeTargets.Length != 0 && characterObject.Tier <= 3)
								{
									hero.VolunteerTypes[i] = characterObject.UpgradeTargets[MBRandom.RandomInt(characterObject.UpgradeTargets.Length)];
									flag = true;
								}
							}
						}
					}
					if (flag)
					{
						CharacterObject[] volunteerTypes = hero.VolunteerTypes;
						for (int j = 1; j < 6; j++)
						{
							CharacterObject characterObject2 = volunteerTypes[j];
							if (characterObject2 != null)
							{
								int num2 = 0;
								int num3 = j - 1;
								CharacterObject characterObject3 = volunteerTypes[num3];
								while (num3 >= 0 && (characterObject3 == null || (float)characterObject2.Level + (characterObject2.IsMounted ? 0.5f : 0f) < (float)characterObject3.Level + (characterObject3.IsMounted ? 0.5f : 0f)))
								{
									if (characterObject3 == null)
									{
										num3--;
										num2++;
										if (num3 >= 0)
										{
											characterObject3 = volunteerTypes[num3];
										}
									}
									else
									{
										volunteerTypes[num3 + 1 + num2] = characterObject3;
										num3--;
										num2 = 0;
										if (num3 >= 0)
										{
											characterObject3 = volunteerTypes[num3];
										}
									}
								}
								volunteerTypes[num3 + 1 + num2] = characterObject2;
							}
						}
					}
				}
			}
		}

        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {

           
        }

      
    }
}
