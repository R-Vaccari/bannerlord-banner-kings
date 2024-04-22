﻿using BannerKings.Managers.Court.Members;
using BannerKings.Settings;
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

        public float EverySkillCourtMember { get; set; } = 0;
        public string CourtPosition { get; set; } = "";

        public float EverySkillRoyalCourtMember { get; set; } = 0;
        public string CourtRoyalPosition { get; set; } = "";

        public float EverySkillOthers { get; set; } = 0;
        public SkillScale SkillScale { get; set; } = SkillScale.None;

        public string DescriptionMain { get; set; }
        public string DescriptionSecondary { get; set; }
        public string DescriptionCourt { get; set; }
        public string DescriptionRoylCourt { get; set; }
        public string DescriptionOthers { get; set; }
        public string DescriptionMax { get; set; } = " (total max {MINMAXVALUE})";

        public SkillEffect.PerkRole? Role { get; set; }

        public List<SkillEffect.PerkRole> AdditionalRoles { get; set; }



        public TextObject GetFormattedDescription(SkillEffect.EffectIncrementType incrementType)
        {
            var desc = "";
            if (!string.IsNullOrWhiteSpace(DescriptionMain))
            {
                desc += GetFormattedDesc(DescriptionMain, incrementType);
            }
            if (!string.IsNullOrWhiteSpace(DescriptionSecondary)&& BannerKingsSettings.Instance.EnableUsefulGovernorPerksFromSettlementOwner)
            {
                desc += ",\n";
                desc += GetFormattedDesc(DescriptionSecondary, incrementType);
            }
            if (!string.IsNullOrWhiteSpace(DescriptionRoylCourt))
            {
                desc += ",\n";
                desc += GetFormattedDesc(DescriptionRoylCourt, incrementType);
            }
            if (!string.IsNullOrWhiteSpace(DescriptionCourt))
            {
                desc += ",\n";
                desc += GetFormattedDesc(DescriptionCourt, incrementType);
            }
            if (!string.IsNullOrWhiteSpace(DescriptionOthers)&& BannerKingsSettings.Instance.EnableUsefulPerksFromAllPartyMembers)
            {
                desc += ",\n";
                desc += GetFormattedDesc(DescriptionOthers, incrementType);
            }

            desc += ".";
            if (!string.IsNullOrWhiteSpace(DescriptionMax))
            {
                desc += GetFormattedDesc(DescriptionMax, incrementType);
            }
            return new TextObject(desc);
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
            text = DefaultCouncilPositions.Instance.GetPositionName(CourtPosition, false).ToString();
            description.SetTextVariable("COURTPOSITION", text.ToLower());
            text = DefaultCouncilPositions.Instance.GetPositionName(CourtPosition, true).ToString();
            description.SetTextVariable("ROYALCOURTPOSITION", text.ToLower());
            if (EverySkillMain > 1)
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
            }
            else
            {
                description.SetTextVariable("EVERYSKILLOTHERS", "");
            }
            if (EverySkillCourtMember > 1)
            {
                description.SetTextVariable("EVERYSKILLCOURTMEMBER", EverySkillCourtMember);
            }
            else
            {
                description.SetTextVariable("EVERYSKILLCOURTMEMBER", "");
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
