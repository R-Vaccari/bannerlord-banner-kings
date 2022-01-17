using TaleWorlds.CampaignSystem;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static Populations.Managers.TitleManager;
using TaleWorlds.CampaignSystem.Actions;

namespace Populations.Behaviors
{
    class FeudalCompanionBehavior : CampaignBehaviorBase
    {

        private FeudalTitle titleGiven = null;
        List<InquiryElement> lordshipsToGive = new List<InquiryElement>();
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, new Action<Hero, Hero, KillCharacterAction.KillCharacterActionDetail, bool>(this.OnHeroKilled));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialog(campaignGameStarter);
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification = true)
        {
        
        }

        private void AddDialog(CampaignGameStarter starter)
        {
            StringBuilder knighthoodSb = new StringBuilder();
            knighthoodSb.Append("By knighting, you are granting this person nobility and they will be bound to you as your vassal by the standard contract of the kingdom. A lordship must be given away to seal the contract.");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append(" ");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append("Their lands and titles henceforth can not be revoked without lawful cause, and any fief revenue will be theirs, taxed or not by you as per contract");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append(" ");
            knighthoodSb.Append(Environment.NewLine);
            knighthoodSb.Append("As a knight, they are capable of raising a personal retinue and are obliged to fulfill their duties.");

            starter.AddPlayerLine("companion_grant_knighthood", "companion_role", "companion_knighthood_question", "Would you like to serve me as my knight?", 
                new ConversationSentence.OnConditionDelegate(this.companion_grant_knighthood_on_condition), delegate {
                    InformationManager.ShowInquiry(new InquiryData("Bestowing Knighthood", knighthoodSb.ToString(), true, false, "Understood", null, null, null), false);
                }, 100, null, null);

            starter.AddDialogLine("companion_grant_knighthood_response", "companion_knighthood_question", "companion_knighthood_response",
                "My lord, I would be honored.", null, null, 100, null); 

            starter.AddPlayerLine("companion_grant_knighthood_response_confirm", "companion_knighthood_response", "companion_knighthood_accepted", "Let us decide your fief.",
                new ConversationSentence.OnConditionDelegate(this.companion_knighthood_accepted_on_condition), new ConversationSentence.OnConsequenceDelegate(this.companion_knighthood_accepted_on_consequence), 100, null, null);

            starter.AddPlayerLine("companion_grant_knighthood_response_cancel", "companion_knighthood_response", "companion_role_pretalk", "Actualy, I would like to discuss this at a later time.",
               null, null, 100, null, null);

            starter.AddPlayerLine("companion_grant_knighthood_granted", "companion_knighthood_accepted", "close_window", "It is decided then. I bestow upon you the title of Knight.",
                null, null, 100, null, null);
        }

        private bool companion_grant_knighthood_on_condition()
        {
            if (PopulationConfig.Instance.TitleManager == null) return false;
            Hero companion = Hero.OneToOneConversationHero;
            FeudalTitle title = PopulationConfig.Instance.TitleManager.GetHighestTitle(Hero.MainHero);
            if (companion != null && companion.Clan == Clan.PlayerClan && Hero.MainHero.Clan.Tier >= 2 &&
                Hero.MainHero.Clan.Kingdom != null && title != null && title.type != TitleType.Lordship)
                return !PopulationConfig.Instance.TitleManager.IsHeroKnighted(companion);
            else return false;
        }

        private bool companion_knighthood_accepted_on_condition()
        {
            lordshipsToGive.Clear();
            HashSet<FeudalTitle> titles = PopulationConfig.Instance.TitleManager.GetTitles(Hero.MainHero);
            foreach (FeudalTitle title in titles)
            {
                if (title.type != TitleType.Lordship || title.fief == null || title.deJure != Hero.MainHero) continue;
                lordshipsToGive.Add(new InquiryElement(title, title.name.ToString(), new ImageIdentifier()));
            }

            if (lordshipsToGive.Count == 0)
                InformationManager.DisplayMessage(new InformationMessage("You currently do not lawfully own a lordship that could be given away."));

            return lordshipsToGive.Count >= 1;
        }

        private void companion_knighthood_accepted_on_consequence()
        {
            InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    "Select the fief you would like to give away", string.Empty, lordshipsToGive, true, 1, 
                    GameTexts.FindText("str_done", null).ToString(), "", new Action<List<InquiryElement>>(this.OnNewPartySelectionOver), 
                    new Action<List<InquiryElement>>(this.OnNewPartySelectionOver), ""), false);
        }

        private void OnNewPartySelectionOver(List<InquiryElement> element)
        {
            if (element.Count == 0)
                return;
            
            this.titleGiven = (FeudalTitle)element[0].Identifier;
            PopulationConfig.Instance.TitleManager.GrantLordship(this.titleGiven, Hero.MainHero, Hero.OneToOneConversationHero);
        }
    }
}
