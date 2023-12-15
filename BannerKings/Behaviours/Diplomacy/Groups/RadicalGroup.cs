using BannerKings.Actions;
using BannerKings.Utils.Extensions;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class RadicalGroup : DiplomacyGroup
    {
        public RadicalGroup(string stringId) : base(stringId)
        {
        }

        public float TotalStrength
        {
            get
            {
                float power = 0f;
                foreach (Hero member in Members)
                    power += member.Clan.TotalStrength;

                return power;
            }
        }

        public float PowerProportion
        {
            get
            {
                float revoltPower = TotalStrength;
                float kingdomPower = 0f;
                foreach (Clan clan in KingdomDiplomacy.Kingdom.Clans)
                {
                    if (!Members.Contains(clan.Leader)) kingdomPower += clan.TotalStrength;
                }

                return revoltPower / kingdomPower;
            }
        }

        public TextObject KingdomName { get; private set; }
        public float Radicalism { get; private set; }
        public override bool IsInterestGroup => false;

        public override void AddMember(Hero hero)
        {
            if (hero != null && !Members.Contains(hero) && CanHeroJoin(hero, KingdomDiplomacy))
            {
                AddMemberInternal(hero);
                if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has joined the {GROUP} group.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
                else if (hero.MapFaction.Leader == Hero.MainHero) 
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has joined the {GROUP} radical group against you!")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public override bool CanHeroJoin(Hero hero, KingdomDiplomacy diplomacy) => hero.MapFaction == diplomacy.Kingdom &&
            hero.MapFaction.Leader != hero && hero.IsClanLeader();

        public override bool CanHeroLeave(Hero hero, KingdomDiplomacy diplomacy)
        {
            if (JoinTime.TryGetValue(hero, out var joinTime))
            {
                return joinTime.ElapsedYearsUntilNow >= 1f;
            }

            return true;
        }

        public override void RemoveMember(Hero hero, bool forced = false)
        {
            if (hero != null && Members.Contains(hero))
            {
                if (!forced && !CanHeroLeave(hero, KingdomDiplomacy)) return;

                Members.Remove(hero);
                if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has left the {GROUP} group.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetRelationDecisionSound());
                }

                if (!forced)
                {
                    if (Leader == hero)
                    {
                        foreach (var member in Members)
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, member, -10, false);

                        SetNewLeader(KingdomDiplomacy);
                    }
                    else
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, Leader, -20, false);
                        foreach (var member in Members)
                            if (MBRandom.RandomFloat < 0.3f)
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, member, -6, false);
                    }
                }
            }
        }

        public override void SetNewLeader(KingdomDiplomacy diplomacy)
        {
            throw new NotImplementedException();
        }

        public void TriggerRevolt()
        {
            RebellionActions.CreateRebelKingdom(this, Leader.Clan, Members.ConvertAll(x => x.Clan), KingdomDiplomacy.Kingdom);
        }

        public override void Tick()
        {
            TickInternal();

            if (Leader == Hero.MainHero || Leader == null || FactionLeader == null)
            {
                return;
            }

            float proportion = PowerProportion;
            if (proportion >= 0.5f) Radicalism += 0.01f;
            else Radicalism -= 0.01f;

            if (Radicalism > 1f)
            {
                Radicalism = 1f;
                if (MBRandom.RandomFloat < 0.1f) TriggerRevolt();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is RadicalGroup)
            {
                return (obj as RadicalGroup).StringId == StringId && KingdomDiplomacy == (obj as RadicalGroup).KingdomDiplomacy;
            }
            return base.Equals(obj);
        }
    }
}
