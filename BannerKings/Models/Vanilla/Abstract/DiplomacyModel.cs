using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Titles.Governments;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla.Abstract
{
    public abstract class DiplomacyModel : DefaultDiplomacyModel
    {
        public abstract ExplainedNumber WillMercenaryDeclareWar(Kingdom kingdom, Clan firedMercenary, bool explanations = false);
        public abstract ExplainedNumber GetRightInnfluenceCost(ContractRight right, Hero suzerain, Hero vassal);
        public abstract ExplainedNumber WillSuzerainAcceptRight(ContractRight right, Hero suzerain, Hero vassal);
        public abstract ExplainedNumber GetAnnexationCostExplained(Clan proposingClan, Town town = null);
        public abstract ExplainedNumber CalculateHeroFiefScore(Settlement settlement, Hero annexing, bool explanations = false);
        public abstract ExplainedNumber MercenaryLeaveScore(Clan mercenaryClan, Kingdom kingdom, bool explanations = false);
        public abstract ExplainedNumber GetTruceDenarCost(Kingdom proposer, Kingdom proposed, float years = 3f, bool explanations = false);
        public abstract void AddProposeDiplomacyCostEffects(Hero proposer, ref ExplainedNumber result);
        public abstract void AddAmicablePactDesireEffects(Kingdom proposer, Kingdom proposed, ref ExplainedNumber result, bool explanations = false);
        public abstract ExplainedNumber GetTradeDesire(Kingdom proposer, Kingdom proposed, bool explanations = false);
        public abstract ExplainedNumber GetAllianceDesire(Kingdom proposer, Kingdom proposed, bool explanations = false);
        public abstract bool WillAcceptTrade(Kingdom proposer, Kingdom proposed, bool explanations = false);
        public abstract bool IsTruceAcceptable(Kingdom proposer, Kingdom proposed, bool explanations = false);
        public abstract bool WillAcceptAlliance(Kingdom proposer, Kingdom proposed);
        public abstract ExplainedNumber GetAllianceDenarCost(Kingdom proposer, Kingdom proposed, bool explanations = false);
        public abstract ExplainedNumber GetTradePactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false);
        public abstract ExplainedNumber GetPactInfluenceCost(Kingdom proposer, Kingdom proposed, bool explanations = false);
        public abstract ExplainedNumber WillJoinWar(IFaction attacker, IFaction defender, IFaction ally,
            DeclareWarAction.DeclareWarDetail detail, bool explanations = false);
        public abstract ExplainedNumber GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan,
           out TextObject warReason, CasusBelli casusBelli = null, bool explanations = false);
        public abstract ExplainedNumber GetMercenaryDownPayment(Clan mercenaryClan, Kingdom kingdom, bool explanations = false);
    }
}
