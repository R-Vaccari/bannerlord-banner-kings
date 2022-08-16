﻿using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Titles;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Linq;
using TaleWorlds.Core;
using BannerKings.UI.Court;
using BannerKings.Managers.Court;
using BannerKings.UI.Cultures;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("Refresh")]
    internal class EncyclopediaClanPageMixin : BaseViewModelMixin<EncyclopediaClanPageVM>
    {
        private EncyclopediaClanPageVM clanPageVM;
        private MBBindingList<HeroVM> knights;
        private MBBindingList<CustomNameHeroVM> councillors, companions;
        private EncyclopediaCultureVM cultureVM;
        private bool addedFields = false;
        public EncyclopediaClanPageMixin(EncyclopediaClanPageVM vm) : base(vm)
        {
            clanPageVM = vm;
            knights = new MBBindingList<HeroVM>();
            councillors = new MBBindingList<CustomNameHeroVM>();
            companions = new MBBindingList<CustomNameHeroVM>();
        }

        public override void OnRefresh()
        {
            clanPageVM.ClanInfo.Clear();
            Knights.Clear();
            Councillors.Clear();
            Companions.Clear();
            Clan clan = clanPageVM.Obj as Clan;
            CultureInfo = new EncyclopediaCultureVM(clan.Culture);
            int caravans = 0;
            int workshops = 0;

            string highestTitle = null;
            foreach (Hero member in clan.Lords) 
            {
                if (member.IsDead || member.IsChild) continue;
                caravans += member.OwnedCaravans.Count;
                workshops += member.OwnedWorkshops.Count;

                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(member);
                if (title != null)
                {
                    if (member != member.Clan.Leader && title.type == TitleType.Lordship) 
                    {
                        knights.Add(new HeroVM(member));
                        if (clanPageVM.Members.Any(x => x.Hero == member))
                        {
                            HeroVM vm = clanPageVM.Members.First(x => x.Hero == member);
                            clanPageVM.Members.Remove(vm);
                        }
                    } 
                    else if (member == member.Clan.Leader) highestTitle = Utils.Helpers.GetTitlePrefix(title.type,
                        title.contract.Government, member.MapFaction.Culture);
                }
            }

            foreach (Hero companion in clan.Companions)
            {
                if (companion.IsDead || companion.IsChild) continue;
                TextObject roleTitle = null;

                if (companion.PartyBelongedTo != null && !companion.IsPartyLeader)
                {
                    SkillEffect.PerkRole role = companion.PartyBelongedTo.GetHeroPerkRole(companion);
                    if (role != SkillEffect.PerkRole.None) roleTitle = GameTexts.FindText("str_clan_role", role.ToString().ToLower());
                }

                if (clanPageVM.Members.Any(x => x.Hero == companion))
                {
                    HeroVM vm = clanPageVM.Members.First(x => x.Hero == companion);
                    clanPageVM.Members.Remove(vm);
                }

                Companions.Add(new CustomNameHeroVM(companion, roleTitle, true));
            }

            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (council != null) 
                foreach (CouncilMember position in council.GetOccupiedPositions())
                    Councillors.Add(new CustomNameHeroVM(position.Member, position.GetName()));

            if (!addedFields)
            {
                Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);

                GameTexts.SetVariable("LEFT", new TextObject("{=tTLvo8sM}Clan Tier", null).ToString());
                clanPageVM.ClanInfo.Add(new StringPairItemVM(GameTexts.FindText("str_LEFT_colon", null).ToString(), clan.Tier.ToString(), null));

                GameTexts.SetVariable("LEFT", new TextObject("{=ODEnkg0o}Clan Strength", null).ToString());
                clanPageVM.ClanInfo.Add(new StringPairItemVM(GameTexts.FindText("str_LEFT_colon", null).ToString(), clan.TotalStrength.ToString("F0"), null));

                GameTexts.SetVariable("LEFT", GameTexts.FindText("str_wealth", null).ToString());
                clanPageVM.ClanInfo.Add(new StringPairItemVM(GameTexts.FindText("str_LEFT_colon", null).ToString(), CampaignUIHelper.GetClanWealthStatusText(clan), null));

                if (rel != null) clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Faith:").ToString(),
                    rel.Faith.GetFaithName().ToString(), null));

                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Owned Caravans:").ToString(),
                    caravans.ToString(), null));

                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Owned Workshops:").ToString(),
                    workshops.ToString(), null));

                if (highestTitle != null) clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Title Level:").ToString(),
                    highestTitle, null));

                ExplainedNumber income = BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(clan, true);
                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Income:").ToString(),
                    income.ResultNumber.ToString(), new BasicTooltipViewModel(() => income.GetExplanations())));

                ExplainedNumber expenses = BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanExpenses(clan, true);
                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Expenses:").ToString(),
                    expenses.ResultNumber.ToString(), new BasicTooltipViewModel(() => expenses.GetExplanations())));

                addedFields = true;
            }
        }

        [DataSourceProperty]
        public string CultureText => GameTexts.FindText("str_culture").ToString();

        [DataSourceProperty]
        public string KnightsText => new TextObject("{=!}Knights").ToString();

        [DataSourceProperty]
        public string CompanionsText => new TextObject("{=!}Companions").ToString();

        [DataSourceProperty]
        public string CouncilText => new TextObject("{=!}Council").ToString();

        [DataSourceProperty]
        public EncyclopediaCultureVM CultureInfo
        {
            get => cultureVM;
            set
            {
                if (value != cultureVM)
                {
                    cultureVM = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "CultureInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomNameHeroVM> Companions
        {
            get => companions;
            set
            {
                if (value != companions)
                {
                    companions = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "Companions");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeroVM> Knights
        {
            get => knights;
            set
            {
                if (value != knights)
                {
                    knights = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "Knights");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CustomNameHeroVM> Councillors
        {
            get => councillors;
            set
            {
                if (value != councillors)
                {
                    councillors = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "Councillors");
                }
            }
        }
    }
}