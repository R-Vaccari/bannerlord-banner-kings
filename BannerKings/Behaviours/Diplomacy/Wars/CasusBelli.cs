using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class CasusBelli : BannerKingsObject
    {
        private Func<War, bool> isFulfilled;
        private Func<War, bool> isInvalid;
        private Func<IFaction, IFaction, CasusBelli, bool> isAdequate;
        private Func<Kingdom, bool> showAsOption;
        public CasusBelli(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, float conquest, float raid, float capture, float declareWarScore,
            Func<War, bool> isFulfilled, Func<War, bool> isInvalid, Func<IFaction, IFaction, CasusBelli, bool> isAdequate,
            Func<Kingdom, bool> showAsOption, Dictionary<TraitObject, float> traitWeights)
        {
            Initialize(name, description);
            this.isFulfilled = isFulfilled;
            this.isInvalid = isInvalid;
            this.isAdequate = isAdequate;
            this.showAsOption = showAsOption;
            TraitWeights = traitWeights;
            ConquestWeight = conquest;
            RaidWeight = raid;
            CaptureWeight = capture;
            DeclareWarScore = declareWarScore;
        }

        public CasusBelli GetCopy()
        {
            var copy = new CasusBelli(StringId);
            copy.Initialize(Name, Description, ConquestWeight, RaidWeight, DeclareWarScore,
                CaptureWeight, IsFulfilled, IsInvalid, IsAdequate, ShowAsOption, TraitWeights);
            return copy;
        }

        public float DeclareWarScore { get; private set; }
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

        public Settlement Fief { get; set; }

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
