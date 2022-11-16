using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Marriage
{
    public class MarriageContract
    {
        public MarriageContract(Hero proposer, Hero proposed, Clan finalClan, int dowry, int influence, bool arrangedMarriage, bool alliance, bool feast)
        {
            Proposer = proposer;
            Proposed = proposed;
            FinalClan = finalClan;
            Dowry = dowry;
            Influence = influence;
            ArrangedMarriage = arrangedMarriage;
            Alliance = alliance;
            Feast = feast;
        }

        public Hero Proposer { get; private set; }
        public Hero Proposed { get; private set; }
        public Clan FinalClan { get; private set; }
        public int Dowry { get; private set; }
        public int Influence { get; private set; }
        public bool ArrangedMarriage { get; private set; }
        public bool Alliance { get; private set; }
        public bool Feast { get; private set;  }

        public bool Confirmed { get; set; }


        public (TextObject, bool) IsContractAdequate()
        {
            (TextObject, bool) result = new(new TextObject("{=!}The proposal is adequate."), true);

            if (Proposer.Clan.Influence < Influence)
            {
                result = new(GameTexts.FindText("str_decision_not_enough_influence"), false);
            }


            if (FinalClan == Proposer.Clan)
            {
                if (Proposer.Clan.Gold < Dowry)
                {
                    result = new(GameTexts.FindText("str_decision_not_enough_gold"), false);
                }
            }
            else
            {
                if (Proposed.Clan.Gold < Dowry)
                {
                    result = new(new TextObject("{=!}We don't have enough denars for the dowry."), false);
                }
            }


            return result;
        }
    }
}
