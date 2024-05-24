using BannerKings.Managers.Titles;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class CasusBelli : BannerKingsObject
    {
        private Func<War, bool> isFulfilled;
        private Func<War, bool> isInvalid;
        private Func<IFaction, IFaction, CasusBelli, bool> isAdequate;
        private Func<Kingdom, bool> showAsOption;
        private TextObject warDeclaredText;
        private Action<War> onFinish, onStart;

        public CasusBelli(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, 
            TextObject description, 
            TextObject objectiveText, 
            float conquest, 
            float raid, 
            float capture, 
            float declareWarScore,
            Func<War, bool> isFulfilled, 
            Func<War, bool> isInvalid, 
            Func<IFaction, IFaction, CasusBelli, bool> isAdequate,
            Func<Kingdom, bool> showAsOption, 
            Dictionary<TraitObject, float> traitWeights, 
            TextObject warDeclaredText, 
            bool requiresFief = false,
            bool requiresClaimant = false,
            Action<War> onStart = null,
            Action<War> onFinish = null)
        {
            Initialize(name, description);
            ObjectiveText = objectiveText;
            this.isFulfilled = isFulfilled;
            this.isInvalid = isInvalid;
            this.isAdequate = isAdequate;
            this.showAsOption = showAsOption;
            TraitWeights = traitWeights;
            ConquestWeight = conquest;
            RaidWeight = raid;
            CaptureWeight = capture;
            DeclareWarScore = declareWarScore;
            this.warDeclaredText = warDeclaredText;
            RequiresFief = requiresFief;
            RequiresClaimant = requiresClaimant;
            this.onStart = onStart;
            this.onFinish = onFinish;
        }

        public CasusBelli GetCopy()
        {
            var copy = new CasusBelli(StringId);
            copy.Initialize(Name, 
                Description, 
                ObjectiveText, 
                ConquestWeight, 
                RaidWeight, 
                CaptureWeight,
                DeclareWarScore, 
                IsFulfilled, 
                IsInvalid, 
                IsAdequate, 
                ShowAsOption, 
                TraitWeights, 
                warDeclaredText,
                RequiresFief,
                RequiresClaimant,
                onStart,
                onFinish);
            return copy;
        }

        public void PostInitialize()
        {
            var copy = DefaultCasusBelli.Instance.GetById(this);
            Initialize(copy.name, copy.description, copy.ObjectiveText, copy.ConquestWeight,
                copy.RaidWeight, copy.CaptureWeight, copy.DeclareWarScore, copy.isFulfilled,
                copy.isInvalid, copy.isAdequate, copy.showAsOption, copy.TraitWeights,
                copy.WarDeclaredText);
        }

        public void OnStart(War war)
        {
            if (onStart != null) onStart(war);
        }

        public void OnFinish(War war)
        {
            if (onFinish != null) onFinish(war);
        }

        public void SetInstanceData(Kingdom attacker, Kingdom defender, Settlement fief = null)
        {
            Attacker = attacker;
            Defender = defender;
            Fief = fief;
            if (Fief != null) Title = BannerKingsConfig.Instance.TitleManager.GetTitle(Fief);
        }

        public void SetInstanceData(Kingdom attacker, Kingdom defender, FeudalTitle title, Hero claimant)
        {
            Attacker = attacker;
            Defender = defender;
            Fief = title.Fief;
            Title = title;
            Claimant = claimant;
            if (Fief != null) Title = BannerKingsConfig.Instance.TitleManager.GetTitle(Fief);
        }

        [SaveableProperty(10)] public Kingdom Attacker { get; private set; }
        [SaveableProperty(11)] public Kingdom Defender { get; private set; }
        [SaveableProperty(12)] public Settlement Fief { get; private set; }   
        [SaveableProperty(13)] public Hero Claimant { get; private set; }
        [SaveableProperty(14)] public FeudalTitle Title { get; private set; }
        public TextObject ObjectiveText { get; private set; }

        public TextObject GetDescriptionWithModifers() => new TextObject("{=pwxzjqjJ}{DESCRIPTION}\n\nConquest Modifier: {CONQUEST}%\nHostage Modifier: {HOSTAGE}%\nRaiding Modifier: {RAID}%")
            .SetTextVariable("DESCRIPTION", Description)
            .SetTextVariable("CONQUEST", ConquestWeight * 100)
            .SetTextVariable("HOSTAGE", CaptureWeight * 100)
            .SetTextVariable("RAID", RaidWeight * 100);

        public TextObject WarDeclaredText => warDeclaredText.SetTextVariable("FIEF", Fief != null ? Fief.Name : new TextObject())
            .SetTextVariable("ATTACKER", Attacker != null ? Attacker.Name : new TextObject())
            .SetTextVariable("DEFENDER", Defender != null ? Defender.Name : new TextObject());

        public TextObject QueryNameText => Fief != null ? new TextObject("{=TZXcmqKk}{FIEF} - {NAME}")
            .SetTextVariable("FIEF", Fief.Name)
            .SetTextVariable("NAME", Name)
            : Name;

        public float DeclareWarScore { get; private set; }
        public bool RequiresFief { get; private set; }
        public bool RequiresClaimant { get; private set; }
        private Dictionary<TraitObject, float> TraitWeights { get; set; }
        public float ConquestWeight { get; private set; }
        public float CaptureWeight { get; private set; }
        public float RaidWeight { get; private set; }
        public float GetTraitWeight(TraitObject trait)
        {
            float result = 0f;
            if (TraitWeights.ContainsKey(trait))
            {
                return TraitWeights[trait]; 
            }

            return result;
        }

        public bool IsFulfilled(War war) => isFulfilled(war);
        public bool IsInvalid(War war) => isInvalid(war);
        public bool IsAdequate(IFaction faction1, IFaction faction2, CasusBelli casusBelli) => isAdequate(faction1, faction2, casusBelli);
        public bool ShowAsOption(Kingdom kingdom) => showAsOption(kingdom);

        public override bool Equals(object obj)
        {
            if (obj is CasusBelli)
            {
                return StringId == (obj as CasusBelli).StringId;
            }

            return base.Equals(obj);
        }
    }
}
