using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public class DarusosianExecution : ContextualRite
    {
        private Hero input;

        public override void Execute(Hero executor)
        {
            TextObject reason;
            if (!MeetsCondition(executor, out reason))
            {
                return;
            }

            var options = new List<InquiryElement>();
            foreach (var hero in GetAdequateSacrifices(executor))
            {
                options.Add(new InquiryElement(hero, hero.Name.ToString(),
                    new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject))));
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GetName().ToString(),
                    GetDescription().ToString(),
                    options, false, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                    delegate(List<InquiryElement> x)
                    {
                        input = (Hero?) x[0].Identifier;
                        SetDialogue();
                    }, null, string.Empty));
        }

        public override void Complete(Hero actionTaker)
        {
            if (input == null)
            {
                return;
            }

            var inputReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(input);
            var actionTakerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(actionTaker);
            if (inputReligion != null && inputReligion == actionTakerReligion)
            {
                return;
            }

            var piety = GetPietyReward();
            KillCharacterAction.ApplyByExecution(input, actionTaker, false);
            MBInformationManager.AddQuickInformation(new TextObject("{=DW2LjgpT}{SACRIFICE} was executed as a traitor by {HERO}.")
                    .SetTextVariable("HERO", actionTaker.Name)
                    .SetTextVariable("SACRIFICE", input.Name),
                0, actionTaker.CharacterObject, "event:/ui/notification/relation");

            BannerKingsConfig.Instance.ReligionsManager.AddPiety(actionTaker, piety, actionTaker.Clan == Clan.PlayerClan);
            actionTaker.AddSkillXp(BKSkills.Instance.Theology, piety * 1.2f);

            /*foreach (Clan clan in Clan.All)
            {
                Religion clanReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                if (clan != actionTaker.Clan && (clanReligion == null || !clanReligion.Doctrines.Contains("sacrifice")))
                {
                    bool affectRelatives;
                    int relationChangeForExecutingHero = Campaign.Current.Models.ExecutionRelationModel.GetRelationChangeForExecutingHero(input, actionTaker, out affectRelatives);
                    if (relationChangeForExecutingHero != 0)
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(actionTaker, clan.Leader, relationChangeForExecutingHero, true);
                }
            }*/

            if (actionTaker.GetPerkValue(BKPerks.Instance.TheologyRitesOfPassage))
            {
                actionTaker.Clan.AddRenown(5f);
            }

            input = null;
        }

        private List<Hero> GetAdequateSacrifices(Hero hero)
        {
            var list = new List<Hero>();    
            if (hero.IsPartyLeader && hero.PartyBelongedTo.PrisonRoster.TotalHeroes > 0)
            {
                foreach (TroopRosterElement element in hero.PartyBelongedTo.PrisonRoster.GetTroopRoster())
                {
                    var elementHero = element.Character.HeroObject;
                    if (elementHero != null && elementHero.Clan != null)
                    {
                        var kingdom = elementHero.Clan.Kingdom;
                        if (kingdom != null && (kingdom.StringId == "empire_w" || kingdom.StringId == "empire"))
                        {
                            list.Add(elementHero);
                        }
                    }
                }
            }

            return list;
        }

        public override bool MeetsCondition(Hero hero, out TextObject reason)
        {
            reason = new TextObject("{=oo3xtFfT}This rite is available to be performed.");
            var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
            bool baseResult = hero.IsAlive && !hero.IsChild && !hero.IsPrisoner && hero.PartyBelongedTo != null &&
                             data != null && data.HasTimePassedForRite(GetRiteType(), GetTimeInterval(hero));
            var hasTarget = GetAdequateSacrifices(hero).Count > 0;
            
            if (!baseResult)
            {
                reason = new TextObject("{=NZyz0ChH}Not enough time ({YEARS} years) have passed since the last rite of this type was performed.")
                    .SetTextVariable("YEARS", GetTimeInterval(hero).ToString("0.0"));
            }

            if (!hasTarget)
            {
                var kingdom = Kingdom.All.FirstOrDefault(x => x.StringId == "empire_s");
                TextObject name = kingdom != null ? kingdom.Name : new TextObject("{=frBQ9mbP}Southern Empire");
                reason = new TextObject("{=u3xzCV63}You have no lord prisoners from Imperial contestors of the {KINGDOM}.")
                    .SetTextVariable("KINGDOM", name);
            }

            bool southernEmpire = hero.Clan.Kingdom.StringId == "empire_s";
            if (!southernEmpire)
            {
                var kingdom = Kingdom.All.FirstOrDefault(x => x.StringId == "empire_s");
                reason = new TextObject("{=H6CdxwrS}You are not part of the {KINGDOM}.")
                    .SetTextVariable("KINGDOM", kingdom != null ? kingdom.Name : new TextObject("{=frBQ9mbP}Southern Empire"));
            }

            return baseResult && hasTarget && southernEmpire;
        }

        public override TextObject GetDescription()
        {
            return new TextObject("{=mbcRbm0q}Execute a traitor to the Empire. Dissedents of the Western and Northern rebels may be executed as punishment for the betrayal of the Empire, the Imperial family and the Triad.");
        }

        public override TextObject GetName()
        {
            return new TextObject("{=pB7vkmae}Execute Traitor");
        }

        public override RiteType GetRiteType()
        {
            return RiteType.DONATION;
        }

        public override float GetTimeInterval(Hero hero)
        {
            var result = 2f;
            if (hero.GetPerkValue(BKPerks.Instance.TheologyRitesOfPassage))
            {
                result -= 0.25f;
            }

            return result;
        }

        public override float GetPietyReward()
        {
            var renown = 100f;
            if (input.Clan != null)
            {
                renown = input.Clan.Renown / 10f;
            }

            return renown;
        }

        public override void SetDialogue()
        {
            //MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM", new TextObject("{=eTqH0eDT}The fate of {HERO} was sealed once they dared draw sword on us. Let us rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
            //   .SetTextVariable("HERO", input.Name), false);
            MBTextManager.SetTextVariable("CLERGYMAN_RITE_CONFIRM",
                new TextObject("{=uSMsxDP1}The fate of {HERO} was sealed once they dared draw sword on us. Affirm the rite and we shall rejoice upon the glory we bathe ourselves in as the enemy bleeds!")
                    .SetTextVariable("HERO", input.Name));
        }

        public override TextObject GetRequirementsText(Hero hero)
        {
            return new TextObject("{=mkC8opdf}May be performed every {YEARS} years\nBe part of Souther Empire\nRequires a prisoner lord from Western or Northern empire factions")
                .SetTextVariable("YEARS", GetTimeInterval(hero));
        }
    }
}