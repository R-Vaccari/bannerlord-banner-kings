using BannerKings.Dialogue;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

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

        [SaveableProperty(1)] public Hero Proposer { get; private set; }
        [SaveableProperty(2)] public Hero Proposed { get; private set; }
        [SaveableProperty(3)] public Clan FinalClan { get; private set; }
        [SaveableProperty(4)] public int Dowry { get; private set; }
        [SaveableProperty(5)] public int Influence { get; private set; }
        [SaveableProperty(6)] public bool ArrangedMarriage { get; private set; }
        [SaveableProperty(7)] public bool Alliance { get; private set; }
        [SaveableProperty(8)] public bool Feast { get; private set;  }

        [SaveableProperty(9)] public bool Confirmed { get; set; }

        public (TextObject, bool) IsContractAdequate()
        {
            (TextObject, bool) result = new(new TextObject("{=!}The proposal is adequate."), true);

            var willAccept = BannerKingsConfig.Instance.MarriageModel.IsMarriageAdequate(Proposer, Proposed, true);
            if (willAccept.ResultNumber < 1f)
            {
                var text = DialogueHelper.GetRandomText(Hero.OneToOneConversationHero, DialogueHelper.GetMarriageInadequateTexts(this));
                result = new(text, false);
                return result;
            }

            if (Proposer.Clan.Influence < Influence)
            {
                result = new(new TextObject("{=!}Our house is one of great importance in the realm. As such, we expect to marry into other influential families."), false);
                return result;
            }

            if (FinalClan == Proposer.Clan)
            {
                if (Proposer.Clan.Gold < Dowry)
                {
                    result = new(new TextObject("{=!}It seems to me you are not able to pay the dowry. In order for me to accept this proposal, prove you are truly invested into it's success with a fair dowry."), false);
                    return result;
                }

                if (Feast && FinalClan.Fiefs.Count == 0)
                {
                    result = new(new TextObject("{=!}You propose a feast but your family does not hold any appropriate place for such. As your family is the one being married to, you should be able to provide such place."), false);
                    return result;
                }
            }
            else
            {
                if (Proposed.Clan.Gold < Dowry)
                {
                    result = new(new TextObject("{=!}We don't have enough denars for the dowry."), false);
                    return result;
                }

                if (Feast && FinalClan.Fiefs.Count == 0)
                {
                    result = new(new TextObject("{=!}Unfortunately, we do not currently hold a town or castle in which we could host a feast. As the family being married into, it would be shame to promise a feast and not be able to provide it."), false);
                    return result;
                }
            }

            return result;
        }
    }
}
