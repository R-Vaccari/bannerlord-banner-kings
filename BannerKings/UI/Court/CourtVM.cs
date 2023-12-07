using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Behaviours.Diplomacy;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Grace;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Education.Languages;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using BannerKings.Utils.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Court
{
    internal class CourtVM : BannerKingsViewModel
    {
        private MBBindingList<CouncilPositionVM> corePositions, extraPositions;
        private readonly CouncilData council;
        private CouncilMember councilPosition;
        private CouncilVM councilVM;
        private MBBindingList<InformationElement> courtInfo, privilegesInfo, courtierInfo;
        private CharacterVM currentCharacter;
        private MBBindingList<ClanLordItemVM> family, courtiers, guests;
        private bool isRoyal, hasExtraPositions, selectorsVisible;
        private string positionName, positionDescription, positionEffects;
        private BannerKingsSelectorVM<CourtExpenseSelectorItemVM> extravaganceSelector, servantsSelector,
            suppliesSelector, securitySelector, lodgingsSelector;

        private readonly ITeleportationCampaignBehavior teleportationBehavior =
            TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<ITeleportationCampaignBehavior>();

        public CourtVM(bool royal) : base(null, false)
        {
            if (!royal)
            {
                council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Hero.MainHero);
            }
            else if (Clan.PlayerClan.Kingdom != null)
            {
                council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan.Kingdom.Leader);
            }

            family = new MBBindingList<ClanLordItemVM>();
            courtiers = new MBBindingList<ClanLordItemVM>();
            corePositions = new MBBindingList<CouncilPositionVM>();
            extraPositions = new MBBindingList<CouncilPositionVM>();
            courtInfo = new MBBindingList<InformationElement>();
            courtierInfo = new MBBindingList<InformationElement>();
            privilegesInfo = new MBBindingList<InformationElement>();
            guests = new MBBindingList<ClanLordItemVM>();
            isRoyal = royal;
            currentCharacter = new CharacterVM(Hero.MainHero, null);
        }

        [DataSourceProperty] public string FamilyText => new TextObject("{=QCw05MZN}Household").ToString();
        [DataSourceProperty] public string CourtiersText => new TextObject("{=PykdjcGm}Courtiers").ToString();
        [DataSourceProperty] public string GuestsText => new TextObject("{=FVA72PZG}Guests").ToString();
        [DataSourceProperty] public string EffectsText => new TextObject("{=K7df68TT}Effects").ToString();
        [DataSourceProperty] public string PrivilegesText => new TextObject("{=77D4i3pG}Privileges").ToString();
        [DataSourceProperty] public string PrivyCouncilText => new TextObject("{=7NeZtxVP}Privy Council").ToString();
        [DataSourceProperty] public string ExtendedCouncilText => new TextObject("{=BkJbqOWj}Extended Council").ToString();
        [DataSourceProperty] public string ExtravaganceText => new TextObject("{=hYV6dQoP}Extravagance").ToString();
        [DataSourceProperty] public string SuppliesText => new TextObject("{=SaWHeTiw}Supplies").ToString();
        [DataSourceProperty] public string SecurityText => new TextObject("{=MqCH7R4A}Security").ToString();
        [DataSourceProperty] public string ServantsText => new TextObject("{=BrMCkQMQ}Servants").ToString();
        [DataSourceProperty] public string LodgingsText => new TextObject("{=TqMRp818}Lodgings").ToString();
        [DataSourceProperty] public bool PlayerOwned => council.Owner == Hero.MainHero;
        [DataSourceProperty] public bool DisableButtons => !PlayerOwned;
     
        public override void RefreshValues()
        {
            base.RefreshValues();
            Family.Clear();
            Courtiers.Clear();
            CourtInfo.Clear();
            CorePositions.Clear();
            ExtraPositions.Clear();
            CourtierInfo.Clear();
            PrivilegesInfo.Clear();
            Guests.Clear();
           
            if (councilPosition == null)
            {
                councilPosition = council.Positions.FirstOrDefault();
                if (councilPosition == null)
                {
                    return;
                }
            }

            var heroes = council.GetCourtMembers();
            CouncilVM = new CouncilVM(SetCouncilMember, council, councilPosition, heroes);

            foreach (var hero in heroes)
            {
                if (hero.Clan == council.Owner.Clan)
                {
                    Family.Add(new ClanLordItemVM(hero, teleportationBehavior, null, SetCurrentCharacter,
                        OnRequestRecall, OnRequestRecall));
                }
                else
                {
                    Courtiers.Add(new ClanLordItemVM(hero, teleportationBehavior, null, SetCurrentCharacter,
                        OnRequestRecall, OnRequestRecall));
                }
            }

            foreach (var guest in council.Guests)
            {
                Guests.Add(new ClanLordItemVM(guest, teleportationBehavior, null, SetCurrentCharacter,
                        OnRequestRecall, OnRequestRecall));
            }

            KingdomDiplomacy diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>()
                .GetKingdomDiplomacy(council.Clan.Kingdom);
            if (council.IsRoyal && diplomacy != null)
            {
                BKExplainedNumber legitimacy = diplomacy.LegitimacyTargetExplained;
                CourtInfo.Add(new InformationElement(new TextObject("{=!}Legitimacy:").ToString(),
                    new TextObject("{=FgAWmeXm}{GRACE} / {TOTAL} ({CHANGE} / day)")
                    .SetTextVariable("GRACE", FormatValue(diplomacy.Legitimacy))
                    .SetTextVariable("TOTAL", FormatValue(legitimacy.ResultNumber))
                    .SetTextVariable("CHANGE", FormatFloatGain(diplomacy.LegitimacyChange * 100f))
                    .ToString(),
                    new TextObject("{=!}Legitimacy represents how a ruler is seen by the other members of a realm, concerning their right to rule.{newline}{newline}Legitimacy Target: {TARGET}{newline}{newline}{EXPLANATIONS}")
                    .SetTextVariable("TARGET", FormatValue(legitimacy.ResultNumber))
                    .SetTextVariable("EXPLANATIONS", legitimacy.GetFormattedPercentage())
                    .ToString()));
            }

            if (council.CourtGrace != null)
            {
                ExplainedNumber graceTarget = BannerKingsConfig.Instance.CouncilModel.CalculateGraceTarget(council, true);
                CourtInfo.Add(new InformationElement(new TextObject("{=PQ9D66GT}Grace:").ToString(),
                new TextObject("{=FgAWmeXm}{GRACE} / {TOTAL} ({CHANGE} / day)")
                .SetTextVariable("GRACE", council.CourtGrace.Grace.ToString("0.0"))
                .SetTextVariable("TOTAL", graceTarget.ResultNumber.ToString("0"))
                .SetTextVariable("CHANGE", FormatFloatGain(council.CourtGrace.GraceChange))
                .ToString(),
                new TextObject("{=VAOqDMk0}Grace is the measure of importance attributed to your court by others. A more prestigious court location and higher court expenses are naturally seen as more gracious. A more gracious court allows you more political leverage within the realm.\nGrace Target: {TARGET}\n{EXPLANATIONS}")
                .SetTextVariable("TARGET", graceTarget.ResultNumber.ToString("0"))
                .SetTextVariable("EXPLANATIONS", graceTarget.GetExplanations())
                .ToString()));

                ExplainedNumber expectedGrace = BannerKingsConfig.Instance.CouncilModel.CalculateExpectedGrace(council, true);
                CourtInfo.Add(new InformationElement(new TextObject("{=qU6Zq60j}Expected grace:").ToString(),
                expectedGrace.ResultNumber.ToString("0"),
                new TextObject("{=gWrNRjSF}Expected grace is the threshold where grace yields positive political leverage. Grace being under this number will instead diminish your influence. As nobles become more important within the realm, , specially through title hierarchy and clan tier, more grace is expected from them.\nExplanations:\n{EXPLANATIONS}")
                .SetTextVariable("EXPLANATIONS", expectedGrace.GetExplanations())
                .ToString()));
            }

            ExplainedNumber influenceCap = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(council.Clan, true);
            CourtInfo.Add(new InformationElement(new TextObject("{=bNS2Lg7L}Influence Limit:").ToString(),
                MBRandom.RoundRandomized(influenceCap.ResultNumber).ToString(),
                new TextObject("{=j26DQzoz}Influence limit represents how much political power your clan can leverage within the realm. Nobles higher in the title hierarchy or with more demesnes tend to be more relevant within the realm and thus have a higher influence limit. Clan influence will tend down to this limit if it supersedes the limit, meaning that less important families will tend to have less influence overall. Explanations:\n{EXPLANATIONS}")
                .SetTextVariable("EXPLANATIONS", influenceCap.GetExplanations())
                .ToString()));

            ExplainedNumber admCosts = BannerKingsConfig.Instance.CouncilModel.CalculateAdmCosts(council, true);
            CourtInfo.Add(new InformationElement(new TextObject("{=7OQ7dN1T}Administrative costs:").ToString(),
                FormatValue(admCosts.ResultNumber),
                new TextObject("{=Tj6ouL4D}Costs associated with payment of council members and court expenses. These costs represent a percentage of all your fiefs' revenues, and thus scale as your fiefs yield higher taxes. Explanations:\n{EXPLANATIONS}")
                .SetTextVariable("EXPLANATIONS", admCosts.GetExplanations())
                .ToString()));

            if (council.CourtGrace != null)
            {
                Dictionary<ItemCategory, int> categories = new Dictionary<ItemCategory, int>();
                foreach (CourtExpense expense in council.CourtGrace.Expenses)
                {
                    if (!expense.ConsumeItems) continue;

                    foreach (var pair in expense.ItemCategories)
                    {
                        if (categories.ContainsKey(pair.Key))
                        {
                            categories[pair.Key] += pair.Value;
                        }
                        else
                        {
                            categories.Add(pair.Key, pair.Value);
                        }
                    }
                }

                int total = 0;
                foreach (var pair in categories) total += pair.Value;

                CourtInfo.Add(new InformationElement(new TextObject("{=qJXn9rnK}Goods requirements:").ToString(),
                total.ToString(),
                new TextObject("{=qazQxKVv}Court expenses require goods to operate, on a seasonal basis, or four times a year. These goods are bought locally or used from the stash (with the exception of the Supplies expense, which only buys but does not consume them). Missing these goods or having them supplied in lower quality will yield negative grace. Higher quality goods will instead yield positive grace.\nGoods:\n{EXPLANATIONS}")
                .SetTextVariable("EXPLANATIONS", string.Join(Environment.NewLine, categories.Select(x => $"{x.Key}: {x.Value}")))
                .ToString()));
            }

            TextObject locationName = council.Location != null ? council.Location.Name : 
                new TextObject("{=sJFMPgj1}Lacking court location");
            CourtInfo.Add(new InformationElement(new TextObject("{=qGGGAMzW}Court location:").ToString(),
                locationName.ToString(),
                new TextObject("{=bohHNqF2}The fief in which your court will gather. Having a designated location is crucial for the operations of a court as well as to the image of your grace.")
                .ToString()));

            Language language = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(council.Clan.Culture);

            CourtInfo.Add(new InformationElement(new TextObject("{=7AkO8Nui}Court language:").ToString(),
                language.Name.ToString(),
                new TextObject("{=BEHbVOK5}The native language spoken by the council leader. Speaking the court language is important for councillours to fulfill their tasks adequately.")
                .ToString()));

            Managers.Institutions.Religions.Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(council.Owner);
            CourtInfo.Add(new InformationElement(new TextObject("{=KxC0nxz1}Court religion:").ToString(),
                rel != null ? rel.Faith.GetFaithName().ToString() : new TextObject("{=koX9okuG}None").ToString(),
                new TextObject("{=yGeij7MB}The council leader's faith. Spiritual positions need to follow the same faith. Other positions may be favored or not according to the candidate's faith, and how well predisposed the council owner's faith is towards it.")
                .ToString()));

            var royalExplanation = BannerKingsConfig.Instance.CouncilModel.IsCouncilRoyal(council.Owner.Clan);
            IsRoyal = royalExplanation.Item1;

            foreach (var position in council.Positions)
            {
                if (position.IsCorePosition(position.StringId))
                {
                    CorePositions.Add(new CouncilPositionVM(position, SetId, UpdatePositionTexts));
                }
                else
                {
                    ExtraPositions.Add(new CouncilPositionVM(position, SetId, UpdatePositionTexts));
                }
            }

            HasExtraPositions = ExtraPositions.Count > 0;

            var member = council.GetCouncilPosition(councilPosition);
            if (member != null)
            {
                positionName = member.Name.ToString();
                positionDescription = member.Description.ToString();
                positionEffects = member.GetEffects().ToString();

                foreach (var privilege in member.AllPrivileges)
                {
                    PrivilegesInfo.Add(new InformationElement(
                        GameTexts.FindText("str_bk_council_privilege", privilege.ToString().ToLower()).ToString(),
                        string.Empty,
                        GameTexts.FindText("str_bk_council_privilege_description", privilege.ToString().ToLower()).ToString()));
                }
            }
           
            RefreshSelectors();
            RefreshCharacter();
        }

        private void RefreshSelectors()
        {
            if (council.CourtGrace == null || council.Clan != Clan.PlayerClan)
            {
                SelectorsHidden = true;
                return;
            }

            SelectorsHidden = false;
            ExtravaganceSelector = new BannerKingsSelectorVM<CourtExpenseSelectorItemVM>(true, 0, null);
            int selected = 0;
            int index = 0;
            foreach (CourtExpense expense in DefaultCourtExpenses.Instance.All.Where(x => x.Type == CourtExpense.ExpenseType.Extravagance))
            {
                CourtExpense current = council.CourtGrace.GetExpense(expense.Type);
                ExtravaganceSelector.AddItem(new CourtExpenseSelectorItemVM(expense,
                    true));

                if (current.Equals(expense))
                {
                    selected = index;
                }

                index++;
            }

            ExtravaganceSelector.SelectedIndex = selected;
            ExtravaganceSelector.SetOnChangeAction(OnSelectorChange);

            ServantsSelector = new BannerKingsSelectorVM<CourtExpenseSelectorItemVM>(true, 0, null);
            selected = 0;
            index = 0;
            foreach (CourtExpense expense in DefaultCourtExpenses.Instance.All.Where(x => x.Type == CourtExpense.ExpenseType.Servants))
            {
                CourtExpense current = council.CourtGrace.GetExpense(expense.Type);
                ServantsSelector.AddItem(new CourtExpenseSelectorItemVM(expense,
                    true));

                if (current.Equals(expense))
                {
                    selected = index;
                }

                index++;
            }

            ServantsSelector.SelectedIndex = selected;
            ServantsSelector.SetOnChangeAction(OnSelectorChange);

            LodgingsSelector = new BannerKingsSelectorVM<CourtExpenseSelectorItemVM>(true, 0, null);
            selected = 0;
            index = 0;
            foreach (CourtExpense expense in DefaultCourtExpenses.Instance.All.Where(x => x.Type == CourtExpense.ExpenseType.Lodgings))
            {
                CourtExpense current = council.CourtGrace.GetExpense(expense.Type);
                LodgingsSelector.AddItem(new CourtExpenseSelectorItemVM(expense,
                    true));

                if (current.Equals(expense))
                {
                    selected = index;
                }

                index++;
            }

            LodgingsSelector.SelectedIndex = selected;
            LodgingsSelector.SetOnChangeAction(OnSelectorChange);

            SecuritySelector = new BannerKingsSelectorVM<CourtExpenseSelectorItemVM>(true, 0, null);
            selected = 0;
            index = 0;
            foreach (CourtExpense expense in DefaultCourtExpenses.Instance.All.Where(x => x.Type == CourtExpense.ExpenseType.Security))
            {
                CourtExpense current = council.CourtGrace.GetExpense(expense.Type);
                SecuritySelector.AddItem(new CourtExpenseSelectorItemVM(expense,
                    true));

                if (current.Equals(expense))
                {
                    selected = index;
                }

                index++;
            }

            SecuritySelector.SelectedIndex = selected;
            SecuritySelector.SetOnChangeAction(OnSelectorChange);

            SuppliesSelector = new BannerKingsSelectorVM<CourtExpenseSelectorItemVM>(true, 0, null);
            selected = 0;
            index = 0;
            foreach (CourtExpense expense in DefaultCourtExpenses.Instance.All.Where(x => x.Type == CourtExpense.ExpenseType.Supplies))
            {
                CourtExpense current = council.CourtGrace.GetExpense(expense.Type);
                SuppliesSelector.AddItem(new CourtExpenseSelectorItemVM(expense,
                    true));

                if (current.Equals(expense))
                {
                    selected = index;
                }

                index++;
            }

            SuppliesSelector.SelectedIndex = selected;
            SuppliesSelector.SetOnChangeAction(OnSelectorChange);
        }

        private void OnSelectorChange(SelectorVM<CourtExpenseSelectorItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                CourtExpenseSelectorItemVM vm = obj.GetCurrentItem();
                CourtExpense expense = vm.Expense;
                CourtExpense current = council.CourtGrace.GetExpense(expense.Type);
                if (expense != current)
                {
                    int cost = council.CourtGrace.GetExpenseChangeCost(council.Clan, expense, current);
                    TextObject description;
                    if (cost > 0)
                    {
                        description = new TextObject("{=PxX0AYBK}Are you sure that you want to increase your expenses to {EXPENSE}? This change will yield {GRACE} more grace, but cost {GOLD}{GOLD_ICON}, as well as increase your administrative costs by {ADM_COST}%.")
                            .SetTextVariable("GOLD", cost)
                            .SetTextVariable("EXPENSE", expense.Name)
                            .SetTextVariable("GRACE", (expense.Grace - current.Grace).ToString("0"))
                            .SetTextVariable("ADM_COST", ((expense.AdministrativeCost - current.AdministrativeCost) * 100f).ToString("0"));
                    }
                    else
                    {
                        description = new TextObject("{=PL6mEtzJ}Are you sure that you want to diminish your expenses to {EXPENSE}? Once reduced, increasing them again will incur financial costs. Reducing this expense will save you {ADM_COST}% in administrative costs, but reduce your grace by {GRACE}.")
                            .SetTextVariable("GOLD", cost)
                            .SetTextVariable("EXPENSE", expense.Name)
                            .SetTextVariable("GRACE", (current.Grace - expense.Grace).ToString("0"))
                            .SetTextVariable("ADM_COST", ((current.AdministrativeCost - expense.AdministrativeCost) * 100f).ToString("0"));
                    }

                    InformationManager.ShowInquiry(new InquiryData(new TextObject().ToString(),
                        description.ToString(),
                        Hero.MainHero.Gold >= cost,
                        true,
                        GameTexts.FindText("str_accept").ToString(),
                        GameTexts.FindText("str_reject").ToString(),
                        () =>
                        {
                            council.CourtGrace.AddExpense(council.Clan, expense);
                            RefreshSelectors();
                        },
                        null));
                }
            }
        }

        private void RefreshCharacter()
        {
            if (currentCharacter != null)
            {
                CourtierInfo.Clear();
                CourtierInfo.Add(new InformationElement(GameTexts.FindText("str_enc_sf_occupation").ToString(),
                    CampaignUIHelper.GetHeroOccupationName(currentCharacter.Hero), string.Empty));

                var positionString = GameTexts.FindText("role", "None").ToString();
                var heroPosition = council.GetHeroPositions(currentCharacter.Hero).FirstOrDefault();
                if (heroPosition != null)
                {
                    positionString = heroPosition.Name.ToString();
                }
                else if (currentCharacter.Hero == council.Owner)
                {
                    positionString = GameTexts.FindText("role", "ClanLeader").ToString();
                }

                CourtierInfo.Add(new InformationElement(new TextObject("{=S9zTcqbp}Council Position:").ToString(), positionString,
                    string.Empty));

                var languagesString = "";
                foreach (var pair in BannerKingsConfig.Instance.EducationManager.GetHeroEducation(currentCharacter.Hero)
                             .Languages)
                {
                    languagesString += new TextObject("{=4fLp8Y5t}{LANGUAGE} ({COMPETENCE}),")
                        .SetTextVariable("LANGUAGE", pair.Key.Name)
                        .SetTextVariable("COMPETENCE", UIHelper.GetLanguageFluencyText(pair.Value));
                }

                CourtierInfo.Add(new InformationElement(new TextObject("{=yCaxpVGh}Languages:").ToString(),
                    languagesString.Remove(languagesString.Length - 1), string.Empty));
            }
        }

        private void OnRequestRecall()
        {
        }

        private void UpdatePositionTexts(string id)
        {
            var member = council.GetCouncilPosition(DefaultCouncilPositions.Instance.All.FirstOrDefault(x => x.StringId == id));
            if (member != null)
            {
                CurrentPositionNameText = member.Name.ToString();
                CurrentPositionDescriptionText = member.Description.ToString();
                CurrentEffectsDescriptionText = member.GetEffects().ToString();
                PrivilegesInfo.Clear();
                foreach (var privilege in member.Privileges)
                {
                    PrivilegesInfo.Add(new InformationElement(
                        GameTexts.FindText("str_bk_council_privilege", privilege.ToString().ToLower()).ToString(),
                        string.Empty,
                        GameTexts.FindText("str_bk_council_privilege_description", privilege.ToString().ToLower()).ToString()));
                }
            }
        }

        private void SetId(string id)
        {
            var newPosition = council.GetCouncilPosition(DefaultCouncilPositions.Instance.All.FirstOrDefault(x => x.StringId == id));
            if (councilPosition != newPosition)
            {
                councilPosition = newPosition;
                CouncilVM.Position = newPosition;
                RefreshValues();
            }

            CouncilVM.ShowOptions();
        }

        private void SetCouncilMember(Hero member)
        {
            RefreshValues();
        }

        private void SetCurrentCharacter(ClanLordItemVM vm)
        {
            CurrentCharacter = new CharacterVM(vm.GetHero(), null);
            RefreshCharacter();
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<CourtExpenseSelectorItemVM> ExtravaganceSelector
        {
            get => extravaganceSelector;
            set
            {
                if (value != extravaganceSelector)
                {
                    extravaganceSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<CourtExpenseSelectorItemVM> LodgingsSelector
        {
            get => lodgingsSelector;
            set
            {
                if (value != lodgingsSelector)
                {
                    lodgingsSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<CourtExpenseSelectorItemVM> SecuritySelector
        {
            get => securitySelector;
            set
            {
                if (value != securitySelector)
                {
                    securitySelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<CourtExpenseSelectorItemVM> ServantsSelector
        {
            get => servantsSelector;
            set
            {
                if (value != servantsSelector)
                {
                    servantsSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<CourtExpenseSelectorItemVM> SuppliesSelector
        {
            get => suppliesSelector;
            set
            {
                if (value != suppliesSelector)
                {
                    suppliesSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string CurrentEffectsDescriptionText
        {
            get => positionEffects;
            set
            {
                if (value != positionEffects)
                {
                    positionEffects = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string CurrentPositionNameText
        {
            get => positionName;
            set
            {
                if (value != positionName)
                {
                    positionName = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string CurrentPositionDescriptionText
        {
            get => positionDescription;
            set
            {
                if (value != positionDescription)
                {
                    positionDescription = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool HasExtraPositions
        {
            get => hasExtraPositions;
            set
            {
                if (value != hasExtraPositions)
                {
                    hasExtraPositions = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool SelectorsHidden
        {
            get => selectorsVisible;
            set
            {
                if (value != selectorsVisible)
                {
                    selectorsVisible = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool IsRoyal
        {
            get => isRoyal;
            set
            {
                if (value != isRoyal)
                {
                    isRoyal = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public CharacterVM CurrentCharacter
        {
            get => currentCharacter;
            set
            {
                if (value != currentCharacter)
                {
                    currentCharacter = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public CouncilVM CouncilVM
        {
            get => councilVM;
            set
            {
                if (value != councilVM)
                {
                    councilVM = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ClanLordItemVM> Family
        {
            get => family;
            set
            {
                if (value != family)
                {
                    family = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ClanLordItemVM> Guests
        {
            get => guests;
            set
            {
                if (value != guests)
                {
                    guests = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        } 

        [DataSourceProperty]
        public MBBindingList<ClanLordItemVM> Courtiers
        {
            get => courtiers;
            set
            {
                if (value != courtiers)
                {
                    courtiers = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> PrivilegesInfo
        {
            get => privilegesInfo;
            set
            {
                if (value != privilegesInfo)
                {
                    privilegesInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> CourtierInfo
        {
            get => courtierInfo;
            set
            {
                if (value != courtierInfo)
                {
                    courtierInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> CourtInfo
        {
            get => courtInfo;
            set
            {
                if (value != courtInfo)
                {
                    courtInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CouncilPositionVM> CorePositions
        {
            get => corePositions;
            set
            {
                if (value != corePositions)
                {
                    corePositions = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<CouncilPositionVM> ExtraPositions
        {
            get => extraPositions;
            set
            {
                if (value != extraPositions)
                {
                    extraPositions = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}