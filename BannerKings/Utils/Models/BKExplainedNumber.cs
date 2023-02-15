using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Utils.Models
{
    public struct BKExplainedNumber
    {
        public float ResultNumber
        {
            get
            {
                return MathF.Clamp(BaseNumber + BaseNumber * _sumOfFactors, LimitMinValue, LimitMaxValue);
            }
        }

        public float BaseNumber { get; private set; }

        public float LimitMinValue
        {
            get
            {
                if (_limitMinValue == null)
                {
                    return float.MinValue;
                }
                return _limitMinValue.Value;
            }
        }

        public float LimitMaxValue
        {
            get
            {
                if (_limitMaxValue == null)
                {
                    return float.MaxValue;
                }
                return _limitMaxValue.Value;
            }
        }

        public BKExplainedNumber(float baseNumber = 0f, bool includeDescriptions = false, TextObject baseText = null)
        {
            BaseNumber = baseNumber;
            _explainer = (includeDescriptions ? new BKExplainedNumber.StatExplainer() : null);
            _sumOfFactors = 0f;
            _limitMinValue = new float?(float.MinValue);
            _limitMaxValue = new float?(float.MaxValue);
            if (_explainer != null && !BaseNumber.ApproximatelyEqualsTo(0f, 1E-05f))
            {
                _explainer.AddLine((baseText ?? BKExplainedNumber.BaseText).ToString(), BaseNumber, BKExplainedNumber.StatExplainer.OperationType.Base);
            }
        }

        public string GetExplanations()
        {
            if (_explainer == null)
            {
                return "";
            }
            MBStringBuilder mbstringBuilder = default(MBStringBuilder);
            mbstringBuilder.Initialize(16, "GetExplanations");
            foreach (ValueTuple<string, float> valueTuple in _explainer.GetLines(BaseNumber, ResultNumber))
            {
                string value = string.Format("{0} : {1}{2:0.##}\n", valueTuple.Item1, (valueTuple.Item2 > 0.001f) ? "+" : "", valueTuple.Item2);
                mbstringBuilder.Append<string>(value);
            }
            return mbstringBuilder.ToStringAndRelease();
        }

        public string GetFormattedPercentage()
        {
            if (_explainer == null)
            {
                return "";
            }
            MBStringBuilder mbstringBuilder = default(MBStringBuilder);
            mbstringBuilder.Initialize(16, "GetExplanations");
            foreach (ValueTuple<string, float> valueTuple in _explainer.GetLines(BaseNumber, ResultNumber))
            {
                string value = string.Format("{0} : {1}{2:0.00}\n", valueTuple.Item1, (valueTuple.Item2 > 0.001f) ? "+" : "", (valueTuple.Item2 * 100).ToString() + '%');
                mbstringBuilder.Append<string>(value);
            }

            if (_explainer.Lines.Count == 0)
            {
                mbstringBuilder.Append(new TextObject("{=!}No factors yet").ToString());
            }
            return mbstringBuilder.ToStringAndRelease();
        }

        public List<ValueTuple<string, float>> GetLines()
        {
            if (_explainer == null)
            {
                return new List<ValueTuple<string, float>>();
            }
            return _explainer.GetLines(BaseNumber, ResultNumber);
        }
        public void Add(float value, TextObject description = null, TextObject variable = null)
        {
            if (value.ApproximatelyEqualsTo(0f, 1E-05f))
            {
                return;
            }
            BaseNumber += value;
            if (description != null && _explainer != null && !value.ApproximatelyEqualsTo(0f, 1E-05f))
            {
                if (variable != null)
                {
                    description.SetTextVariable("A0", variable);
                }
                _explainer.AddLine(description.ToString(), value, BKExplainedNumber.StatExplainer.OperationType.Add);
            }
        }

        public void AddFactor(float value, TextObject description = null)
        {
            if (value.ApproximatelyEqualsTo(0f, 1E-05f))
            {
                return;
            }
            _sumOfFactors += value;
            if (description != null && _explainer != null && !value.ApproximatelyEqualsTo(0f, 1E-05f))
            {
                _explainer.AddLine(description.ToString(), MathF.Round(value, 3) * 100f, BKExplainedNumber.StatExplainer.OperationType.Multiply);
            }
        }

        public void LimitMin(float minValue)
        {
            _limitMinValue = new float?(minValue);
            if (_explainer != null)
            {
                _explainer.AddLine(BKExplainedNumber.LimitMinText.ToString(), minValue, BKExplainedNumber.StatExplainer.OperationType.LimitMin);
            }
        }

        public void LimitMax(float maxValue)
        {
            _limitMaxValue = new float?(maxValue);
            if (_explainer != null)
            {
                _explainer.AddLine(BKExplainedNumber.LimitMaxText.ToString(), maxValue, BKExplainedNumber.StatExplainer.OperationType.LimitMax);
            }
        }

        public void Clamp(float minValue, float maxValue)
        {
            LimitMin(minValue);
            LimitMax(maxValue);
        }
        private static readonly TextObject LimitMinText = new TextObject("{=GNalaRaN}Minimum", null);
        private static readonly TextObject LimitMaxText = new TextObject("{=cfjTtxWv}Maximum", null);
        private static readonly TextObject BaseText = new TextObject("{=basevalue}Base", null);
        private float? _limitMinValue;
        private float? _limitMaxValue;
        private BKExplainedNumber.StatExplainer _explainer;
        private float _sumOfFactors;

        // Token: 0x020004B6 RID: 1206
        private class StatExplainer
        {
            public List<BKExplainedNumber.StatExplainer.ExplanationLine> Lines { get; private set; } = new List<BKExplainedNumber.StatExplainer.ExplanationLine>();
            public BKExplainedNumber.StatExplainer.ExplanationLine? BaseLine { get; private set; }
            public BKExplainedNumber.StatExplainer.ExplanationLine? LimitMinLine { get; private set; }
            public BKExplainedNumber.StatExplainer.ExplanationLine? LimitMaxLine { get; private set; }

            public List<ValueTuple<string, float>> GetLines(float baseNumber, float resultNumber)
            {
                List<ValueTuple<string, float>> list = new List<ValueTuple<string, float>>();
                if (BaseLine != null)
                {
                    list.Add(new ValueTuple<string, float>(BaseLine.Value.Name, BaseLine.Value.Number));
                }
                foreach (BKExplainedNumber.StatExplainer.ExplanationLine explanationLine in Lines)
                {
                    float num = explanationLine.Number;
                    if (explanationLine.OperationType == BKExplainedNumber.StatExplainer.OperationType.Multiply)
                    {
                        num = baseNumber * num * 0.01f;
                    }
                    list.Add(new ValueTuple<string, float>(explanationLine.Name, num));
                }
                if (LimitMinLine != null && LimitMinLine.Value.Number > resultNumber)
                {
                    list.Add(new ValueTuple<string, float>(LimitMinLine.Value.Name, LimitMinLine.Value.Number));
                }
                if (LimitMaxLine != null && LimitMaxLine.Value.Number < resultNumber)
                {
                    list.Add(new ValueTuple<string, float>(LimitMaxLine.Value.Name, LimitMaxLine.Value.Number));
                }
                return list;
            }

            public void AddLine(string name, float number, BKExplainedNumber.StatExplainer.OperationType opType)
            {
                BKExplainedNumber.StatExplainer.ExplanationLine explanationLine = new BKExplainedNumber.StatExplainer.ExplanationLine(name, number, opType);
                if (opType == BKExplainedNumber.StatExplainer.OperationType.Add || opType == BKExplainedNumber.StatExplainer.OperationType.Multiply)
                {
                    int num = -1;
                    for (int i = 0; i < Lines.Count; i++)
                    {
                        if (Lines[i].Name.Equals(name) && Lines[i].OperationType == opType)
                        {
                            num = i;
                            break;
                        }
                    }
                    if (num < 0)
                    {
                        Lines.Add(explanationLine);
                        return;
                    }
                    explanationLine = new BKExplainedNumber.StatExplainer.ExplanationLine(name, number + Lines[num].Number, opType);
                    Lines[num] = explanationLine;
                    return;
                }
                else
                {
                    if (opType == BKExplainedNumber.StatExplainer.OperationType.Base)
                    {
                        BaseLine = new BKExplainedNumber.StatExplainer.ExplanationLine?(explanationLine);
                        return;
                    }
                    if (opType == BKExplainedNumber.StatExplainer.OperationType.LimitMin)
                    {
                        LimitMinLine = new BKExplainedNumber.StatExplainer.ExplanationLine?(explanationLine);
                        return;
                    }
                    if (opType == BKExplainedNumber.StatExplainer.OperationType.LimitMax)
                    {
                        LimitMaxLine = new BKExplainedNumber.StatExplainer.ExplanationLine?(explanationLine);
                    }
                    return;
                }
            }

            public enum OperationType
            {
                Base,
                Add,
                Multiply,
                LimitMin,
                LimitMax
            }

            // Token: 0x02000765 RID: 1893
            public readonly struct ExplanationLine
            {
                public ExplanationLine(string name, float number, BKExplainedNumber.StatExplainer.OperationType operationType)
                {
                    Name = name;
                    Number = number;
                    OperationType = operationType;
                }

                public readonly float Number;
                public readonly string Name;
                public readonly BKExplainedNumber.StatExplainer.OperationType OperationType;
            }
        }
    }
}
