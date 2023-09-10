using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Populations;
using BannerKings.UI.Items.UI;
using Bannerlord.UIExtenderEx.Attributes;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Management
{
    public class ReligionVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> notablesList;
        private MBBindingList<InformationElement> cultureInfo;
        private MBBindingList<InformationElement> religionsList;
        private readonly Settlement settlement;
        private MBBindingList<InformationElement> statsInfo;
        private bool appointEnabled, removeEnabled;

        public ReligionVM(PopulationData data, Settlement _settlement, bool _isSelected) : base(data, _isSelected)
        {
            notablesList = new MBBindingList<InformationElement>();
            religionsList = new MBBindingList<InformationElement>();
            cultureInfo = new MBBindingList<InformationElement>();
            statsInfo = new MBBindingList<InformationElement>();
            settlement = _settlement;
            RefreshValues();
        }

        [DataSourceProperty]
        public string FaithText => new TextObject("{=OKw2P9m1}Faith").ToString();
        [DataSourceProperty]
        public string PopulationText => new TextObject("{=o3Ohk2hA}Population").ToString();
        [DataSourceProperty]
        public string AppointText => new TextObject("{=!}Appoint Preacher").ToString();
        [DataSourceProperty]
        public string RemoveText => new TextObject("{=!}Banish Preacher").ToString();

        public override void RefreshValues()
        {
            base.RefreshValues();
            if (data.ReligionData?.Religions == null)
            {
                return;
            }

            NotablesList.Clear();
            ReligionList.Clear();
            ReligionInfo.Clear();
            StatsInfo.Clear();

            var totalFaithsWeight = 0f;

            foreach (var pair in data.ReligionData.Religions)
            {
                totalFaithsWeight += BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(pair.Key, settlement)
                    .ResultNumber;

                ReligionList.Add(new InformationElement(
                    pair.Key.Faith.GetFaithName().ToString(),
                    FormatValue(pair.Value),
                    new TextObject("{=tJePaqqg}{RELIGION}\nPresence: {PRESENCE}")
                    .SetTextVariable("RELIGION", pair.Key.Faith.GetFaithDescription())
                    .SetTextVariable("PRESENCE", FormatValue(pair.Value))
                    .ToString()));
            }

            AppointEnabled = true;
            var playerFaith = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            foreach (var notable in settlement.Notables)
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager
                                           .GetHeroReligion(notable);

                if (rel == null)
                {
                    continue;
                }

                var factor = BannerKingsConfig.Instance.ReligionModel.GetNotableFactor(notable, settlement);
                var result = FormatValue(factor / totalFaithsWeight);
                NotablesList.Add(new InformationElement(new TextObject("{=!}{HERO} ({FAITH})")
                    .SetTextVariable("HERO", notable.Name)
                    .SetTextVariable("FAITH", rel.Faith.GetFaithName())
                    .ToString(),
                    result,
                    new TextObject("{=GxcNDrXt}{HERO} holds sway over {PERCENTAGE} of the population. Changing their faith would strengthen the new faith's grip in the settlement.")
                    .SetTextVariable("HERO", notable.Name)
                    .SetTextVariable("PERCENTAGE", result)
                    .ToString()));

                if (notable.IsPreacher)
                {
                    var notableRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(notable);
                    if (notableRel == playerFaith)
                    {
                        AppointEnabled = false;
                    }
                    else
                    {
                        RemoveEnabled = true;
                    }
                }
            }

            var tension = BannerKingsConfig.Instance.ReligionModel.CalculateTensionTarget(data.ReligionData);
            StatsInfo.Add(new InformationElement(new TextObject("{=QiyEsZ4L}Religious Tension:").ToString(),
                FormatValue(tension.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=cz0WGSC1}Tensions between the different faiths in this settlement. The less homogenous the population's faith is, the more tensions there are. Tensions are also affected by the dominant religion's view on the other faiths. Tolerated faiths do not incur extra tensions. Untolerated faiths do, and hostile faiths incur a lot of tension. Religious tensions will significantly affect your settlement's loyalty and performance."))
                    .SetTextVariable("EXPLANATIONS", tension.GetExplanations())
                    .ToString()));

            var dominant = data.ReligionData.DominantReligion;
            ReligionInfo.Add(new InformationElement(new TextObject("{=ZcGwd8sq}Dominant Faith:").ToString(),
                dominant.Faith.GetFaithName().ToString(),
                new TextObject("{=8ootTEcK}The most assimilated culture in this settlement, and considered the legal culture.").ToString()));
            
            if (playerFaith != null)
            {
                var presence = BannerKingsConfig.Instance.ReligionModel.CalculateReligionWeight(playerFaith, settlement);
                ReligionInfo.Add(new InformationElement(new TextObject("{=gTzbdsBY}Faith Presence:").ToString(),
                    FormatValue(presence.ResultNumber / totalFaithsWeight),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=STxuNCBU}The faith's presence in the settlement. Presence describes how much of the population adheres to the faith. Presence is affected by various factors, such as the faith's fervor, and whether it accepts the culture's settlement or not."))
                        .SetTextVariable("EXPLANATIONS", presence.GetExplanations())
                        .ToString()));


                var fervor = BannerKingsConfig.Instance.ReligionModel.CalculateFervor(playerFaith);
                ReligionInfo.Add(new InformationElement(new TextObject("{=PUwmzUZy}Fervor:").ToString(),
                    FormatValue(fervor.ResultNumber),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=!}The faith's fervor. A faith's fervor makes its populations and heroes harder to convert. In settlements, fervor grealy contributes to the faith's presence. Heroes instead are less likely and/or require more resources to convert. Fervor is based on doctrines, settlements and clans that follow the faith. Additionaly, holding the Faith Seat and the faith's Holy Sites are important factors to fervor."))
                        .SetTextVariable("EXPLANATIONS", fervor.GetExplanations())
                        .ToString()));
            }
        }

        [DataSourceMethod]
        private void AppointPreacher()
        {
            var playerFaith = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            TextObject text = null;
            bool possible;
            float piety = 0f;
            float cost = 0f;

            int rank = playerFaith.Faith.GetIdealRank(Settlement.CurrentSettlement);
            if (rank > 0)
            {
                piety = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel.GetAppointCost(Hero.MainHero, data.ReligionData).ResultNumber);
                cost = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel.GetAppointInfluence(Hero.MainHero, data.ReligionData).ResultNumber);
                text = new TextObject("{=!}The {FAITH} faith would admit a preacher of rank {PREACHER} for {FIEF}. Appointing such a preacher would cost you {PIETY}{PIETY_ICON} and {INFLUENCE}{INFLUENCE_ICON}")
                    .SetTextVariable("FAITH", playerFaith.Faith.GetFaithName())
                    .SetTextVariable("PREACHER", playerFaith.Faith.GetRankTitle(rank))
                    .SetTextVariable("FIEF", Settlement.CurrentSettlement.Name)
                    .SetTextVariable("PIETY", piety)
                    .SetTextVariable("PIETY_ICON", Utils.TextHelper.PIETY_ICON)
                    .SetTextVariable("INFLUENCE", cost)
                    .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON);
                possible = true;
              
                if (BannerKingsConfig.Instance.ReligionsManager.GetPiety(Hero.MainHero) < piety)
                {
                    possible = false;
                }

                if (Clan.PlayerClan.Influence < cost)
                {
                    possible = false;
                }
            }
            else
            {
                text = new TextObject("{=!}The {FAITH} faith does not admit any type of preacher for {FIEF}. It is not possible to appoint one.")
                                    .SetTextVariable("FAITH", playerFaith.Faith.GetName())
                                    .SetTextVariable("FIEF", Settlement.CurrentSettlement.Name);
                possible = false;
            }

            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Appoint Preacher").ToString(),
                text.ToString(),
                possible,
                true,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_selection_widget_cancel").ToString(),
                () =>
                {
                    Clergyman clergy = playerFaith.GenerateClergyman(settlement);
                    if (clergy != null)
                    {
                        ChangeClanInfluenceAction.Apply(Clan.PlayerClan, -cost);
                        BannerKingsConfig.Instance.ReligionsManager.AddPiety(Hero.MainHero, -piety, true);
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=!}{HERO} was installed as a preacher at {FIEF}")
                            .SetTextVariable("HERO", clergy.Hero.Name)
                            .SetTextVariable("FIEF", settlement.Name)
                            .ToString(),
                            Color.FromUint(Utils.TextHelper.COLOR_LIGHT_BLUE)));
                        RefreshValues();
                    }
                },
                null));
        }

        [DataSourceMethod]
        private void BanishPreacher()
        {
            List<InquiryElement> elements = new List<InquiryElement>(1);
            var playerFaith = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
            foreach (Hero notable in settlement.Notables)
            {
                if (!notable.IsPreacher) continue;

                var faith = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(notable);
                if (faith != playerFaith)
                {
                    bool possible = true;
                    TextObject hint = null;

                    if (faith == data.ReligionData.DominantReligion)
                    {
                        possible = false;
                        hint = new TextObject("{=!}Not possible to banish a preacher of the local dominant religion.");
                    }
                    else
                    {
                        float piety = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel
                                                .GetRemoveCost(Hero.MainHero, notable, data.ReligionData).ResultNumber);

                        float cost = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel
                            .GetRemoveInfluence(Hero.MainHero, notable, data.ReligionData).ResultNumber);

                        float loyalty = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel
                            .GetRemoveLoyaltyCost(Hero.MainHero, notable, data.ReligionData).ResultNumber);

                        if (BannerKingsConfig.Instance.ReligionsManager.GetPiety(Hero.MainHero) < piety)
                        {
                            hint = new TextObject("{=!}Not enough influence.");
                            possible = false;
                        }

                        if (Clan.PlayerClan.Influence < cost)
                        {
                            hint = new TextObject("{=!}Not enough influence.");
                            possible = false;
                        }

                        if (possible)
                        {
                            hint = new TextObject("{=!}Removing {HERO} will cost you {PIETY}{PIETY_ICON} and {INFLUENCE}{INFLUENCE_ICON}. Moreover, due to their influence over the populace, the fief will suffer a loyalty hit of {LOYALTY} points reduction.")
                                .SetTextVariable("LOYALTY", loyalty)
                                .SetTextVariable("INFLUENCE", cost)
                                .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                                .SetTextVariable("PIETY", piety)
                                .SetTextVariable("PIETY_ICON", Utils.TextHelper.PIETY_ICON);
                        }
                    }

                    elements.Add(new InquiryElement(notable,
                        notable.Name.ToString(),
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(notable.CharacterObject, true)),
                        possible,
                        hint.ToString()));
                }
            }
            
            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("").ToString(),
                new TextObject("").ToString(),
                null,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_selection_widget_cancel").ToString(),
                (List<InquiryElement> list) =>
                {
                    Hero notable = list.First().Identifier as Hero;

                    float piety = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel
                        .GetRemoveCost(Hero.MainHero, notable, data.ReligionData).ResultNumber);

                    float cost = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel
                        .GetRemoveInfluence(Hero.MainHero, notable, data.ReligionData).ResultNumber);

                    float loyalty = MBRandom.RoundRandomized(BannerKingsConfig.Instance.ReligionModel
                        .GetRemoveLoyaltyCost(Hero.MainHero, notable, data.ReligionData).ResultNumber);

                    var faith = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(notable);
                    faith.RemoveClergyman(settlement);

                    ChangeClanInfluenceAction.Apply(Clan.PlayerClan, -cost);
                    BannerKingsConfig.Instance.ReligionsManager.AddPiety(Hero.MainHero, -piety, true);
                    if (settlement.Town != null)
                    {
                        settlement.Town.Loyalty -= loyalty;
                    }

                    InformationManager.DisplayMessage(new InformationMessage(
                        new TextObject("{=!}{HERO} was removed as a preacher at {FIEF}")
                        .SetTextVariable("HERO", notable.Name)
                        .SetTextVariable("FIEF", settlement.Name)
                        .ToString(),
                        Color.FromUint(Utils.TextHelper.COLOR_LIGHT_YELLOW)));
                    RefreshValues();
                },
                null));
        }

        [DataSourceProperty]
        public bool AppointEnabled
        {
            get => appointEnabled;
            set
            {
                if (value != appointEnabled)
                {
                    appointEnabled = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool RemoveEnabled
        {
            get => removeEnabled;
            set
            {
                if (value != removeEnabled)
                {
                    removeEnabled = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> ReligionList
        {
            get => religionsList;
            set
            {
                if (value != religionsList)
                {
                    religionsList = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> NotablesList
        {
            get => notablesList;
            set
            {
                if (value != notablesList)
                {
                    notablesList = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> ReligionInfo
        {
            get => cultureInfo;
            set
            {
                if (value != cultureInfo)
                {
                    cultureInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> StatsInfo
        {
            get => statsInfo;
            set
            {
                if (value != statsInfo)
                {
                    statsInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}