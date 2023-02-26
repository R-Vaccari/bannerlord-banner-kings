using System;
using System.Linq;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
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
        private MBBindingList<ClanLordItemVM> family, courtiers;
        private bool isRoyal, hasExtraPositions;
        private string positionName, positionDescription, positionEffects;

        private readonly ITeleportationCampaignBehavior teleportationBehavior =
            Campaign.Current.GetCampaignBehavior<ITeleportationCampaignBehavior>();

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
            isRoyal = royal;
            currentCharacter = new CharacterVM(Hero.MainHero, null);
        }

        [DataSourceProperty] public string FamilyText => new TextObject("{=!}Household").ToString();
        [DataSourceProperty] public string CourtiersText => new TextObject("{=PykdjcGm}Courtiers").ToString();
        [DataSourceProperty] public string EffectsText => new TextObject("{=K7df68TT}Effects").ToString();
        [DataSourceProperty] public string PrivilegesText => new TextObject("{=77D4i3pG}Privileges").ToString();
        [DataSourceProperty] public string PrivyCouncilText => new TextObject("{=!}Privy Council").ToString();
        [DataSourceProperty] public string ExtendedCouncilText => new TextObject("{=!}Extended Council").ToString();
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

            CourtInfo.Add(new InformationElement(new TextObject("{=7OQ7dN1T}Administrative costs:").ToString(), 
                FormatValue(council.AdministrativeCosts),
                new TextObject("{=1zAZjJy9}Costs associated with payment of council members, deducted on all your fiefs' revenues.")
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

            RefreshCharacter();
        }

        private void RefreshCharacter()
        {
            if (currentCharacter != null)
            {
                CourtierInfo.Clear();
                CourtierInfo.Add(new InformationElement(GameTexts.FindText("str_enc_sf_occupation").ToString(),
                    CampaignUIHelper.GetHeroOccupationName(currentCharacter.Hero), string.Empty));

                var positionString = GameTexts.FindText("role", "None").ToString();
                var heroPosition = council.GetHeroPosition(currentCharacter.Hero);
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
            council.GetCouncilPosition(councilPosition).SetMember(member);
            RefreshValues();
        }

        private void SetCurrentCharacter(ClanLordItemVM vm)
        {
            CurrentCharacter = new CharacterVM(vm.GetHero(), null);
            RefreshCharacter();
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