using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations
{
    public class CultureData : BannerKingsData
    {
        public CultureData(Hero settlementOwner, List<CultureDataClass> cultures)
        {
            this.settlementOwner = settlementOwner;
            this.cultures = cultures;
        }

        [SaveableProperty(1)] private List<CultureDataClass> cultures { get; set; }

        [SaveableProperty(2)] private Hero settlementOwner { get; set; }

        public List<CultureDataClass> Cultures => cultures;

        public CultureObject DominantCulture
        {
            get
            {
                var eligible = new List<(CultureObject, float)>();
                foreach (var data in cultures)
                {
                    if (data.Culture.MilitiaPartyTemplate != null && data.Culture.DefaultPartyTemplate != null &&
                        !data.Culture.IsBandit)
                    {
                        eligible.Add((data.Culture, data.Assimilation));
                    }
                }

                eligible.OrderByDescending(pair => pair.Item2);

                return eligible[0].Item1;
            }
        }

        public Hero SettlementOwner
        {
            get => settlementOwner;
            set
            {
                settlementOwner = value;
                if (!IsCulturePresent(settlementOwner.Culture))
                {
                    if (settlementOwner.Culture == DominantCulture)
                    {
                        AddCulture(settlementOwner.Culture, 1f, 1f);
                    }
                    else
                    {
                        AddCulture(settlementOwner.Culture, 0f);
                    }
                }
            }
        }

        public bool IsCulturePresent(CultureObject culture)
        {
            var data = cultures.FirstOrDefault(x => x.Culture == culture);
            return data != null;
        }

        public void AddCulture(CultureObject culture, float acceptance)
        {
            CultureDataClass dataClass = null;
            foreach (var data in cultures)
            {
                if (data.Culture == culture)
                {
                    dataClass = data;
                    break;
                }
            }

            if (dataClass == null)
            {
                cultures.Add(new CultureDataClass(culture, 0f, acceptance));
            }
            else
            {
                dataClass.Acceptance = acceptance;
            }
        }

        public void AddCulture(CultureObject culture, float acceptance, float assim)
        {
            CultureDataClass dataClass = null;
            foreach (var data in cultures)
            {
                if (data.Culture == culture)
                {
                    dataClass = data;
                    break;
                }
            }

            if (dataClass == null)
            {
                cultures.Add(new CultureDataClass(culture, assim, acceptance));
            }
            else
            {
                dataClass.Acceptance = acceptance;
                dataClass.Assimilation = assim;
            }
        }

        public float GetAssimilation(CultureObject culture)
        {
            var data = cultures.FirstOrDefault(x => x.Culture == culture);
            return data?.Assimilation ?? 0f;
        }

        public float GetAcceptance(CultureObject culture)
        {
            var data = cultures.FirstOrDefault(x => x.Culture == culture);
            return data?.Acceptance ?? 0f;
        }

        internal override void Update(PopulationData data)
        {
            SettlementOwner = data.Settlement.Owner;


            BalanceCultures(data);
            var dominant = DominantCulture;
            if (dominant.BasicTroop != null && dominant.MilitiaSpearman != null)
            {
                data.Settlement.Culture = dominant;
                if (data.Settlement.Notables is {Count: > 0})
                {
                    foreach (var notable in data.Settlement.Notables)
                    {
                        notable.Culture = dominant;
                    }
                }
            }
        }

        private void BalanceCultures(PopulationData data)
        {
            var toDelete = new HashSet<CultureDataClass>();
            var foreignerShare = 0f;

            foreach (var cultureData in cultures)
            {
                if (cultureData.Culture != settlementOwner.Culture && cultureData.Assimilation <= 0.01)
                {
                    toDelete.Add(cultureData);
                    continue;
                }

                if (IsForeigner(data, cultureData))
                {
                    foreignerShare += cultureData.Assimilation;
                }
                else
                {
                    cultureData.Acceptance += BannerKingsConfig.Instance.CultureAcceptanceModel
                        .CalculateEffect(data.Settlement, cultureData).ResultNumber;
                    cultureData.Assimilation += BannerKingsConfig.Instance.CultureAssimilationModel
                        .CalculateEffect(data.Settlement, cultureData).ResultNumber;
                }
            }

            if (toDelete.Count > 0)
            {
                foreach (var cultureData in toDelete)
                {
                    cultures.Remove(cultureData);
                }
            }

            var totalAssim = 0f;
            foreach (var cultureData in cultures)
            {
                totalAssim += cultureData.Assimilation;
            }

            if (totalAssim != 1f)
            {
                var diff = totalAssim - 1f;
                var foreignerTarget = data.Foreigner.ResultNumber;

                var candidates = new List<(CultureDataClass, float)>();
                foreach (var cultureData in cultures)
                {
                    if (cultureData.Assimilation > diff)
                    {
                        var value = cultureData.Assimilation;
                        if (foreignerShare > foreignerTarget && IsForeigner(data, cultureData))
                        {
                            value *= 10f;
                        }

                        candidates.Add(new ValueTuple<CultureDataClass, float>(cultureData, value));
                    }
                }

                var result = MBRandom.ChooseWeighted(candidates);
                if (result != null)
                {
                    result.Assimilation += diff;
                }
                else
                {
                    cultures[0].Assimilation += diff;
                }
            }
        }

        private bool IsForeigner(PopulationData data, CultureDataClass cultureData)
        {
            return cultureData.Culture != settlementOwner.Culture && cultureData.Culture != data.Settlement.Culture;
        }
    }
}