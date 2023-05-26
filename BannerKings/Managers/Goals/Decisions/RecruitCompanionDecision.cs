using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class RecruitCompanionDecision : Goal
    {
        private List<CompanionType> companionTypes;
        private CompanionType selectedCompanionType;
        private CultureObject selectedCulture;

        public RecruitCompanionDecision() : base("goal_recruit_companion_decision", GoalCategory.Personal, GoalUpdateType.Manual)
        {
            var name = new TextObject("{=HcGkCnSH}Seek Guests");
            var description = new TextObject("{=Ug94AACX}Invite guests to your court. They will live within your court for some time, where you can reliably find them. Seeking out guests costs influence relative to your House's position. Guests of different cultures and expertises can be sought out, for different costs.");

            Initialize(name, description);
            companionTypes = new List<CompanionType>();
        }

        private void UpdateTypes()
        {
            var cap = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(GetFulfiller().Clan).ResultNumber;
            companionTypes = new List<CompanionType>
            {
                new CompanionType(new TextObject("Commander"),
                new TextObject("{=61scYHtR}A guest adept as a commander. An expensive service given the constant need for quality leadership. A commander will likely have at least 60 proficiency in leadership."),
                MathF.Max(cap * 0.14f, 60f),
                new List<TraitObject>()
                {
                    DefaultTraits.Commander,
                    DefaultTraits.SergeantCommandSkills
                }),

                new CompanionType(new TextObject("{=Q8Zv4D7p}Soldier"),
                new TextObject("{=h6QeRUFk}A guest adept as a soldier, regardless of their fighting style. A solder will likely have at least 60 proficiency in several combat skills."),
                MathF.Max(cap * 0.06f, 20f),
                new List<TraitObject>()
                {
                    DefaultTraits.ArcherFIghtingSkills,
                    DefaultTraits.CavalryFightingSkills,
                    DefaultTraits.HopliteFightingSkills,
                    DefaultTraits.HorseArcherFightingSkills,
                    DefaultTraits.HuscarlFightingSkills,
                    DefaultTraits.KnightFightingSkills,
                    DefaultTraits.PeltastFightingSkills
                }),

                new CompanionType(new TextObject("{=XrR7XZWp}Healer"),
                new TextObject("{=4WhG1re9}A guest adept in the healing arts. Due to their high demand, their services are expensive. A healder will likely have at least 60 proficiency in medical skill."),
                MathF.Max(cap * 0.09f, 35f),
                new List<TraitObject>()
                {
                    DefaultTraits.Surgery
                }),

                new CompanionType(new TextObject("{=t0UENgOQ}Engineer"),
                new TextObject("{=jUWDvhFm}A guest adept in the engineering fields. An engineer will likely have at least 60 proficiency in siegecraft."),
                MathF.Max(cap * 0.1f, 40f),
                new List<TraitObject>()
                {
                    DefaultTraits.Siegecraft
                }),

                new CompanionType(new TextObject("{=vGEs0w41}Rogue"),
                new TextObject("{=mu1hg1y0}A guest adept in roguery. A rogue will likely have at least 60 proficiency in roguery."),
                MathF.Max(cap * 0.07f, 25f),
                new List<TraitObject>()
                {
                    DefaultTraits.RogueSkills
                }),

                new CompanionType(new TextObject("{=cQwb9BX0}Scout"),
                new TextObject("{=dBYCCT4W}A guest adept of scouting regardless of the terrain. A necessity for any warband of significant size. A scout will likely have at least 60 proficiency in scouting."),
                MathF.Max(cap * 0.07f, 25f),
                new List<TraitObject>()
                {
                    DefaultTraits.DesertScoutSkills,
                    DefaultTraits.WoodsScoutSkills,
                    DefaultTraits.HillScoutSkills,
                    DefaultTraits.SteppeScoutSkills
                }),

                new CompanionType(new TextObject("{=R5VvmxJ7}Trader"),
                new TextObject("{=5dcdprut}A guest adept in the art of trading. Exceptional caravaneers when paired with scouting abilities. A trader will likely have at least 60 proficiency in trading."),
                MathF.Max(cap * 0.08f, 30f),
                new List<TraitObject>()
                {
                    DefaultTraits.Manager
                }),

                new CompanionType(new TextObject("Steward"),
                new TextObject("{=gfmewont}A guest adept in stewardship. Stewards make for good governors to handle your demesne, as well as capable quartermasters. A rare gift that comes for a premium price. A steward will likely have at least 60 proficiency in stewardship."),
                MathF.Max(cap * 0.11f, 45f),
                new List<TraitObject>()
                {
                    DefaultTraits.Manager
                })
            };
        }

        internal override bool IsAvailable()
        {
            var clan = GetFulfiller().Clan;
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            return council.Location != null;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var clan = GetFulfiller().Clan;
            if (clan.Companions.Count >= clan.CompanionLimit)
            {
                failedReasons.Add(new TextObject("{=L9NQ0yOu}Your clan has reached it's companion limit."));
            }

            return failedReasons.Count == 0;
        }

        internal override void ShowInquiry()
        {
            UpdateTypes();
            var influence = GetFulfiller().Clan?.Influence ?? 0f;

            var cultureOptions = new List<InquiryElement>();
            foreach (var culture in Campaign.Current.ObjectManager.GetObjectTypeList<CultureObject>())
            {
                if (culture.NotableAndWandererTemplates != null && culture.NotableAndWandererTemplates.Count > 0 ||
                    culture.CanHaveSettlement && !culture.IsBandit && culture.IsMainCulture)
                {
                    cultureOptions.Add(new InquiryElement(culture,
                        culture.Name.ToString(),
                        null,
                        true,
                        culture.EncyclopediaText.ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=FVA72PZG}Guests (1/2)").ToString(),
                new TextObject("{=zfQkfCPp}Determine the cultural origin of the guests you would entertain. Beware foreigners will often be more expensive.").ToString(),
                cultureOptions,
                true,
                1,
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> selectedOptions)
                {
                    selectedCulture = (CultureObject)selectedOptions.First().Identifier;

                    var companionOptions = new List<InquiryElement>();
                    foreach (var companionType in companionTypes)
                    {
                        var hint = companionType.Description;
                        var template = GetAdequateCharacter(companionType);
                        if (template.Count == 0)
                        {
                            hint = new TextObject("{=!}No available guests of this type and culture.");
                        }

                        float influence = companionType.InfluenceCost;
                        if (selectedCulture != GetFulfiller().Culture)
                        {
                            influence *= 1.3f;
                        }

                        companionOptions.Add(new InquiryElement(companionType, 
                            new TextObject("{=Hyfgj4Mw}{TYPE} - {INFLUENCE}{INFLUENCE_ICON}")
                            .SetTextVariable("TYPE", companionType.Name)
                            .SetTextVariable("INFLUENCE", MBRandom.RoundRandomized(influence))
                            .SetTextVariable("INFLUENCE_ICON", "<img src=\"General\\Icons\\Influence@2x\" extend=\"7\">")
                            .ToString(), 
                            null,
                            influence >= companionType.InfluenceCost && template.Count > 0, 
                            hint.ToString()));
                    }

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                       new TextObject("{=FVA72PZG}Guests (2/2)").ToString(),
                       new TextObject("{=TmBXiTjD}Determine what kind of guest your court will receive.").ToString(),
                       companionOptions,
                       true,
                       1,
                       GameTexts.FindText("str_done").ToString(),
                       GameTexts.FindText("str_cancel").ToString(),
                       delegate (List<InquiryElement> selectedOptions)
                       {
                           selectedCompanionType = (CompanionType)selectedOptions.First().Identifier;
                           ApplyGoal();
                       },
                       null,
                       string.Empty));
                },
                null,
                string.Empty));
        }

        private List<CharacterObject> GetAdequateCharacter(CompanionType type)
        {
            var possibleTemplates = new List<CharacterObject>();
            foreach (var template in selectedCulture.NotableAndWandererTemplates.Where(t => t.Occupation == Occupation.Wanderer))
            {
                foreach (var trait in type.Trait)
                {
                    if (template.GetTraitLevel(trait) > 4)
                    {
                        possibleTemplates.Add(template);
                        break;
                    }
                }
            }

            return possibleTemplates;
        }

        internal override void ApplyGoal()
        {
            var hero = GetFulfiller();
            var characterTemplate = GetAdequateCharacter(selectedCompanionType).GetRandomElement();

            var bornSettlement = Settlement.All.GetRandomElementWithPredicate(s => s.Culture == selectedCulture);
            var companion = HeroCreator.CreateSpecialHero(characterTemplate, 
                bornSettlement, 
                null,
                null, 
                Campaign.Current.Models.AgeModel.HeroComesOfAge + MBRandom.RandomInt(32));

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(hero.Clan);
            TeleportHeroAction.ApplyImmediateTeleportToSettlement(companion, council.Location.Settlement);
            council.AddGuest(companion);

            float influence = selectedCompanionType.InfluenceCost;
            if (selectedCulture != GetFulfiller().Culture)
            {
                influence *= 1.3f;
            }
            GainKingdomInfluenceAction.ApplyForDefault(hero.Clan.Leader, -influence);
            selectedCompanionType = null;
            selectedCulture = null;
        }

        public override void DoAiDecision()
        {

        }

        private class CompanionType
        {
            public CompanionType(TextObject name, TextObject description, float influenceCost, List<TraitObject> trait)
            {
                Name = name;
                Description = description;
                InfluenceCost = influenceCost;
                Trait = trait;
            }

            public TextObject Name { get; set; } 
            public TextObject Description { get; set; } 
            public float InfluenceCost { get; set; }
            public List<TraitObject> Trait { get; set; }
        }
    }
}