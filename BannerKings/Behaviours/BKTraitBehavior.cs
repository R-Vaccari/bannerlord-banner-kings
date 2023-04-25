using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public class BKTraitBehavior : BannerKingsBehavior
    {
        public override void RegisterEvents()
        {
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnHeroCreated(Hero hero, bool bornNaturally)
        {
            foreach (TraitObject trait in BKTraits.Instance.PersonalityTraits)
            {
                float chance = 0.01f;
                int level = 1;

                if (MBRandom.RandomFloat < MBRandom.RandomFloat)
                {
                    level += MBRandom.RandomInt(-1, 1);
                }

                if (MBRandom.RandomFloat < MBRandom.RandomFloat)
                {
                    level += MBRandom.RandomInt(-1, 1);
                }

                if (hero.IsGangLeader)
                {
                    if (trait == BKTraits.Instance.Deceitful)
                    {
                        chance += 0.09f;
                        level += 1;
                    }

                    if (trait == BKTraits.Instance.Just)
                    {
                        level -= 2;
                    }
                }

                if (hero.IsPreacher)
                {
                    if (trait == BKTraits.Instance.Zealous)
                    {
                        chance += 0.13f;
                        level += 1;
                    }
                }

                var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
                if (religion == null)
                {
                    if (trait == BKTraits.Instance.Zealous)
                    {
                        chance = 0f;
                    }
                }

                if (MBRandom.RandomFloat < chance)
                {
                    hero.SetTraitLevel(trait, level);
                }
            }
        }
    }
}
