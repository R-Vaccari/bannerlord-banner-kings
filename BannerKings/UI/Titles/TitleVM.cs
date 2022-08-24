using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Titles
{
    public class TitleVM : ViewModel
    {
        private MBBindingList<DecisionElement> decisions;
        private BasicTooltipViewModel hint;
        private ImageIdentifierVM imageIdentifier;
        private readonly FeudalTitle title;

        public TitleVM(FeudalTitle title)
        {
            this.title = title;
            decisions = new MBBindingList<DecisionElement>();
            RefreshValues();
        }

        [DataSourceProperty]
        public MBBindingList<DecisionElement> Decisions
        {
            get => decisions;
            set
            {
                if (value != decisions)
                {
                    decisions = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public BasicTooltipViewModel Hint
        {
            get => hint;
            set
            {
                if (value != hint)
                {
                    hint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => imageIdentifier;
            set
            {
                if (value != imageIdentifier)
                {
                    imageIdentifier = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty] public string NameText => title.FullName.ToString();

        public override void RefreshValues()
        {
            base.RefreshValues();
            Decisions.Clear();


            if (title != null)
            {
                var model = BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel;
                var characterCode = CharacterCode.CreateFrom(title.deJure.CharacterObject);
                ImageIdentifier = new ImageIdentifierVM(characterCode);

                var actions = new List<TitleAction>();
                var usurpData = model.GetAction(ActionType.Usurp, title, Hero.MainHero);
                if (title.GetHeroClaim(Hero.MainHero) != ClaimType.None)
                {
                    var usurpButton = new DecisionElement().SetAsButtonOption(new TextObject("{=xZPva3Ys}Usurp").ToString(),
                        () => UIHelper.ShowActionPopup(usurpData, this));
                    usurpButton.Enabled = usurpData.Possible;
                    Decisions.Add(usurpButton);
                }

                var claimAction = model.GetAction(ActionType.Claim, title, Hero.MainHero);
                if (claimAction.Possible)
                {
                    var claimButton = new DecisionElement().SetAsButtonOption(new TextObject("{=535uvcbA}Claim").ToString(),
                        () => UIHelper.ShowActionPopup(claimAction, this));
                    claimButton.Enabled = claimAction.Possible;
                    Decisions.Add(claimButton);
                }

                var grantData = model.GetAction(ActionType.Grant, title, Hero.MainHero);
                if (grantData.Possible)
                {
                    var grantButton = new DecisionElement().SetAsButtonOption(new TextObject("{=tQ0fpGPj}Grant").ToString(),
                        () => UIHelper.ShowActionPopup(grantData, this));
                    grantButton.Enabled = grantData.Possible;
                    Decisions.Add(grantButton);
                }

                var revokeData = model.GetAction(ActionType.Revoke, title, Hero.MainHero);
                if (revokeData.Possible)
                {
                    var revokeButton = new DecisionElement().SetAsButtonOption(new TextObject("{=8rTKBaA9}Revoke").ToString(),
                        () => UIHelper.ShowActionPopup(revokeData, this));
                    revokeButton.Enabled = revokeData.Possible;
                    Decisions.Add(revokeButton);
                }

                if (title.deJure != Hero.MainHero)
                {
                    actions.Add(usurpData);
                    actions.Add(claimAction);
                    actions.Add(revokeData);
                }
                else
                {
                    actions.Add(grantData);
                }

                Hint = new BasicTooltipViewModel(() => UIHelper.GetTitleTooltip(title, actions));
            }
        }

        public void ExecuteLink()
        {
            if (title.deJure != null)
            {
                Campaign.Current.EncyclopediaManager.GoToLink(title.deJure.EncyclopediaLink);
            }
        }
    }
}