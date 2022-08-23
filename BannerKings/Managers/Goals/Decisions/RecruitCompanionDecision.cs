using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class RecruitCompanionDecision : Goal
    {
        private readonly List<CompanionType> companionTypes;
        private CompanionType selectedCompanionType;

        public RecruitCompanionDecision() : base("goal_recruit_companion_decision", GoalUpdateType.Manual)
        {

            var name = new TextObject("{=!}Recruit Companion");
            var description = new TextObject("{!=}Select a type of companion to recruit");

            Initialize(name, description);

            companionTypes = new List<CompanionType>
            {
                new("commander", "Commander", "A companion that meets the criteria for a Commander.", 5000, 100,
                    new List<TraitObject> {DefaultTraits.Commander},
                    new List<PerkObject>(),
                    new List<SkillObject>()),
                new("thief", "Thief", "A companion that meets the criteria for a Thief.", 5000, 100,
                    new List<TraitObject> {DefaultTraits.Thief},
                    new List<PerkObject>(),
                    new List<SkillObject>()),
                new("surgeon", "Surgeon", "A companion that meets the criteria for a Surgeon.", 5000, 100,
                    new List<TraitObject> {DefaultTraits.Surgery},
                    new List<PerkObject>(),
                    new List<SkillObject>()),
                new("caravaneer", "Caravaneer", "A companion that meets the criteria for a Caravaneer.", 5000, 100,
                    new List<TraitObject>{DefaultTraits.Manager},
                    new List<PerkObject>(),
                    new List<SkillObject>()),
                new("warrior", "Warrior", "A companion that meets the criteria for a Warrior.", 5000, 100,
                    new List<TraitObject> {DefaultTraits.Fighter},
                    new List<PerkObject>(),
                    new List<SkillObject>())
            };
        }

        internal override bool IsAvailable()
        {
            return true;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var gold = GetFulfiller().Gold;
            var influence = GetFulfiller().Clan?.Influence;

            if (companionTypes.All(ct => gold < ct.GoldCost && influence < ct.InfluenceCost))
            {
                failedReasons.AddRange(companionTypes.Select(companionType => 
                    new TextObject("{=!}A {COMPANION_NAME} can't be afforded. {GOLD}{GOLD_ICON} and {INFLUENCE}{INFLUENCE_ICON} is needed.")
                        .SetTextVariable("COMPANION_NAME", companionType.Name)
                        .SetTextVariable("GOLD", companionType.GoldCost)
                        .SetTextVariable("INFLUENCE", companionType.InfluenceCost)));
            }

            return failedReasons.IsEmpty();
        }

        internal override Hero GetFulfiller()
        {
            return Hero.MainHero;
        }

        internal override void ShowInquiry()
        {
            IsFulfilled(out var failedReasons);

            var gold = GetFulfiller().Gold;
            var influence = GetFulfiller().Clan?.Influence;

            var options = new List<InquiryElement>();
            for (var index = 0; index < companionTypes.Count; index++)
            {
                var companionType = companionTypes[index];
                var enabled = gold >= companionType.GoldCost && influence >= companionType.InfluenceCost;

                options.Add(new InquiryElement(companionType,
                    companionType.Name,
                    null,
                    enabled,
                    enabled ? companionType.Description : failedReasons[index].ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Companions").ToString(),
                new TextObject("{=!}Choose a companion to recruit.").ToString(),
                options, 
                true, 
                1, 
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> x)
                {
                    selectedCompanionType = (CompanionType)x[0].Identifier;
                    ApplyGoal();
                }, 
                null, 
                string.Empty));
        }

        internal override void ApplyGoal()
        {
            var hero = GetFulfiller();

            var possibleTemplates = new List<(CharacterObject template, float weight)>();
            foreach (var template in hero.Culture.NotableAndWandererTemplates.Where(t => t.Occupation == Occupation.Wanderer))
            {
                var weight = 0f;

                foreach (var trait in selectedCompanionType.Traits.Where(trait => template.GetTraitLevel(trait) >= 2))
                {
                    weight++;
                }

                foreach (var perk in selectedCompanionType.Perks.Where(perk => template.GetPerkValue(perk)))
                {
                    weight++;
                }

                foreach (var skill in selectedCompanionType.Skills.Where(skill => template.GetSkillValue(skill) >= 50))
                {
                    weight++;
                }

                if (weight > 1f)
                {
                    possibleTemplates.Add((template, weight));
                }
            }

            if (possibleTemplates.Count == 0)
            {
                InformationManager.ShowInquiry
                (
                    new InquiryData
                    (
                        "Companion Recruitment",
                        new TextObject("No competent companion was found.").ToString(),
                        true, 
                        false, 
                        GameTexts.FindText("str_accept").ToString(), 
                        null, 
                        null, 
                        null
                    ),
                    true
                );
            }

            var characterTemplate = MBRandom.ChooseWeighted(possibleTemplates);

            var possibleEquipmentRosters = MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>().Where(e => e.EquipmentCulture == hero.Culture).ToList();
            var equipmentRoster = possibleEquipmentRosters.Where(e => e.EquipmentCulture == hero.Culture).ToList().GetRandomElementWithPredicate(x => x.StringId.Contains("bannerkings_companion")) 
                                  ?? possibleEquipmentRosters.Where(e => e.EquipmentCulture == hero.Culture).ToList().GetRandomElementWithPredicate(x => x.HasEquipmentFlags(EquipmentFlags.IsMediumTemplate));

            var companion = HeroCreator.CreateSpecialHero(characterTemplate, null, null, null, Campaign.Current.Models.AgeModel.HeroComesOfAge + MBRandom.RandomInt(30));
            EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipmentRoster.AllEquipments.GetRandomElement());
            companion.CompanionOf = hero.Clan;

            var companionFoundMessage = new TextObject("{COMPANION.LINK} was discovered and joined you as companion.");
            companionFoundMessage.SetCharacterProperties("COMPANION", companion.CharacterObject);
            InformationManager.ShowInquiry
            (
                new InquiryData
                (
                    "Companion Recruitment",
                    companionFoundMessage.ToString(),
                    true, 
                    false, 
                    GameTexts.FindText("str_accept").ToString(), 
                    null, 
                    null, 
                    null
                ),
                true
            );
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }

        private class CompanionType
        {
            public CompanionType(string stringId, string name, string description, int goldCost, int influenceCost, List<TraitObject> traits, List<PerkObject> perks, List<SkillObject> skills)
            {
                StringId = stringId;
                Name = name;
                Description = description;
                GoldCost = goldCost;
                InfluenceCost = influenceCost;
                Traits = traits;
                Perks = perks;
                Skills = skills;
            }

            public string StringId { get; set; }

            public string Name { get; set; } 

            public string Description { get; set; } 

            public int GoldCost { get; set; }

            public int InfluenceCost { get; set; }

            public List<TraitObject> Traits { get; set; }

            public List<PerkObject> Perks { get; set; }

            public List<SkillObject> Skills { get; set; }
        }
    }
}