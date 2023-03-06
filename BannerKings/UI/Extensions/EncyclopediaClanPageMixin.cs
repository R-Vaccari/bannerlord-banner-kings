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
        private MBBindingList<HeroVM> knights, vassals;
        private MBBindingList<HeirVM> heirs;
        private HeirVM mainHeir;

        public EncyclopediaClanPageMixin(EncyclopediaClanPageVM vm) : base(vm)
        {
            clanPageVM = vm;
            knights = new MBBindingList<HeroVM>();
            councillors = new MBBindingList<CustomNameHeroVM>();
            companions = new MBBindingList<CustomNameHeroVM>();
            Heirs = new MBBindingList<HeirVM>();
            Vassals = new MBBindingList<HeroVM>();
        }

        [DataSourceProperty] public string CultureText => GameTexts.FindText("str_culture").ToString();
        [DataSourceProperty] public string KnightsText => new TextObject("{=ph4LMn6k}Knights").ToString();
        [DataSourceProperty] public string CompanionsText => new TextObject("{=a3G31iZ0}Companions").ToString();
        [DataSourceProperty] public string CouncilText => new TextObject("{=mUaJDjqO}Council").ToString();
        [DataSourceProperty] public string InheritanceText => new TextObject("{=aELuNrRC}Inheritance").ToString();
        [DataSourceProperty] public string VassalsText => new TextObject("{=!}Vassals").ToString(); 

        [DataSourceProperty]
        public string HeirText => new TextObject("{=vArnerHC}Heir").ToString();

        public override void OnRefresh()
        {
            var clan = clanPageVM.Obj as Clan;
            CultureInfo = new EncyclopediaCultureVM(clan.Culture);
            Companions.Clear();
            Councillors.Clear();
            Heirs.Clear();
            Vassals.Clear();
            Knights.Clear();
            var caravans = 0;
            var workshops = 0;

            List<Hero> vassals = BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(clan);
            foreach (Hero hero in vassals)
            {
                Vassals.Add(new HeroVM(hero, true));
            }

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
                    Councillors.Add(new CustomNameHeroVM(position.Member, position.Name));
                }
            }

            var sorted = BannerKingsConfig.Instance.TitleModel.CalculateInheritanceLine(clan, null, 4);
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
                if (rel != null)
                {
                    clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=OKw2P9m1}Faith:").ToString(),
                        rel.Faith.GetFaithName().ToString()));
                }

                ExplainedNumber influenceCap = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(clan, true);
                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=!}Influence Limit:").ToString(),
                   MBRandom.RoundRandomized(influenceCap.ResultNumber).ToString(),
                   new BasicTooltipViewModel(() => influenceCap.GetExplanations())));

                var currentDemesne = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(clan, true);
                var demesneCap = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(clan.Leader, true);

                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=02REd9mG}Demesne limit:").ToString(),
                   $"{currentDemesne.ResultNumber:n2}/{demesneCap.ResultNumber:n2}",
                   new BasicTooltipViewModel(() => new TextObject("{=dhr9NJoA}{TEXT}\nCurrent demesne:\n{CURRENT}\n \nLimit:\n{LIMIT}")
                       .SetTextVariable("TEXT",new TextObject("{=oHJ6Y66V}Demesne limit describes how many settlements you may own without negative implications. Different settlement types have different weights, villages being the lowest, towns being the highest. Being over the limit reduces stability across all your settlements. Owning a settlement's title will reduce it's weight."))
                       .SetTextVariable("CURRENT", currentDemesne.GetExplanations())
                       .SetTextVariable("LIMIT", demesneCap.GetExplanations())
                       .ToString())));

                var currentUnlandedDemesne = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentUnlandedDemesne(clan);
                var unlandedDemesneCap = BannerKingsConfig.Instance.StabilityModel.CalculateUnlandedDemesneLimit(clan.Leader);

                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=8J3DQsNE}Unlanded Demesne limit:").ToString(),
                    $"{currentUnlandedDemesne.ResultNumber:n2}/{unlandedDemesneCap.ResultNumber:n2}",
                    new BasicTooltipViewModel(() => new TextObject("{=dhr9NJoA}{TEXT}\nCurrent demesne:\n{CURRENT}\n \nLimit:\n{LIMIT}")
                        .SetTextVariable("TEXT", new TextObject("{=XAvCOvv4}Unlanded demesne limit describes how many unlanded titles you may own. Unlanded titles are titles such as dukedoms and kingdoms - titles not directly associated with a settlement. Dukedoms have the lowest weight while empires have the biggest. Being over the limit progressively reduces relations with your vassals."))
                        .SetTextVariable("CURRENT", currentUnlandedDemesne.GetExplanations())
                        .SetTextVariable("LIMIT", unlandedDemesneCap.GetExplanations())
                        .ToString())));

                var currentVassals = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentVassals(clan);
                var vassalsCap = BannerKingsConfig.Instance.StabilityModel.CalculateVassalLimit(clan.Leader);

                clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=dB5y6tTY}Vassal limit:").ToString(),
                    $"{currentVassals.ResultNumber:n2}/{vassalsCap.ResultNumber:n2}",
                    new BasicTooltipViewModel(() => new TextObject("{=Q50amDu9}{TEXT}\nCurrent vassals:\n{CURRENT}\n \nLimit:\n{LIMIT}")
                        .SetTextVariable("TEXT", new TextObject("{=nhBf1JY5}Vassal limit is how many vassals you may have without negative consequences. Vassals are clans whose highest title are under your own (ie, a barony title under your county title, or knight clans with a single lordship) or knights in your clan. Knights only weight 0.5 towards the limit, while clan leaders weight 1. Companions and family members do not count. Being over the limit progressively reduces your influence gain."))
                        .SetTextVariable("CURRENT", currentVassals.GetExplanations())
                        .SetTextVariable("LIMIT", vassalsCap.GetExplanations())
                        .ToString())));

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
                    clanPageVM.ClanInfo.Add(new StringPairItemVM(new TextObject("{=89Wxt2hs}Peerage:").ToString(),
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
        public MBBindingList<HeroVM> Vassals
        {
            get => vassals;
            set
            {
                if (value != vassals)
                {
                    vassals = value;
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
    }
}