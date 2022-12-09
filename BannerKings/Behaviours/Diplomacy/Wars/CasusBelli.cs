using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Diplomacy.Wars
{
    public class CasusBelli : BannerKingsObject
    {
        private Func<War, bool> isFulfilled;
        private Func<War, bool> isAdequate;

        public CasusBelli(string id) : base(id)
        {
        }

        public void Initialize(TextObject name, TextObject description, float conquest, float raid, float capture,
            Func<War, bool> isFulfilled, Func<War, bool> isAdequate, Dictionary<TraitObject, float> traitWeights)
        {
            Initialize(name, description);
            this.isFulfilled = isFulfilled;
            this.isAdequate = isAdequate;
            TraitWeights = traitWeights;
            ConquestWeight = conquest;
            RaidWeight = raid;
            CaptureWeight = capture;
        }

        public CasusBelli GetCopy()
        {
            var copy = new CasusBelli(StringId);
            copy.Initialize(Name, Description, ConquestWeight,
                RaidWeight, CaptureWeight, IsFulfilled, IsAdequate, TraitWeights);
            return copy;
        }
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

        public Settlement Fief { get; private set; }

        public bool IsFulfilled(War war) => isFulfilled(war);
        public bool IsAdequate(War war) => isAdequate(war);

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
