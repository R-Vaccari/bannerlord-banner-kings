using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.UI.Court;
using BannerKings.UI.Cultures;
using BannerKings.UI.Kingdoms;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("Refresh")]
    internal class EncyclopediaClanPageMixin : BaseViewModelMixin<EncyclopediaClanPageVM>
    {
        private bool addedFields;
        private readonly EncyclopediaClanPageVM clanPageVM;
        private MBBindingList<CustomNameHeroVM> councillors, companions;
        private EncyclopediaCultureVM cultureVM;
        private MBBindingList<HeroVM> knights;
        private MBBindingList<HeirVM> heirs;
        private HeirVM mainHeir;

        public EncyclopediaClanPageMixin(EncyclopediaClanPageVM vm) : base(vm)
        {
            clanPageVM = vm;
            knights = new MBBindingList<HeroVM>();
            councillors = new MBBindingList<CustomNameHeroVM>();
            companions = new MBBindingList<CustomNameHeroVM>();
            Heirs = new MBBindingList<HeirVM>();
        }

        [DataSourceProperty] public string CultureText => GameTexts.FindText("str_culture").ToString();

        [DataSourceProperty] public string KnightsText => new TextObject("{=ph4LMn6k}Knights").ToString();

        [DataSourceProperty] public string CompanionsText => new TextObject("{=a3G31iZ0}Companions").ToString();

        [DataSourceProperty] public string CouncilText => new TextObject("{=mUaJDjqO}Council").ToString();

        [DataSourceProperty] public string InheritanceText => new TextObject("{=aELuNrRC}Inheritance").ToString();

        [DataSourceProperty]
        public string HeirText => new TextObject("{=!}Heir").ToString();



        [DataSourceProperty]
        public MBBindingList<HeirVM> Heirs
        {
            get => heirs;
            set
            {
                if (value != heirs)
                {
                    heirs = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "Heirs");
                }
            }
        }

        [DataSourceProperty]
        public HeirVM MainHeir
        {
            get => mainHeir;
            set
            {
                if (value != mainHeir)
                {
                    mainHeir = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "MainHeir");
                }
            }
        }

        [DataSourceProperty]
        public EncyclopediaCultureVM CultureInfo
        {
            get => cultureVM;
            set
            {
                if (value != cultureVM)
                {
                    cultureVM = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
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
                    ViewModel!.OnPropertyChangedWithValue(value);
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
                    ViewModel!.OnPropertyChangedWithValue(value);
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
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            clanPageVM.ClanInfo.Clear();
            Knights.Clear();
            Councillors.Clear();
            Companions.Clear();
            Heirs.Clear();
            var clan = clanPageVM.Obj as Clan;
            CultureInfo = new EncyclopediaCultureVM(clan.Culture);
            var caravans = 0;
            var workshops = 0;

            string highestTitle = null;
            foreach (var member in clan.Lords)
            {
                if (member.IsDead || member.IsChild)
                {
                    continue;
                }

                caravans += member.OwnedCaravans.Count;
                workshops += member.OwnedWorkshops.Count;

                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(member);
                if (title != null)
                {
                    if (member != member.Clan.Leader && title.type == TitleType.Lordship)
                    {
                        knights.Add(new HeroVM(member));
                        if (clanPageVM.Members.Any(x => x.Hero == member))
                        {
                            var vm = clanPageVM.Members.First(x => x.Hero == member);
                            clanPageVM.Members.Remove(vm);
                        }
                    }
                    else if (member == member.Clan.Leader)
                    {
                        highestTitle = Utils.Helpers.GetTitlePrefix(title.type,
                            title.contract.Government, member.MapFaction.Culture);
                    }
                }
            }

            foreach (var companion in clan.Companions)
            {
                if (companion.IsDead || companion.IsChild)
                {
                    continue;
                }

                TextObject roleTitle = null;

                if (companion.PartyBelongedTo != null && !companion.IsPartyLeader)
                {
                    var role = companion.PartyBelongedTo.GetHeroPerkRole(companion);
                    if (role != SkillEffect.PerkRole.None)
                    {
                        roleTitle = GameTexts.FindText("str_clan_role", role.ToString().ToLower());
                    }
                }

                if (clanPageVM.Members.Any(x => x.Hero == companion))
                {
                    var vm = clanPageVM.Members.First(x => x.Hero == companion);
                    clanPageVM.Members.Remove(vm);
                }

                Companions.Add(new CustomNameHeroVM(companion, roleTitle, true));
            }

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (council != null)
            {
                foreach (var position in council.GetOccupiedPositions())
                {
                    Councillors.Add(new CustomNameHeroVM(position.Member, position.GetName()));
                }
            }

            var sorted = BannerKingsConfig.Instance.TitleModel.CalculateInheritanceLine(clan);
            for (int i = 0; i < sorted.Count(); i++)
            {
                var hero = sorted.ElementAt(i).Key;
                var exp = sorted.ElementAt(i).Value;
                if (i == 0)
                {
                    MainHeir = new HeirVM(hero, exp);
                }
                else
                {
                    Heirs.Add(new HeirVM(hero, exp));
                }
            }


            if (!addedFields)
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);

                GameTexts.SetVariable("LEFT", new TextObject("{=t9cmsSU1}Clan Tier").ToString());
                clanPageVM.ClanInfo.Add(new StringPairItemVM(GameTexts.FindText("str_LEFT_colon").ToString(),
                    clan.Tier.ToString()));

                GameTexts.SetVariable("LEFT", new TextObject("{=d9qmQEZh}Clan Strength").ToString());
                clanPageVM.ClanInfo.Add(new StringPairItemVM(GameTexts.FindText("str_LEFT_colon").ToString(),
                    clan.TotalStrength.ToString("F0")));

                GameTexts.SetVariable("LEFT", GameTexts.FindText("str_wealth").ToString());
                clanPageVM.ClanInfo.Add(new StringPairItemVM(GameTexts.FindText("str_LEFT_colon").ToString(),
                    CampaignUIHelper.GetClanWealthStatusText(clan)));

                if (rel != null)
                {
                    clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=OKw2P9m1}Faith:").ToString(),
                        rel.Faith.GetFaithName().ToString()));
                }

                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=K7hq6hSE}Owned Caravans:").ToString(),
                    caravans.ToString()));

                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=2qcNKU3J}Owned Workshops:").ToString(),
                    workshops.ToString()));

                if (highestTitle != null)
                {
                    clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=jQa22yjg}Title Level:").ToString(),
                        highestTitle));
                }

                if (council.Peerage != null)
                {
                    clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Peerage:").ToString(),
                        council.Peerage.Name.ToString(), new BasicTooltipViewModel(() => council.Peerage.GetRights().ToString())));
                }

                var income = BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(clan, true);
                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=43UgU7C4}Income:").ToString(),
                    income.ResultNumber.ToString(), new BasicTooltipViewModel(() => income.GetExplanations())));

                var expenses = BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanExpenses(clan, true);
                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=Locuup55}Expenses:").ToString(),
                    expenses.ResultNumber.ToString(), new BasicTooltipViewModel(() => expenses.GetExplanations())));

                addedFields = true;
            }
        }
    }
}