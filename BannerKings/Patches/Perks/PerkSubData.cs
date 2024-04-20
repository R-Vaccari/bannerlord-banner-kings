using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Patches.Perks
{
    class PerkSubData
    {

        public float BonusBase { get; set; }
        public float BonusEverySkill { get; set; }
        public float MaxBonus { get; set; } = 0;
        public float MinBonus { get; set; } = 0;

        public int StartSkillLevel { get; set; } = 0;
        public SkillObject ScaleOnSkill { get; set; }
        public float EverySkillMain { get; set; } = 0;
        public float EverySkillSecondary { get; set; } = 0;
        public float EverySkillOthers { get; set; } = 0;
        public SkillScale SkillScale { get; set; } = SkillScale.None;

        public string Description1 { get; set; }
        public string Description2 { get; set; }

        public SkillEffect.PerkRole? Role { get; set; }

        public List<SkillEffect.PerkRole> AdditionalRoles { get; set; }



        public TextObject GetFormattedDescription1(SkillEffect.EffectIncrementType incrementType)
        {
            return GetFormattedDesc(Description1, incrementType);
        }
        public TextObject GetFormattedDescription2(SkillEffect.EffectIncrementType incrementType)
        {
            return GetFormattedDesc(Description1, incrementType);
        }
        private TextObject GetFormattedDesc(string desc, SkillEffect.EffectIncrementType incrementType)
        {
            TextObject textObject = new TextObject(desc);
            SetDescriptionTextVariable(textObject, incrementType);

            return textObject;
        }
        private void SetDescriptionTextVariable(TextObject description, SkillEffect.EffectIncrementType effectIncrementType)
        {
            string text = GetFormattedValueText(BonusEverySkill, effectIncrementType);
            description.SetTextVariable("VALUE", text);
            text = GetFormattedValueText(BonusBase, effectIncrementType);
            description.SetTextVariable("VALUEBASE", text);
            text = GetFormattedValueText(MaxBonus, effectIncrementType);
            description.SetTextVariable("MAXVALUE", text);
            text = GetFormattedValueText(MinBonus, effectIncrementType);
            description.SetTextVariable("MINVALUE", text);
            text = GetFormattedValueText(MinBonus == 0 ? MaxBonus : MinBonus, effectIncrementType);
            description.SetTextVariable("MINMAXVALUE", text);
            if (EverySkillMain>1)
            {
                description.SetTextVariable("EVERYSKILLMAIN", EverySkillMain);
            }
            else
            {
                description.SetTextVariable("EVERYSKILLMAIN", "");
            }
            if (EverySkillSecondary > 1)
            {
                description.SetTextVariable("EVERYSKILLSECONDARY", EverySkillSecondary);
            }
            else
            {
                description.SetTextVariable("EVERYSKILLSECONDARY", "");
            }
            if (EverySkillOthers > 1)
            {
                description.SetTextVariable("EVERYSKILLOTHERS", EverySkillOthers);
            }else
            {
                description.SetTextVariable("EVERYSKILLOTHERS", "");
            }
           
            description.SetTextVariable("STARTSKILLLEVEL", StartSkillLevel);
        }

        private string GetFormattedValueText(float value, SkillEffect.EffectIncrementType effectIncrementType)
        {
            float num = effectIncrementType == SkillEffect.EffectIncrementType.AddFactor ? value * 100f : value;
            string text = $"{num:0.#}";
            if (BonusEverySkill > 0f)
            {
                text = "+" + text;
            }

            if (effectIncrementType == SkillEffect.EffectIncrementType.AddFactor)
            {
                text = text + "%";
            }

            return text;
        }



    }
}
