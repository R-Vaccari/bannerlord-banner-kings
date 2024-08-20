using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.CampaignContent.Traits
{
    public class DefaultTraitEffects : DefaultTypeInitializer<DefaultTraitEffects, TraitEffect>
    {
        public TraitEffect Seductiveness { get; } = new TraitEffect("Seductiveness");
        public TraitEffect JustRuler { get; } = new TraitEffect("JustRelation");
        public TraitEffect JustLord { get; } = new TraitEffect("JustLord");

        public TraitEffect ValorMorale { get; } = new TraitEffect("ValorMorale");
        public TraitEffect ValorLegitimacy { get; } = new TraitEffect("ValorLegitimacy");
        public TraitEffect ValorCommander { get; } = new TraitEffect("ValorCommander");
        public TraitEffect ValorGovernor { get; } = new TraitEffect("ValorGovernor");

        public TraitEffect HonorInfluence { get; } = new TraitEffect("HonorInfluence");
        public TraitEffect HonorDiplomacy { get; } = new TraitEffect("HonorDiplomacy");
        public TraitEffect HonorRelation { get; } = new TraitEffect("HonorRelation");

        public TraitEffect GenerosityRelation { get; } = new TraitEffect("GenerosityRelation");
        public TraitEffect GenerosityIncome { get; } = new TraitEffect("GenerosityIncome");
        public TraitEffect GenerosityPartyCost { get; } = new TraitEffect("GenerosityPartyCost");

        public TraitEffect CalculatingProposals { get; } = new TraitEffect("CalculatingCohesion");
        public TraitEffect CalculatingCohesion { get; } = new TraitEffect("CalculatingCohesion");
        public TraitEffect CalculatingCouncil { get; } = new TraitEffect("CalculatingCouncil");
        public TraitEffect CalculatingPartySize { get; } = new TraitEffect("CalculatingPartySize");
        public TraitEffect CalculatingSecurity { get; } = new TraitEffect("CalculatingSecurity");
        public TraitEffect CalculatingSupplies { get; } = new TraitEffect("CalculatingSupplies");

        public TraitEffect MercyRaid { get; } = new TraitEffect("MercyRaid");
        public TraitEffect MercyLoyalty { get; } = new TraitEffect("MercyLoyalty");

        public override IEnumerable<TraitEffect> All
        {
            get
            {
                yield return Seductiveness;
                yield return JustRuler;
                yield return JustLord;
                yield return ValorLegitimacy;
                yield return ValorMorale;
                yield return ValorCommander;
                yield return ValorGovernor;
                yield return HonorDiplomacy; 
                yield return HonorRelation;
                yield return HonorInfluence;
                yield return GenerosityRelation;
                yield return GenerosityIncome;
                yield return GenerosityPartyCost;
                yield return MercyRaid;
                yield return MercyLoyalty;
                yield return CalculatingProposals;
                yield return CalculatingCouncil;
                yield return CalculatingPartySize;
                yield return CalculatingCohesion;
                yield return CalculatingSecurity;
                yield return CalculatingSupplies;
            }
        }

        public List<TraitEffect> GetTraitEffects(TraitObject trait) => All.ToList().FindAll(x => x.Trait == trait);

        public override void Initialize()
        {
            Seductiveness.Initialize(new TextObject("{=!}Sexual attractiveness: {EFFECT}%"),
                BKTraits.Instance.Seductive,
                SkillEffect.PerkRole.Personal,
                0.15f,
                true);

            JustRuler.Initialize(new TextObject("{=!}Relation target with nobles: {EFFECT}"),
                BKTraits.Instance.Just,
                SkillEffect.PerkRole.Ruler,
                8f);

            JustLord.Initialize(new TextObject("{=!}Relation target with notables: {EFFECT}"),
                BKTraits.Instance.Just,
                SkillEffect.PerkRole.ClanLeader,
                10f);

            ValorMorale.Initialize(new TextObject("{=!}Party morale: {EFFECT}"),
                DefaultTraits.Valor,
                SkillEffect.PerkRole.PartyLeader,
                5f);

            ValorLegitimacy.Initialize(new TextObject("{=!}Legitimacy: {EFFECT}"),
                DefaultTraits.Valor,
                SkillEffect.PerkRole.Ruler,
                0.03f);

            ValorCommander.Initialize(new TextObject("{=!}Likelihood to besiege fiefs: {EFFECT}%"),
                DefaultTraits.Valor,
                SkillEffect.PerkRole.ArmyCommander,
                0.1f,
                true);

            ValorGovernor.Initialize(new TextObject("{=!}Fief militia: {EFFECT}"),
                DefaultTraits.Valor,
                SkillEffect.PerkRole.Governor,
                0.3f);

            HonorDiplomacy.Initialize(new TextObject("{=!}Desirability of amicable diplomatic pacts: {EFFECT}%"),
                DefaultTraits.Honor,
                SkillEffect.PerkRole.Ruler,
                0.15f,
                true);

            HonorInfluence.Initialize(new TextObject("{=!}Clan influence limit: {EFFECT}%"),
                DefaultTraits.Honor,
                SkillEffect.PerkRole.ClanLeader,
                0.1f,
                true);

            HonorRelation.Initialize(new TextObject("{=!}Overall opinion towards you: {EFFECT}"),
                DefaultTraits.Honor,
                SkillEffect.PerkRole.Personal,
                0.1f);

            GenerosityRelation.Initialize(new TextObject("{=!}Relation increase with NPCs: {EFFECT}%"),
                DefaultTraits.Generosity,
                SkillEffect.PerkRole.Personal,
                0.08f,
                true); 

            GenerosityIncome.Initialize(new TextObject("{=!}Fief revenues: {EFFECT}%"),
                DefaultTraits.Generosity,
                SkillEffect.PerkRole.Governor,
                -0.1f,
                true);

            GenerosityPartyCost.Initialize(new TextObject("{=!}Party wage: {EFFECT}%"),
                DefaultTraits.Generosity,
                SkillEffect.PerkRole.PartyLeader,
                -0.12f,
                true);

            MercyRaid.Initialize(new TextObject("{=!}Likelihood to raid fiefs: {EFFECT}%"),
                DefaultTraits.Mercy,
                SkillEffect.PerkRole.ArmyCommander,
                -0.15f,
                true);

            MercyLoyalty.Initialize(new TextObject("{=!}Fief loyalty: {EFFECT}"),
                DefaultTraits.Mercy,
                SkillEffect.PerkRole.Governor,
                0.2f);

            CalculatingProposals.Initialize(new TextObject("{=!}Influence to propose votes: {EFFECT}%"),
                DefaultTraits.Calculating,
                SkillEffect.PerkRole.ClanLeader,
                -0.1f,
                true);

            CalculatingSupplies.Initialize(new TextObject("{=!}Party Supplies necessity: {EFFECT}%"),
                DefaultTraits.Calculating,
                SkillEffect.PerkRole.Quartermaster,
                -0.12f);

            CalculatingCouncil.Initialize(new TextObject("{=!}Competency as council member: {EFFECT}%"),
                DefaultTraits.Calculating,
                SkillEffect.PerkRole.Personal,
                0.12f);

            CalculatingSecurity.Initialize(new TextObject("{=!}Fief security: {EFFECT}"),
                DefaultTraits.Calculating,
                SkillEffect.PerkRole.Governor,
                0.2f);

            CalculatingCohesion.Initialize(new TextObject("{=!}Army cohesion: {EFFECT}"),
                DefaultTraits.Calculating,
                SkillEffect.PerkRole.ArmyCommander,
                0.1f);
        }
    }
}
